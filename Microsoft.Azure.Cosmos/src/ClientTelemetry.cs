﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Handler;
    using HdrHistogram;
    using Microsoft.Azure.Cosmos.Core.Trace;
    using Microsoft.Azure.Cosmos.CosmosElements;
    using Microsoft.Azure.Cosmos.CosmosElements.Telemetry;
    using Microsoft.Azure.Cosmos.Tracing;
    using Microsoft.Azure.Cosmos.Util;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Collections;
    using Microsoft.Azure.Documents.Rntbd;
    using Newtonsoft.Json.Linq;
    using static Microsoft.Azure.Cosmos.Handlers.DiagnosticsHandler;

    /// <summary>
    /// This class collect all the telemetry information
    /// </summary>
    internal class ClientTelemetry : IDisposable
    {
        internal readonly CancellationTokenSource CancellationTokenSource;
        internal ClientTelemetryInfo ClientTelemetryInfo;
        internal double ClientTelemetrySchedulingInSeconds;
        internal DocumentClient documentClient;
        internal CosmosHttpClient httpClient;
        internal AuthorizationTokenProvider TokenProvider;

        private bool isDisposed = false;
        private Task accountInfoTask;
        private Task vmTask;
        private Task telemetryTask;

        public ClientTelemetry(
            DocumentClient documentClient,
            bool? acceleratedNetworking,
            ConnectionPolicy connectionPolicy,
            AuthorizationTokenProvider authorizationTokenProvider)
        {
            this.documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
            if (connectionPolicy == null)
            {
                throw new ArgumentNullException(nameof(connectionPolicy));
            }

            this.ClientTelemetryInfo = new ClientTelemetryInfo(
                clientId: Guid.NewGuid().ToString(), 
                processId: System.Diagnostics.Process.GetCurrentProcess().ProcessName, 
                userAgent: connectionPolicy.UserAgentContainer.UserAgent, 
                connectionMode: connectionPolicy.ConnectionMode,
                acceleratedNetworking: acceleratedNetworking);

            this.ClientTelemetrySchedulingInSeconds = ClientTelemetryOptions.GetSchedulingInSeconds();
            this.httpClient = documentClient.httpClient;
            this.CancellationTokenSource = new CancellationTokenSource();
            this.TokenProvider = authorizationTokenProvider;
        }

        /// <summary>
        ///  Start telemetry Process which trigger 3 seprate processes
        ///  1. Set Account information
        ///  2. Load VM metedata information
        ///  3. Calculate and Send telemetry Information (infinite process)
        /// </summary>
        internal void Start()
        {
            this.accountInfoTask = Task.Run(this.SetAccountNameAsync);
            this.vmTask = Task.Run(this.LoadAzureVmMetaDataAsync);

            this.telemetryTask = Task.Run(this.CalculateAndSendTelemetryInformationAsync);
        }

        /// <summary>
        /// Gets Account Properties from cache if available otherwise make a network call
        /// </summary>
        /// <returns>It is a Task</returns>
        internal async Task SetAccountNameAsync()
        {
            AccountProperties accountProperties = await this.documentClient.GlobalEndpointManager.GetDatabaseAccountAsync();
            this.ClientTelemetryInfo.GlobalDatabaseAccountName = accountProperties.Id;
        }

        /// <summary>
        /// It is a separate thread which collects virtual machine metadata information.
        /// </summary>
        /// <returns>Async Task</returns>
        internal async Task LoadAzureVmMetaDataAsync()
        {
            try
            {
                static ValueTask<HttpRequestMessage> CreateRequestMessage()
                {
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri(ClientTelemetryOptions.GetVmMetadataUrl()),
                        Method = HttpMethod.Get,
                    };
                    request.Headers.Add("Metadata", "true");

                    return new ValueTask<HttpRequestMessage>(request);
                }
                using HttpResponseMessage httpResponseMessage = await this.httpClient
                    .SendHttpAsync(CreateRequestMessage, ResourceType.Unknown, HttpTimeoutPolicyDefault.Instance, null, this.CancellationTokenSource.Token);
                   
                AzureVMMetadata azMetadata = await ClientTelemetryOptions.ProcessResponseAsync(httpResponseMessage);

                this.ClientTelemetryInfo.ApplicationRegion = azMetadata.Location;
                this.ClientTelemetryInfo.HostEnvInfo = ClientTelemetryOptions.GetHostInformation(azMetadata);
            }
            catch (Exception ex)
            {
                DefaultTrace.TraceError("Exception in LoadAzureVmMetaDataAsync() " + ex.Message);
            }
        }

        internal async Task CalculateAndSendTelemetryInformationAsync()
        {
            if (this.CancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(this.ClientTelemetrySchedulingInSeconds), this.CancellationTokenSource.Token);

                this.ClientTelemetryInfo.TimeStamp = DateTime.UtcNow.ToString(ClientTelemetryOptions.DateFormat);

                this.RecordSystemUtilization();
                this.CalculateMetrics();
                //this.Send();
            }
            catch (Exception ex)
            {
                DefaultTrace.TraceError("Exception in CalculateAndSendTelemetryInformationAsync() : " + ex.Message);
            }
            finally
            {
                await this.CalculateAndSendTelemetryInformationAsync();
            }
        }

        /// <summary>
        /// This function is being called on each operation and it collects corresponding telemetry information.
        /// </summary>
        /// <param name="cosmosDiagnostics"></param>
        /// <param name="statusCode"></param>
        /// <param name="responseSizeInBytes"></param>
        /// <param name="containerId"></param>
        /// <param name="databaseId"></param>
        /// <param name="operationType"></param>
        /// <param name="resourceType"></param>
        /// <param name="consistencyLevel"></param>
        /// <param name="requestCharge"></param>
        internal void Collect(CosmosDiagnostics cosmosDiagnostics,
                            HttpStatusCode statusCode,
                            int responseSizeInBytes,
                            string containerId,
                            string databaseId,
                            OperationType operationType,
                            ResourceType resourceType,
                            ConsistencyLevel? consistencyLevel,
                            double requestCharge)
        {
            IReadOnlyList<(string regionName, Uri uri)> regionList = cosmosDiagnostics.GetContactedRegions();
            IList<Uri> regionUris = new List<Uri>();
            foreach ((_, Uri uri) in regionList)
            {
                regionUris.Add(uri);
            }
            
            // Recordig Request Latency
            this.ClientTelemetryInfo
                .OperationInfoMap
                .GetOrAdd(new ReportPayload(regionsContacted: string.Join(",", regionUris),
                                            responseSizeInBytes: responseSizeInBytes,
                                            consistency: consistencyLevel.GetValueOrDefault(),
                                            databaseName: databaseId,
                                            containerName: containerId,
                                            operation: operationType,
                                            resource: resourceType,
                                            statusCode: (int)statusCode, 
                                            ClientTelemetryOptions.RequestLatencyName, 
                                            ClientTelemetryOptions.RequestLatencyUnit), 
                          new LongConcurrentHistogram(1,
                                                      ClientTelemetryOptions.RequestLatencyMaxMicroSec,
                                                      statusCode.IsSuccess() ? 
                                                        ClientTelemetryOptions.RequestLatencySuccessPrecision : 
                                                        ClientTelemetryOptions.RequestLatencyFailurePrecision))
                .RecordValue((long)cosmosDiagnostics.GetClientElapsedTime().TotalMilliseconds * 1000);
            
            // Recording Request Charge
            this.ClientTelemetryInfo
                .OperationInfoMap
                .GetOrAdd(new ReportPayload(regionsContacted: string.Join(",", regionUris),
                                                            responseSizeInBytes: responseSizeInBytes,
                                                            consistency: consistencyLevel.GetValueOrDefault(),
                                                            databaseName: databaseId,
                                                            containerName: containerId,
                                                            operation: operationType,
                                                            resource: resourceType,
                                                            statusCode: (int)statusCode, 
                                                            ClientTelemetryOptions.RequestChargeName, 
                                                            ClientTelemetryOptions.RequestChargeUnit), 
                          new LongConcurrentHistogram(1, 
                                                      ClientTelemetryOptions.RequestChargeMax, 
                                                      ClientTelemetryOptions.RequestChargePrecision))
                .RecordValue((long)requestCharge);
        }

        private void RecordSystemUtilization()
        {
            // Waiting for msdata repo changes
        }

        private void CalculateMetrics()
        {
            this.FillMetricInformation(this.ClientTelemetryInfo.CacheRefreshInfoMap);
            this.FillMetricInformation(this.ClientTelemetryInfo.OperationInfoMap);
            this.FillMetricInformation(this.ClientTelemetryInfo.SystemInfoMap);
        }

        private void FillMetricInformation(IDictionary<ReportPayload, LongConcurrentHistogram> metrics)
        {
            foreach (KeyValuePair<ReportPayload, LongConcurrentHistogram> entry in metrics)
            {
                ReportPayload payload = entry.Key;
                LongConcurrentHistogram histogram = entry.Value;

                LongConcurrentHistogram copyHistogram = (LongConcurrentHistogram)histogram.Copy();
                payload.MetricInfo.Count = copyHistogram.TotalCount;
                payload.MetricInfo.Max = copyHistogram.GetMaxValue();
                payload.MetricInfo.Min = copyHistogram.GetMinValue();
                payload.MetricInfo.Mean = copyHistogram.GetMean();
                IDictionary<Double, Double> percentile = new Dictionary<Double, Double>
                {
                    { ClientTelemetryOptions.Percentile50,  copyHistogram.GetValueAtPercentile(ClientTelemetryOptions.Percentile50) },
                    { ClientTelemetryOptions.Percentile90,  copyHistogram.GetValueAtPercentile(ClientTelemetryOptions.Percentile90) },
                    { ClientTelemetryOptions.Percentile95,  copyHistogram.GetValueAtPercentile(ClientTelemetryOptions.Percentile95) },
                    { ClientTelemetryOptions.Percentile99,  copyHistogram.GetValueAtPercentile(ClientTelemetryOptions.Percentile99) },
                    { ClientTelemetryOptions.Percentile999, copyHistogram.GetValueAtPercentile(ClientTelemetryOptions.Percentile999) }
                };
                payload.MetricInfo.Percentiles = percentile;
            }
        }

       /* private void Send()
        {

            this.TokenProvider.AddAuthorizationHeaderAsync(
                       new INameValueCollection.(),
                       new Uri(ClientTelemetryOptions.GetClientTelemetryEndpoint()),
                       "POST",
                       AuthorizationTokenType.PrimaryMasterKey);
        }*/

        internal void Reset()
        {
            this.ClientTelemetryInfo.OperationInfoMap.Clear();
            this.ClientTelemetryInfo.SystemInfoMap.Clear();
            this.ClientTelemetryInfo.CacheRefreshInfoMap.Clear();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Dispose of cosmos client
        /// </summary>
        /// <param name="disposing">True if disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing && !this.CancellationTokenSource.IsCancellationRequested)
                {
                    this.CancellationTokenSource.Cancel();
                    this.CancellationTokenSource.Dispose();

                    this.telemetryTask = null;
                    this.vmTask = null;
                    this.accountInfoTask = null;
                }

                this.isDisposed = true;
            }
        }
    }
}