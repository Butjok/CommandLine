using System;
using System.Collections.Generic;
using System.Linq;

namespace Butjok {

    public interface ICompletion {
        string Word { get; }
        Func<string> GetSubstituted { get; }
        int Cursor { get; }
    }

    public static class Completions {

        private readonly struct Completion : ICompletion {

            public string Word { get; }
            public Func<string> GetSubstituted { get; }
            public int Cursor { get; }

            public Completion(string word, Func<string> getSubstituted, int cursor) {
                Assert.That(word != null);
                Assert.That(getSubstituted != null);
                Assert.That(cursor >= 0);

                Word = word;
                GetSubstituted = getSubstituted;
                Cursor = cursor;
            }
        }

        public static IEnumerable<ICompletion>
            Find(string text, Func<int, (bool Found, IToken Token)> getTokenAt, int cursor,
                IEnumerable<string> commands) {
            Assert.That(text != null);
            Assert.That(getTokenAt != null);
            Assert.That(cursor >= 0);
            Assert.That(cursor <= text.Length); // Cursor can be at the end of the string.

            if (string.IsNullOrWhiteSpace(text))
                return commands.Select(name => (ICompletion) new Completion(name, () => name, name.Length));

            if (cursor == 0)
                return commands.Select(name
                    => (ICompletion) new Completion(name, () => name + text, name.Length));

            var (_, previous) = getTokenAt(cursor - 1);

            if (previous.Type != CommandLineLexer.Whitespace && previous.Type != CommandLineLexer.Identifier)
                return Enumerable.Empty<ICompletion>();

            if (previous.Type == CommandLineLexer.Whitespace) {
                return commands.Select(name
                    => (ICompletion) new Completion(name, () => {

                            // Cursor might be in the middle of whitespace token: split the whitespace by cursor position then.
                            var before = text.Substring(0, cursor);
                            var after = text.Substring(cursor, text.Length - cursor);
                            return before + name + after;
                        },
                        cursor + name.Length));
            }

            if (previous.Type == CommandLineLexer.Identifier) {
                var pattern = previous.FindText();
                return commands
                    .Where(name => Fuzzy.Match(pattern, name))
                    .OrderBy(name => Levenshtein.Distance(pattern, name, Levenshtein.MatchCase))
                    .ThenBy(name => Levenshtein.Distance(pattern, name, Levenshtein.IgnoreCase))
                    .Select(name
                        => (ICompletion) new Completion(name, () => {
                                var before = text.Substring(0, previous.Start);
                                var after = text.Substring(previous.Stop + 1, text.Length - previous.Stop - 1);
                                return before + name + after;
                            },
                            previous.Start + name.Length));
            }

            throw new CheckException();
        }
    }
}