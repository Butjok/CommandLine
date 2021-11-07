using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Butjok
{
    public class CommandLineGUI : MonoBehaviour
    {
        public GUISkin guiSkin;
        [Command]
        public int radius = 5;
        [Command]
        public string prefixFormat = "[{0}/{1}] ";
        [Command]
        public bool includeNamespaceName = true;
        public bool dontDestroyOnLoad = true;
        [Command]
        public int maximumMultipleObjects = 3;

        private string input;
        private int index;
        private List<string> matches;
        private bool initialized;
        private Action action;
        
        private void Awake() {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            if (!initialized)
                Initialize();
        }
        [Command]
        private void Initialize() {
            initialized = true;
            input = "";
            index = -1;
            matches = new List<string>();
            action = null;
            CommandLine.Initialize();
            Commands.Initialize(includeNamespaceName);
            CommandHistory.Initialize();
            SyntaxHighlighting.Initialize();
        }
        public void AddCommand(MemberInfo memberInfo, string name = null, bool absolute = false) {
            if (!initialized)
                Initialize();
            var generatedName = Commands.GenerateName(memberInfo, new CommandAttribute(name, absolute), includeNamespaceName);
            Commands.Add(generatedName, memberInfo);
        }
        
        private void OnGUI() {
            
            GUI.skin = guiSkin;

            if (Event.current.type == EventType.KeyDown) {
                switch (Event.current.keyCode) {
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow: {
                        Event.current.Use();
                        var offset = Event.current.keyCode == KeyCode.UpArrow ? -1 : 1;
                        if (CommandHistory.Lookup(offset)) {
                            input = CommandHistory.Text;
                            index = -1;
                            FindCompletions();
                            action = () => {
                                var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                                textEditor.selectIndex = textEditor.cursorIndex = 999;
                            };
                        }
                        break;
                    }
                    case KeyCode.Escape:
                        Event.current.Use();
                        input = "";
                        index = -1;
                        matches.Clear();
                        break;
                    case KeyCode.Return:
                        Event.current.Use();
                        CommandHistory.Add(input);
                        CommandLine.Execute(input);
                        input = "";
                        index = -1;
                        matches.Clear();
                        break;
                    case KeyCode.Tab: {
                        Event.current.Use();
                        if (matches.Count == 0) {
                            index = -1;
                            FindCompletions();
                            break;
                        }
                        var offset = Event.current.modifiers.HasFlag(EventModifiers.Shift) ? -1 : 1;
                        index = (index + matches.Count + offset) % matches.Count;
                        var completion = matches[index];
                        input = completion + ' ';
                        if (Commands.IsVariable(completion))
                            try {
                                input += CommandLine.Format(Commands.Invoke(completion, Array.Empty<object>(), Enumerable.Empty<KeyValuePair<string, object>>()));
                            }
                            catch (MultipleObjectsException e) {
                                input += CommandLine.Format(e.values.Last());
                            }
                            catch (Exception) {
                                // ignored
                            }
                        action = () => {
                            var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                            textEditor.cursorIndex = 999;
                            textEditor.selectIndex = completion.Length + 1;
                        };
                        break;
                    }
                }
            }
            if (Event.current.type == EventType.Repaint && action != null) {
                action();
                action = null;
            }
            var oldInput = input;
            var rect = GUILayoutUtility.GetRect(new GUIContent(input), GUI.skin.textField, GUILayout.Width(Screen.width));
            input = GUI.TextField(rect, input);
            GUI.Label(rect, SyntaxHighlighting.Colorize(input));
            if (input != oldInput) {
                CommandHistory.SetText(input);
                index = -1;
                FindCompletions();
            }

            var prefix = string.Format(prefixFormat, index + 1, matches.Count);
            var prefixMaxLength = string.Format(prefixFormat, matches.Count, matches.Count).Length;

            var (low, high) = SlidingWindow(matches.Count, index - radius, index + radius);
            for (var i = low; i <= high; i++) {
                var name = matches[i];
                var info = "";
                if (Commands.IsVariable(name))
                    try {
                        info =  SyntaxHighlighting.Colorize(CommandLine.Format(Commands.Invoke(name, Array.Empty<object>(), Enumerable.Empty<KeyValuePair<string, object>>())));
                    }
                    catch (MultipleObjectsException e) {
                        info = "[" + string.Join(", ", e.values.Take(maximumMultipleObjects).Select(value => SyntaxHighlighting.Colorize(CommandLine.Format(value))));
                        if (e.values.Length > maximumMultipleObjects)
                            info += "...";
                        info += "]";
                    }
                    catch (Exception e) {
                        while (e.InnerException != null)
                            e = e.InnerException;
                        info = $"<i>{e.Message.Split('\n')[0]}</i>";
                        //throw;
                    }
                name = SyntaxHighlighting.Colorize(name);
                GUILayout.Label(index == i
                    ? prefix.PadRight(prefixMaxLength) + $"<b>{name}</b> {info}"
                    : new string(' ', prefixMaxLength) + $"{name} {info}");
            }
        }

        // todo: add completion at cursor position
        private void FindCompletions() {
            var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            //Debug.Log(textEditor.cursorIndex);
            var pattern = string.Join("[^.]*", input.Select(c => Regex.Escape(c.ToString()))) + "[^.]*$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            matches.Clear();
            matches.AddRange(Commands.Names
                .Where(name => regex.IsMatch(name))
                .OrderBy(name => Levenshtein.Distance(input, name))
                .ThenBy(name => name));
        }

        public static (int low, int high) SlidingWindow(int count, int low, int high) {
            if (low < 0 && high >= count) {
                low = 0;
                high = count - 1;
            }
            else if (low < 0) {
                high = Mathf.Min(count - 1, high - low);
                low = 0;
            }
            else if (high >= count) {
                low = Mathf.Max(0, low - high + count - 1);
                high = count - 1;
            }
            return (low, high);
        }

        private void OnApplicationQuit() {
            CommandHistory.Save();
        }
    }
}