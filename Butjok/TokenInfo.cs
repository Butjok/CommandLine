using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {

    public interface ITokenInfo {
        string Name { get; }
        int Type { get; }
        string LiteralName { get; }
        bool IsKeyword { get; }
    }

    public readonly struct TokenInfo : ITokenInfo {

        public string Name { get; }
        public int Type { get; }
        public string LiteralName { get; }
        public bool IsKeyword { get; }

        private static readonly CommandLineLexer Lexer = new CommandLineLexer(null);

        public static readonly Regex IdentifierRegex;
        public static IReadOnlyDictionary<int, ITokenInfo> All;

        public TokenInfo(string name, int type, string literalName) {
            Assert.That(name != null);

            Name = name;
            Type = type;
            LiteralName = literalName;
            IsKeyword = false;
            IsKeyword = literalName != null && IdentifierRegex.IsMatch(literalName);
        }

        static TokenInfo() {

            IdentifierRegex = new Regex(
                @"^ \s* [a-zA-Z_][a-zA-Z_0-9]* (?: \. [a-zA-Z_][a-zA-Z_0-9]* )* \s* $",
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            All = CommandLineLexer.ruleNames
                .Select((name, type) => (ITokenInfo) new TokenInfo(name, type,
                    CommandLineLexer.DefaultVocabulary.GetLiteralName(type)?.Trim('\'')))
                .Where(info => info.Name != null)
                .ToDictionary(info => info.Type, info => info);
        }
    }
}