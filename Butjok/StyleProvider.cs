using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEngine;

namespace Butjok {

    public partial class StyleProvider : MonoBehaviour {

        [Serializable]
        private class TokenStyle {
            // This is used only as a header for list element in Inspector.
            [HideInInspector] public string name = nameof(TokenConstants.InvalidType);
            [HideInInspector] public int type = TokenConstants.InvalidType;
            public Color color = Color.white;
            public bool bold;
            public bool italic;
            public bool isKeyword;
        }

        private class SyntaxErrorException : Exception {
            public int Line, Column;
            public string Message;
        }

        private class Reader : CommandLineBaseListener, IAntlrErrorListener<int>, IAntlrErrorListener<IToken> {

            private static Dictionary<string, int> _tokenTypes;
            private static List<TokenStyle> _newStyles = new List<TokenStyle>();
            private static TokenStyle _default = new TokenStyle();
            private static TokenStyle _error = new TokenStyle {color = Color.red};
            private static TokenStyle _unknownCommand = new TokenStyle();
            private static TokenStyle _procedureCommand = new TokenStyle {color = Color.green};
            private static TokenStyle _variableCommand = new TokenStyle {color = Color.yellow};

            private static readonly CommandLineLexer Lexer;
            private static readonly CommandLineParser Parser;
            private static readonly ValueEvaluator Evaluator;

            public static readonly Reader Default = new Reader();

            static Reader() {

                Lexer = new CommandLineLexer(null);
                Parser = new CommandLineParser(null);
                Evaluator = new ValueEvaluator();

                Lexer.AddErrorListener(Default);
                Parser.AddErrorListener(Default);
            }

            public static void Load(string input, ref List<TokenStyle> styles,
                ref TokenStyle @default, ref TokenStyle error,
                ref TokenStyle unknownCommand, ref TokenStyle procedureCommand, ref TokenStyle variableCommand) {

                /*
                 * Read token types from Lexer class into a dictionary: tokenTypeName => intValue.
                 *
                 * Cannot place this code in static Styles() constructor because if Unity has domain reload disabled
                 * then _tokenTypes might contain old data.    
                 */

                _tokenTypes = CommandLineLexer.ruleNames
                    .Select((name, value) => (
                        Name: name,
                        Value: value,
                        SymbolicName: CommandLineLexer.DefaultVocabulary.GetSymbolicName(value)))
                    .Where(tuple => tuple.SymbolicName != null)
                    .ToDictionary(tuple => tuple.SymbolicName, tuple => tuple.Value);

                _newStyles.Clear();

                Lexer.SetInputStream(new AntlrInputStream(input));
                Parser.TokenStream = new CommonTokenStream(Lexer);

                try {
                    ParseTreeWalker.Default.Walk(Default, Parser.styles());
                }
                catch (SyntaxErrorException e) {
                    Debug.LogWarning($"Problem while reading styles file:\n{e.Message.Capitalise()} at line {e.Line}, position {e.Column}.\n");
                    return;
                }

                _newStyles.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

                Swap.Values(ref styles, ref _newStyles);
                Swap.Values(ref @default, ref _default);
                Swap.Values(ref error, ref _error);
                Swap.Values(ref unknownCommand, ref _unknownCommand);
                Swap.Values(ref procedureCommand, ref _procedureCommand);
                Swap.Values(ref variableCommand, ref _variableCommand);
            }

            public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
                int charPositionInLine, string msg, RecognitionException e) {
                throw new SyntaxErrorException {Line = line, Column = charPositionInLine, Message = msg};
            }
            public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
                int charPositionInLine, string msg, RecognitionException e) {
                throw new SyntaxErrorException {Line = line, Column = charPositionInLine, Message = msg};
            }

            public override void EnterStyle(CommandLineParser.StyleContext context) {
                Check.That(_tokenTypes != null);

                TokenStyle style;
                if (context.@string() == null) {
                    var name = context.Identifier().GetText();
                    switch (name) {
                        case "Default":
                            style = _default;
                            break;
                        case "SyntaxError":
                            style = _error;
                            break;
                        case "UnknownCommand":
                            style = _unknownCommand;
                            break;
                        case "ProcedureCommand":
                            style = _procedureCommand;
                            break;
                        case "VariableCommand":
                            style = _variableCommand;
                            break;
                        default:
                            Debug.LogWarning($"Styles file contains invalid special style '{name}', skipping it.");
                            return;
                    }
                }
                else {
                    var name = (string) Evaluator.Visit(context.@string());
                    var isValidName = _tokenTypes.TryGetValue(name, out var type);
                    if (!isValidName) {
                        Debug.LogWarning($"Styles file contains style for non-existent token '{name}', skipping it.");
                        return;
                    }
                    style = new TokenStyle {type = type, name = name};
                    _newStyles.Add(style);
                }

                var color = (Color) Evaluator.Visit(context.color());
                var bold = (bool) Evaluator.Visit(context.boolean(0));
                var italic = (bool) Evaluator.Visit(context.boolean(1));
                var isKeyword = (bool) Evaluator.Visit(context.boolean(2));

                style.color = color;
                style.bold = bold;
                style.italic = italic;
                style.isKeyword = isKeyword;
            }
        }

        [SerializeField] private TextAsset file;
        [SerializeField] private TokenStyle @default = new TokenStyle();
        [SerializeField] private TokenStyle error = new TokenStyle {color = Color.red};
        [SerializeField] private TokenStyle unknownCommand = new TokenStyle();
        [SerializeField] private TokenStyle procedureCommand = new TokenStyle {color = Color.green};
        [SerializeField] private TokenStyle variableCommand = new TokenStyle {color = Color.yellow};
        [SerializeField] private List<TokenStyle> styles;

        private static Butjok.TokenStyle ToStruct(TokenStyle style) {
            return new Butjok.TokenStyle {
                Type = style.type,
                Color = style.color,
                Bold = style.bold,
                Italic = style.italic,
                IsKeyword = style.isKeyword
            };
        }
        public Style Style => new Style {
            Default = ToStruct(@default),
            Error = ToStruct(error),
            UnknownCommand = ToStruct(unknownCommand),
            ProcedureCommand = ToStruct(procedureCommand),
            VariableCommand = ToStruct(variableCommand),
            Styles = styles.Select(ToStruct).ToList()
        };
        
        [ContextMenu("Load From File")]
        private void LoadFromFile() {
            if (!file) {
                Debug.Log("Please assign source style file into 'File' field of Style Provider.", this);
                return;
            }
            Load(file.text);
        }
        private void Reset() {
            ResetToDefaults();
            file = Resources.Load<TextAsset>("Butjok.CommandLine.Dracula");
            if (file)
                Load(file.text);
        }
        public void Load(string input) {
            Reader.Load(input,
                ref styles, ref @default, ref error, ref unknownCommand, ref procedureCommand, ref variableCommand);
        }
    }

    public struct TokenStyle {
        public int Type;
        public Color Color;
        public bool Bold;
        public bool Italic;
        public bool IsKeyword;
    }
    public struct Style {
        public List<TokenStyle> Styles;
        public TokenStyle Default;
        public TokenStyle Error;
        public TokenStyle UnknownCommand;
        public TokenStyle ProcedureCommand;
        public TokenStyle VariableCommand;
    }
}