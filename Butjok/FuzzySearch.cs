using UnityEngine;

namespace Butjok {

    public static class FuzzySearch {

        public static bool Match(string pattern, string text) {
            Assert.That(pattern != null);
            Assert.That(text != null);
            
            var offset = 0;
            foreach (var character in pattern) {
                var offset0 = text.IndexOf(char.ToLowerInvariant(character), offset);
                var offset1 = text.IndexOf(char.ToUpperInvariant(character), offset);
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