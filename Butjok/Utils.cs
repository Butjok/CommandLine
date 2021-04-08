using UnityEngine;

namespace Butjok {

    public static class Swap {
        public static void Values<T>(ref T a, ref T b) {
            var temp = a;
            a = b;
            b = temp;
        }
    }

    public static class StringExtensions {
        public static string Capitalise(this string s) {
            if (s == null) return null;
            if (s.Length == 0) return "";
            return char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1);
        }
        public static string Dot(this string s) {
            if (s == null) return null;
            if (s.Length == 0) return "";
            var trimmed = s.TrimEnd();
            var trim = s.Substring(trimmed.Length, s.Length - trimmed.Length);
            if (trimmed.Length == 0 || trimmed[trimmed.Length - 1] == '.')
                return s;
            return trimmed + '.' + trim;
        }
    }

    public static class Math {

        public static int PositiveMod(int a, int b) {
            Assert.That(b > 0);
            return (a % b + b) % b;
        }

        public static (int Low, int High) SlidingWindow(int elementsCount, (int Backward, int Forward) radius,
            int centerIndex) {
            Assert.That(centerIndex >= 0);
            Assert.That(centerIndex < elementsCount);
            Assert.That(radius.Backward >= 0);
            Assert.That(radius.Forward >= 0);

            var low = centerIndex - radius.Backward;
            var high = centerIndex + radius.Forward;

            if (low < 0 && high >= elementsCount)
                return (0, elementsCount - 1);
            if (low >= 0 && high < elementsCount)
                return (low, high);

            return low < 0
                ? (0, Mathf.Min(elementsCount - 1, high - low))
                : (Mathf.Max(0, low - high + elementsCount - 1), elementsCount - 1);
        }
    }
}