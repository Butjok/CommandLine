using System;
using UnityEngine;

namespace Butjok {

    [Serializable]
    public class InputField {

        [SerializeField] private string controlName;
        private string _text = "";
        private string _richText = "";
        private string _underscores = "";
        private GUIStyle _style, _overlayStyle;
        private SyntaxHighlighting _syntaxHighlighting;
        private TextParser _parser = new TextParser();
        public event Action<(string Old, string New)> Edited;

        public InputField() {
            controlName = "Butjok.CommandLine";
        }
        public InputField(GUIStyle style, GUIStyle overlayStyle, Func<string, bool> exists,
            Func<string, bool> isVariable, ColorTheme colorScheme) {
            Assert.That(style != null);
            Assert.That(overlayStyle != null);
            Assert.That(exists != null);
            Assert.That(isVariable != null);

            _style = style;
            controlName = "Butjok.CommandLine";
            _overlayStyle = overlayStyle;
            _syntaxHighlighting = new SyntaxHighlighting(colorScheme, exists, isVariable);
        }
        public string Text {
            get => _text;
            set {
                Assert.That(value != null);

                _text = value;
                _parser.Parse(_text);
                _syntaxHighlighting.Colorize(_text, _parser.Tokens, out _richText, out _underscores);
            }
        }
        public void Draw(Rect rectangle) {

            GUI.SetNextControlName(controlName);
            var newText = GUI.TextField(rectangle, _text, _style)
                .Replace("`", ""); // TODO: small hack to remove unwanted backquotes (incomplete)
            GUI.Label(rectangle, _richText, _overlayStyle);
            GUI.Label(rectangle, _underscores, _overlayStyle);

            if (_text != newText) {
                var oldText = _text;
                Text = newText;
                Edited?.Invoke((oldText, newText));
            }
        }
        public int Cursor {
            get => ((TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).cursorIndex;
            set {
                var editor = ((TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl));
                editor.cursorIndex = value;
                editor.SelectNone();
            }
        }
    }
}