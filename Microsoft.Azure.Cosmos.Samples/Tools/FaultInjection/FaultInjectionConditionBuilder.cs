﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.FaultInjection
{
    using System;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// This class is used to build a <see cref="FaultInjectionCondition"/>
    /// </summary>
    public sealed class FaultInjectionConditionBuilder
    {
        private FaultInjectionOperationType operationType;
        private FaultInjectionConnectionType connectionType;
        private string region = string.Empty;
        private FaultInjectionEndpoint? endpoint;
        private string containerResourceId = string.Empty;

        /// <summary>
        /// Optional. Specifies which operation type rule will target. Once set, the rule will only target requests with this operation type.
        /// By default, the rule will target all operation types.
        /// </summary>
        /// <param name="operationType"></param>
        /// <returns>the <see cref="FaultInjectionCondition"/>.</returns>
        public FaultInjectionConditionBuilder WithOperationType(FaultInjectionOperationType operationType) 
        {
            this.operationType = operationType;
            return this;
        }

        /// <summary>
        /// Optional. Specifies which connection type rule will target. Once set, the rule will only target requests with this connection type.
        /// By default, the rule will target all connection types.
        /// </summary>
        /// <param name="connectionType"></param>
        /// <returns>the <see cref="FaultInjectionConditionBuilder"/>.</returns>
        public FaultInjectionConditionBuilder WithConnectionType(FaultInjectionConnectionType connectionType)
        {
            this.connectionType = connectionType;
            return this;
        }

        /// <summary>
        /// Optional. Specifies which region the rule will target. Once set, the rule will only target requests targeting that region. 
        /// By default, the rule will target all regions.
        /// </summary>
        /// <param name="region"></param>
        /// <returns>the <see cref="FaultInjectionConditionBuilder"/></returns>
        public FaultInjectionConditionBuilder WithRegion(string region)
        {
            RegionNameMapper mapper = new RegionNameMapper();
            string regionName = mapper.GetCosmosDBRegionName(region);
            this.region = string.IsNullOrEmpty(regionName) 
                ? throw new ArgumentNullException(nameof(region), "Argument 'region' cannot be null.") 
                : regionName;

            return this;
        }

        /// <summary>
        /// Optional. Specifies which endpoint the rule will target. Once set, the rule will only target requests targeting that endpoint. 
        /// Only applicable to direct mode. 
        /// You must specify the conainter and partition key definition if you specify an endpoint.
        /// By default, the rule will target all endpoints
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="containerResourceId">The container resource id</param>
        /// <returns> the <see cref="FaultInjectionConditionBuilder"/></returns>
        public FaultInjectionConditionBuilder WithEndpoint(FaultInjectionEndpoint endpoint, string containerResourceId)
        {
            this.endpoint = endpoint;
            this.containerResourceId = containerResourceId;
            return this;
        }

        /// <summary>
        /// Creates the <see cref="FaultInjectionCondition"/>.
        /// </summary>
        /// <returns>the <see cref="FaultInjectionCondition"/>.</returns>
        public FaultInjectionCondition Build()
        {
            return new FaultInjectionCondition(
                this.operationType, 
                this.connectionType, 
                this.region, 
                this.endpoint ?? FaultInjectionEndpoint.Empty, 
                this.containerResourceId);
        }

    }
}