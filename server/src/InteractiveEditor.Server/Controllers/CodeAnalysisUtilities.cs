using System.Collections.Immutable;
using System.Linq;
using System.Text;
using InteractiveEditor.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.CodeAnalysis.Tags;
using Microsoft.CodeAnalysis.Text;

namespace InteractiveEditor.Server.Controllers
{
  [ApiController]
  [Route("editor")]
  public static class CodeAnalysisUtilities
  {
    private static readonly ImmutableArray<string> KindTags = ImmutableArray.Create(
      WellKnownTags.Class,
      WellKnownTags.Constant,
      WellKnownTags.Delegate,
      WellKnownTags.Enum,
      WellKnownTags.EnumMember,
      WellKnownTags.Event,
      WellKnownTags.ExtensionMethod,
      WellKnownTags.Field,
      WellKnownTags.Interface,
      WellKnownTags.Intrinsic,
      WellKnownTags.Keyword,
      WellKnownTags.Label,
      WellKnownTags.Local,
      WellKnownTags.Method,
      WellKnownTags.Module,
      WellKnownTags.Namespace,
      WellKnownTags.Operator,
      WellKnownTags.Parameter,
      WellKnownTags.Property,
      WellKnownTags.RangeVariable,
      WellKnownTags.Reference,
      WellKnownTags.Structure,
      WellKnownTags.TypeParameter);

    public static Document CreateDocumentFromCode(string code)
    {
      var parseOptions = new CSharpParseOptions()
      .WithDocumentationMode(DocumentationMode.Diagnose);

      var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        .WithUsings(new[] { "System", "InteractiveEditor.Abstractions" });

      var workspace = new AdhocWorkspace();
      var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
        .WithMetadataReferences(new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ViewModel).Assembly.Location)
        })
        .WithParseOptions(parseOptions)
        .WithCompilationOptions(compilationOptions);

      var document = project.AddDocument("TestDocument", SourceText.From(code));

      return document;
    }

    public static string GetKind(CompletionItem completionItem)
    {
      foreach (var tag in KindTags)
      {
        if (completionItem.Tags.Contains(tag))
        {
          return tag;
        }
      }

      return null;
    }

    public static int GetCaretPositionFromLinePosition(string code, LinePosition position)
    {
      int line = 0;
      int character = 0;
      int offset = 0;
      for (; offset < code.Length; offset++)
      {
        if (line >= position.Line && character >= position.Character)
        {
          break;
        }

        switch (code[offset])
        {
          case '\n':
            line++;
            character = 0;
            break;
          default:
            character++;
            break;
        }
      }

      return offset;
    }

    public static string GetMarkdownStringFromQuickInfo(QuickInfoItem info)
    {
      var stringBuilder = new StringBuilder();
      var description = info.Sections.FirstOrDefault(s => QuickInfoSectionKinds.Description.Equals(s.Kind))?.Text ?? string.Empty;
      var documentation = info.Sections.FirstOrDefault(s => QuickInfoSectionKinds.DocumentationComments.Equals(s.Kind))?.Text ?? string.Empty;

      if (!string.IsNullOrEmpty(description))
      {
        stringBuilder.Append(description);
        if (!string.IsNullOrEmpty(documentation))
        {
          stringBuilder.Append("\r\n> ").Append(documentation);
        }
      }

      return stringBuilder.ToString();
    }
  }
}
