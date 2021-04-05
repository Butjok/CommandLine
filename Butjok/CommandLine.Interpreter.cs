using System;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UnityEngine;

namespace Butjok {
    public partial class CommandLine {

        private class Interpreter : CommandLineBaseListener {

            public CommandLine CommandLine;
            
            public override void EnterCommand(CommandLineParser.CommandContext context) {
    
                var commandName = context.Name().GetText();
                if (!CommandLine._commands.TryGetValue(commandName, out var command))
                    throw new Exception(commandName);
    
                var valueContexts = context.value();
                switch (command.Type) {
    
                    case CommandType.Field:
                    case CommandType.Property:
                    case CommandType.GetSet:
    
                        switch (valueContexts.Length) {
                            case 0:
                                Debug.Log(command.Value);
                                break;
                            case 1:
                                command.Value = new ValueEvaluator {CommandLine = CommandLine}.Visit(valueContexts[0]);
                                goto case 0;
                            default:
                                throw new Exception(valueContexts.Length.ToString());
                        }
                        break;
    
                    case CommandType.Method:
                    case CommandType.Lambda:
                    case CommandType.UnityEvent:
    
                        command.Invoke(valueContexts.Select(new ValueEvaluator {CommandLine = CommandLine}.Visit).ToArray());
                        break;
    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public void Execute(string input) {

            var lexer = new CommandLineLexer(new AntlrInputStream(input));
            var parser = new CommandLineParser(new CommonTokenStream(lexer));

            lexer.AddErrorListener(this);
            parser.AddErrorListener(this);

            ParseTreeWalker.Default.Walk(new Interpreter{CommandLine = this}, parser.input());
        }
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
            int charPositionInLine, string msg, RecognitionException e) {
            throw new Exception($"{line}:{charPositionInLine}: {msg}");
        }
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
            int charPositionInLine, string msg, RecognitionException e) {
            throw new Exception($"{line}:{charPositionInLine}: {msg}");
        }
    }
}