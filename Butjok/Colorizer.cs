using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {

    public interface IColorizer {
        void Colorize(string text, out string richText, out string underscores);
    }

    [CLSCompliant(false)]
    public class Colorizer : IColorizer {

        public interface ITextParser {
            IEnumerable<IToken> Parse(string text);
        }
        public interface ITokenStyle {
            bool IsBold { get; }
            bool IsItalic { get; }
            Color Color { get; }
        }
        public interface IColorTheme {
            IReadOnlyDictionary<int, ITokenStyle> Tokens { get; }
            ITokenStyle Default { get; }
            ITokenStyle UnknownCommand { get; }
            ITokenStyle ProcedureCommand { get; }
            ITokenStyle VariableCommand { get; }
            ITokenStyle Error { get; }
        }

        private readonly IColorTheme _colorTheme;
        private readonly ICommands _commands;
        private readonly ITextParser _textParser;

        private static readonly StringBuilder Sb = new StringBuilder();
        private static readonly StringBuilder Sb2 = new StringBuilder();

        public Colorizer(IColorTheme colorTheme, ICommands commands, ITextParser textParser) {
            Assert.That(colorTheme.Tokens != null);
            Assert.That(textParser != null);

            _colorTheme = colorTheme;
            _commands = commands;
            _textParser = textParser;
        }

        public void Colorize(string text, out string richText, out string underscores) {
            Assert.That(text != null);

            Sb.Clear();
            Sb2.Clear();
            foreach (var token in _textParser.Parse(text)) {

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
                        underscoreColor = Parse.HexColor(token.FindText());
                        break;
                    }

                    case CommandLineLexer.Identifier:
                        if (_commands == null)
                            break;
                        var name = token.FindText();
                        tokenStyle = !_commands.Exists(name)
                            ? _colorTheme.UnknownCommand
                            : _commands.IsVariable(name)
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
                    if (tokenStyle.IsBold)
                        Sb.Append("<b>");
                    if (tokenStyle.IsItalic)
                        Sb.Append("<i>");
                    Sb.Append(text[i]);
                    if (tokenStyle.IsItalic)
                        Sb.Append("</i>");
                    if (tokenStyle.IsBold)
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