using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Butjok.Parsers;
using UnityEngine;

namespace Butjok
{
    public static class SyntaxHighlighting
    {
        public class Style
        {
            public Color color = Color.white;
            public bool bold, italic;
        }
        public static Style defaultStyle;
        public static Style errorStyle;
        public static Dictionary<int, Style> styles;

        public static void Initialize() {
            defaultStyle = new Style { };
            errorStyle = defaultStyle;
            styles = new Dictionary<int, Style>();
            var bold = new Style { bold = true };
            styles[ConsoleLexer.String] = bold;
            styles[ConsoleLexer.True] = bold;
            styles[ConsoleLexer.False] = bold;
            styles[ConsoleLexer.Int2] = bold;
            styles[ConsoleLexer.Rgb] = bold;
            styles[ConsoleLexer.Interpolation] = bold;
        }

        public static string Colorize(string text) {
            var tokens = new ConsoleLexer(new AntlrInputStream(text)).GetAllTokens();
            var tokenAt = new Dictionary<int, IToken>();
            foreach (var token in tokens)
                for (var i = token.StartIndex; i <= token.StopIndex; i++)
                    tokenAt[i] = token;
            var result = new StringBuilder();
            for (var i = 0; i < text.Length; i++) {
                var token = tokenAt.TryGetValue(i, out var tok) ? tok : null;
                var style = token == null ? errorStyle : styles.TryGetValue(token.Type, out var s) ? s : defaultStyle;
                if (style.bold)
                    result.Append("<b>");
                if (style.italic)
                    result.Append("<i>");
                result.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(style.color)}>{text[i]}</color>");
                if (style.italic)
                    result.Append("</i>");
                if (style.bold)
                    result.Append("</b>");
            }
            return result.ToString();
        }
    }
}