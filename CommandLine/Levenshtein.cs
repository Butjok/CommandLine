using System;
using UnityEngine;

namespace Butjok
{
    public static class Levenshtein
    {
        public const int maxLength = 1000;
        private static int[,] distance;

        public static int Distance(string a, string b, bool ignoreCase = true) {

            if (a.Length > maxLength || b.Length > maxLength)
                throw new ArgumentException();

            if (distance == null)
                distance = new int[maxLength, maxLength];

            if (a.Length == 0)
                return b.Length;
            if (b.Length == 0)
                return a.Length;

            for (var i = 0; i <= a.Length; distance[i, 0] = i++) { }
            for (var j = 0; j <= b.Length; distance[0, j] = j++) { }

            for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++) {

                var cost = (ignoreCase
                    ? char.ToUpperInvariant(b[j - 1]) == char.ToUpperInvariant(a[i - 1])
                    : b[j - 1] == a[i - 1])
                    ? 0
                    : 1;

                distance[i, j] = Mathf.Min(
                    Mathf.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
            return distance[a.Length, b.Length];
        }
    }
}