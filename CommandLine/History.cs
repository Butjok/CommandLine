using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Butjok.CommandLine
{
    public static class History
    {
        private static string input = "";
        private static readonly List<string> lines = new List<string>();
        private static int index;

        [Command]
        public static string PlayerPrefsKey => typeof(History).FullName;

        public static string Text => index == lines.Count ? input : lines[index];
        static History() {
            var serialized = PlayerPrefs.GetString(PlayerPrefsKey, null);
            if (serialized != null)
                lines.AddRange(serialized.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            index = lines.Count;
        }
        public static void Save() {
            if (lines.Count > 0)
                PlayerPrefs.SetString(PlayerPrefsKey, string.Join("\n", lines));
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
            PlayerPrefs.DeleteKey(PlayerPrefsKey);
        }
    }
}