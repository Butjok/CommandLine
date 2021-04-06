using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Tree.Xpath;
using UnityEngine;

namespace Butjok {
    public partial class CommandLine {

        public IEnumerable<string> FindCompletions(string text,
            IReadOnlyList<(TokenType type, int start, int stop)> tokens, int cursor, int[,] levenshteinContext) {
            Check.That(text != null);
            Check.That(tokens != null);
            Check.That(cursor >= 0);
            Check.That(cursor <= text.Length);
            Check.That(levenshteinContext != null);

            var cursorIdentifierTokenIndex = -1;
            for (var i = 0; i < tokens.Count; i++) {
                var token = tokens[i];
                if (token.start > cursor)
                    break;
                if (token.start <= cursor && cursor <= token.stop + 1 &&
                    token.type == TokenType.UnknownCommand ||
                    token.type == TokenType.ProcedureCommand ||
                    token.type == TokenType.VariableCommand) {
                    
                    cursorIdentifierTokenIndex = i;
                    break;
                }
            }

            var allNames = _commands.Keys.Concat(TokenStyles.Values
                .Where(style => style.IsKeyword)
                .Select(style => TokenLiterals.Map[style.Type]));
            
            IEnumerable<string> completions;

            // If identifier token index is -1, it means the cursor is not over the identifier token to complete.
            // Complete with all possible completions then.

            if (cursorIdentifierTokenIndex == -1)
                completions = allNames.Select(completion => text.Insert(cursor, completion));

            else {
                var token = tokens[cursorIdentifierTokenIndex];
                var tokenText = text.Substring(
                    token.start,
                    token.stop - token.start + 1);

                completions = allNames
                    .Where(name => Match(tokenText, name))
                    .Select(name => text.Substring(0, token.start) +
                                    name +
                                    text.Substring(token.stop + 1, text.Length - token.stop - 1));
            }

            return completions
                .OrderBy(completion => Levenshtein(_levenshtein, text, completion, MatchCase))
                .ThenBy(completion => Levenshtein(_levenshtein, text, completion, IgnoreCase))
                .ThenBy(completion => completion);
        }

        public static string Replace(string text, (TokenType type, int start, int stop) token, string replacement) {
            Check.That(text != null);
            Check.That(token.start >= 0);
            Check.That(token.start <= token.stop);
            Check.That(token.stop < text.Length);
            Check.That(replacement != null);

            return text.Substring(0, token.start) +
                   replacement +
                   text.Substring(token.stop + 1, text.Length - token.stop - 1);
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