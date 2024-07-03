# PdfTemplateEngine

A C# PDF generator using Razor and Playwright. The idea is to use the compiled html from Razor Component and send it to Playwright for PDF generation.

> [!WARNING]
> Nuget packages aren't available at this stage. It is in early stage of development. However, welcome to raise issues or suggestions.

#### Configuration

```csharp
builder.Services.AddPdfTemplateEngine(config =>
{
    config.UseRazorRenderer();
    config.UsePlaywrightGenerator(options =>
    {
        options.MinInstances = 1;
        options.MaxInstances = 5;
        options.IdleTimeoutMinutes = 30;
    });
});
```

#### Usage (ASP.NET Core Minimal API)

###### Generate PDF
```csharp
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
```

###### Generate HTML
```csharp
app.MapGet("/render-html", async (
    [AsParameters] SampleComponentModel model, 
    [FromServices] IPdfRenderer renderer) =>
{
    var html = await renderer.Render<SampleComponent, SampleComponentModel>(model);

    return Results.Content(html, contentType: "text/html");
})
.WithOpenApi();
```

> [!NOTE]  
> SampleComponent is a Razor component and SampleComponentModel is a POCO class as the model

> [!TIP]
> The model can be passed an API endpoint parameter and validated etc..

> [!IMPORTANT]
> Razor Component's features won't be fully supported. However, using as a static html templating engine, it is good enough.