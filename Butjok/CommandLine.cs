using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Butjok {

    [RequireComponent(typeof(StyleProvider))]
    public class CommandLine : MonoBehaviour {

        [Serializable]
        private struct Completion {
            public string text;
            public string coloredText;
            public string underscores;
        }

        [SerializeField] private Commands commands = new Commands();
        [SerializeField] private GUISkin skin;
        [SerializeField] private StyleProvider styleProvider;
        [SerializeField] private SyntaxHighlighting syntaxHighlighting;
        private Style _style;

        [Header("Debug")]
        [SerializeField] private string input;
        [SerializeField] private string coloredInput;
        [SerializeField] private string underscores;
        private readonly List<(string Name, string Text, string Colored, string Underscores)> _completions =
            new List<(string, string, string, string)>();

        private IEnumerable<string> AllCompletions => commands.Names
            .Concat(TokenInfos.Infos.Where(info
                => info.Literal != null && _style.Styles.TryGetValue(info.Type, out var tokenStyle) &&
                   tokenStyle.IsKeyword)
                .Select(info => info.Literal));

        private void Reset() {

            styleProvider = GetComponent<StyleProvider>();
            Assert.That(styleProvider);

            skin = Resources.Load<GUISkin>("Butjok.CommandLine");

            input = "";
            coloredInput = "";
            underscores = "";
        }

        [SerializeField] private bool fly;
        private int Answer { get; set; } = 42;

        private void Awake() {
            Assert.That(skin);

            _style = styleProvider.Provide;
            syntaxHighlighting = new SyntaxHighlighting(commands.Exists, commands.IsVariable)
                {Style = _style};

            for (var i = 0; i < 100; i++)
                commands.Add(name: $"fly{i}",
                    fieldInfo: GetType().GetField("fly", BindingFlags.Instance | BindingFlags.NonPublic),
                    targetObject: this);

            commands.Add(propertyInfo: GetType().GetProperty("Answer", BindingFlags.Instance | BindingFlags.NonPublic),
                targetObject: this);
        }

        private readonly TextParser _inputParser = new TextParser();
        private readonly TextParser _completionParser = new TextParser();

        private void OnGUI() {
            GUI.skin = skin;
            var height = skin.GetStyle("Input").CalcHeight(GUIContent.none, Screen.width);
            var rect = new Rect(0, 0, Screen.width, height);

            var newInput = GUI.TextField(rect, input, skin.GetStyle("Input"));
            if (newInput != input) {
                input = newInput;
                _inputParser.Text = input;
                syntaxHighlighting.HighlightSyntax(input, _inputParser.Tokens, out coloredInput, out underscores);
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
                            foreach (var (name, getSubstituted) in Completions.Find(input, _inputParser.TokenAt,
                                state.cursorIndex, AllCompletions)) {

                                _completionParser.Text = name;
                                syntaxHighlighting.HighlightSyntax(name, _completionParser.Tokens,
                                    out var colored, out var underscores);
                                _completions.Add((name, getSubstituted(), colored, underscores));
                            }
                            break;
                        }
                    }
                    break;
            }

            for (var i = 0; i < _completions.Count; i++) {
                var size = skin.GetStyle("Completion").CalcSize(new GUIContent(_completions[i].Colored));
                var rect2 = new Rect(rect.xMin, rect.yMax + i * size.y, size.x, size.y);

                GUI.Label(rect2, $"{_completions[i].Colored} --to-- '{_completions[i].Text}'",
                    skin.GetStyle("Completion"));
                GUI.Label(rect2, _completions[i].Underscores, skin.GetStyle("CompletionUnderscores"));
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