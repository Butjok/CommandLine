/*
 * Generated automatically from Antlr lexer class. See Window > Command Line > Regenerate tokens.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Butjok {
    public enum TokenType {
        BlockComment = 19,
        Color = 7,
        Comma = 5,
        DoubleQuotedString = 12,
        False = 9,
        Identifier = 13,
        Integer = 10,
        LeftParenthesis = 3,
        LongHexRgbaColor = 17,
        LongHexRgbColor = 16,
        Null = 1,
        ProcedureCommand = -3,
        Real = 11,
        RightParenthesis = 4,
        Semicolon = 2,
        ShortHexRgbaColor = 15,
        ShortHexRgbColor = 14,
        SingleLineComment = 18,
        True = 8,
        Unknown = -1,
        UnknownCommand = -2,
        VariableCommand = -4,
        Vector2Int = 6,
        Whitespace = 20,
    }

    public static class TokenLiterals {
        public static readonly Dictionary<TokenType, string> Map = new Dictionary<TokenType, string> {
            [TokenType.BlockComment] = null,
            [TokenType.Color] = "Color",
            [TokenType.Comma] = ",",
            [TokenType.DoubleQuotedString] = null,
            [TokenType.False] = "false",
            [TokenType.Identifier] = null,
            [TokenType.Integer] = null,
            [TokenType.LeftParenthesis] = "(",
            [TokenType.LongHexRgbaColor] = null,
            [TokenType.LongHexRgbColor] = null,
            [TokenType.Null] = "null",
            [TokenType.ProcedureCommand] = null,
            [TokenType.Real] = null,
            [TokenType.RightParenthesis] = ")",
            [TokenType.Semicolon] = ";",
            [TokenType.ShortHexRgbaColor] = null,
            [TokenType.ShortHexRgbColor] = null,
            [TokenType.SingleLineComment] = null,
            [TokenType.True] = "true",
            [TokenType.Unknown] = null,
            [TokenType.UnknownCommand] = null,
            [TokenType.VariableCommand] = null,
            [TokenType.Vector2Int] = "Vector2Int",
            [TokenType.Whitespace] = null,
        }; 
    }

    public partial class CommandLine {
        [SerializeField] private List<TokenStyle> tokenStylesList = new List<TokenStyle> {
            new TokenStyle(TokenType.BlockComment, description: nameof(TokenType.BlockComment)),
            new TokenStyle(TokenType.Color, description: nameof(TokenType.Color)),
            new TokenStyle(TokenType.Comma, description: nameof(TokenType.Comma)),
            new TokenStyle(TokenType.DoubleQuotedString, description: nameof(TokenType.DoubleQuotedString)),
            new TokenStyle(TokenType.False, description: nameof(TokenType.False)),
            new TokenStyle(TokenType.Identifier, description: nameof(TokenType.Identifier)),
            new TokenStyle(TokenType.Integer, description: nameof(TokenType.Integer)),
            new TokenStyle(TokenType.LeftParenthesis, description: nameof(TokenType.LeftParenthesis)),
            new TokenStyle(TokenType.LongHexRgbaColor, description: nameof(TokenType.LongHexRgbaColor)),
            new TokenStyle(TokenType.LongHexRgbColor, description: nameof(TokenType.LongHexRgbColor)),
            new TokenStyle(TokenType.Null, description: nameof(TokenType.Null)),
            new TokenStyle(TokenType.ProcedureCommand, description: nameof(TokenType.ProcedureCommand)),
            new TokenStyle(TokenType.Real, description: nameof(TokenType.Real)),
            new TokenStyle(TokenType.RightParenthesis, description: nameof(TokenType.RightParenthesis)),
            new TokenStyle(TokenType.Semicolon, description: nameof(TokenType.Semicolon)),
            new TokenStyle(TokenType.ShortHexRgbaColor, description: nameof(TokenType.ShortHexRgbaColor)),
            new TokenStyle(TokenType.ShortHexRgbColor, description: nameof(TokenType.ShortHexRgbColor)),
            new TokenStyle(TokenType.SingleLineComment, description: nameof(TokenType.SingleLineComment)),
            new TokenStyle(TokenType.True, description: nameof(TokenType.True)),
            new TokenStyle(TokenType.Unknown, description: nameof(TokenType.Unknown)),
            new TokenStyle(TokenType.UnknownCommand, description: nameof(TokenType.UnknownCommand)),
            new TokenStyle(TokenType.VariableCommand, description: nameof(TokenType.VariableCommand)),
            new TokenStyle(TokenType.Vector2Int, description: nameof(TokenType.Vector2Int)),
            new TokenStyle(TokenType.Whitespace, description: nameof(TokenType.Whitespace)),
        };
    }
}