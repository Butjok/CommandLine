using System.Reflection;
using UnityEngine;

namespace Butjok {

    public class CommandLine : MonoBehaviour {

        [SerializeField] private Commands commands;
        [SerializeField] private GUISkin skin;
        [SerializeField] private StyleSettings styleSettings;
        private ColorTheme _colorTheme;
        [SerializeField] private InputField _inputField;
        [SerializeField] private Vector2Int completionsRadius = new Vector2Int(5, 5);
        [SerializeField] private CompletionsManager completionsManager;
        [SerializeField] private CompletionsDrawer completionsDrawer;
        private bool _refreshCompletions;
        private int? newCursorPosition;
        private Interpreter _interpreter;
        private  float _inputFieldHeight;

        private void Reset() {

            commands = new Commands();
            
            styleSettings = Resources.Load<StyleSettings>("Butjok.CommandLine");
            Assert.That(styleSettings);

            skin = Resources.Load<GUISkin>("Butjok.CommandLine");
            _inputField = new InputField();
        }

        [SerializeField] private bool fly;
        
        private int Answer { get; set; } = 42;

        private void Awake() {
            Assert.That(skin);
            Assert.That(styleSettings);
            
            commands.Initialize();

            _colorTheme = styleSettings.Provide;

            for (var i = 0; i < 100; i++)
                commands.Add(name: $"fly{i}",
                    fieldInfo: GetType().GetField("fly", BindingFlags.Instance | BindingFlags.NonPublic),
                    targetObject: this);

            commands.Add(propertyInfo: GetType().GetProperty("Answer", BindingFlags.Instance | BindingFlags.NonPublic),
                targetObject: this);

            _inputField = new InputField(skin.GetStyle("Input"), skin.GetStyle("ColoredInput"), commands.Exists,
                commands.IsVariable, _colorTheme);

            _inputField.Edited += arguments => {
                _refreshCompletions = true;
                completionsManager.Clear();
            };

            completionsManager = new CompletionsManager(commands.Names, _colorTheme);
            completionsDrawer = new CompletionsDrawer(_colorTheme, commands.Exists, commands.IsVariable, commands.GetValue,
                commands.GetArguments, commands.GetHelpText, skin.GetStyle("Completion"),
                skin.GetStyle("CompletionUnderscores"), skin.GetStyle("CompletionInfo"),
                skin.GetStyle("SelectedCompletion"), Complete, skin.GetStyle("CompletionUnderscores"));
            
            _inputFieldHeight = skin.GetStyle("Input").CalcHeight(GUIContent.none, Screen.width);

            _interpreter = new Interpreter(commands.Invoke, commands.GetValue, commands.SetValue, commands.IsVariable);
        }

        private void Complete(Completion completion) {
            _inputField.Text = completion.GetSubstituted();
            newCursorPosition = completion.Cursor;
        }

        private void OnGUI() {

            GUI.skin = skin;

            // This is ugly, but I don't know how to do it better.
            if (Event.current.type == EventType.Repaint && newCursorPosition != null) {
                _inputField.Cursor = (int) newCursorPosition;
                newCursorPosition = null;
            }

            if (Event.current.type == EventType.KeyDown) {
                switch (Event.current.keyCode) {

                    case KeyCode.Tab:
                        Event.current.Use();

                        if (_refreshCompletions) {
                            _refreshCompletions = false;
                            completionsManager.FindCompletions(_inputField.Text, _inputField.Cursor);
                        }

                        completionsManager.NextCompletion(
                            Event.current.modifiers.HasFlag(EventModifiers.Shift) ? -1 : 1);

                        if (completionsManager.Index != -1)
                            Complete(completionsManager.Completions[completionsManager.Index]);
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        Event.current.Use();
                        
                        _interpreter.Execute(_inputField.Text, false);
                        
                        _inputField.Text = "";
                        completionsManager.Clear();
                        _refreshCompletions = true;
                        
                        completionsDrawer.ClearCache();
                        break;
                }
            }

            _inputField.Draw(new Rect(0, 0, Screen.width, _inputFieldHeight));

            completionsDrawer.Draw(completionsManager.Completions, (completionsRadius.x, completionsRadius.y), 
                completionsManager.Index, _inputFieldHeight);
        }
    }
}