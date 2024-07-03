using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace PdfTemplateEngine.Generators.Playwright;

public interface IPlaywrightInstanceManager : IAsyncDisposable
{
    Task InitializeAsync();
    Task<IBrowser> GetAvailableBrowserInstance();
    Task ReleaseBrowserInstance(IBrowser browser);
}

public class PlaywrightInstanceManager(int minInstances = 1, int maxInstances = 5) : IPlaywrightInstanceManager
{
    private IPlaywright? _playwright;
    private readonly ConcurrentBag<IBrowser> _browserPool = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly int _minInstances = minInstances;
    private readonly int _maxInstances = maxInstances;
    private int _currentInstances = 0;
    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_isInitialized) return;

            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            for (int i = 0; i < _minInstances; i++)
            {
                IBrowser browser = await CreateBrowserInstance();                

                _browserPool.Add(browser);

                _currentInstances++;
            }

            _isInitialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<IBrowser> CreateBrowserInstance() => await _playwright!.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = true,
    });

    public async Task<IBrowser> GetAvailableBrowserInstance()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
        }

        if (_browserPool.TryTake(out var browser))
        {
            return browser;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_currentInstances < _maxInstances)
            {
                browser = await CreateBrowserInstance();
                _currentInstances++;
                return browser;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        // If we reach here, we need to wait for a browser to become available
        while (!_browserPool.TryTake(out browser))
        {
            await Task.Delay(100);
        }

        return browser;
    }

    public async Task ReleaseBrowserInstance(IBrowser browser)
    {
        await _semaphore.WaitAsync();

        try
        {
            var shouldDispose = _currentInstances > _minInstances;
            _browserPool.Add(browser);

        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            while (_browserPool.TryTake(out var browser))
            {
                await browser.DisposeAsync();
                _currentInstances--;
            }

            _playwright?.Dispose();
        }
        finally
        {
            _semaphore.Release();
        }

        GC.SuppressFinalize(this);
    }
}