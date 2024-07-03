namespace PdfTemplateEngine;

public interface IPdfTemplate<TModel> where TModel : class
{
    TModel Model { get; set; }
}
