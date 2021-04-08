using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Butjok {

    public interface IToken {
        int Type { get; }
        int Start { get; }
        int Stop { get; }
        string Source { get; }
    }

    public static class TokenExtensions {

        public static string FindText(this IToken token) {
            Assert.That(token != null);
            Assert.That(token.Source != null);
            Assert.That(token.Start >= 0);
            Assert.That(token.Stop < token.Source.Length);
            Assert.That(token.Start <= token.Stop);

            return token.Source.Substring(token.Start, token.Stop - token.Start + 1);
        }
    }

    public class TextParser : Colorizer.ITextParser {

        private readonly struct Token : IToken {

            public int Type { get; }
            public int Start { get; }
            public int Stop { get; }
            public string Source { get; }

            public static readonly Token Invalid = new Token(TokenConstants.InvalidType, -1, -2, null);

            public Token(int type, int start, int stop, string source) {
                Type = type;
                Start = start;
                Stop = stop;
                Source = source;
            }
        }

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

        public IEnumerable<IToken> Tokens => _tokens.Values.Cast<IToken>();

        public TextParser() {
            _tokens = new SortedList<Interval, Token>(IntervalComparer.Default);
        }

        public IEnumerable<IToken> Parse(string text) {
            Assert.That(text != null);

            _text = text;
            _tokens.Clear();

            Lexer.SetInputStream(new AntlrInputStream(text));

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

            return _tokens.Values.Cast<IToken>();
        }

        public string Text => _text;

        public (bool Found, IToken Token) TokenAt(int textPosition) {
            Assert.That(_text != null);
            Assert.That(textPosition >= 0, textPosition.ToString);
            var length = _text.Length;
            Assert.That(textPosition < length, (textPosition, length).ToString);

            var key = new Interval {Start = textPosition, Stop = textPosition};
            return _tokens.TryGetValue(key, out var token)
                ? (true, token)
                : (false, Token.Invalid);
        }

        private void AddToken(int type, int start, int stop) {
            Assert.That(start >= 0);
            Assert.That(stop < _text.Length);
            Assert.That(start <= stop);

            _tokens.Add(new Interval {Start = start, Stop = stop}, new Token(type, start, stop, _text));
        }
        private IToken LastToken {
            get {
                Assert.That(_tokens.Count > 0);
                return _tokens[_tokens.Keys[_tokens.Count - 1]];
            }
        }
    }
}