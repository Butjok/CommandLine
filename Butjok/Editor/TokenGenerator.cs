using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/*
 * These enums are only used in Settings class.
 * They are used to display a proper drop list of command types to assign colors etc.
 */

namespace Butjok {

    public static class TokenGenerator {

        [MenuItem("Window/Command Line/Generate default style")]
        private static void Regenerate() {

            var paths = AssetDatabase.FindAssets("")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith("/Butjok/StyleProvider.ResetToDefaults.cs"))
                .ToList();
            Assert.That(paths.Count == 1, paths.Count.ToString);

            var tokenTypes = CommandLineLexer.ruleNames
                .Select((name, value) => (
                    Name: name,
                    Value: value,
                    SymbolicName: CommandLineLexer.DefaultVocabulary.GetSymbolicName(value)))
                .Where(tuple => tuple.SymbolicName != null)
                .ToDictionary(tuple => tuple.SymbolicName, tuple => tuple.Value);

            var code =
                $@"/*
 * Generated automatically from Antlr lexer class. See Window > Command Line > Regenerate styles.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Butjok {{
    public partial class StyleProvider {{

        [ContextMenu(""Reset To Defaults"")]
        public void ResetToDefaults() {{

            @default = new TokenStyle();
            error = new TokenStyle {{color = Color.red}};
            unknownCommand = new TokenStyle();
            procedureCommand = new TokenStyle {{color = Color.green}};
            variableCommand = new TokenStyle {{color = Color.yellow}};

            styles = new List<TokenStyle> {{
{string.Join("\n", tokenTypes.OrderBy(pair => pair.Key).Select(pair => $"                new TokenStyle{{name = \"{pair.Key}\", type = {pair.Value}}},"))}
            }};
        }}
    }}
}}";
            File.WriteAllText(paths[0], code);
            //AssetDatabase.Refresh();

            Debug.Log("Successfully regenerated.");
        }
    }
}