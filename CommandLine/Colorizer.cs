using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Butjok.CommandLine.Parsers;
using UnityEngine;

namespace Butjok.CommandLine
{
    public static class Colorizer
    {
        private static readonly CommandLineLexer lexer = new CommandLineLexer(null);
        private static readonly List<IToken> tokens = new List<IToken>();
        private static readonly List<IToken> tokenAt = new List<IToken>();
        private static readonly StringBuilder sb = new StringBuilder();

        public static string Colorize(string text, Theme theme = null) {
            if (!theme)
                return text;

            lexer.SetInputStream(new AntlrInputStream(text));
            tokens.Clear();
            for (var token = lexer.NextToken(); token.Type != TokenConstants.EOF; token = lexer.NextToken())
                tokens.Add(token);

            tokenAt.Clear();
            for (var i = 0; i < text.Length; i++)
                tokenAt.Add(null);
            foreach (var token in tokens)
                for (var i = token.StartIndex; i <= token.StopIndex; i++)
                    tokenAt[i] = token;

            sb.Clear();
            for (var i = 0; i < text.Length; i++) {
                var token = tokenAt[i];
                var style = token == null ? theme.error : theme.TryGetStyle((Token)token.Type, out var s) ? s : theme.normal;
                if (style.bold)
                    sb.Append("<b>");
                if (style.italic)
                    sb.Append("<i>");
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(style.color)}>{text[i]}</color>");
                if (style.italic)
                    sb.Append("</i>");
                if (style.bold)
                    sb.Append("</b>");
            }
            return sb.ToString();
        }
    }
}