using System;
using UnityEngine;

namespace Butjok {

    public static class LevenshteinDistance {

        private const int MaxTokenLength = 1000;
        private static readonly int[,] Distance = new int[MaxTokenLength + 1, MaxTokenLength + 1];

        public static int Compute(string a, string b, bool matchCase) {

            var maxLength = (a: Distance.GetLength(0) - 1, b: Distance.GetLength(1) - 1);
            Assert.That(maxLength.a >= 0);
            Assert.That(maxLength.b >= 0);

            Assert.That(a != null);
            Assert.That(b != null);

            // If strings are longer than we can handle - just return int.MaxValue.
            if (a.Length > maxLength.a || b.Length > maxLength.b)
                return int.MaxValue;

            if (a.Length == 0)
                return b.Length;
            if (b.Length == 0)
                return a.Length;

            for (var i = 0; i <= a.Length; Distance[i, 0] = i++) {}
            for (var j = 0; j <= b.Length; Distance[0, j] = j++) {}

            var areEqual = matchCase ? (Func<char, char, bool>) MatchCase : IgnoreCase;
            for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++) {
                var cost = areEqual(b[j - 1], a[i - 1]) ? 0 : 1;
                Distance[i, j] = Mathf.Min(
                    Mathf.Min(Distance[i - 1, j] + 1, Distance[i, j - 1] + 1),
                    Distance[i - 1, j - 1] + cost);
            }
            return Distance[a.Length, b.Length];
        }
        private static bool MatchCase(char a, char b) {
            return a == b;
        }
        private static bool IgnoreCase(char a, char b) {
            return char.ToUpperInvariant(a) == char.ToUpperInvariant(b);
        }
    }
}