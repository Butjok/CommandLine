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
                return commands.Select(name => new Completion(name, () => name , name.Length));

            if (cursor == 0)
                return commands.Select(name
                    => new Completion(name, () => name  + text, name.Length ));

            var (_, previous) = getTokenAt(cursor - 1);

            if (previous.Type != CommandLineLexer.Whitespace && previous.Type != CommandLineLexer.Identifier)
                return Enumerable.Empty<Completion>();
            
            if (previous.Type == CommandLineLexer.Whitespace) {
                return commands.Select(name
                    => new Completion(name, () => {
                            
                            // Cursor might be in the middle of whitespace token: split the whitespace by cursor position then.
                            var before = text.Substring(0, cursor);
                            var after = text.Substring(cursor, text.Length - cursor);
                            return before + name  + after;
                        },
                        cursor + name.Length ));
            }

            if (previous.Type == CommandLineLexer.Identifier) {
                var pattern = text.Substring(previous.Start, previous.Stop - previous.Start + 1);
                return commands
                    .Where(name => Fuzzy.Match(pattern, name))
                    .OrderBy(name => Levenshtein.Distance(pattern, name, Levenshtein.MatchCase))
                    .ThenBy(name => Levenshtein.Distance(pattern, name, Levenshtein.IgnoreCase))
                    .Select(name
                        => new Completion(name, () => {
                                var before = text.Substring(0, previous.Start);
                                var after = text.Substring(previous.Stop + 1, text.Length - previous.Stop - 1);
                                return before + name + after;
                            },
                            previous.Start + name.Length ));
            }

            throw new CheckException();
        }
    }
}