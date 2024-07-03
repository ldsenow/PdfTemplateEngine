using Microsoft.AspNetCore.Components;

namespace PdfTemplateEngine.Renderers.Razor;

public class PdfTemplateBase<TModel> : ComponentBase, IPdfTemplate<TModel>
    where TModel : class
{
    [Parameter]
    public required TModel Model { get; set; }
}
