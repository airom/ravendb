// -----------------------------------------------------------------------
//  <copyright file="SubscriptionBatchOptions.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System;
using System.Threading;
using Raven.Abstractions.Util;
using Raven.Client.Extensions;
using Raven.Imports.Newtonsoft.Json;
using Raven.Json.Linq;

namespace Raven.Abstractions.Data
{
    public class SubscriptionConnectionClientMessage
    {
        public enum MessageType
        {
            None,
            Acknowledge
        }

        public MessageType Type { get; set; }
        public long Etag { get; set; }
    }

    public class SubscriptionConnectionServerMessage
    {
        public enum MessageType
        {
            None,
            CoonectionStatus,
            EndOfBatch,
            Data,
            Confirm,
            Terminated
        }

        public enum ConnectionStatus
        {
            None,
            Accepted,
            InUse,
            Closed,
            NotFound
        }

        public MessageType Type { get; set; }
        public ConnectionStatus Status { get; set; }
        public RavenJObject Data { get; set; }
    }

    public class SubscriptionConnectionOptions
    {
        public long SubscriptionId { get; set; }

        public SubscriptionConnectionOptions()
        {
            Strategy = SubscriptionOpeningStrategy.OpenIfFree;
            MaxDocsPerBatch = 4096;
        }

        [JsonIgnore] public CancellationTokenSource CancellationTokenSource;
        [JsonIgnore] public IDisposable DisposeOnDisconnect;

        public int TimeToWaitBeforeConnectionRetryMilliseconds { get; set; } = 5000;

        public bool IgnoreSubscribersErrors { get; set; }

        public SubscriptionOpeningStrategy Strategy { get; set; }

        public int? MaxBatchSize { get; set; }

        public int MaxDocsPerBatch { get; set; }
    }        
}