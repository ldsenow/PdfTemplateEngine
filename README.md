# PdfTemplateEngine

A C# PDF generator using Razor and Playwright. The idea is to use the compiled html from Razor Component and send it to Playwright for PDF generation.

> [!WARNING]
> Nuget packages aren't available at this stage. It is in very stage of development. However, welcome to raise issues or suggestions.

#### Usage (ASP.NET Core Minimal API)

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

> [!NOTE]  
> SampleComponent is a Razor component and SampleComponentModel is a POCO class as the model

> [!TIP]
> The model can be passed an API endpoint parameter and validated etc..

> [!IMPORTANT]
> Razor Component's features won't be fully supported. However, using as a static html templating engine, it is good enough.