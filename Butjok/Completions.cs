using System;
using System.Collections.Generic;
using System.Linq;

namespace Butjok {

    public static class Completions {

        public static IEnumerable<(string Name, Func<string> GetSubstituted)>
            Find(string text, Func<int, (bool Found, Token Token)> getTokenAt, int cursor,
                IEnumerable<string> commands) {
            Assert.That(text != null);
            Assert.That(getTokenAt != null);
            Assert.That(cursor >= 0);
            Assert.That(cursor <= text.Length); // Cursor can be at the end of the string.

            if (string.IsNullOrWhiteSpace(text))
                return commands.Select(name => (name, (Func<string>) (() => name + ' ')));

            var (_, next) = cursor == text.Length ? (false, Token.Invalid) : getTokenAt(cursor);
            var space = next.Type == CommandLineLexer.Whitespace ? "" : " ";

            if (cursor == 0)
                return commands.Select(name => (name, (Func<string>) (() => name + space + text)));

            var (_, prev) = getTokenAt(cursor - 1);

            if (prev.Type != CommandLineLexer.Whitespace && prev.Type != CommandLineLexer.Identifier)
                return Enumerable.Empty<(string, Func<string>)>();

            // Cursor might be in the middle of whitespace token: then separate the whitespace by cursor position.
            if (prev.Type == CommandLineLexer.Whitespace) {

                var before = text.Substring(0, cursor);
                var after = text.Substring(cursor, text.Length - cursor);

                return commands.Select(name
                    => (name, (Func<string>) (() => before + name + space + after)));
            }

            if (prev.Type == CommandLineLexer.Identifier) {

                var before = text.Substring(0, prev.Start);
                var pattern = text.Substring(prev.Start, prev.Stop - prev.Start + 1);
                var after = text.Substring(prev.Stop + 1, text.Length - prev.Stop - 1);

                return commands
                    .Where(name => Fuzzy.Match(pattern, name))
                    .OrderBy(name => LevenshteinDistance.Compute(pattern, name, matchCase: true))
                    .ThenBy(name => LevenshteinDistance.Compute(pattern, name, matchCase: false))
                    .Select(name => (name, (Func<string>) (() => before + name + space + after)));
            }

            throw new CheckException();
        }
    }
}