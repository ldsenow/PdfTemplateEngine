using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace PdfTemplateEngine.Generators.Playwright;

public interface IPlaywrightInstanceManager : IAsyncDisposable
{
    Task Initialize();
    /// <summary>
    /// Default wait timeout is 30 seconds
    /// </summary>
    Task<IBrowser> GetAvailableBrowserInstance(TimeSpan? waitTimeout = default);
    Task ReleaseBrowserInstance(IBrowser browser);
}

public class PlaywrightInstanceManager : IPlaywrightInstanceManager
{
    private IPlaywright? _playwright;
    private readonly ConcurrentDictionary<IBrowser, DateTime> _browserPool = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly PlaywrightInstanceManagerOptions _options = new();
    private readonly TimeProvider _timeProvider;
    private readonly ITimer _idleCheckTimer;
    private int _currentInstances = 0;
    private bool _isInitialized = false;

    public PlaywrightInstanceManager(TimeProvider timeProvider, Action<PlaywrightInstanceManagerOptions>? config = null)
    {
        config?.Invoke(_options);

        _timeProvider = timeProvider;
        _idleCheckTimer = _timeProvider.CreateTimer(
            callback: CheckIdleInstances,
            state: this,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromMinutes(_options.IdleTimeoutMinutes));
    }

    public async Task Initialize()
    {
        if (_isInitialized) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_isInitialized) return;

            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            for (int i = 0; i < _options.MinInstances; i++)
            {
                var browser = await CreateBrowserInstance();
                _browserPool.TryAdd(browser, GetUtcNow());
            }

            _isInitialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IBrowser> GetAvailableBrowserInstance(TimeSpan? waitTimeout = default)
    {
        if (!_isInitialized)
        {
            await Initialize();
        }

        waitTimeout ??= TimeSpan.FromSeconds(30);

        var startTime = GetUtcNow();

        while (GetUtcNow() - startTime < waitTimeout)
        {
            foreach (var kvp in _browserPool)
            {
                if (_browserPool.TryRemove(kvp.Key, out _))
                {
                    _browserPool[kvp.Key] = GetUtcNow();
                    return kvp.Key;
                }
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_currentInstances < _options.MaxInstances)
                {
                    var browser = await CreateBrowserInstance();

                    _browserPool[browser] = GetUtcNow();
                    _currentInstances++;

                    return browser;
                }
            }
            finally
            {
                _semaphore.Release();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100), _timeProvider);
        }

        throw new TimeoutException("Timeout waiting for available browser instance.");
    }

    public async Task ReleaseBrowserInstance(IBrowser browser)
    {
        ArgumentNullException.ThrowIfNull(browser);

        await _semaphore.WaitAsync();

        try
        {
            _browserPool[browser] = GetUtcNow();
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
            _idleCheckTimer.Dispose();

            foreach (var browser in _browserPool.Keys)
            {
                await browser.DisposeAsync();
                _currentInstances--;
            }

            _browserPool.Clear();

            _playwright?.Dispose();
        }
        finally
        {
            _semaphore.Release();
        }

        GC.SuppressFinalize(this);
    }

    private async void CheckIdleInstances(object? state)
    {
        await _semaphore.WaitAsync();
        try
        {
            var idleThreshold = GetUtcNow().AddMinutes(-_options.IdleTimeoutMinutes);
            var idleBrowsers = _browserPool.Where(x => x.Value <= idleThreshold).ToList();

            foreach (var kvp in idleBrowsers)
            {
                if (_currentInstances > _options.MinInstances && _browserPool.TryRemove(kvp.Key, out _))
                {
                    await kvp.Key.DisposeAsync();
                    _currentInstances--;
                    break; // Remove only one instance at a time
                }
            }
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

    private DateTime GetUtcNow() => _timeProvider.GetUtcNow().DateTime;
}
