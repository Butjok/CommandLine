using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEngine;

namespace Butjok {
    
    public class Interpreter : CommandLineBaseListener, IAntlrErrorListener<int>, IAntlrErrorListener<IToken> {

        private readonly Func<string, (bool Found, Command Command)> _getCommand;
        private bool _setSilently;
        private readonly ValueEvaluator _valueEvaluator;

        private static readonly CommandLineLexer Lexer = new CommandLineLexer(null);
        private static readonly CommandLineParser Parser = new CommandLineParser(null);

        public Interpreter(Func<string, (bool Found, Command Command)> getCommand) {
            Check.That(getCommand != null);

            _getCommand = getCommand;
            _valueEvaluator = new ValueEvaluator(name => {
                var (found, command) = getCommand(name);
                Check.That(found, name.ToString);
                Check.That(command.VariableGet != null, name.ToString);
                return command.VariableGet();
            });
        }
        public override void EnterCommand(CommandLineParser.CommandContext context) {
            Check.That(_getCommand != null);

            var name = context.Identifier().GetText();
            var (found, command) = _getCommand(name);

            Check.That(found, name.ToString);
            Validate(name, command);

            var valueContexts = context.value();
            if (command.Procedure != null) {
                command.Procedure(valueContexts.Select(_valueEvaluator.Visit).ToArray());
            }
            else {
                switch (valueContexts.Length) {
                    case 0:
                        Debug.Log(command.VariableGet());
                        break;
                    case 1:
                        Check.That(command.VariableSet != null, name.ToString);
                        command.VariableSet(_valueEvaluator.Visit(valueContexts[0]));
                        if (_setSilently)
                            break;
                        goto case 0;
                    default:
                        throw new CheckException(valueContexts.Length.ToString());
                }
            }
        }
        private static void Validate(string name, Command command) {
            Check.That(!(command.Procedure == null && command.VariableGet == null && command.VariableSet == null),
                name.ToString);
            Check.That(command.Procedure != null && command.VariableGet == null && command.VariableSet == null ||
                       command.Procedure == null && command.VariableGet != null,
                name.ToString);
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
            _setSilently = setSilently;

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