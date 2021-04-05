using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEngine;

namespace Butjok {

    [Serializable]
    public class TokenStyle {
        [HideInInspector] [SerializeField] private string description;
        [HideInInspector] [SerializeField] private TokenType type;
        [SerializeField] private Color color;
        [SerializeField] private bool bold;
        [SerializeField] private bool italic;
        public TokenStyle(TokenType type, Color? color = null, string description = "", bool bold = false,
            bool italic = false) {
            this.description = description;
            this.type = type;
            this.bold = bold;
            this.italic = italic;
            this.color = color ?? Color.white;
        }
        public TokenType Type => type;
        public Color Color { get => color; set => color = value; }
        public bool Bold { get => bold; set => bold = value; }
        public bool Italic { get => italic; set => italic = value; }
    }

    public partial class CommandLine {

        private class ColorSchemeListener : CommandLineBaseListener {
            
            public CommandLine CommandLine;
            private readonly ValueEvaluator _valueEvaluator = new ValueEvaluator();
            
            public override void EnterStyle(CommandLineParser.StyleContext context) {
                var name = (string) _valueEvaluator.Visit(context.@string());
                if (!Enum.TryParse(name, out TokenType type)) {
                    Debug.LogWarning(
                        $"Color scheme has invalid key '{name}', there is no such token type, skipping...");
                    return;
                }
                var style = CommandLine.tokenStylesList.SingleOrDefault(item => item.Type == type);
                if (style == null) {
                    Debug.LogWarning(
                        $"Cannot find a style for {type}. Did you forget to regenerate code for tokens? See Window > Regenerate code. Skipping...");
                    return;
                }
                style.Color = (Color) _valueEvaluator.Visit(context.color());
                style.Bold = (bool) _valueEvaluator.Visit(context.boolean(0));
                style.Italic = (bool) _valueEvaluator.Visit(context.boolean(1));
            }
        }

        [SerializeField] private TextAsset colorSchemeFile;
        private Dictionary<TokenType, TokenStyle> _tokenStyles;

        [ContextMenu("Set color scheme")]
        private void SetColorScheme() {
            Check.That(colorSchemeFile);

            var lexer = new CommandLineLexer(new AntlrInputStream(colorSchemeFile.text));
            var parser = new CommandLineParser(new CommonTokenStream(lexer));
            
            lexer.AddErrorListener(this);
            parser.AddErrorListener(this);
            
            ParseTreeWalker.Default.Walk(new ColorSchemeListener {CommandLine = this}, parser.colorScheme());
        }
        
        private void InitializeTokensSettings() {
            _tokenStyles = new Dictionary<TokenType, TokenStyle>();
            foreach (var item in tokenStylesList) {
                Check.That(!_tokenStyles.ContainsKey(item.Type), item.Type.ToString);
                _tokenStyles.Add(item.Type, item);
            }
        }

        public TokenStyle GetTokenStyle(TokenType type) {
            if (!_tokenStyles.TryGetValue(type, out var settings))
                throw new Exception(type.ToString());
            return settings;
        }
        
        public void HighlightSyntax(string text, IList<IToken> tokens, out string coloredText, out string underscores) {
            Check.That(text != null);
            Check.That(tokens != null);

            var parts = new List<(TokenType, int, int)>();
            if (tokens.Count == 0)
                parts.Add((TokenType.Unknown, 0, text.Length - 1));

            else {
                parts.Add((TokenType.Unknown, 0, tokens[0].StartIndex - 1));
                parts.Add(((TokenType) tokens[0].Type, tokens[0].StartIndex, tokens[0].StopIndex));
                for (var i = 1; i < tokens.Count; i++) {
                    parts.Add((TokenType.Unknown, tokens[i - 1].StopIndex + 1, tokens[i].StartIndex - 1));
                    parts.Add(((TokenType) tokens[i].Type, tokens[i].StartIndex, tokens[i].StopIndex));
                }
                parts.Add((TokenType.Unknown, tokens[tokens.Count - 1].StopIndex + 1, text.Length - 1));
            }

            var sb = new StringBuilder();
            var sb2 = new StringBuilder();
            foreach (var (type, start, stop) in parts) {

                var settings = _tokenStyles[type];
                Color? previewColor = null;
                switch (type) {

                    case TokenType.Name: {
                        var tokenText = text.Substring(start, stop - start + 1);
                        if (!_commands.TryGetValue(tokenText, out var command))
                            settings = _tokenStyles[TokenType.UnknownCommand];
                        else if (command.Type.IsProcedure())
                            settings = _tokenStyles[TokenType.ProcedureCommand];
                        else if (command.Type.IsVariable())
                            settings = _tokenStyles[TokenType.VariableCommand];
                        else
                            throw new CheckException(command.Type.ToString());
                        break;
                    }

                    case TokenType.ShortHexRgbColor:
                    case TokenType.ShortHexRgbaColor:
                    case TokenType.LongHexRgbColor:
                    case TokenType.LongHexRgbaColor: {
                        var tokenText = text.Substring(start, stop - start + 1);
                        previewColor = ParseHexColor(tokenText);
                        break;
                    }
                }

                var color = ColorUtility.ToHtmlStringRGBA(settings.Color);
                for (var i = start; i <= stop; i++) {
                    sb.Append($"<color=#{color}>");
                    if (settings.Bold)
                        sb.Append("<b>");
                    if (settings.Italic)
                        sb.Append("<i>");
                    sb.Append(text[i]);
                    if (settings.Italic)
                        sb.Append("</i>");
                    if (settings.Bold)
                        sb.Append("</b>");
                    sb.Append("</color>");
                }

                if (previewColor == null)
                    sb2.Append(' ', stop - start + 1);
                else {
                    sb2.Append($"<color=#{ColorUtility.ToHtmlStringRGBA((Color) previewColor)}>");
                    sb2.Append('_', stop - start + 1);
                    sb2.Append("</color>");
                }
            }
            coloredText = sb.ToString();
            underscores = sb2.ToString();
        }
    }
}