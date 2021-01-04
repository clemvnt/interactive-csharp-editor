using System;
using System.Linq;
using System.Threading.Tasks;
using InteractiveEditor.Server.Models;
using InteractiveEditor.Server.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace InteractiveEditor.Server.Controllers
{
  [ApiController]
  [Route("editor")]
  public class EditorController : ControllerBase
  {
    private readonly ILogger<EditorController> _logger;

    public EditorController(ILogger<EditorController> logger)
    {
      _logger = logger;
    }

    [HttpPost("completions")]
    public async Task<IActionResult> GetCompletions(GetCompletionsRequest request)
    {
      var document = CodeAnalysisUtilities.CreateDocumentFromCode(request.Code);
      var completionService = CompletionService.GetService(document);
      var caretPosition = CodeAnalysisUtilities.GetCaretPositionFromLinePosition(request.Code, new LinePosition(request.LineNumber - 1, request.Column - 1));
      var completions = await completionService.GetCompletionsAsync(document, caretPosition);

      return Ok(new
      {
        Completions = completions.Items.Select(c => new Completion()
        {
          Label = c.DisplayText,
          InsertText = c.FilterText,
          Kind = Enum.TryParse<CompletionItemKind>(CodeAnalysisUtilities.GetKind(c), out var parsedKind) ? parsedKind : CompletionItemKind.Property,
          Documentation = c.InlineDescription,
        })
      });
    }

    [HttpPost("hover")]
    public async Task<IActionResult> GetHover(GetHoverRequest request)
    {
      var document = CodeAnalysisUtilities.CreateDocumentFromCode(request.Code);
      var quickInfoService = QuickInfoService.GetService(document);
      var caretPosition = CodeAnalysisUtilities.GetCaretPositionFromLinePosition(request.Code, new LinePosition(request.LineNumber - 1, request.Column - 1));
      var info = await quickInfoService.GetQuickInfoAsync(document, caretPosition);

      return Ok(new Hover()
      {
        Text = CodeAnalysisUtilities.GetMarkdownStringFromQuickInfo(info)
      });
    }

    [HttpPost("diagnostics")]
    public async Task<IActionResult> GetDiagnostics(GetDiagnosticsRequest request)
    {
      var document = CodeAnalysisUtilities.CreateDocumentFromCode(request.Code);
      var semanticModel = await document.GetSemanticModelAsync();
      var diagnostics = semanticModel.GetDiagnostics();

      return Ok(diagnostics.Select(d => new Models.Diagnostic()
      {
        StartLineNumber = d.Location.GetLineSpan().StartLinePosition.Line + 1,
        StartColumn = d.Location.GetLineSpan().StartLinePosition.Character + 1,
        EndLineNumber = d.Location.GetLineSpan().EndLinePosition.Line + 1,
        EndColumn = d.Location.GetLineSpan().EndLinePosition.Character + 1,
        Message = d.GetMessage(),
        Severity = Enum.TryParse<Models.DiagnosticSeverity>(d.Severity.ToString(), out var parsedSeverity) ? parsedSeverity : Models.DiagnosticSeverity.Info,
      }));
    }
  }
}
