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
        private readonly IColorizer _colorizer;
        private readonly ICommands _commands;
        private readonly Action<ICompletion> _onSelect;
        private readonly GUIStyle _style, _overlayStyle, _infoStyle, _selectedStyle, _selectedOverlayStyle;
        private readonly UnityEngine.GUIContent _info = new UnityEngine.GUIContent();
        private readonly string _infoFormat = "[{0}/{1}]";
        private Vector2Int _previousInfoData = new Vector2Int(-1, -1);
        private readonly List<GUIContent> _pool = new List<GUIContent>();
        private readonly Texture _keywordIcon, _procedureIcon, _variableIcon;

        public CompletionsDrawer(ColorTheme colorTheme, ICommands commands,
            GUIStyle style, GUIStyle overlayStyle, GUIStyle infoStyle,
            GUIStyle selectedStyle, Action<ICompletion> onSelect, GUIStyle selectedOverlayStyle,
            Texture keywordIcon, Texture procedureIcon, Texture variableIcon, IColorizer colorizer) {

            Assert.That(commands != null);

            Assert.That(style != null);
            Assert.That(overlayStyle != null);
            Assert.That(infoStyle != null);
            Assert.That(selectedStyle != null);
            Assert.That(selectedOverlayStyle != null);

            Assert.That(onSelect != null);

            _commands = commands;

            _style = style;
            _overlayStyle = overlayStyle;
            _infoStyle = infoStyle;
            _selectedStyle = selectedStyle;
            _selectedOverlayStyle = selectedOverlayStyle;
            _keywordIcon = keywordIcon;
            _procedureIcon = procedureIcon;
            _variableIcon = variableIcon;
            _colorizer = colorizer;

            _onSelect = onSelect;
        }

        public void ClearCache() {
            _pool.AddRange(_cache.Values);
            _cache.Clear();
        }

        private GUIContent MakeCompletion(ICompletion completion) {

            GUIContent guiContent;
            if (_pool.Count > 0) {
                guiContent = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
                guiContent = new GUIContent();

            _sb.Clear();
            _sb.Append(completion.Word);

            var isCommand = _commands.Exists(completion.Word);
            var isVariable = isCommand && _commands.IsVariable(completion.Word);
            if (isCommand) {
                if (isVariable) {
                    guiContent.image = _variableIcon;
                    _sb.Append(' ');
                    _sb.Append(Format.Value(_commands.GetValue(completion.Word)));
                }
                else
                    guiContent.image = _procedureIcon;
            }
            else
                guiContent.image = _keywordIcon;

            _colorizer.Colorize(_sb.ToString(), out var richText, out guiContent.underscores);

            _sb.Clear();
            _sb.Append(richText);

            if (isCommand && !isVariable) {
                var arguments = _commands.GetArguments(completion.Word);
                if (arguments != null)
                    foreach (var argument in arguments) {
                        _sb.Append(' ');
                        _sb.Append(argument);
                    }
            }

            if (isCommand) {
                var helpText = _commands.GetHelpText(completion.Word);
                if (helpText != null) {
                    _sb.Append(" - ");
                    _sb.Append(helpText.Capitalise().Dot());
                }
            }

            guiContent.text = _sb.ToString();

            _cache.Add(completion.Word, guiContent);
            return guiContent;
        }

        public void Draw(IReadOnlyList<ICompletion> completions, (int Backward, int Forward) radius, int index,
            float startY) {

            if (completions.Count <= 0)
                return;

            var y = startY;
            var (low, high) = Math.SlidingWindow(completions.Count, radius, index);
            for (var i = low; i <= high; i++) {

                var completion = completions[i];
                if (!_cache.TryGetValue(completion.Word, out var content))
                    content = MakeCompletion(completion);

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