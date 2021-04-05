using System.Linq;
using System.Reflection;
using System.Text;
using Antlr4.Runtime;
using Butjok;
using UnityEngine;

public class Main : MonoBehaviour {
    public CommandLine CommandLine ;
    public GUISkin skin;
    public string input = "";

    private void Awake() {
        
        for(var i=0;i<100;i++)
        CommandLine.AddCommand(new Command(CommandType.Property,
            name: $"    True{i}   ",
            property: typeof(Application).GetProperty(nameof(Application.persistentDataPath),
                BindingFlags.Static | BindingFlags.Public)));

    }

    [ContextMenu("Execute")]
    void Execute() {
        CommandLine.Execute(input);
    }

    public void Lol(Arguments arguments) {
        Debug.Log("lol");
    }

    private static readonly StringBuilder StringBuilder = new StringBuilder();
    private static readonly int[,] Distance = new int[1000, 1000];
    
    private void OnGUI() {
        GUI.skin = skin;
        input = GUILayout.TextField(input, skin.GetStyle("Input"));

        var tokens = new CommandLineLexer(new AntlrInputStream(input)).GetAllTokens();
        
        CommandLine.HighlightSyntax(input,tokens, out var coloredText, out var underscores);
        var rect = GUILayoutUtility.GetRect(new GUIContent(coloredText), skin.GetStyle("ColoredInput"));
        GUI.Label(rect, coloredText, skin.GetStyle("ColoredInput"));
        GUI.Label(rect, underscores, skin.GetStyle("ColoredInput"));

        var state = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor),
            GUIUtility.keyboardControl);
        ;

        StringBuilder.Clear();
        foreach (var completion in CommandLine.FindCompletions(tokens, state.cursorIndex, Distance, out var token)) {
            var replace = CommandLine.Replace(input, token, completion);
            StringBuilder.AppendLine(replace);
        }
        var text = StringBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(text))
            Debug.Log(text);

        //GUILayout.Label("Hello");
    }
}