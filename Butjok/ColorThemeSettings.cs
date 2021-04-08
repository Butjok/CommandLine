using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Butjok {

    [CreateAssetMenu(fileName = nameof(ColorThemeSettings))]
    [CLSCompliant(false)]
    public class ColorThemeSettings : ScriptableObject {

        [Serializable]
        private class TokenStyle : Colorizer.ITokenStyle {
            // This is used only as a header for list element in Inspector.
            [HideInInspector] public string name = nameof(TokenConstants.InvalidType);

            [HideInInspector] public int type = TokenConstants.InvalidType;
            public Color color = Color.white;
            public bool isBold;
            public bool isItalic;

            public bool IsBold => isBold;
            public bool IsItalic => isItalic;
            public Color Color => color;
        }

        private class SyntaxErrorException : Exception {
            public int Line, Column;
            public new string Message;
        }

        // This class cannot be moved to outer scope because it uses TokenStyle class.
        // This is handy because we need a reference type here instead of Butjok.TokenStyle which is struct.

        private class StyleReader : CommandLineBaseListener, IAntlrErrorListener<int>,
            IAntlrErrorListener<Antlr4.Runtime.IToken> {

            private static Dictionary<string, int> _tokenTypes;
            private static List<TokenStyle> _newStyles = new List<TokenStyle>();
            private static TokenStyle _default = new TokenStyle();
            private static TokenStyle _error = new TokenStyle {color = Color.red};
            private static TokenStyle _unknownCommand = new TokenStyle();
            private static TokenStyle _procedureCommand = new TokenStyle {color = Color.green};
            private static TokenStyle _variableCommand = new TokenStyle {color = Color.yellow};
            private static TokenStyle _command = new TokenStyle {color = Color.cyan};

            private static readonly CommandLineLexer Lexer;
            private static readonly CommandLineParser Parser;
            private static readonly ValueEvaluator Evaluator;

            public static readonly StyleReader Default = new StyleReader();

            static StyleReader() {

                Lexer = new CommandLineLexer(null);
                Parser = new CommandLineParser(null);
                Evaluator = new ValueEvaluator();

                Lexer.AddErrorListener(Default);
                Parser.AddErrorListener(Default);
            }

            public static void Load(string input, ref List<TokenStyle> styles,
                ref TokenStyle @default, ref TokenStyle error,
                ref TokenStyle unknownCommand, ref TokenStyle procedureCommand,
                ref TokenStyle variableCommand, ref TokenStyle command) {

                // Read token types from Lexer class into a dictionary: tokenTypeName => intValue.
                // Cannot place this code in static Styles() constructor because if Unity has domain reload disabled
                // then _tokenTypes might contain old data.    

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
                    Debug.LogWarning(
                        $"Problem while reading styles file:\n{e.Message.Capitalise()} at line {e.Line}, position {e.Column}.\n");
                    return;
                }

                _newStyles.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

                Swap.Values(ref styles, ref _newStyles);
                Swap.Values(ref @default, ref _default);
                Swap.Values(ref error, ref _error);
                Swap.Values(ref unknownCommand, ref _unknownCommand);
                Swap.Values(ref procedureCommand, ref _procedureCommand);
                Swap.Values(ref variableCommand, ref _variableCommand);
                Swap.Values(ref command, ref _command);
            }

            public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
                int charPositionInLine, string msg, RecognitionException e) {
                throw new SyntaxErrorException {Line = line, Column = charPositionInLine, Message = msg};
            }
            public void SyntaxError(TextWriter output, IRecognizer recognizer, Antlr4.Runtime.IToken offendingSymbol,
                int line,
                int charPositionInLine, string msg, RecognitionException e) {
                throw new SyntaxErrorException {Line = line, Column = charPositionInLine, Message = msg};
            }

            public override void EnterStyle(CommandLineParser.StyleContext context) {
                Assert.That(_tokenTypes != null);

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
                        case "Command":
                            style = _command;
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
                style.isBold = bold;
                style.isItalic = italic;
            }
        }

        [SerializeField] private TextAsset input;
        [SerializeField] private TextAsset output;

        [Space]
        [SerializeField] private TokenStyle @default;
        [SerializeField] private TokenStyle error;
        [SerializeField] private TokenStyle unknownCommand;
        [SerializeField] private TokenStyle command;
        [SerializeField] private TokenStyle procedureCommand;
        [SerializeField] private TokenStyle variableCommand;
        [SerializeField] private List<TokenStyle> tokens;

        public ColorTheme Provide => new ColorTheme(
            tokens.ToDictionary(style => style.type, style => (Colorizer.ITokenStyle) style),
            @default,
            error,
            unknownCommand,
            procedureCommand,
            variableCommand,
            command);

        [ContextMenu("Load From File")]
        private void LoadFromFile() {
            if (!input) {
                Debug.Log("Please assign source style file into 'Input' field.", this);
                return;
            }
            Load(input.text);
        }
#if UNITY_EDITOR
        [ContextMenu("Save Into File")]
        private void SaveIntoFile() {
            if (!output) {
                Debug.Log("Please assign target style file into 'Output' field.", this);
                return;
            }
            var path = AssetDatabase.GetAssetPath(output);
            var sb = new StringBuilder(@"");
            foreach (var style in tokens) {}
        }
#endif
        [ContextMenu("Reset To Defaults")]
        public void ResetToDefaults() {

            @default = new TokenStyle();
            error = new TokenStyle {color = Color.red};
            unknownCommand = new TokenStyle();
            command = new TokenStyle {color = Color.magenta};
            procedureCommand = new TokenStyle {color = Color.green};
            variableCommand = new TokenStyle {color = Color.yellow};

            tokens.Clear();
            foreach (var info in TokenInfo.All.Values)
                tokens.Add(new TokenStyle {type = info.Type, name = info.Name,});
        }
        private void Reset() {
            tokens = new List<TokenStyle>();
            ResetToDefaults();
            if (input)
                Load(input.text);
        }
        public void Load(string input) {
            StyleReader.Load(input, ref tokens, ref @default, ref error, ref unknownCommand, ref procedureCommand,
                ref variableCommand, ref command);
        }
    }

    [CLSCompliant(false)]
    public readonly struct TokenStyle : Colorizer.ITokenStyle {

        public int Type { get; }
        public Color Color { get; }
        public bool IsBold { get; }
        public bool IsItalic { get; }

        public TokenStyle(int type, Color color, bool isBold, bool isItalic) {
            Type = type;
            Color = color;
            IsBold = isBold;
            IsItalic = isItalic;
        }
    }

    [CLSCompliant(false)]
    public readonly struct ColorTheme : Colorizer.IColorTheme {

        public IReadOnlyDictionary<int, Colorizer.ITokenStyle> Tokens { get; }
        public Colorizer.ITokenStyle Default { get; }
        public Colorizer.ITokenStyle Error { get; }
        public Colorizer.ITokenStyle UnknownCommand { get; }
        public Colorizer.ITokenStyle Command { get; }
        public Colorizer.ITokenStyle ProcedureCommand { get; }
        public Colorizer.ITokenStyle VariableCommand { get; }

        public ColorTheme(IReadOnlyDictionary<int, Colorizer.ITokenStyle> tokens, Colorizer.ITokenStyle @default,
            Colorizer.ITokenStyle error, Colorizer.ITokenStyle unknownCommand, Colorizer.ITokenStyle procedureCommand,
            Colorizer.ITokenStyle variableCommand, Colorizer.ITokenStyle command) {
            Assert.That(tokens != null);

            Tokens = tokens;
            Default = @default;
            Error = error;
            UnknownCommand = unknownCommand;
            ProcedureCommand = procedureCommand;
            VariableCommand = variableCommand;
            Command = command;
        }
    }
}