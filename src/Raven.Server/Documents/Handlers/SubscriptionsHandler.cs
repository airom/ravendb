﻿using System;
using System.Threading.Tasks;
using Raven.Abstractions.Logging;
using Raven.Server.Routing;
using Raven.Server.ServerWide.Context;
using Sparrow.Json.Parsing;
using Sparrow.Json;
using System.Net.WebSockets;


namespace Raven.Server.Documents.Handlers
{
    public class SubscriptionsHandler : DatabaseRequestHandler
    {
        [RavenAction("/databases/*/subscriptions/create", "POST", "/databases/{databaseName:string}/subscriptions/create?startEtag={startEtag:long|optional}")]
        public async Task Create()
        {
            DocumentsOperationContext context;
            using (ContextPool.AllocateOperationContext(out context))
            {
                var startEtag = GetLongQueryString("startEtag") ?? 0;

                var subscriptionCriteriaRaw = await context.ReadForDiskAsync(RequestBodyStream(), null).ConfigureAwait(false);
                var subscriptionId = Database.SubscriptionStorage.CreateSubscription(subscriptionCriteriaRaw, startEtag);
                HttpContext.Response.StatusCode = 201; // NoContent

                using (var writer = new BlittableJsonTextWriter(context, ResponseBodyStream()))
                {
                    context.Write(writer, new DynamicJsonValue
                    {
                        ["Id"] = subscriptionId
                    });
                }
            }
        }

        [RavenAction("/databases/*/subscriptions", "DELETE",
            "/databases/{databaseName:string}/subscriptions?id={subscriptionId:long}")]
        public Task Delete()
        {
            var ids = HttpContext.Request.Query["id"];
            if (ids.Count == 0)
                throw new ArgumentException("The 'id' query string parameter is mandatory");

            long id;
            if (long.TryParse(ids[0], out id) == false)
                throw new ArgumentException("The 'id' query string parameter must be a valid long");

            Database.SubscriptionStorage.DeleteSubscription(id);

            HttpContext.Response.StatusCode = 204; // NoContent

            return Task.CompletedTask;
        }


        [RavenAction("/databases/*/subscriptions", "GET", "/databases/{databaseName:string}/subscriptions?start={start:int}&pageSize={pageSize:int}")]
        public Task Get()
        {
            var start = GetStart();
            var take = GetPageSize(Database.Configuration.Core.MaxPageSize);
            DocumentsOperationContext context;
            using (ContextPool.AllocateOperationContext(out context))
            {
                context.OpenReadTransaction();
                var subscriptionTableValues = Database.SubscriptionStorage.GetSubscriptions(start, take);
                HttpContext.Response.StatusCode = 200;

                using (var writer = new BlittableJsonTextWriter(context, ResponseBodyStream()))
                {
                    Database.SubscriptionStorage.WriteSubscriptionTableValues(writer, context, subscriptionTableValues);
                }
            }
            return Task.CompletedTask;
        }
    }
}