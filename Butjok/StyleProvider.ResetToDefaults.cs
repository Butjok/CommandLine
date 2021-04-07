/*
 * Generated automatically from Antlr lexer class. See Window > Command Line > Regenerate styles.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Butjok {
    public partial class StyleProvider {

        [ContextMenu("Reset To Defaults")]
        public void ResetToDefaults() {

            @default = new TokenStyle();
            error = new TokenStyle {color = Color.red};
            unknownCommand = new TokenStyle();
            procedureCommand = new TokenStyle {color = Color.green};
            variableCommand = new TokenStyle {color = Color.yellow};

            styles = new List<TokenStyle> {
                new TokenStyle{name = "BlockComment", type = 19},
                new TokenStyle{name = "Color", type = 7},
                new TokenStyle{name = "Comma", type = 5},
                new TokenStyle{name = "DoubleQuotedString", type = 12},
                new TokenStyle{name = "False", type = 9},
                new TokenStyle{name = "Identifier", type = 13},
                new TokenStyle{name = "Integer", type = 10},
                new TokenStyle{name = "LeftParenthesis", type = 3},
                new TokenStyle{name = "LongHexRgbaColor", type = 17},
                new TokenStyle{name = "LongHexRgbColor", type = 16},
                new TokenStyle{name = "Null", type = 1},
                new TokenStyle{name = "Real", type = 11},
                new TokenStyle{name = "RightParenthesis", type = 4},
                new TokenStyle{name = "Semicolon", type = 2},
                new TokenStyle{name = "ShortHexRgbaColor", type = 15},
                new TokenStyle{name = "ShortHexRgbColor", type = 14},
                new TokenStyle{name = "SingleLineComment", type = 18},
                new TokenStyle{name = "True", type = 8},
                new TokenStyle{name = "Vector2Int", type = 6},
                new TokenStyle{name = "Whitespace", type = 20},
            };
        }
    }
}