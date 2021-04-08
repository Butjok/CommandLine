using System;
using UnityEngine;

namespace Butjok {

    [CLSCompliant(false)]
    public class InputField {

        private readonly string _controlName;
        private string _text = "";
        private string _richText = "";
        private string _underscores = "";
        private readonly GUIStyle _style;
        private readonly GUIStyle _overlayStyle;
        private readonly IColorizer _colorizer;
        public event Action<(string Old, string New)> Edited;

        public InputField(GUIStyle style, GUIStyle overlayStyle, string controlName, IColorizer colorizer) {
            Assert.That(style != null);
            Assert.That(overlayStyle != null);
            Assert.That(colorizer != null);

            _style = style;
            _controlName = controlName;
            _overlayStyle = overlayStyle;
            _colorizer = colorizer;
        }
        public string Text {
            get => _text;
            set {
                Assert.That(value != null);

                _text = value;
                _colorizer.Colorize(_text, out _richText, out _underscores);
            }
        }
        public void Draw(Rect rectangle) {

            GUI.SetNextControlName(_controlName);
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