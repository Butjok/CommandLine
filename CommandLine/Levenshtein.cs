using System;
using UnityEngine;

namespace Butjok
{
    public static class Levenshtein
    {
        private static int[,] distance;

        public static int Distance(string a, string b, bool ignoreCase = true) {

            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));

            if (distance == null || a.Length >= distance.GetLength(0) || b.Length >= distance.GetLength(1))
                distance = new int[a.Length + 1, b.Length + 1];

            if (a.Length == 0)
                return b.Length;
            if (b.Length == 0)
                return a.Length;

            for (var i = 0; i <= a.Length; distance[i, 0] = i++) { }
            for (var j = 0; j <= b.Length; distance[0, j] = j++) { }

            for (var i = 1; i <= a.Length; i++)
            for (var j = 1; j <= b.Length; j++) {

                var areEqual = ignoreCase ? char.ToUpperInvariant(b[j - 1]) == char.ToUpperInvariant(a[i - 1]) : b[j - 1] == a[i - 1];
                var cost = areEqual ? 0 : 1;

                distance[i, j] = Mathf.Min(
                    Mathf.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
            return distance[a.Length, b.Length];
        }
    }
}