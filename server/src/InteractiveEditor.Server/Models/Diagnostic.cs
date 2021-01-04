namespace InteractiveEditor.Server.Models
{
  public class Diagnostic
  {
    public int StartLineNumber { get; set; }

    public int StartColumn { get; set; }

    public int EndLineNumber { get; set; }

    public int EndColumn { get; set; }

    public string Message { get; set; }

    public DiagnosticSeverity Severity { get; set; }
  }

  public enum DiagnosticSeverity
  {
    Hint = 1,
    Info = 2,
    Warning = 4,
    Error = 8
  }
}
