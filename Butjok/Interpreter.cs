using System;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEngine;

namespace Butjok {

    [CLSCompliant(false)]
    public class Interpreter : CommandLineBaseListener, IAntlrErrorListener<int>, IAntlrErrorListener<IToken> {

        private readonly Func<string, bool> _isVariable;
        private readonly Action<string, object[]> _invoke;
        private readonly Func<string, object> _getValue;
        private readonly Action<string, object> _setValue;
        public bool SetSilently;
        private readonly ValueEvaluator _valueEvaluator;

        private static readonly CommandLineLexer Lexer = new CommandLineLexer(null);
        private static readonly CommandLineParser Parser = new CommandLineParser(null);

        public Interpreter(Action<string, object[]> invoke, Func<string, object> getValue, 
            Action<string, object> setValue, Func<string, bool> isVariable) {
            Assert.That(invoke != null);
            Assert.That(getValue != null);
            Assert.That(setValue != null);
            Assert.That(isVariable != null);

            _invoke = invoke;
            _getValue = getValue;
            _setValue = setValue;
            _isVariable = isVariable;

            _valueEvaluator = new ValueEvaluator(getValue);
        }
        public override void EnterCommand(CommandLineParser.CommandContext context) {
            var name = context.Identifier().GetText();
            var valueContexts = context.value();
            if (!_isVariable(name)) {
                _invoke(name, valueContexts.Select(_valueEvaluator.Visit).ToArray());
            }
            else {
                switch (valueContexts.Length) {
                    case 0:
                        Debug.Log(_getValue(name));
                        break;
                    case 1:
                        _setValue(name, _valueEvaluator.Visit(valueContexts[0]));
                        if (SetSilently)
                            break;
                        goto case 0;
                    default:
                        throw new CheckException(valueContexts.Length.ToString());
                }
            }
        }
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
            int charPositionInLine, string msg, RecognitionException e) {
            throw new Exception($"{line}:{charPositionInLine}: {msg}");
        }
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
            int charPositionInLine, string msg, RecognitionException e) {
            throw new Exception($"{line}:{charPositionInLine}: {msg}");
        }
        public void Execute(string input, bool setSilently) {
            SetSilently = setSilently;

            Lexer.SetInputStream(new AntlrInputStream(input));
            Parser.TokenStream = new CommonTokenStream(Lexer);

            Lexer.AddErrorListener(this);
            Parser.AddErrorListener(this);
            try {
                ParseTreeWalker.Default.Walk(this, Parser.commands());
            }
            finally {
                Lexer.RemoveErrorListener(this);
                Parser.RemoveErrorListener(this);
            }
        }
    }
}