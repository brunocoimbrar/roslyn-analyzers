﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslyn.Diagnostics.Analyzers;

namespace Roslyn.Diagnostics.CSharp.Analyzers
{
    using static RoslynDiagnosticsAnalyzersResources;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PreferNullLiteral : DiagnosticAnalyzer
    {
        internal static DiagnosticDescriptor Rule = new(
            RoslynDiagnosticIds.PreferNullLiteralRuleId,
            CreateLocalizableResourceString(nameof(PreferNullLiteralTitle)),
            CreateLocalizableResourceString(nameof(PreferNullLiteralMessage)),
            DiagnosticCategory.RoslynDiagnosticsMaintainability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: CreateLocalizableResourceString(nameof(PreferNullLiteralDescription)),
            helpLinkUri: null,
            customTags: WellKnownDiagnosticTags.Telemetry);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(HandleDefaultValueOperation, OperationKind.DefaultValue);
        }

        private void HandleDefaultValueOperation(OperationAnalysisContext context)
        {
            if (context.Operation.IsImplicit)
            {
                // Ignore implicit operations since they don't appear in source code.
                return;
            }

            var type = context.Operation.Type;
            if (type is null)
            {
                return;
            }

            if (type.TypeKind == TypeKind.Pointer)
            {
                // Pointers can use 'null'
            }
            else if (type.TypeKind == TypeKind.Error)
            {
                return;
            }
            else if (type.IsValueType)
            {
                if (type is not INamedTypeSymbol namedType
                    || namedType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                {
                    return;
                }
            }
            else if (!type.IsReferenceType)
            {
                return;
            }

            context.ReportDiagnostic(context.Operation.CreateDiagnostic(Rule));
        }
    }
}
