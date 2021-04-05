using System.Globalization;
using System.Text;
using UnityEngine;

namespace Butjok {
    public partial class CommandLine {

        private static bool IsHex(char c) {
            return '0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F';
        }
        private static int HexToDec(char hex) {
            if ('0' <= hex && hex <= '9') return hex - '0';
            if ('a' <= hex && hex <= 'f') return 10 + hex - 'a';
            if ('A' <= hex && hex <= 'F') return 10 + hex - 'A';
            throw new CheckException(hex.ToString());
        }
        private static int HexToDec(char hex0, char hex1) {
            Check.That(IsHex(hex0), hex0.ToString);
            Check.That(IsHex(hex1), hex1.ToString);

            return HexToDec(hex0) * 16 + HexToDec(hex1);
        }
        private static Color ParseHexColor(string text) {
            Check.That(!string.IsNullOrWhiteSpace(text));

            var offset = text[0] == '#' ? 1 : 0;
            var length = text.Length - offset;
            switch (length) {
                case 3:
                case 4: {
                    var r = HexToDec(text[offset], text[offset]);
                    var g = HexToDec(text[offset + 1], text[offset + 1]);
                    var b = HexToDec(text[offset + 2], text[offset + 2]);
                    var a = length == 4 ? HexToDec(text[offset + 3], text[offset + 3]) : 255;
                    return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                }
                case 6:
                case 8: {
                    var r = HexToDec(text[offset], text[offset + 1]);
                    var g = HexToDec(text[offset + 2], text[offset + 3]);
                    var b = HexToDec(text[offset + 4], text[offset + 5]);
                    var a = length == 8 ? HexToDec(text[offset + 6], text[offset + 7]) : 255;
                    return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                }
                default:
                    throw new CheckException(text);
            }
        }
        private static bool ParseBoolean(string text) {
            Check.That(!string.IsNullOrWhiteSpace(text));

            switch (text.ToUpperInvariant()) {
                case "TRUE":
                case "YES":
                case "ON":
                case "T":
                case "Y":
                case "+":
                case "1":
                    return true;
                case "FALSE":
                case "NO":
                case "OFF":
                case "F":
                case "N":
                case "-":
                case "0":
                    return false;
                default:
                    throw new CheckException(text);
            }
        }
        private static int ParseInteger(string text) {
            Check.That(!string.IsNullOrWhiteSpace(text));

            return int.Parse(text);
        }
        private static float ParseFloat(string text) {
            Check.That(!string.IsNullOrWhiteSpace(text));

            return float.Parse(text, CultureInfo.InvariantCulture);
        }
        private static string ParseString(string text) {

            Check.That(!string.IsNullOrWhiteSpace(text));
            Check.That(text.Length >= 2);
            Check.That(text[0] == '"');
            Check.That(text[text.Length - 1] == '"');

            var sb = new StringBuilder();
            for (var i = 1; i < text.Length - 1;)
                switch (text[i]) {
                    case '\\':
                        switch (text[i + 1]) {
                            case '\\':
                                sb.Append('\\');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case '"':
                                sb.Append('"');
                                break;
                            default:
                                throw new CheckException(text[i + 1].ToString());
                        }
                        i += 2;
                        break;
                    default:
                        sb.Append(text[i++]);
                        break;
                }
            return sb.ToString();
        }
    }
}