using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Butjok {

    [Serializable]
    public class CompletionsManager {

        private TextParser _parser;
        [SerializeField] private List<string> allWords;
        [SerializeField]private List<Completion> completions;
        [SerializeField] private int index;

        public CompletionsManager(IEnumerable<string> commandNames, ColorTheme colorTheme) {
            Assert.That(commandNames != null);

            _parser = new TextParser();

            var keywords = TokenInfo.All
                .Where(info => colorTheme.Styles.TryGetValue(info.Type, out var tokenStyle) && tokenStyle.IsKeyword)
                .Select(info => info.LiteralName);
            allWords = new List<string>(commandNames.Concat(keywords));

            completions = new List<Completion>();
            index = -1;
        }
        public int FindCompletions(string text, int cursor) {
            _parser.Parse(text);
            completions.Clear();
            foreach (var completion in Butjok.Completions.Find(text, _parser.TokenAt, cursor, allWords)) {
                completions.Add(completion);
            }
            return completions.Count;
        }
        public int NextCompletion(int shift = 1) {
            
            if (completions.Count == 0)
                return -1;
            
            index = index == -1
                ? shift > 0 ? 0 : completions.Count - 1
                : Math.PositiveMod(index + shift, completions.Count);
            
            return index;
        }
        public void Clear() {
            completions.Clear();
            index = -1;
        }
        public int Index => index;
        public IReadOnlyList<Completion> Completions => completions;
    }
}