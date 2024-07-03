namespace PdfTemplateEngine.Generators.Playwright;

public class PlaywrightInstanceManagerOptions
{
    /// <summary>
    /// Default = 1
    /// </summary>
    public int MinInstances { get; set; } = 1;

    /// <summary>
    /// Default = 5
    /// </summary>
    public int MaxInstances { get; set; } = 5;

    /// <summary>
    /// Default = 30
    /// </summary>
    public int IdleTimeoutMinutes { get; set; } = 30;
}