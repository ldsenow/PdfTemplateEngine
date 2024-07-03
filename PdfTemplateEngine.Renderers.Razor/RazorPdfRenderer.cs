using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System.Reflection;

namespace PdfTemplateEngine.Renderers.Razor;

public class RazorPdfRenderer(HtmlRenderer htmlRenderer) : IPdfRenderer
{
    private readonly HtmlRenderer htmlRenderer = htmlRenderer;

    public async Task<string> Render<TTemplate, TModel>(TModel model)
        where TTemplate : IPdfTemplate<TModel>
        where TModel : class
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!typeof(TTemplate).IsSubclassOf(typeof(PdfTemplateBase<TModel>)))
            throw new InvalidOperationException($"The template must inherit from {nameof(PdfTemplateBase<TModel>)}");

        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { nameof(PdfTemplateBase<TModel>.Model), model }
        });

        var html = await RenderComponent<TTemplate, TModel>(parameters);

        return html;
    }

    private Task<string> RenderComponent<TComponent, TModel>(ParameterView parameters)
    {
        return htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await htmlRenderer.RenderComponentAsync(typeof(LayoutView<TComponent, TModel>), parameters);
            return output.ToHtmlString();
        });
    }

    private class LayoutView<TComponent, TModel> : IComponent
    {
        private RenderHandle _renderHandle;

        [Parameter]
        public TModel? Model { get; set; }

        public void Attach(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        public Task SetParametersAsync(ParameterView parameters)
        {
            Model = parameters.GetValueOrDefault<TModel>(nameof(Model));
            _renderHandle.Render(BuildRenderTree);
            return Task.CompletedTask;
        }

        private void BuildRenderTree(RenderTreeBuilder builder)
        {
            var componentType = typeof(TComponent);
            var layoutAttribute = componentType.GetCustomAttribute<LayoutAttribute>();

            if (layoutAttribute != null)
            {
                var layoutType = layoutAttribute.LayoutType;
                builder.OpenComponent(0, layoutType);
                builder.AddAttribute(1, "Body", (RenderFragment)(bodyBuilder =>
                {
                    bodyBuilder.OpenComponent(0, componentType);
                    bodyBuilder.AddAttribute(1, nameof(Model), Model);
                    bodyBuilder.CloseComponent();
                }));
                builder.CloseComponent();
            }
            else
            {
                builder.OpenComponent(0, componentType);
                builder.AddAttribute(1, nameof(Model), Model);
                builder.CloseComponent();
            }
        }
    }
}
