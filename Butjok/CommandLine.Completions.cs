using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {
    public partial class CommandLine {

        public static IToken FindToken(IList<IToken> tokens, int cursor, TokenType type) {
            Check.That(tokens != null);
            Check.That(cursor >= 0);

            foreach (var token in tokens) {
                if (token.StartIndex > cursor)
                    break;
                if (token.Type == (int) type && token.StartIndex <= cursor && cursor <= token.StopIndex + 1)
                    return token;
            }
            return null;
        }
        public IEnumerable<string> FindCompletions(IList<IToken>tokens, int cursor, int[,] levenshteinContext, out IToken token) {
            Check.That(tokens != null);
            Check.That(cursor >= 0);
            Check.That(levenshteinContext != null);

            token = FindToken(tokens, cursor, TokenType.Name);
            if (token == null)
                return Enumerable.Empty<string>();

            var tokenText = token.Text;
            return _commands.Keys
                .Where(name => Match(tokenText, name))
                .OrderBy(name => Levenshtein(levenshteinContext, tokenText, name, MatchCase))
                .ThenBy(name => Levenshtein(levenshteinContext, tokenText, name, IgnoreCase))
                .ThenBy(name => name);
        }
        public static string Replace(string text, IToken token, string replacement) {
            Check.That(text != null);
            Check.That(token != null);
            Check.That(token.StartIndex >= 0);
            Check.That(token.StopIndex < text.Length);
            Check.That(replacement != null);

            return text.Substring(0, token.StartIndex) +
                   replacement +
                   text.Substring(token.StopIndex + 1, text.Length - token.StopIndex - 1);
        }
        public static bool Match(string text, string completionText) {
            var offset = 0;
            foreach (var character in text) {
                var offset0 = completionText.IndexOf(char.ToLowerInvariant(character), offset);
                var offset1 = completionText.IndexOf(char.ToUpperInvariant(character), offset);
                if (offset0 == -1 && offset1 == -1)
                    return false;
                if (offset1 == -1)
                    offset = offset0 + 1;
                else if (offset0 == -1)
                    offset = offset1 + 1;
                else
                    offset = Mathf.Min(offset0, offset1) + 1;
            }
            return true;
        }
        public static int Levenshtein(int[,] distance, string a, string b, Func<char, char, bool> areEqual) {
            Check.That(distance != null);

            var maxLength = (a: distance.GetLength(0) - 1, b: distance.GetLength(1) - 1);
            Check.That(maxLength.a >= 0);
            Check.That(maxLength.b >= 0);

            Check.That(a != null);
            Check.That(b != null);

            // If strings are longer than we can handle - just return int.MaxValue.
            if (a.Length > maxLength.a || b.Length > maxLength.b)
                return int.MaxValue;

            if (a.Length == 0)
                return b.Length;
            if (b.Length == 0)
                return a.Length;

            for (var i = 0; i <= a.Length; distance[i, 0] = i++) {}
            for (var j = 0; j <= b.Length; distance[0, j] = j++) {}

            for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++) {
                var cost = areEqual(b[j - 1], a[i - 1]) ? 0 : 1;
                distance[i, j] = Mathf.Min(
                    Mathf.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
            return distance[a.Length, b.Length];
        }
        public static bool MatchCase(char a, char b) {
            return a == b;
        }
        public static bool IgnoreCase(char a, char b) {
            return char.ToUpperInvariant(a) == char.ToUpperInvariant(b);
        }
    }
}