using Microsoft.AspNetCore.Components.Web;
using PdfTemplateEngine;
using PdfTemplateEngine.Examples.Api;
using PdfTemplateEngine.Generators.Playwright;
using PdfTemplateEngine.Renderers.Razor;
using PdfTemplateEngine.Templates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<HtmlRenderer>();
builder.Services.AddScoped<IPdfGenerator, PlaywrightPdfGenerator>();
builder.Services.AddScoped<IPdfRenderer, RazorPdfRenderer>();
builder.Services.AddSingleton<IPlaywrightInstanceManager>(sp =>
    new PlaywrightInstanceManager(sp.GetRequiredService<TimeProvider>()));

builder.Services.AddHostedService<PlaywrightBrowserInstanceHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/render-pdf", async (IPdfGenerator pdfGenerator) =>
{
    var pdfBytes = await pdfGenerator.Generate<SampleComponent, SampleComponentModel>(new SampleComponentModel
    {
        Text = "Hello, world!"
    });

    return Results.File(
        fileDownloadName: $"output_{DateTime.Now.Ticks}.pdf",
        fileContents: pdfBytes,
        contentType: "application/pdf");
})
.WithOpenApi();

app.MapGet("/render-html", async (IPdfRenderer renderer) =>
{
    var html = await renderer.Render<SampleComponent, SampleComponentModel>(new SampleComponentModel
    {
        Text = "Hello, world!"
    });

    return Results.Content(html, contentType: "text/html");
})
.WithOpenApi();

app.Run();