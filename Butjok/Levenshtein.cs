using System;
using UnityEngine;

namespace Butjok {

    public static class Levenshtein {

        private const int MaxTokenLength = 1000;
        private static readonly int[,] Matrix = new int[MaxTokenLength + 1, MaxTokenLength + 1];

        public static int Distance(string a, string b, Func<char, char, bool> areEqual) {
            Assert.That(a != null);
            Assert.That(b != null);
            Assert.That(areEqual != null);

            // If strings are longer than we can handle - just return int.MaxValue.
            if (a.Length > MaxTokenLength || b.Length > MaxTokenLength)
                return int.MaxValue;

            if (a.Length == 0)
                return b.Length;
            if (b.Length == 0)
                return a.Length;

            for (var i = 0; i <= a.Length; Matrix[i, 0] = i++) {}
            for (var j = 0; j <= b.Length; Matrix[0, j] = j++) {}

            for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++) {
                var cost = areEqual(b[j - 1], a[i - 1]) ? 0 : 1;
                Matrix[i, j] = Mathf.Min(
                    Mathf.Min(Matrix[i - 1, j] + 1, Matrix[i, j - 1] + 1),
                    Matrix[i - 1, j - 1] + cost);
            }
            return Matrix[a.Length, b.Length];
        }
        public static bool MatchCase(char a, char b) {
            return a == b;
        }
        public static bool IgnoreCase(char a, char b) {
            return char.ToUpperInvariant(a) == char.ToUpperInvariant(b);
        }
    }
}