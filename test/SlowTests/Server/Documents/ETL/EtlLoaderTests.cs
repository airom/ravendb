﻿using System;
using System.Threading.Tasks;
using Raven.Client.Server.ETL;
using Raven.Server.Documents.ETL;
using Raven.Server.Documents.ETL.Providers.Raven;
using Raven.Server.NotificationCenter.Notifications;
using Sparrow.Collections;
using Xunit;

namespace SlowTests.Server.Documents.ETL
{
    public class EtlLoaderTests : EtlTestBase
    {
        [Fact]
        public async Task Raises_alert_if_script_has_invalid_name()
        {
            using (var store = GetDocumentStore())
            {
                var database = await GetDatabase(store.Database);

                var notifications = new AsyncQueue<Notification>();
                using (database.NotificationCenter.TrackActions(notifications, null))
                {
                    SetupEtl(store, new EtlDestinationsConfiguration
                    {
                        RavenDestinations =
                        {
                            new EtlConfiguration<RavenDestination>()
                            {
                                Destination =
                                    new RavenDestination
                                    {
                                        Url = "http://127.0.0.1:8080",
                                        Database = "Northwind",
                                    },
                                Transforms =
                                {
                                    new Transformation()
                                    {
                                        Collections = {"Users"}
                                    }
                                }
                            }
                        }
                    });

                    var alert = await notifications.TryDequeueOfTypeAsync<AlertRaised>(TimeSpan.FromSeconds(30));

                    Assert.True(alert.Item1);

                    Assert.Equal("Invalid ETL configuration for destination: Northwind@http://127.0.0.1:8080. Reason: Script name cannot be empty.", alert.Item2.Message);
                }
            }
        }

        [Fact]
        public async Task Raises_alert_if_scipts_have_non_unique_names()
        {
            using (var store = GetDocumentStore())
            {
                var database = await GetDatabase(store.Database);

                var notifications = new AsyncQueue<Notification>();
                using (database.NotificationCenter.TrackActions(notifications, null))
                {
                    SetupEtl(store, new EtlDestinationsConfiguration
                    {
                        RavenDestinations =
                            {
                                new EtlConfiguration<RavenDestination>()
                                {
                                    Destination = new RavenDestination()
                                    {
                                        Url = "http://127.0.0.1:8080",
                                        Database = "Northwind",
                                    },
                                    Transforms =
                                    {
                                        new Transformation()
                                        {
                                            Name = "MyEtl",
                                            Collections = { "Users"}
                                        },
                                        new Transformation()
                                        {
                                            Name = "MyEtl",
                                            Collections = {"People"}
                                        }
                                    }
                                }
                            }
                    });

                    var alert = await notifications.TryDequeueOfTypeAsync<AlertRaised>(TimeSpan.FromSeconds(30));

                    Assert.True(alert.Item1);

                    Assert.Equal("Invalid ETL configuration for destination: Northwind@http://127.0.0.1:8080. Reason: Script name 'MyEtl' name is already defined. The script names need to be unique.", alert.Item2.Message);
                }
            }
        }

        [Fact]
        public async Task Raises_alert_if_ETLs_are_defined_for_the_same_destination()
        {
            using (var store = GetDocumentStore())
            {
                var database = await GetDatabase(store.Database);

                var notifications = new AsyncQueue<Notification>();
                using (database.NotificationCenter.TrackActions(notifications, null))
                {
                    SetupEtl(store, new EtlDestinationsConfiguration
                    {
                        RavenDestinations =
                        {
                            new EtlConfiguration<RavenDestination>()
                            {
                                Destination = new RavenDestination()
                                {
                                    Url = "http://127.0.0.1:8080",
                                    Database = "Northwind",
                                },
                                Transforms =
                                {
                                    new Transformation()
                                    {
                                        Name = "TransformUsers",
                                        Collections = { "Users"}
                                    }
                                }
                            },
                            new EtlConfiguration<RavenDestination>()
                            {
                                Destination = new RavenDestination()
                                {
                                    Url = "http://127.0.0.1:8080",
                                    Database = "Northwind",
                                },
                                Transforms =
                                {
                                    new Transformation()
                                    {
                                        Name = "TransformOrders",
                                        Collections = { "Orders" }
                                    }
                                }
                            }
                        }
                    });

                    var alert = await notifications.TryDequeueOfTypeAsync<AlertRaised>(TimeSpan.FromSeconds(30));

                    Assert.True(alert.Item1);

                    Assert.Equal("Invalid ETL configuration for destination: Northwind@http://127.0.0.1:8080. Reason: ETL to this destination is already defined. Please just combine transformation scripts for the same destination.", alert.Item2.Message);
                }
            }
        }
    }
}