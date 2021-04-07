using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Butjok {

    [CLSCompliant(false)]
    public class CompletionsDrawer {

        [Serializable]
        private class GUIContent : UnityEngine.GUIContent {
            public string underscores;
        }

        private readonly StringBuilder _sb = new StringBuilder();
        private readonly Dictionary<string, GUIContent> _cache = new Dictionary<string, GUIContent>();

        private readonly TextParser _parser = new TextParser();
        private readonly SyntaxHighlighting _colorizer;
        private readonly Func<string, bool> _exists;
        private readonly Func<string, bool> _isVariable;
        private readonly Func<string, object> _getValue;
        private readonly Func<string, IReadOnlyList<string>> _getArguments;
        private readonly Func<string, string> _getHelpText;
        private readonly Action<Completion> _onSelect;
        private readonly GUIStyle _style, _overlayStyle, _infoStyle, _selectedStyle, _selectedOverlayStyle;
        private readonly UnityEngine.GUIContent _info = new UnityEngine.GUIContent();
        private readonly string _infoFormat = "[{0}/{1}]";
        private Vector2Int _previousInfoData = new Vector2Int(-1, -1);
        private readonly List<GUIContent> _pool = new List<GUIContent>();
        private readonly Texture _keywordIcon, _procedureIcon, _variableIcon;

        public CompletionsDrawer(ColorTheme colorTheme,
            Func<string, bool> exists, Func<string, bool> isVariable, Func<string, object> getValue,
            Func<string, IReadOnlyList<string>> getArguments, Func<string, string> getHelpText,
            GUIStyle style, GUIStyle overlayStyle, GUIStyle infoStyle,
            GUIStyle selectedStyle, Action<Completion> onSelect, GUIStyle selectedOverlayStyle,
            Texture keywordIcon, Texture procedureIcon, Texture variableIcon) {

            Assert.That(exists != null);
            Assert.That(isVariable != null);
            Assert.That(getValue != null);
            Assert.That(getArguments != null);
            Assert.That(getHelpText != null);

            Assert.That(style != null);
            Assert.That(overlayStyle != null);
            Assert.That(infoStyle != null);
            Assert.That(selectedStyle != null);
            Assert.That(selectedOverlayStyle != null);

            Assert.That(onSelect != null);

            _exists = exists;
            _isVariable = isVariable;
            _getValue = getValue;
            _getArguments = getArguments;
            _getHelpText = getHelpText;

            _style = style;
            _overlayStyle = overlayStyle;
            _infoStyle = infoStyle;
            _selectedStyle = selectedStyle;
            _selectedOverlayStyle = selectedOverlayStyle;
            this._keywordIcon = keywordIcon;
            this._procedureIcon = procedureIcon;
            this._variableIcon = variableIcon;

            _onSelect = onSelect;

            _colorizer = new SyntaxHighlighting(colorTheme, exists, isVariable);
        }

        public void ClearCache() {
            _pool.AddRange(_cache.Values);
            _cache.Clear();
        }

        private GUIContent CacheCompletion(Completion completion) {

            GUIContent guiContent;
            if (_pool.Count > 0) {
                guiContent = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
                guiContent = new GUIContent();

            _sb.Clear();
            _sb.Append(completion.Word);

            var isCommand = _exists(completion.Word);
            var isVariable = isCommand && _isVariable(completion.Word);
            if (isCommand) {
                if (isVariable) {
                    guiContent.image = _variableIcon;
                    _sb.Append(' ');
                    _sb.Append(Format.Value(_getValue(completion.Word)));
                }
                else
                    guiContent.image = _procedureIcon;
            }
            else
                guiContent.image = _keywordIcon;

            var text = _sb.ToString();
            _parser.Parse(text);
            _colorizer.Colorize(text, _parser.Tokens, out var richText, out guiContent.underscores);

            _sb.Clear();
            _sb.Append(richText);

            if (isCommand && !isVariable) {
                var arguments = _getArguments(completion.Word);
                if (arguments != null)
                    foreach (var argument in arguments) {
                        _sb.Append(' ');
                        _sb.Append(argument);
                    }
            }

            if (isCommand) {
                var helpText = _getHelpText(completion.Word);
                if (helpText != null) {
                    _sb.Append(" - ");
                    _sb.Append(helpText.Capitalise().Dot());
                }
            }

            guiContent.text = _sb.ToString();

            _cache.Add(completion.Word, guiContent);
            return guiContent;
        }

        public void Draw(IReadOnlyList<Completion> completions, (int Backward, int Forward) radius, int index,
            float startY) {

            if (completions.Count <= 0)
                return;

            var y = startY;
            var (low, high) = Math.SlidingWindow(completions.Count, radius, index);
            for (var i = low; i <= high; i++) {

                var completion = completions[i];
                if (!_cache.TryGetValue(completion.Word, out var content))
                    content = CacheCompletion(completion);

                var style = index == i ? _selectedStyle : _style;
                var overlayStyle = index == i ? _selectedOverlayStyle : _overlayStyle;
                var size = style.CalcSize(content);
                var rectangle = new Rect(0, y, size.x, size.y);

                GUI.Label(rectangle, content.underscores, overlayStyle);
                if (GUI.Button(rectangle, content, style)) {
                    _onSelect(completion);
                }

                if (index == i) {
                    if (_previousInfoData.x != completions.Count || _previousInfoData.y != index + 1) {
                        _previousInfoData = new Vector2Int(completions.Count, index + 1);
                        _info.text = string.Format(_infoFormat, index + 1, completions.Count);
                    }

                    var size2 = _infoStyle.CalcSize(_info);
                    GUI.Label(new Rect(size.x, y, size2.x, size2.y), _info, _infoStyle);
                }

                y += size.y;
            }
        }
    }
}