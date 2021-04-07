using System.Collections.Generic;
using System.Linq;

namespace Butjok {

    public readonly struct TokenInfo {
        
        public readonly string Name;
        public readonly int Type;
        public readonly string LiteralName;
        public TokenInfo(string name, int type, string literalName) {
            Name = name;
            Type = type;
            LiteralName = literalName;
        }

        public static IReadOnlyList<TokenInfo> All {
            get {
                return CommandLineLexer.ruleNames
                    .Select((name, type) => new TokenInfo(name, type,
                        CommandLineLexer.DefaultVocabulary.GetLiteralName(type)?.Trim('\'')))
                    .Where(tuple => tuple.Name != null)
                    .OrderBy(tuple => tuple.Name)
                    .ToList();
            }
        }
    }
}