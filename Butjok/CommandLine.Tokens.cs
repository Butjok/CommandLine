/*
 * Generated automatically from Antlr lexer class. See Window > Regenerate code.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Butjok {
    public  enum TokenType {
        Semicolon = 1,
        LeftParenthesis = 2,
        RightParenthesis = 3,
        Comma = 4,
        Vector2Int = 5,
        Color = 6,
        True = 7,
        False = 8,
        Integer = 9,
        Real = 10,
        DoubleQuotedString = 11,
        Name = 12,
        ShortHexRgbColor = 13,
        ShortHexRgbaColor = 14,
        LongHexRgbColor = 15,
        LongHexRgbaColor = 16,
        SingleLineComment = 17,
        BlockComment = 18,
        Whitespace = 19,
        Unknown = -1,
        UnknownCommand = -2,
        ProcedureCommand = -3,
        VariableCommand = -4,
    }

    public static class TokenLiterals {
        public static readonly HashSet<string> Values = new HashSet<string> {";", "(", ")", ",", "Vector2Int", "Color", "true", "false"};
    }

    public partial class CommandLine {
        [SerializeField] private List<TokenStyle> tokenStylesList = new List<TokenStyle> {
            new TokenStyle(TokenType.Semicolon, description: nameof(TokenType.Semicolon)),
            new TokenStyle(TokenType.LeftParenthesis, description: nameof(TokenType.LeftParenthesis)),
            new TokenStyle(TokenType.RightParenthesis, description: nameof(TokenType.RightParenthesis)),
            new TokenStyle(TokenType.Comma, description: nameof(TokenType.Comma)),
            new TokenStyle(TokenType.Vector2Int, description: nameof(TokenType.Vector2Int)),
            new TokenStyle(TokenType.Color, description: nameof(TokenType.Color)),
            new TokenStyle(TokenType.True, description: nameof(TokenType.True)),
            new TokenStyle(TokenType.False, description: nameof(TokenType.False)),
            new TokenStyle(TokenType.Integer, description: nameof(TokenType.Integer)),
            new TokenStyle(TokenType.Real, description: nameof(TokenType.Real)),
            new TokenStyle(TokenType.DoubleQuotedString, description: nameof(TokenType.DoubleQuotedString)),
            new TokenStyle(TokenType.Name, description: nameof(TokenType.Name)),
            new TokenStyle(TokenType.ShortHexRgbColor, description: nameof(TokenType.ShortHexRgbColor)),
            new TokenStyle(TokenType.ShortHexRgbaColor, description: nameof(TokenType.ShortHexRgbaColor)),
            new TokenStyle(TokenType.LongHexRgbColor, description: nameof(TokenType.LongHexRgbColor)),
            new TokenStyle(TokenType.LongHexRgbaColor, description: nameof(TokenType.LongHexRgbaColor)),
            new TokenStyle(TokenType.SingleLineComment, description: nameof(TokenType.SingleLineComment)),
            new TokenStyle(TokenType.BlockComment, description: nameof(TokenType.BlockComment)),
            new TokenStyle(TokenType.Whitespace, description: nameof(TokenType.Whitespace)),
            new TokenStyle(TokenType.Unknown, description: nameof(TokenType.Unknown)),
            new TokenStyle(TokenType.UnknownCommand, description: nameof(TokenType.UnknownCommand)),
            new TokenStyle(TokenType.ProcedureCommand, description: nameof(TokenType.ProcedureCommand)),
            new TokenStyle(TokenType.VariableCommand, description: nameof(TokenType.VariableCommand)),
        };
    }
}