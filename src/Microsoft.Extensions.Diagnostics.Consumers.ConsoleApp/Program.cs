﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthReporters;
using Microsoft.Extensions.Diagnostics.Inputs;
using Microsoft.Extensions.Diagnostics.Outputs;

namespace Microsoft.Extensions.Diagnostics.Consumers.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // HealthReporter
            using (TemporaryFile configFile = CreateConfigFile())
            using (IHealthReporter reporter = new CsvHealthReporter(configFile.FilePath))
            {
                ConfigurationBuilder builder = new ConfigurationBuilder();
                builder.AddJsonFile(configFile.FilePath);
                var configuration = builder.Build();

                var pipeline = DiagnosticsPipelineFactory.CreatePipeline(configuration, reporter) as DiagnosticsPipeline;

                // Build up the pipeline
                Console.WriteLine("Pipeline is created.");

                // Send a trace to the pipeline
                Trace.TraceInformation("This is a message from trace . . .");
                MyEventSource.Log.Message("This is a message from EventSource ...");

                // Check the result
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey(true);
            }
        }

        private static TemporaryFile CreateConfigFile()
        {
            TemporaryFile configFile = new TemporaryFile();

            try
            {
                string pipelineConfiguration = @"
                    {
                        ""inputs"": [
                            {
                                ""type"": ""EventSource"",
                                ""sources"": [
                                    { ""providerName"": ""MyEventSource"" }
                                ]
                            },
                            {
                                ""type"": ""Trace""
                            }
                        ],

                        ""filters"": [
                        ],

                        ""outputs"": [
                            {
                                ""type"": ""StdOutput"",

                                ""filters"": [
                                ]
                            }
                        ],

                        ""schema-version"": ""2016-08-11"",
                    }";

                configFile.Write(pipelineConfiguration);
                return configFile;
            }
            catch (Exception)
            {
                configFile.Dispose();
                throw;
            }
        }
    }
}