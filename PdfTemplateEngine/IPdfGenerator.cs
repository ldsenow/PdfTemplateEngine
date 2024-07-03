namespace PdfTemplateEngine;

public interface IPdfGenerator
{
    Task<byte[]> Generate<TTemplate, TModel>(TModel model)
        where TTemplate : IPdfTemplate<TModel>
        where TModel : class;
}
