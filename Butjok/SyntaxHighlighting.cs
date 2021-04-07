using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {

    [CLSCompliant(false)]
    public class SyntaxHighlighting {

        private readonly ColorTheme _colorTheme;
        private readonly Func<string, bool> _exists;
        private readonly Func<string, bool> _isVariable;

        private static readonly StringBuilder Sb = new StringBuilder();
        private static readonly StringBuilder Sb2 = new StringBuilder();

        public SyntaxHighlighting(ColorTheme colorTheme, Func<string, bool> exists, Func<string, bool> isVariable) {
            Assert.That(colorTheme.Tokens != null);

            _colorTheme = colorTheme;
            _exists = exists;
            _isVariable = isVariable;
        }

        public void Colorize(string text, IEnumerable<Token> tokens, out string richText, out string underscores) {
            Assert.That(text != null);
            Assert.That(tokens != null);

            Sb.Clear();
            Sb2.Clear();
            foreach (var token in tokens) {

                Assert.That(text != null);
                Assert.That(token.Start >= 0);
                Assert.That(token.Stop < text.Length);
                Assert.That(token.Start <= token.Stop);

                var tokenStyle = _colorTheme.Tokens.TryGetValue(token.Type, out var result)
                    ? result
                    : _colorTheme.Default;

                Color? underscoreColor = null;
                switch (token.Type) {

                    case CommandLineLexer.ShortHexRgbColor:
                    case CommandLineLexer.ShortHexRgbaColor:
                    case CommandLineLexer.LongHexRgbColor:
                    case CommandLineLexer.LongHexRgbaColor: {
                        underscoreColor = Parse.HexColor(text.Substring(token.Start, token.Stop - token.Start + 1));
                        break;
                    }

                    case CommandLineLexer.Identifier:
                        if (_exists == null || _isVariable == null)
                            break;
                        var name = text.Substring(token.Start, token.Stop - token.Start + 1);
                        tokenStyle = !_exists(name)
                            ? _colorTheme.UnknownCommand
                            : _isVariable == null
                                ? _colorTheme.Command
                                : _isVariable(name)
                                    ? _colorTheme.VariableCommand
                                    : _colorTheme.ProcedureCommand;
                        break;

                    case TokenConstants.InvalidType:
                        tokenStyle = _colorTheme.Error;
                        break;
                }

                var colorTag = $"<color=#{ColorUtility.ToHtmlStringRGBA(tokenStyle.Color)}>";
                for (var i = token.Start; i <= token.Stop; i++) {
                    Sb.Append(colorTag);
                    if (tokenStyle.Bold)
                        Sb.Append("<b>");
                    if (tokenStyle.Italic)
                        Sb.Append("<i>");
                    Sb.Append(text[i]);
                    if (tokenStyle.Italic)
                        Sb.Append("</i>");
                    if (tokenStyle.Bold)
                        Sb.Append("</b>");
                    Sb.Append("</color>");
                }

                if (underscoreColor == null)
                    Sb2.Append(' ', token.Stop - token.Start + 1);
                else {
                    Sb2.Append($"<color=#{ColorUtility.ToHtmlStringRGBA((Color) underscoreColor)}>");
                    Sb2.Append('_', token.Stop - token.Start + 1);
                    Sb2.Append("</color>");
                }
            }
            richText = Sb.ToString();
            underscores = Sb2.ToString();
        }
    }
}