using PdfTemplateEngine;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsToServiceCollection
{
    public static IServiceCollection AddPdfTemplateEngine(this IServiceCollection services, Action<PdfTemplateEngineBuilder> config)
    {
        var builder = new PdfTemplateEngineBuilder
        {
            Services = services,
        };

        config(builder);

        return services;
    }
}
