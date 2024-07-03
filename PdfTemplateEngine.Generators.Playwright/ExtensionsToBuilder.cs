using Microsoft.Extensions.DependencyInjection;
using PdfTemplateEngine.Generators.Playwright;

namespace PdfTemplateEngine;

public static class ExtensionsToBuilder
{
    public static PdfTemplateEngineBuilder UsePlaywrightGenerator(this PdfTemplateEngineBuilder builder, Action<PlaywrightInstanceManagerOptions>? config = null)
    {
        builder.Services.AddScoped<IPdfGenerator, PlaywrightPdfGenerator>();
        builder.Services.AddSingleton<IPlaywrightInstanceManager>(sp =>
            new PlaywrightInstanceManager(sp.GetRequiredService<TimeProvider>(), config));

        return builder;
    }
}
