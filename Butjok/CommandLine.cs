using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Butjok {

    [CLSCompliant(false)]
    public class CommandLine : MonoBehaviour {

        [SerializeField] protected Commands commands;
        protected ColorTheme ColorTheme;
        protected InputField InputField;
        [SerializeField] protected Vector2Int completionsRadius = new Vector2Int(10, 10);
        protected CompletionsManager CompletionsManager;
        protected CompletionsDrawer CompletionsDrawer;
        protected bool RefreshCompletions;
        protected int? NewCursorPosition;
        protected Interpreter Interpreter;
        protected float InputFieldHeight;
        [SerializeField] protected GUISkin skin;
        [SerializeField] protected ColorThemeSettings colorThemeSettings;
        [SerializeField] private Texture keywordIcon;
        [SerializeField] private Texture variableIcon;
        [SerializeField] private Texture procedureIcon;

        protected virtual void Awake() {
            Assert.That(skin);
            Assert.That(colorThemeSettings);

            commands.Initialize();
            ColorTheme = colorThemeSettings.Provide;

            InputField = new InputField(skin.GetStyle("Input"), skin.GetStyle("ColoredInput"), commands.Exists,
                commands.IsVariable, ColorTheme, "Butjok.CommandLine");

            RefreshCompletions = true;
            InputField.Edited += arguments => {
                RefreshCompletions = true;
                CompletionsManager.Clear();
            };

            CompletionsManager = new CompletionsManager(() => commands.Names, ColorTheme);
            CompletionsDrawer = new CompletionsDrawer(ColorTheme, commands.Exists, commands.IsVariable,
                commands.GetValue,
                commands.GetArguments, commands.GetHelpText, skin.GetStyle("Completion"),
                skin.GetStyle("CompletionUnderscores"), skin.GetStyle("CompletionInfo"),
                skin.GetStyle("SelectedCompletion"), Complete, skin.GetStyle("CompletionUnderscores"),
                keywordIcon, variableIcon, procedureIcon);

            InputFieldHeight = skin.GetStyle("Input").CalcHeight(GUIContent.none, Screen.width);

            Interpreter = new Interpreter(commands.Invoke, commands.GetValue, commands.SetValue, commands.IsVariable);

            // Add default commands.
            commands.RegisterFrom(Assembly.GetExecutingAssembly());
            commands.RegisterFrom(this);

            const BindingFlags all = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public |
                                     BindingFlags.NonPublic;
            commands.Add(propertyInfo: typeof(Application).GetProperty(nameof(Application.persistentDataPath), all));
        }

        [Command(arguments: new[] {"commandName"}, helpText: "Show debug comment for command.")]
        private void DebugComment(Arguments arguments) {
            var commandName = arguments.Get<string>("commandName");
            if (!commands.Exists(commandName))
                Debug.Log($"No such command: '{commandName}'");
            else {
                var debugComment = commands.GetDebugComment(commandName);
                if (debugComment != null)
                    Debug.Log(debugComment);
            }
        }

        protected virtual void Complete(Completion completion) {
            InputField.Text = completion.GetSubstituted();
            NewCursorPosition = completion.Cursor;
        }

        protected virtual void OnGUI() {

            GUI.skin = skin;

            // This is ugly, but I don't know how to do it better.
            if (Event.current.type == EventType.Repaint && NewCursorPosition != null) {
                InputField.Cursor = (int) NewCursorPosition;
                NewCursorPosition = null;
            }

            if (Event.current.type == EventType.KeyDown) {
                switch (Event.current.keyCode) {

                    case KeyCode.Tab:
                        Event.current.Use();

                        if (RefreshCompletions) {
                            RefreshCompletions = false;
                            CompletionsManager.FindCompletions(InputField.Text, InputField.Cursor);
                        }

                        CompletionsManager.NextCompletion(
                            Event.current.modifiers.HasFlag(EventModifiers.Shift) ? -1 : 1);

                        if (CompletionsManager.Index != -1)
                            Complete(CompletionsManager.Completions[CompletionsManager.Index]);
                        break;

                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        Event.current.Use();

                        Interpreter.Execute(InputField.Text, false);

                        InputField.Text = "";
                        CompletionsManager.Clear();
                        RefreshCompletions = true;

                        CompletionsDrawer.ClearCache();
                        break;
                }
            }

            InputField.Draw(new Rect(0, 0, Screen.width, InputFieldHeight));

            CompletionsDrawer.Draw(CompletionsManager.Completions, (completionsRadius.x, completionsRadius.y),
                CompletionsManager.Index, InputFieldHeight);
        }
    }
}