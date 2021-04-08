using System;
using System.Collections.Generic;
using System.Linq;

namespace Butjok {

    [CLSCompliant(false)]
    public class CompletionsManager {

        private readonly TextParser _parser;
        private readonly List<string> _keywords;
        private readonly List<ICompletion> _completions;
        private readonly Func<IEnumerable<string>> _getCommandNames;
        public int Index { get; private set; }

        public CompletionsManager(Func<IEnumerable<string>> getCommandNames) {
            Assert.That(getCommandNames != null);

            _parser = new TextParser();

            _keywords = TokenInfo.All.Values
                .Where(info => info.IsKeyword)
                .Select(info => info.LiteralName)
                .ToList();
            _getCommandNames = getCommandNames;

            _completions = new List<ICompletion>();
            Index = -1;
        }

        public void FindCompletions(string text, int cursor) {
            Assert.That(text != null);
            Assert.That(cursor >= 0, cursor.ToString);
            Assert.That(cursor <= text.Length, (text.Length, cursor).ToString);

            _parser.Parse(text);
            _completions.Clear();
            foreach (var completion in Butjok.Completions.Find(text, _parser.TokenAt, cursor,
                _getCommandNames().Concat(_keywords))) {

                _completions.Add(completion);
            }
        }

        public void NextCompletion(int shift = 1) {

            if (_completions.Count == 0)
                return;

            Index = Index == -1
                ? shift > 0 ? 0 : _completions.Count - 1
                : Math.PositiveMod(Index + shift, _completions.Count);
        }

        public void Clear() {
            _completions.Clear();
            Index = -1;
        }

        public IReadOnlyList<ICompletion> Completions => _completions;
    }
}