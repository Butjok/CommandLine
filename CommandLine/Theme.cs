using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Butjok.CommandLine
{
    [CreateAssetMenu(menuName = nameof(Theme))]
    public class Theme : ScriptableObject
    {
        public Style normal = new Style();
        public Style error = new Style();
        [SerializeField] private List<Record> tokens = new List<Record>();

        public bool TryGetStyle(Token token, out Style style) {
            var record = tokens.SingleOrDefault(record => record.token == token);
            style = record ?? normal;
            return record != null;
        }

        [Serializable]
        private class Record : Style
        {
            public Token token;
        }
    }

    [Serializable]
    public class Style
    {
        public Color color = Color.white;
        public bool bold, italic;
    }
}