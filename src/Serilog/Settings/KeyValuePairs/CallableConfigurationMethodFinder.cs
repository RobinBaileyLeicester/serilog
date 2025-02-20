﻿// Copyright 2013-2015 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Serilog.Settings.KeyValuePairs;

static class CallableConfigurationMethodFinder
{
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Configuration methods are not trimming safe")]
#endif
    internal static IList<MethodInfo> FindConfigurationMethods(IEnumerable<Assembly> configurationAssemblies, Type configType)
    {
        var methods = configurationAssemblies
            .SelectMany(
#if NET5_0_OR_GREATER
                [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Configuration methods are not trimming safe")]
#endif
                (a) => a.ExportedTypes
                .Where(t => t.IsSealed && t.IsAbstract && !t.IsNested))
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .Where(m => m.IsDefined(typeof(ExtensionAttribute), false) &&
                        m.GetParameters()[0].ParameterType == configType)
            .ToList();

        // some configuration methods are not extension methods. They are added manually
        // so they can be discovered

        // WriteTo.Sink(params ILogEventSink[]) is not an extension method
        // and we want to expose WriteTo.Sink(ILogEventSink sink) to the config system
        if (configType == typeof(LoggerSinkConfiguration))
            methods.AddRange(SurrogateConfigurationMethods.WriteTo);

        // AuditTo.Sink(params ILogEventSink[]) is not an extension method
        // and we want to expose WriteTo.Sink(ILogEventSink sink) to the config system
        if (configType == typeof(LoggerAuditSinkConfiguration))
            methods.AddRange(SurrogateConfigurationMethods.AuditTo);

        // FromLogContext is an instance method rather than an extension.
        if (configType == typeof(LoggerEnrichmentConfiguration))
            methods.AddRange(SurrogateConfigurationMethods.Enrich);

        // Some of the useful Destructure configuration methods are defined as methods rather than extension methods
        if (configType == typeof(LoggerDestructuringConfiguration))
            methods.AddRange(SurrogateConfigurationMethods.Destructure);

        // Some of the useful Filter configuration methods are defined as methods rather than extension methods
        if (configType == typeof(LoggerFilterConfiguration))
            methods.AddRange(SurrogateConfigurationMethods.Filter);

        return methods;
    }
}
