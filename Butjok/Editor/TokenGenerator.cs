using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*
 * These enums are only used in Settings class.
 * They are used to display a proper drop list of command types to assign colors etc.
 */

namespace Butjok {

    public static class TokenGenerator {

        [MenuItem("Window/Command Line/Regenerate tokens")]
        private static void Regenerate() {

            var paths = AssetDatabase.FindAssets("")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith("/CommandLine.Tokens.cs"))
                .ToList();
            Check.That(paths.Count == 1, paths.Count.ToString);

            var lexerType = typeof(CommandLineLexer);
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var tokenTypes = ((string[]) lexerType.GetField("_SymbolicNames", bindingFlags).GetValue(null))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToDictionary(name => name, name => (int) lexerType.GetField(name, bindingFlags).GetValue(null));
            tokenTypes.Add("Unknown", -1);
            tokenTypes.Add("UnknownCommand", -2);
            tokenTypes.Add("ProcedureCommand", -3);
            tokenTypes.Add("VariableCommand", -4);

            var code =
                $@"/*
 * Generated automatically from Antlr lexer class. See Window > Command Line > Regenerate tokens.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Butjok {{
    public enum TokenType {{
{string.Join("\n", tokenTypes.OrderBy(pair => pair.Key).Select(pair => $"        {pair.Key} = {pair.Value},"))}
    }}

    public static class TokenLiterals {{
        public static readonly Dictionary<TokenType, string> Map = new Dictionary<TokenType, string> {{
{string.Join("\n", tokenTypes.OrderBy(pair => pair.Key).Select(pair=> $"            [TokenType.{pair.Key}] = {CommandLineLexer.DefaultVocabulary.GetLiteralName(pair.Value)?.Replace("'", "\"") ?? "null"},"))}
        }}; 
    }}

    public partial class CommandLine {{
        [SerializeField] private List<TokenStyle> tokenStylesList = new List<TokenStyle> {{
{string.Join("\n", tokenTypes.OrderBy(pair => pair.Key).Select(pair=> $"            new TokenStyle(TokenType.{pair.Key}, description: nameof(TokenType.{pair.Key})),"))}
        }};
    }}
}}";
            File.WriteAllText(paths[0], code);
            //AssetDatabase.Refresh();

            Debug.Log("Successfully regenerated.");
        }
    }
}