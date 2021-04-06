using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {

    [Serializable]
    public class TokenStyle {
        [HideInInspector] [SerializeField] private string description;
        [HideInInspector] [SerializeField] private TokenType type;
        [SerializeField] private Color color;
        [SerializeField] private bool bold;
        [SerializeField] private bool italic;
        [SerializeField] private bool isKeyword;
        public TokenStyle(TokenType type, Color? color = null, string description = "", bool bold = false,
            bool italic = false, bool isKeyword = false) {
            this.description = description;
            this.type = type;
            this.bold = bold;
            this.italic = italic;
            this.color = color ?? Color.white;
            this.isKeyword = isKeyword;
        }
        public TokenType Type => type;
        public Color Color { get => color; set => color = value; }
        public bool Bold { get => bold; set => bold = value; }
        public bool Italic { get => italic; set => italic = value; }
        public bool IsKeyword { get => isKeyword; set => isKeyword = value; }
    }

    public partial class CommandLine {

        [SerializeField] private TextAsset colorSchemeFile;
        public Dictionary<TokenType, TokenStyle> TokenStyles { get; private set; }

        [ContextMenu("Load Color Scheme")]
        private void SetColorScheme() {
            Check.That(colorSchemeFile);
            Execute(colorSchemeFile.text, parser => parser.styles());
        }
        [ContextMenu("Save Color Scheme")]
        private void SaveColorScheme() {}
        private void CacheTokenStyles() {
            TokenStyles = new Dictionary<TokenType, TokenStyle>();
            foreach (var item in tokenStylesList) {
                Check.That(!TokenStyles.ContainsKey(item.Type), item.Type.ToString);
                TokenStyles.Add(item.Type, item);
            }
        }
        public void ReadAllTokens(string text, CommandLineLexer lexer,
            List<(TokenType type, int start, int stop)> tokens) {
            Check.That(text != null);
            Check.That(tokens != null);

            lexer.SetInputStream(new AntlrInputStream(text));
            tokens.Clear();
            var token = lexer.NextToken();
            if (token.Type == TokenConstants.EOF) {
                if (text.Length - 1 >= 0)
                    tokens.Add((TokenType.Unknown, 0, text.Length - 1));
            }
            else {
                if (token.StartIndex - 1 >= 0)
                    tokens.Add((TokenType.Unknown, 0, token.StartIndex - 1));
                AddToken(text, tokens, (TokenType) token.Type, token.StartIndex, token.StopIndex);
                token = lexer.NextToken();
                while (token.Type != TokenConstants.EOF) {
                    if (token.StartIndex - 1 >= tokens[tokens.Count - 1].stop + 1)
                        tokens.Add((TokenType.Unknown, tokens[tokens.Count - 1].stop + 1, token.StartIndex - 1));
                    AddToken(text, tokens, (TokenType) token.Type, token.StartIndex, token.StopIndex);
                    token = lexer.NextToken();
                }
                if (text.Length - 1 >= tokens[tokens.Count - 1].stop + 1)
                    tokens.Add((TokenType.Unknown, tokens[tokens.Count - 1].stop + 1, text.Length - 1));
            }
        }
        private void AddToken(string text, ICollection<(TokenType type, int start, int stop)> tokens, 
            TokenType type, int start, int stop) {
            Check.That(tokens!=null);
            Check.That(start >= 0);
            Check.That(start <= stop);
            
            if (type != TokenType.Identifier)
                tokens.Add((type,start,stop));
            else {
                var tokenText = text.Substring(start, stop - start + 1);
                if (!_commands.TryGetValue(tokenText, out var command))
                    type=TokenType.UnknownCommand;
                else if (command.Type.IsProcedure())
                    type=TokenType.ProcedureCommand;
                else if (command.Type.IsVariable())
                    type=TokenType.VariableCommand;
                else
                    throw new CheckException(command.Type.ToString());
                tokens.Add((type,start,stop));
            }
        }
        public void HighlightSyntax(string text, List<(TokenType type, int start, int stop)> tokens,
            StringBuilder sb, StringBuilder sb2, out string coloredText, out string underscores) {
            Check.That(text != null);
            Check.That(tokens != null);
            Check.That(sb != null);
            Check.That(sb2 != null);

            sb.Clear();
            sb2.Clear();
            foreach (var (type, start, stop) in tokens) {
                
                Color? underscoreColor = null;
                switch (type) {
                    case TokenType.ShortHexRgbColor:
                    case TokenType.ShortHexRgbaColor:
                    case TokenType.LongHexRgbColor:
                    case TokenType.LongHexRgbaColor: {
                        var tokenText = text.Substring(start, stop - start + 1);
                        underscoreColor = ParseHexColor(tokenText);
                        break;
                    }
                }

                var settings = TokenStyles[type];
                var colorTag = $"<color=#{ColorUtility.ToHtmlStringRGBA(settings.Color)}>";
                for (var i = start; i <= stop; i++) {
                    sb.Append(colorTag);
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

                if (underscoreColor == null)
                    sb2.Append(' ', stop - start + 1);
                else {
                    sb2.Append($"<color=#{ColorUtility.ToHtmlStringRGBA((Color) underscoreColor)}>");
                    sb2.Append('_', stop - start + 1);
                    sb2.Append("</color>");
                }
            }
            coloredText = sb.ToString();
            underscores = sb2.ToString();
        }
    }
}