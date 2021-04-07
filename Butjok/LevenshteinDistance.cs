using System;
using UnityEngine;

namespace Butjok {

    public static class LevenshteinDistance {

        private const int MaxTokenLength = 1000;
        private static readonly int[,] Distance = new int[MaxTokenLength + 1, MaxTokenLength + 1];
        
        public static int Compute(string a, string b, Func<char, char, bool> areEqual, int[,] distance = null) {
            Assert.That(areEqual != null);

            distance ??= Distance;
            var maxLength = (a: distance.GetLength(0) - 1, b: distance.GetLength(1) - 1);
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