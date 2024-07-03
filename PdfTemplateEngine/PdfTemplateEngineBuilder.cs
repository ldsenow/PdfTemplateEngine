using Microsoft.Extensions.DependencyInjection;
namespace PdfTemplateEngine;

public record PdfTemplateEngineBuilder
{
    public required IServiceCollection Services { get; init; }
}
