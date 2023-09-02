using Microsoft.CodeAnalysis;
using System;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers.Core;

static class AccessibilityHelper
{
    public static string ToCSharpCode(this Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => string.Empty
        };
}