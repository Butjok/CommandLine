using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {

    public class SyntaxHighlighting {

        public Style Style;
        private readonly Func<string, bool> _exists;
        private readonly Func<string, bool> _isVariable;

        private static readonly StringBuilder Sb = new StringBuilder();
        private static readonly StringBuilder Sb2 = new StringBuilder();

        public SyntaxHighlighting(Func<string, bool> exists, Func<string, bool> isVariable) {
            _exists = exists;
            _isVariable = isVariable;
        }

        public void HighlightSyntax(string text, IEnumerable<Token> tokens,
            out string coloredText, out string underscores) {

            Sb.Clear();
            Sb2.Clear();
            foreach (var token in tokens) {

                Assert.That(text != null);
                Assert.That(token.Start >= 0);
                Assert.That(token.Stop < text.Length);
                Assert.That(token.Start <= token.Stop);

                var tokenStyle = Style.Styles.TryGetValue(token.Type, out var result) ? result : Style.Default;

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
                            ? Style.UnknownCommand
                            : _isVariable == null
                                ? Style.Command
                                : _isVariable(name)
                                    ? Style.VariableCommand
                                    : Style.ProcedureCommand;
                        break;

                    case TokenConstants.InvalidType:
                        tokenStyle = Style.Error;
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
            coloredText = Sb.ToString();
            underscores = Sb2.ToString();
        }
    }
}