using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Butjok {

    [Serializable]
    public class CompletionsDrawer {

        [Serializable]
        private class GUIContent : UnityEngine.GUIContent {
            public string underscores;
        }

        private readonly StringBuilder _sb;
        private readonly Dictionary<string, GUIContent> _cache;
        [SerializeField] private Texture keywordIcon;
        [SerializeField] private Texture variableIcon;
        [SerializeField] private Texture procedureIcon;
        private TextParser _parser;
        private SyntaxHighlighting _colorizer;
        private Func<string, bool> _exists;
        private Func<string, bool> _isVariable;
        private Func<string, object> _getValue;
        private Func<string, IReadOnlyList<string>> _getArguments;
        private Func<string, string> _getHelpText;
        private Action<Completion> _onSelect;
        [SerializeField] private GUIStyle style;
        [SerializeField] private GUIStyle overlayStyle;
        [SerializeField] private GUIStyle infoStyle;
        [SerializeField] private GUIStyle selectedStyle;
        [SerializeField] private GUIStyle selectedOverlayStyle;
        [SerializeField] private UnityEngine.GUIContent info;
        [SerializeField] private string infoFormat;
        [SerializeField] private Vector2Int previousInfoData;
        [SerializeField] private List<GUIContent> pool;

        public CompletionsDrawer(ColorTheme colorTheme,
            Func<string, bool> exists, Func<string, bool> isVariable, Func<string, object> getValue,
            Func<string, IReadOnlyList<string>> getArguments, Func<string, string> getHelpText,
            GUIStyle style, GUIStyle overlayStyle, GUIStyle infoStyle,
            GUIStyle selectedStyle, Action<Completion> onSelect, GUIStyle selectedOverlayStyle) {

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

            _cache = new Dictionary<string, GUIContent>();
            _sb = new StringBuilder();

            _exists = exists;
            _isVariable = isVariable;
            _getValue = getValue;
            _getArguments = getArguments;
            _getHelpText = getHelpText;

            this.style = style;
            this.overlayStyle = overlayStyle;
            this.infoStyle = infoStyle;
            this.selectedStyle = selectedStyle;
            this.selectedOverlayStyle = selectedOverlayStyle;

            _onSelect = onSelect;

            _parser = new TextParser();
            _colorizer = new SyntaxHighlighting(colorTheme, exists, isVariable);

            info = new UnityEngine.GUIContent();
            infoFormat = "[{0}/{1}]";
            previousInfoData = new Vector2Int(-1, -1);
            pool = new List<GUIContent>();
        }

        public void ClearCache() {
            pool.AddRange(_cache.Values);
            _cache.Clear();
        }

        private GUIContent CacheCompletion(Completion completion) {

            GUIContent guiContent;
            if (pool.Count > 0) {
                guiContent = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
            }
            else
                guiContent = new GUIContent();

            _sb.Clear();
            _sb.Append(completion.Word);

            var isCommand = _exists(completion.Word);
            if (isCommand) {
                if (_isVariable(completion.Word)) {
                    guiContent.image = variableIcon;
                    _sb.Append(' ');
                    _sb.Append(Format.Value(_getValue(completion.Word)));
                }
                else {
                    guiContent.image = procedureIcon;
                    var arguments = _getArguments(completion.Word);
                    if (arguments != null)
                        foreach (var argument in arguments) {
                            _sb.Append(' ');
                            _sb.Append(argument);
                        }
                }
            }
            else
                guiContent.image = keywordIcon;

            var text = _sb.ToString();
            _parser.Parse(text);
            _colorizer.Colorize(text, _parser.Tokens, out var richText, out guiContent.underscores);

            if (isCommand) {
                var helpText = _getHelpText(completion.Word);
                if (helpText != null) richText += " - " + helpText;
            }

            guiContent.text = richText;

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

                var style = index == i ? selectedStyle : this.style;
                var overlayStyle = index == i ? selectedOverlayStyle : this.overlayStyle;
                var size = style.CalcSize(content);
                var rectangle = new Rect(0, y, size.x, size.y);

                GUI.Label(rectangle, content.underscores, overlayStyle);
                if (GUI.Button(rectangle, content, style)) {
                    _onSelect(completion);
                }

                if (index == i) {
                    if (previousInfoData.x != completions.Count || previousInfoData.y != index + 1) {
                        previousInfoData = new Vector2Int(completions.Count, index + 1);
                        info.text = string.Format(infoFormat, index + 1, completions.Count);
                    }

                    var size2 = infoStyle.CalcSize(info);
                    GUI.Label(new Rect(size.x, y, size2.x, size2.y), info, infoStyle);
                }

                y += size.y;
            }
        }
    }
}