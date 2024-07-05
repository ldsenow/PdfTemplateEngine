namespace PdfTemplateEngine.Generators.Playwright;

public class PlaywrightPdfGenerator(IPlaywrightInstanceManager playwrightInstanceManager, IPdfRenderer pdfRenderer) : IPdfGenerator
{
    private readonly IPlaywrightInstanceManager playwrightInstanceManager = playwrightInstanceManager;
    private readonly IPdfRenderer pdfRenderer = pdfRenderer;

    public async Task<byte[]> Generate<TTemplate, TModel>(TModel model)
        where TTemplate : IPdfTemplate<TModel>
        where TModel : class
    {
        var html = await pdfRenderer.Render<TTemplate, TModel>(model);

        var browser = await playwrightInstanceManager.GetAvailableBrowserInstance();

        try
        {

            var context = await browser.NewContextAsync();

            var page = await context.NewPageAsync();

            await page.SetContentAsync(html);

            var pdfBytes = await page.PdfAsync(new()
            {
                Format = "A4",
                Margin = new() { Top = "0cm", Right = "0cm", Bottom = "0cm", Left = "0cm" },
                PrintBackground = true,
            });

            return pdfBytes;
        }
        finally
        {
            await playwrightInstanceManager.ReleaseBrowserInstance(browser);
        }
    }
}