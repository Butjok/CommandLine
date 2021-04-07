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

            var (_, token) = getTokenAt(cursor - 1);

            if (token.Type != CommandLineLexer.Whitespace && token.Type != CommandLineLexer.Identifier)
                return Enumerable.Empty<(string, Func<string>)>();

            var before = text.Substring(0, token.Start);
            var tokenText = text.Substring(token.Start, token.Stop - token.Start + 1);
            var after = text.Substring(token.Stop + 1, text.Length - token.Stop - 1);

            if (token.Type == CommandLineLexer.Whitespace)
                return commands.Select(name => (name, (Func<string>) (() => before + tokenText + name + space + after)));

            if (token.Type == CommandLineLexer.Identifier)
                return commands
                    .Where(name => Fuzzy.Match(tokenText, name))
                    .OrderBy(name => LevenshteinDistance.Compute(tokenText, name, LevenshteinDistance.MatchCase))
                    .ThenBy(name => LevenshteinDistance.Compute(tokenText, name, LevenshteinDistance.IgnoreCase))
                    .Select(name => (name, (Func<string>) (() => before + name + space + after)));

            throw new CheckException();
        }
    }
}