using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {
    [Serializable]
    public partial class CommandLine : MonoBehaviour, IAntlrErrorListener<int>, IAntlrErrorListener<IToken> {

        [Serializable]
        private struct Completion {
            public string text;
            public string coloredText;
            public string underscores;
        }

        [SerializeField] private bool fly;

        [SerializeField] private GUISkin skin;
        [SerializeField] private string input = "";
        [SerializeField] private string coloredInput = "";
        [SerializeField] private string underscores = "";

        private void Awake() {
            Check.That(skin);

            CacheTokenStyles();
            CacheCommands();

            for (var i = 0; i < 100; i++)
                AddCommand(new Command(this, CommandType.Field,
                    name: $"fly{i}",
                    field: GetType().GetField("fly", BindingFlags.Instance | BindingFlags.NonPublic),
                    targetObject: this));

            AddCommand(new Command(this, CommandType.Method,
                method: GetType().GetMethod("Print", BindingFlags.Instance | BindingFlags.NonPublic),
                targetObject: this));
        }

        private void Print(Arguments arguments) {
            foreach (var argument in arguments)
                Debug.Log(argument);
        }

        [SerializeField] private List<Completion> _completions = new List<Completion>();
        private readonly CommandLineLexer _lexer = new CommandLineLexer(null);
        private readonly List<(TokenType type, int start, int stop)> _tokens
            = new List<(TokenType type, int start, int stop)>();
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly StringBuilder _sb2 = new StringBuilder();
        private readonly int[,] _levenshtein = new int[1000, 1000];

        private readonly List<(TokenType type, int start, int stop)> _tokens2
            = new List<(TokenType type, int start, int stop)>();

        private void OnGUI() {
            GUI.skin = skin;
            var height = skin.GetStyle("Input").CalcHeight(GUIContent.none, Screen.width);
            var rect = new Rect(0, 0, Screen.width, height);

            var newInput = GUI.TextField(rect, input, skin.GetStyle("Input"));
            if (newInput != input) {
                input = newInput;
                _lexer.SetInputStream(new AntlrInputStream(input));
                ReadAllTokens(input, _lexer, _tokens);
                HighlightSyntax(input, _tokens, _sb, _sb2, out coloredInput, out underscores);
            }

            GUI.Label(rect, coloredInput, skin.GetStyle("ColoredInput"));
            GUI.Label(rect, underscores, skin.GetStyle("ColoredInput"));

            switch (Event.current.type) {

                case EventType.KeyDown:

                    switch (Event.current.keyCode) {
                        case KeyCode.Tab: {
                            Event.current.Use();

                            var state = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor),
                                GUIUtility.keyboardControl);

                            _completions.Clear();
                            _completions.AddRange(FindCompletions(input, _tokens, state.cursorIndex, _levenshtein)
                                .Select(text => {
                                    var completion = new Completion {text=text};
                                    ReadAllTokens(text, _lexer, _tokens2);
                                    HighlightSyntax(text, _tokens2, _sb, _sb2, 
                                        out completion.coloredText, out completion.underscores);
                                    return completion;
                                }));
                            break;
                        }
                    }
                    break;
            }

            for (var i = 0; i < _completions.Count; i++) {
                var size = skin.GetStyle("Completion").CalcSize(new GUIContent(_completions[i].coloredText));
                var rect2 = new Rect(rect.xMin, rect.yMax + i * size.y, size.x, size.y);

                GUI.Label(rect2, _completions[i].coloredText, skin.GetStyle("Completion"));
                GUI.Label(rect2, _completions[i].underscores, skin.GetStyle("CompletionUnderscores"));
            }

            /*if (showCompletions) {

                if (_completions.Count == 0) {
                    _noCompletionsGuiContent.text = noCompletionsText;
                    var size = completionsInfoStyleName.CalcSize(_noCompletionsGuiContent);

                    GUI.Label(new Rect(0, inputRectangle.yMin - size.y, size.x, size.y), _noCompletionsGuiContent,
                        noCompletionsStyle);
                }
                else {
                    var completionHeight = completionStyle.CalcHeight(GUIContent.none, Screen.width);
                    var (low, high) = MathUtils.SlidingWindow(_completions.Count, (radiusBackward, radiusForward),
                        completionIndex);

                    var maxWidth = 0f;
                    for (var i = low; i <= high; i++) {
                        var command = _completions[i];

                        if (updateCompletionsInRealTime)
                            UpdateCompletion(command);

                        maxWidth = Mathf.Max(maxWidth,
                            completionStyle.CalcSize(_completionsCache[command].GUIContent).x);
                    }

                    var startY = inputRectangle.yMin - (high - low + 1) * completionHeight;
                    var yOffset = 0f;

                    if (low > 0 || high < _completions.Count - 1) {
                        var h = completionIndexStyle.CalcHeight(_completionIndexGuiContent, Screen.width);
                        startY -= h;
                        GUI.Label(new Rect(0, startY, maxWidth, h), _completionIndexGuiContent, completionIndexStyle);
                        yOffset += h;
                    }
                    for (var i = low; i <= high; i++) {
                        var style = i == completionIndex ? selectedCompletionStyle : completionStyle;
                        GUI.Label(new Rect(0, startY + yOffset, maxWidth, completionHeight),
                            _completionsCache[_completions[i]].GUIContent, style);
                        yOffset += completionHeight;
                    }
                }
            }

            GUI.SetNextControlName(inputFieldControlName);
            var newText = _inputField.Draw(inputRectangle, inputStyle, coloredInputStyle)
                .Replace("`", ""); // TODO: small hack to remove unwanted backquotes (incomplete)

            if (_inputField.Text != newText) {
                SetInputText(newText);
                _history.Touch(newText);
                showCompletions = false;
                mustUpdateCompletions = true;
            }*/

            /*var state = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor),
                GUIUtility.keyboardControl);
            ;

            StringBuilder.Clear();
            foreach (var completion in CommandLine.FindCompletions(tokens, state.cursorIndex, Distance, out var token)) {
                var replace = CommandLine.Replace(input, token, completion);
                StringBuilder.AppendLine(replace);
            }
            var text = StringBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(text))
                Debug.Log(text);*/
        }
    }
}