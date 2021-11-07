using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Butjok {
    public static class CommandHistory
    {
        private static List<string> lines;
        private static int index;
        private static string input;

        public static string Text => index == lines.Count ? input : lines[index];
        public static void Initialize() {
            input = "";
            lines = new List<string>();
            var serialized = PlayerPrefs.GetString(nameof(CommandHistory), null);
            if (serialized != null) 
                lines.AddRange(serialized.Split('\n'));
            index = lines.Count;
        }
        public static void Save() {
            if (lines.Count > 0) 
                PlayerPrefs.SetString(nameof(CommandHistory), string.Join("\n", lines));
        }
        public static void SetText(string text) {
            index = lines.Count;
            input = text;
        }
        public static bool Lookup(int offset) {
            var oldIndex = index;
            index = Mathf.Clamp(index + offset, 0, lines.Count);
            return oldIndex != index;
        }
        public static void Add(string text) {
            if (!string.IsNullOrWhiteSpace(text) && (lines.Count == 0 || lines.Last() != text)) 
                lines.Add(text);
            SetText("");
        }

        [Command]
        public static void Clear() {
            lines.Clear();
            PlayerPrefs.DeleteKey(nameof(CommandHistory));
        }
    }
}