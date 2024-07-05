namespace PdfTemplateEngine.Templates;

public partial class SampleComponent
{
}

public class SampleComponentModel
{
    public required string Text { get; set; }
    public TableRow[] Rows { get; set; } = [];
    public required string TotalOutstanding { get; set; }
}

public class TableRow
{
    public required string Description { get; set; }
    public required string Date { get; set; }
    public required string Amount { get; set; }
    public required string Outstanding { get; set; }
}