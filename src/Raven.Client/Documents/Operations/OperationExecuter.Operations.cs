﻿using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Util;
using Sparrow.Json;

namespace Raven.Client.Documents.Operations
{
    public partial class OperationExecuter
    {
        public Operation Send(IOperation<OperationIdResult> operation)
        {
            return AsyncHelpers.RunSync(() => SendAsync(operation));
        }

        public async Task<Operation> SendAsync(IOperation<OperationIdResult> operation, CancellationToken token = default(CancellationToken))
        {
            JsonOperationContext context;
            using (GetContext(out context))
            {
                var command = operation.GetCommand(_store.Conventions, context, _requestExecuter.Cache);

                await _requestExecuter.ExecuteAsync(command, context, token).ConfigureAwait(false);
                return new Operation(_requestExecuter, _store.Conventions, command.Result.OperationId);
            }
        }
    }
}