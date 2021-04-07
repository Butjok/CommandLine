using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {

    public class SyntaxHighlighting {

        private readonly Dictionary<int, TokenStyle> _cachedStyles = new Dictionary<int, TokenStyle>();
        private TokenStyle _default;
        private TokenStyle _error;
        private TokenStyle _unknownCommand;
        private TokenStyle _procedureCommand;
        private TokenStyle _variableCommand;
        private readonly Func<string, (bool Exists, bool IsVaraible)> _getCommandInfo;

        private static readonly StringBuilder Sb = new StringBuilder();
        private static readonly StringBuilder Sb2 = new StringBuilder();

        public SyntaxHighlighting(Func<string, (bool Exists, bool IsVaraible)> getCommandInfo) {
            _getCommandInfo = getCommandInfo;
        }

        public Style Style {
            set {
                _cachedStyles.Clear();
                foreach (var item in value.Styles) {
                    Check.That(!_cachedStyles.ContainsKey(item.Type), item.Type.ToString);
                    _cachedStyles.Add(item.Type, item);
                }

                _default = value.Default;
                _error = value.Error;
                _unknownCommand = value.UnknownCommand;
                _procedureCommand = value.ProcedureCommand;
                _variableCommand = value.VariableCommand;
            }
        }

        private static string GetText(string text, int start, int stop) {
            Check.That(text != null);
            Check.That(start >= 0);
            Check.That(stop < text.Length);
            Check.That(start <= stop);

            return text.Substring(start, stop - start + 1);
        }

        public void HighlightSyntax(string text, IEnumerable<Token> tokens, 
            out string coloredText, out string underscores) {

            Sb.Clear();
            Sb2.Clear();
            foreach (var token in tokens) {

                ValidateToken(text, token);
                var tokenStyle = _cachedStyles.TryGetValue(token.Type, out var result) ? result : _default;

                Color? underscoreColor = null;
                switch (token.Type) {
                    case CommandLineLexer.ShortHexRgbColor:
                    case CommandLineLexer.ShortHexRgbaColor:
                    case CommandLineLexer.LongHexRgbColor:
                    case CommandLineLexer.LongHexRgbaColor: {
                        underscoreColor = Parse.HexColor(GetText(text, token.Start, token.Stop));
                        break;
                    }

                    case CommandLineLexer.Identifier:
                        if (_getCommandInfo == null)
                            break;
                        var (exists, isVariable) = _getCommandInfo(GetText(text, token.Start, token.Stop));
                        if (!exists)
                            tokenStyle = _unknownCommand;
                        else if (isVariable)
                            tokenStyle = _variableCommand;
                        else
                            tokenStyle = _procedureCommand;
                        break;
                    
                    case TokenConstants.InvalidType:
                        tokenStyle = _error;
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
        private static void ValidateToken(string text, Token token) {
            Check.That(text!=null);
            Check.That(token.Start >=0);
            Check.That(token.Stop < text.Length);
            Check.That(token.Start <= token.Stop);
        }
    }
}