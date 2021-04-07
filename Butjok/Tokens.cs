/*
 * Generated automatically from Antlr lexer class. See Window > Command Line > Generate Tokens.
 */

using System.Collections.Generic;

namespace Butjok {

    public readonly struct TokenInfo {
        public readonly string Name;
        public readonly int Type;
        public readonly string Literal;
        public TokenInfo(string name, int type, string literal) {
            Name = name;
            Type = type;
            Literal = literal;
        }
    }

    public static class Tokens {
        public static IReadOnlyList<TokenInfo> Infos = new List<TokenInfo> {
            new TokenInfo("BlockComment", 19, null),
                new TokenInfo("BlockComment", 20, null),
                new TokenInfo("Color", 6, "Vector2Int"),
                new TokenInfo("Comma", 4, ")"),
                new TokenInfo("DoubleQuotedString", 11, null),
                new TokenInfo("False", 8, "true"),
                new TokenInfo("Hex", 18, null),
                new TokenInfo("Identifier", 12, null),
                new TokenInfo("Integer", 9, "false"),
                new TokenInfo("LeftParenthesis", 2, ";"),
                new TokenInfo("LongHexRgbaColor", 16, null),
                new TokenInfo("LongHexRgbColor", 15, null),
                new TokenInfo("Real", 10, null),
                new TokenInfo("RightParenthesis", 3, "("),
                new TokenInfo("Semicolon", 1, "null"),
                new TokenInfo("ShortHexRgbaColor", 14, null),
                new TokenInfo("ShortHexRgbColor", 13, null),
                new TokenInfo("SingleLineComment", 19, null),
                new TokenInfo("True", 7, "Color"),
                new TokenInfo("Vector2Int", 5, ","),
                new TokenInfo("Word", 17, null),
        };
    }
}