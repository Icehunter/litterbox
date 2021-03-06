﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="SyndicatedLife">
//   Copyright(c) 2017 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   Program.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bootstrapper {
    using System;

    using LitterBox;
    using LitterBox.DocumentDB;
    using LitterBox.Interfaces;
    using LitterBox.Memory;
    using LitterBox.Models;
    using LitterBox.Redis;

    using Microsoft.Azure.Documents.Client;

    internal class Program {
        private static void Main(string[] args) {
            // unless otherwise stated, all values presented are the default if not specified
            var memory = MemoryBox.GetInstance(
                new MemoryConfiguration {
                    DefaultTimeToLive = new TimeSpan(1, 0, 0, 0),
                    DefaultTimeToRefresh = new TimeSpan(0, 0, 5, 0),
                    ExpirationScanFrequency = new TimeSpan(0, 1, 0, 0),
                    PoolSize = 1 // hardcoded to 1 on creation of connectionpool
                });

            // redis connection assumes SSL is enabled; if not you must adjust port and UseSSL
            var redis = RedisBox.GetInstance(
                new RedisConfiguration {
                    DatabaseID = 0,
                    DefaultTimeToLive = new TimeSpan(1, 0, 0, 0),
                    DefaultTimeToRefresh = new TimeSpan(0, 0, 5, 0),
                    Host = "<host>", // defaults to 127.0.0.1
                    Password = "<password>", // only required if using ssl, defaults to ""
                    PoolSize = 5,
                    Port = 6379, // defaults to 6380
                    UseSSL = false // defaults to true
                });

            // for documentDB you must replace the string values with whatever you have setup
            // the backing DB of your cosmosDB MUST be SQL and partitioned/sharded based on /_partitionKey
            var documentDB = DocumentDBBox.GetInstance(
                new DocumentDBConfiguration {
                    Collection = "<collection>",
                    ConnectionPolicy = new ConnectionPolicy(), // defaults to nothing configured
                    Database = "<database>",
                    DefaultTimeToLive = new TimeSpan(1, 0, 0, 0),
                    DefaultTimeToRefresh = new TimeSpan(0, 0, 5, 0),
                    EndpointURI = new Uri("https://<account>.documents.azure.com:443"),
                    PartitionKey = "cache", // defaults to cache, but you can specify what you want
                    PoolSize = 5,
                    PrimaryKey = "<auth_key>"
                });

            // create a new tenancy object`
            var tenancy = new Tenancy(
                new ILitterBox[] {
                    memory,
                    redis,
                    documentDB
                });

            // subscribe to internal errors
            tenancy.ExceptionEvent += TenancyOnExceptionEvent;

            // set an item (will backfill all three caches)
            var x = tenancy.SetItem(
                "foo",
                new LitterBoxItem<dynamic> {
                    Value = new {
                        Args = "boo"
                    }
                }).GetAwaiter().GetResult();

            // get item from cache, checks in order of array initialization. in this case memory -> redis -> documentDB
            var y = tenancy.GetItem<dynamic>("foo").GetAwaiter().GetResult();

            Console.ReadKey();
        }

        private static void TenancyOnExceptionEvent(object sender, ExceptionEvent exceptionEvent) {
            Console.WriteLine(exceptionEvent.Exception.Message);
        }
    }
}