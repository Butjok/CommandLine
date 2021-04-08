using System;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEngine;

namespace Butjok {

    [CLSCompliant(false)]
    public class Interpreter : CommandLineBaseListener, IAntlrErrorListener<int>,
        IAntlrErrorListener<Antlr4.Runtime.IToken> {

        public bool SetSilently;
        private readonly ICommands _commands;
        private readonly ValueEvaluator _valueEvaluator;

        private static readonly CommandLineLexer Lexer = new CommandLineLexer(null);
        private static readonly CommandLineParser Parser = new CommandLineParser(null);

        public Interpreter(ICommands commands) {
            Assert.That(commands != null);

            _commands = commands;
            _valueEvaluator = new ValueEvaluator(commands.GetValue);
        }
        public override void EnterCommand(CommandLineParser.CommandContext context) {
            var name = context.Identifier().GetText();
            var valueContexts = context.value();
            if (!_commands.IsVariable(name)) {
                _commands.Invoke(name, valueContexts.Select(_valueEvaluator.Visit).ToArray());
            }
            else {
                switch (valueContexts.Length) {
                    case 0:
                        Debug.Log(_commands.GetValue(name));
                        break;
                    case 1:
                        _commands.SetValue(name, _valueEvaluator.Visit(valueContexts[0]));
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
        public void SyntaxError(TextWriter output, IRecognizer recognizer, Antlr4.Runtime.IToken offendingSymbol,
            int line,
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