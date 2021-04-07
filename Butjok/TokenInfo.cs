namespace Butjok {
    public readonly struct TokenInfo {
        public readonly string Name;
        public readonly int Type;
        public readonly string Literal;
        public TokenInfo(string name, int type, string literal) {
            Name = name;
            Type = type;
            Literal = literal;
        }
    }
}