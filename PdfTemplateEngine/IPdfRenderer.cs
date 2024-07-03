namespace PdfTemplateEngine;

public interface IPdfRenderer
{
    Task<string> Render<TTemplate, TModel>(TModel model)
        where TTemplate : IPdfTemplate<TModel>
        where TModel : class;
}