using AvaloniaEdit.Snippets;

namespace SkEditor.Utilities.Extensions;

public static class SnippetBuilderExtensions
{
    extension(Snippet snippet)
    {
        public Snippet Text(string text)
        {
            snippet.Elements.Add(new SnippetTextElement { Text = text });
            return snippet;
        }

        public Snippet Placeholder(string defaultText, out SnippetReplaceableTextElement element)
        {
            element = new SnippetReplaceableTextElement { Text = defaultText };
            snippet.Elements.Add(element);
            return snippet;
        }

        public Snippet Placeholder(string defaultText)
        {
            snippet.Elements.Add(new SnippetReplaceableTextElement { Text = defaultText });
            return snippet;
        }

        public Snippet Caret()
        {
            snippet.Elements.Add(new SnippetCaretElement());
            return snippet;
        }
    }
}