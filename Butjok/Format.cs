using System.Globalization;
using System.Text;
using UnityEngine;

namespace Butjok {
    public static class Format {

        public static char DecToHex(int value, char startLetter = 'a') {
            if (0 <= value && value <= 9)
                return (char) ('0' + value);
            if (10 <= value && value <= 15)
                return (char) (startLetter + value - 10);
            throw new CheckException(value.ToString());
        }
        public static void DecToHex(int value, out char hex0, out char hex1, char startLetter = 'a') {
            Check.That(0 <= value && value <= 255, value.ToString);

            hex0 = DecToHex(value >> 4, startLetter);
            hex1 = DecToHex(value & 0xf, startLetter);
        }
        public static string ToHex(Color color, bool alwaysIncludeAlpha = false) {

            var r = Mathf.RoundToInt(color.r * 255);
            var g = Mathf.RoundToInt(color.g * 255);
            var b = Mathf.RoundToInt(color.b * 255);
            var a = Mathf.RoundToInt(color.a * 255);

            DecToHex(r, out var r0, out var r1);
            DecToHex(g, out var g0, out var g1);
            DecToHex(b, out var b0, out var b1);
            DecToHex(a, out var a0, out var a1);

            var canBeShort = r0 == r1 && g0 == g1 && b0 == b1 && a0 == a1;

            var rgb = canBeShort ? $"{r0}{g0}{b0}" : $"{r0}{r1}{g0}{g1}{b0}{b1}";
            return alwaysIncludeAlpha || a != 255 ? rgb + (canBeShort ? $"{a0}" : $"{a0}{a1}") : rgb;
        }
        public static string Boolean(bool value) {
            return value ? "true" : "false";
        }
        public static string Integer(int value) {
            return value.ToString();
        }
        public static string Float(float value) {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }
        public static string String(string text) {
            if (text == null)
                return "null";

            var sb = new StringBuilder("\"");
            foreach (var c in text)
                switch (c) {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            sb.Append('"');
            return sb.ToString();
        }
        public static string Color(Color color) {
            return '#' + ToHex(color);
        }
        public static string Value(object value) {
            return value switch {
                null => "null",
                bool booleanValue => Boolean(booleanValue),
                int integerValue => Integer(integerValue),
                float floatValue => Float(floatValue),
                string stringValue => String(stringValue),
                Color colorValue => Color(colorValue),
                _ => throw new CheckException(value.GetType().ToString())
            };
        }
    }
}