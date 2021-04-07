using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Butjok {

    public static class TokenGenerator {

        [MenuItem("Window/Command Line/Generate tokens")]
        private static void Regenerate() {

            var paths = AssetDatabase.FindAssets("")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith("/Butjok/TokenInfos.cs"))
                .ToList();
            Assert.That(paths.Count == 1, paths.Count.ToString);

            var tokenTypes = CommandLineLexer.ruleNames
                .Select((name, value) => (
                    Name: name,
                    Value: value,
                    SymbolicName: CommandLineLexer.DefaultVocabulary.GetSymbolicName(value),
                    Literal: CommandLineLexer.DefaultVocabulary.GetLiteralName(value)))
                .Where(tuple => tuple.SymbolicName != null)
                .OrderBy(tuple => tuple.Name);

            var code =
                $@"/*
 * Generated automatically from Antlr lexer class. See Window > Command Line > Generate tokens.
 */

using System.Collections.Generic;

namespace Butjok {{

    public readonly struct TokenInfo {{
        public readonly string Name;
        public readonly int Type;
        public readonly string Literal;
        public TokenInfo(string name, int type, string literal) {{
            Name = name;
            Type = type;
            Literal = literal;
        }}
    }}

    public static class TokenInfos {{
        public static IReadOnlyList<TokenInfo> Infos = new List<TokenInfo> {{
            new TokenInfo(""BlockComment"", 19, null),
{string.Join("\n", tokenTypes.Select(t => $"                new TokenInfo(\"{t.Name}\", {t.Value}, {t.Literal?.Replace("'", "\"") ?? "null"}),"))}
        }};
    }}
}}";
            File.WriteAllText(paths[0], code);
            //AssetDatabase.Refresh();

            Debug.Log("Successfully regenerated.");
        }
    }
}