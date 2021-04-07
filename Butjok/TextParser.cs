using System.Collections.Generic;
using Antlr4.Runtime;

namespace Butjok {

    public readonly struct Token {
        public Token(int type, int start, int stop) {
            Type = type;
            Start = start;
            Stop = stop;
        }
        public readonly int Type, Start, Stop;
        public static readonly Token Invalid = new Token(TokenConstants.InvalidType, -1, -2);
    }

    public class TextParser {

        private struct Interval {
            public int Start, Stop;
        }

        private class IntervalComparer : IComparer<Interval> {
            public static readonly IntervalComparer Default = new IntervalComparer();
            public int Compare(Interval interval, Interval target) {
                if (interval.Start <= target.Start && target.Stop <= interval.Stop)
                    return 0;
                return interval.Start > target.Stop ? 1 : -1;
            }
        }

        private readonly SortedList<Interval, Token> _tokens;
        private string _text;

        private static readonly CommandLineLexer Lexer = new CommandLineLexer(null);

        public IEnumerable<Token> Tokens => _tokens.Values;

        public TextParser() {
            _tokens = new SortedList<Interval, Token>(IntervalComparer.Default);
        }
        public string Text {
            get => _text;
            set {
                Assert.That(value != null);

                _text = value;
                _tokens.Clear();

                Lexer.SetInputStream(new AntlrInputStream(value));

                var token = Lexer.NextToken();
                if (token.Type == TokenConstants.EOF) {
                    if (_text.Length - 1 >= 0)
                        AddToken(TokenConstants.InvalidType, 0, _text.Length - 1);
                }
                else {
                    if (token.StartIndex - 1 >= 0)
                        AddToken(TokenConstants.InvalidType, 0, token.StartIndex - 1);
                    AddToken(token.Type, token.StartIndex, token.StopIndex);
                    token = Lexer.NextToken();
                    while (token.Type != TokenConstants.EOF) {
                        if (token.StartIndex - 1 >= LastToken.Stop + 1)
                            AddToken(TokenConstants.InvalidType, LastToken.Stop + 1, token.StartIndex - 1);
                        AddToken(token.Type, token.StartIndex, token.StopIndex);
                        token = Lexer.NextToken();
                    }
                    if (_text.Length - 1 >= LastToken.Stop + 1)
                        AddToken(TokenConstants.InvalidType, LastToken.Stop + 1, _text.Length - 1);
                }
            }
        }
        public (bool Found, Token token) TokenAt(int textPosition) {
            Assert.That(textPosition >= 0, textPosition.ToString);
            var length = Text.Length;
            Assert.That(textPosition < length, (textPosition, length).ToString);

            var key = new Interval {Start = textPosition, Stop = textPosition};
            return _tokens.TryGetValue(key, out var token)
                ? (true, token)
                : (false, Token.Invalid);
        }

        private void AddToken(int type, int start, int stop) {
            _tokens.Add(new Interval {Start = start, Stop = stop}, new Token(type, start, stop));
        }
        private Token LastToken {
            get {
                Assert.That(_tokens.Count > 0);
                return _tokens[_tokens.Keys[_tokens.Count - 1]];
            }
        }
    }
}