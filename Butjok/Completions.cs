using System;
using System.Collections.Generic;
using System.Linq;

namespace Butjok {

    [Serializable]
    public  struct Completion {
        public  string Word;
        public  Func<string> GetSubstituted;
        public  int Cursor;
        public Completion(string word, Func<string> getSubstituted, int cursor) {
            Word = word;
            GetSubstituted = getSubstituted;
            Cursor = cursor;
        }
    }

    public static class Completions {

        public static IEnumerable<Completion>
            Find(string text, Func<int, (bool Found, Token Token)> getTokenAt, int cursor,
                IEnumerable<string> commands) {
            Assert.That(text != null);
            Assert.That(getTokenAt != null);
            Assert.That(cursor >= 0);
            Assert.That(cursor <= text.Length); // Cursor can be at the end of the string.

            if (string.IsNullOrWhiteSpace(text))
                return commands.Select(name => new Completion(name, () => name + ' ', name.Length + 1));

            var (_, next) = cursor == text.Length ? (false, Token.Invalid) : getTokenAt(cursor);
            var space = next.Type == CommandLineLexer.Whitespace ? "" : " ";

            if (cursor == 0)
                return commands.Select(name
                    => new Completion(name, () => name + space + text, name.Length + space.Length));

            var (_, previous) = getTokenAt(cursor - 1);

            if (previous.Type != CommandLineLexer.Whitespace && previous.Type != CommandLineLexer.Identifier)
                return Enumerable.Empty<Completion>();
            
            if (previous.Type == CommandLineLexer.Whitespace) {
                return commands.Select(name
                    => new Completion(name, () => {
                            
                            // Cursor might be in the middle of whitespace token: split the whitespace by cursor position then.
                            var before = text.Substring(0, cursor);
                            var after = text.Substring(cursor, text.Length - cursor);
                            return before + name + space + after;
                        },
                        cursor + name.Length + space.Length));
            }

            if (previous.Type == CommandLineLexer.Identifier) {
                var pattern = text.Substring(previous.Start, previous.Stop - previous.Start + 1);
                return commands
                    .Where(name => FuzzySearch.Match(pattern, name))
                    .OrderBy(name => LevenshteinDistance.Compute(pattern, name, matchCase: true))
                    .ThenBy(name => LevenshteinDistance.Compute(pattern, name, matchCase: false))
                    .Select(name
                        => new Completion(name, () => {
                                var before = text.Substring(0, previous.Start);
                                var after = text.Substring(previous.Stop + 1, text.Length - previous.Stop - 1);
                                return before + name + space + after;
                            },
                            previous.Start + name.Length + space.Length));
            }

            throw new CheckException();
        }
    }
}