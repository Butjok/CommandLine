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
            if (s.Length == 0) return s;
            return char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1);
        }
    }
}