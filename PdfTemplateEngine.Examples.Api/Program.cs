using Microsoft.AspNetCore.Mvc;
using PdfTemplateEngine;
using PdfTemplateEngine.Examples.Api;
using PdfTemplateEngine.Templates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<PlaywrightBrowserInstanceHostedService>();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/render-pdf", async ([FromQuery] string text, [FromServices] IPdfGenerator pdfGenerator) =>
{
    var model = GetModel(text);

    var pdfBytes = await pdfGenerator.Generate<SampleComponent, SampleComponentModel>(model);

    return Results.File(
        fileDownloadName: $"output_{DateTime.Now.Ticks}.pdf",
        fileContents: pdfBytes,
        contentType: "application/pdf");
})
.WithOpenApi();

app.MapGet("/render-html", async (
    [FromQuery] string text,
    [FromServices] IPdfRenderer renderer) =>
{
    var model = GetModel(text);

    var html = await renderer.Render<SampleComponent, SampleComponentModel>(model);

    return Results.Content(html, contentType: "text/html");
})
.WithOpenApi();

app.Run();

static SampleComponentModel GetModel(string text)
{
    (int Id, decimal OutstandingAmount)[] records = Enumerable.Range(1, 100)
        .Select(x => (70000 + x, Random.Shared.Next(10_000, 1_000_000) / 100m))
        .ToArray();

    return new()
    {
        Text = text,
        Rows = records.Select(x => new TableRow
        {
            Description = $"Principal for Order #{x.Id}",
            Date = DateTime.Now.AddDays(-Random.Shared.Next(1, 30)).ToString("dd MMM yyyy"),
            Amount = (x.OutstandingAmount + Random.Shared.Next(1, 10_000)).ToString("N2"),
            Outstanding = x.OutstandingAmount.ToString("N2")
        }).ToArray(),
        TotalOutstanding = records.Sum(x => x.OutstandingAmount).ToString("N2")
    };
}