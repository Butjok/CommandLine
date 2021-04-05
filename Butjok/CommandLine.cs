using System;
using System.Reflection;
using Antlr4.Runtime;
using UnityEngine;

namespace Butjok {
    [Serializable]
    public partial class CommandLine : MonoBehaviour, IAntlrErrorListener<int>, IAntlrErrorListener<IToken> {

        [SerializeField] private bool fly;
        
        [SerializeField]private GUISkin skin;
        [SerializeField]private string input = "";
        [SerializeField]private string coloredInput = "";
        [SerializeField] private string underscores = "";
        
        private void Awake() {
            InitializeTokensSettings();
            InitializeCommands();
            
            AddCommand(new Command(CommandType.Field, 
                field:GetType().GetField("fly", BindingFlags.Instance | BindingFlags.NonPublic),
                targetObject: this));
        }

        private void OnGUI() {
            
            GUI.skin = skin;
            input = GUILayout.TextField(input, skin.GetStyle("Input"));

            var tokens = new CommandLineLexer(new AntlrInputStream(input)).GetAllTokens();
        
            HighlightSyntax(input,tokens, out  coloredInput, out  underscores);
            var rect = GUILayoutUtility.GetRect(new GUIContent(coloredInput), skin.GetStyle("ColoredInput"));
            GUI.Label(rect, coloredInput, skin.GetStyle("ColoredInput"));
            GUI.Label(rect, underscores, skin.GetStyle("ColoredInput"));

            /*var state = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor),
                GUIUtility.keyboardControl);
            ;

            StringBuilder.Clear();
            foreach (var completion in CommandLine.FindCompletions(tokens, state.cursorIndex, Distance, out var token)) {
                var replace = CommandLine.Replace(input, token, completion);
                StringBuilder.AppendLine(replace);
            }
            var text = StringBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(text))
                Debug.Log(text);*/
        }
    }
}