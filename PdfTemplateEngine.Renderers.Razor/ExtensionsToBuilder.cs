using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PdfTemplateEngine.Renderers.Razor;

namespace PdfTemplateEngine;

public static class ExtensionsToBuilder
{
    public static PdfTemplateEngineBuilder UseRazorRenderer(this PdfTemplateEngineBuilder builder)
    {
        builder.Services.TryAddScoped<HtmlRenderer>();
        builder.Services.AddScoped<IPdfRenderer, RazorPdfRenderer>();

        return builder;
    }
}
