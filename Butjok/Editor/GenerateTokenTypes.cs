using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

/*
 * These enums are only used in Settings class.
 * They are used to display a proper drop list of command types to assign colors etc.
 */

namespace Butjok {

    public static class EnumGenerator {

        [MenuItem("Window/Command Line/Regenerate code")]
        private static void Regenerate() {

            var paths = AssetDatabase.FindAssets("")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith("/CommandLine.Tokens.cs"))
                .ToList();
            if (paths.Count != 1)
                throw new Exception(paths.Count.ToString());

            var lexerType = typeof(CommandLineLexer);
            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var tokenTypes = ((string[]) lexerType.GetField("_SymbolicNames", bindingFlags).GetValue(null))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToDictionary(name => name, name => lexerType.GetField(name, bindingFlags).GetValue(null));
            tokenTypes.Add("Unknown", -1);
            tokenTypes.Add("UnknownCommand", -2);
            tokenTypes.Add("ProcedureCommand", -3);
            tokenTypes.Add("VariableCommand", -4);

            var literals = ((string[]) lexerType.GetField("_LiteralNames", bindingFlags).GetValue(null))
                .Where(value => !string.IsNullOrWhiteSpace(value));

            var code = 
                $@"/*
 * Generated automatically from Antlr lexer class. See Window > Regenerate code.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Butjok {{
    public  enum TokenType {{
{string.Join("\n", tokenTypes.Select(pair => $"        {pair.Key} = {pair.Value},"))}
    }}

    public static class TokenLiterals {{
        public static readonly HashSet<string> Values = new HashSet<string> {{{string.Join(", ", literals.Select(literal => literal.Replace("'", "\"")))}}};
    }}

    public partial class CommandLine {{
        [SerializeField] private List<TokenStyle> tokenStylesList = new List<TokenStyle> {{
{string.Join("\n", tokenTypes.Keys.Select(name => $"            new TokenStyle(TokenType.{name}, description: nameof(TokenType.{name})),"))}
        }};
    }}
}}";
            File.WriteAllText(paths[0], code);
            AssetDatabase.Refresh();
        }
    }
}