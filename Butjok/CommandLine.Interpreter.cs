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
            public bool SetSilently;

            public override void EnterCommand(CommandLineParser.CommandContext context) {

                var commandName = context.Identifier().GetText();
                var found = CommandLine._commands.TryGetValue(commandName, out var command);
                Check.That(found, commandName.ToString);

                var valueContexts = context.value();

                if (command.Type.IsProcedure())
                    command.Invoke(valueContexts.Select(new ValueEvaluator
                        {CommandLine = CommandLine}.Visit).ToArray());

                else if (command.Type.IsVariable())
                    switch (valueContexts.Length) {
                        case 0:
                            Debug.Log(command.Value);
                            break;
                        case 1:
                            command.Value = new ValueEvaluator {CommandLine = CommandLine}.Visit(valueContexts[0]);
                            if (SetSilently)
                                break;
                            goto case 0;
                        default:
                            throw new Exception(valueContexts.Length.ToString());
                    }

                else
                    throw new CheckException(command.Type.ToString());
            }
            
            public override void EnterStyle(CommandLineParser.StyleContext context) {
                
                var evaluator = new ValueEvaluator {CommandLine = CommandLine};
                var name = (string) evaluator.Visit(context.@string());
                if (!Enum.TryParse(name, out TokenType type)) {
                    Debug.LogWarning(
                        $"Color scheme has invalid key '{name}', there is no such token type, skipping...");
                    return;
                }
                var style = CommandLine.tokenStylesList.SingleOrDefault(item => item.Type == type);
                if (style == null) {
                    Debug.LogWarning(
                        $"Cannot find a style for {type}. Did you forget to regenerate code for tokens? See Window > Regenerate code. Skipping...");
                    return;
                }
                style.Color = (Color) evaluator.Visit(context.color());
                style.Bold = (bool) evaluator.Visit(context.boolean(0));
                style.Italic = (bool) evaluator.Visit(context.boolean(1));
                style.IsKeyword = (bool) evaluator.Visit(context.boolean(2));
            }
        }

        private void Execute(string input, Func<CommandLineParser, IParseTree> parse = null, bool setSilently = false) {
            Check.That(input != null);

            var lexer = new CommandLineLexer(new AntlrInputStream(input));
            var parser = new CommandLineParser(new CommonTokenStream(lexer));

            lexer.AddErrorListener(this);
            parser.AddErrorListener(this);

            ParseTreeWalker.Default.Walk(new Interpreter {
                CommandLine = this,
                SetSilently = setSilently
            }, parse?.Invoke(parser) ?? parser.commands());
        }
        [ContextMenu(nameof(Execute))]
        private void Execute() {
            Execute(input);
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