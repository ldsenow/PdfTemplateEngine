
using PdfTemplateEngine.Generators.Playwright;

namespace PdfTemplateEngine.Examples.Api;

public class PlaywrightBrowserInstanceHostedService(IPlaywrightInstanceManager playwrightInstanceManager) : IHostedService
{
    private readonly IPlaywrightInstanceManager playwrightInstanceManager = playwrightInstanceManager;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await playwrightInstanceManager.Initialize();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await playwrightInstanceManager.DisposeAsync();
    }
}
