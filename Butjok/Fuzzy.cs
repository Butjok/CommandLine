using UnityEngine;

namespace Butjok {

    public static class Fuzzy {

        public static bool Match(string text, string completionText) {
            var offset = 0;
            foreach (var character in text) {
                var offset0 = completionText.IndexOf(char.ToLowerInvariant(character), offset);
                var offset1 = completionText.IndexOf(char.ToUpperInvariant(character), offset);
                if (offset0 == -1 && offset1 == -1)
                    return false;
                if (offset1 == -1)
                    offset = offset0 + 1;
                else if (offset0 == -1)
                    offset = offset1 + 1;
                else
                    offset = Mathf.Min(offset0, offset1) + 1;
            }
            return true;
        }
    }
}