using Microsoft.CodeAnalysis;

namespace dotnetCampus.Telescope.SourceGeneratorAnalyzers;

class ExportMethodReturnTypeCollectionDiagnostic : IExportMethodReturnTypeCollectionResult
{
    public ExportMethodReturnTypeCollectionDiagnostic(DiagnosticDescriptor diagnosticDescriptor,
        Location? location = null, params object[]? messageArgs)
    {
        DiagnosticDescriptor = diagnosticDescriptor;
        Location = location;
        MessageArgs = messageArgs;
    }

    public Location? Location { get; set; }
    public DiagnosticDescriptor DiagnosticDescriptor { get; }
    public object[]? MessageArgs { set; get; }

    public Diagnostic ToDiagnostic() => Diagnostic.Create(DiagnosticDescriptor, Location, MessageArgs);
}