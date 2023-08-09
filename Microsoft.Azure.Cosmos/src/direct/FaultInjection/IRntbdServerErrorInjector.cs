﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Documents.FaultInjection
{
    using System;
    using Microsoft.Azure.Documents.Rntbd;

    /// <summary>
    /// Interface for RNTBD server error injector
    /// </summary>
    internal interface IRntbdServerErrorInjector
    {
        /// <summary>
        /// Injects a delay in the RNTBD server response
        /// </summary>
        /// <param name="args"></param>
        /// <param name="transportRequestStats"></param>
        /// <returns>a bool representing if the injection was sucessfull.</returns>
        bool InjectRntbdServerResponseDelay(
            ChannelCallArguments args,
            TransportRequestStats transportRequestStats);

        /// <summary>
        /// Injects a server error in the RNTBD server response
        /// </summary>
        /// <param name="args"></param>
        /// <param name="transportRequestStats"></param>
        /// <returns>a bool representing if the injection was sucessfull.</returns>
        bool InjectRntbdServerResponseError(
            ChannelCallArguments args,
            TransportRequestStats transportRequestStats);

        /// <summary>
        /// Injects a delay in the RNTBD server connection
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="callUri"></param>
        /// <param name="request"></param>
        /// <returns>a bool representing if the injection was sucessfull.</returns>
        public bool InjectRntbdServerConnectionDelay(
            Guid activityId,
            string callUri,
            DocumentServiceRequest request);

    }
}