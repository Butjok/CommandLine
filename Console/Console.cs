using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Butjok.Parsers;
using UnityEngine;

namespace Butjok
{
    [Serializable]
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException() { }
        public SyntaxErrorException(string message) : base(message) { }
        public SyntaxErrorException(string message, Exception inner) : base(message, inner) { }
        protected SyntaxErrorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) { }
    }
    public class ExceptionThrower<T> : IAntlrErrorListener<T>
    {
        public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
            throw new SyntaxErrorException(msg);
        }
    }

    public static class Console
    {
        private class Listener : ConsoleBaseListener
        {
            public override void EnterCommand(ConsoleParser.CommandContext context) {
                new Visitor().VisitCommand(context);
            }
        }

        private static Dictionary<char, char> escapeSequences;
        private static Dictionary<char, char> charactersToEscape;

        public static void Initialize() {
            escapeSequences = new Dictionary<char, char> {
                ['\"'] = '\"',
                ['\\'] = '\\',
                ['b'] = '\b',
                ['n'] = '\n',
                ['f'] = '\f',
                ['r'] = '\r',
                ['t'] = '\t',
            };
            charactersToEscape = escapeSequences.ToDictionary(pair => pair.Value, pair => pair.Key);
        }

        private class Visitor : ConsoleBaseVisitor<object>
        {
            public override object VisitInteger(ConsoleParser.IntegerContext context) {
                return int.Parse(context.GetText());
            }
            public override object VisitBoolean(ConsoleParser.BooleanContext context) {
                return context.GetText() == "true";
            }
            public override object VisitReal(ConsoleParser.RealContext context) {
                return float.Parse(context.GetText());
            }
            public override object VisitParenthesis(ConsoleParser.ParenthesisContext context) {
                return Visit(context.value());
            }
            public override object VisitString(ConsoleParser.StringContext context) {
                if (context.Identifier() != null)
                    return context.Identifier().GetText();
                var sb = new StringBuilder();
                var text = context.GetText();
                for (var i = 1; i < text.Length - 1; i++)
                    sb.Append(text[i] != '\\'
                        ? text[i].ToString()
                        : escapeSequences.TryGetValue(text[++i], out var c)
                            ? c.ToString()
                            : "\\" + text[i]);
                return sb.ToString();
            }
            public override object VisitCommand(ConsoleParser.CommandContext context) {
                var arguments = new List<object>();
                var namedArguments = new Dictionary<string, object>();
                foreach (var argumentContext in context.argument()) {
                    var value = Visit(argumentContext.value());
                    if (argumentContext.Identifier() != null)
                        namedArguments[argumentContext.Identifier().GetText()] = value;
                    else
                        arguments.Add(value);
                }
                return Commands.Invoke(context.Identifier().GetText(), arguments, namedArguments);
            }
            public override object VisitInterpolation(ConsoleParser.InterpolationContext context) {
                if (context.Identifier() != null)
                    return Commands.Invoke(context.Identifier().GetText(), Array.Empty<object>(), Enumerable.Empty<KeyValuePair<string, object>>());
                return VisitCommand(context.command());
            }
            public override object VisitUnaryExpression(ConsoleParser.UnaryExpressionContext context) {
#if NET_4_6
                dynamic value = Visit(context.value());
                switch (context.@operator.Text) {
                    case "-": return -value;
                    case "!": return !value;
                    default: throw new ArgumentOutOfRangeException(context.@operator.Text);
                }
#else
                throw new NotSupportedException("Unary operators are supported only in the .NET 4.x API compatibility level.");
#endif
            }
            public override object VisitSummation(ConsoleParser.SummationContext context) {
#if NET_4_6
                dynamic lhs = Visit(context.value(0));
                dynamic rhs = Visit(context.value(1));
                switch (context.@operator.Text) {
                    case "+": return lhs + rhs;
                    case "-": return lhs - rhs;
                    default: throw new ArgumentOutOfRangeException(context.@operator.Text);
                }
#else
                throw new NotSupportedException("Arithmetic operators are supported only in the .NET 4.x API compatibility level.");
#endif
            }
            public override object VisitMultiplication(ConsoleParser.MultiplicationContext context) {
#if NET_4_6
                dynamic lhs = Visit(context.value(0));
                dynamic rhs = Visit(context.value(1));
                switch (context.@operator.Text) {
                    case "*": return lhs * rhs;
                    case "/": return lhs / rhs;
                    case "%": return lhs % rhs;
                    default: throw new ArgumentOutOfRangeException(context.@operator.Text);
                }
#else
                throw new NotSupportedException("Arithmetic operators are supported only in the .NET 4.x API compatibility level.");
#endif
            }
            public override object VisitJunction(ConsoleParser.JunctionContext context) {
#if NET_4_6
                dynamic lhs = Visit(context.value(0));
                dynamic rhs = Visit(context.value(1));
                switch (context.@operator.Text) {
                    case "&&": return lhs && rhs;
                    case "||": return lhs || rhs;
                    default: throw new ArgumentOutOfRangeException(context.@operator.Text);
                }
#else
                throw new NotSupportedException("Logical operators are supported only in the .NET 4.x API compatibility level.");
#endif
            }
            public override object VisitColor(ConsoleParser.ColorContext context) {
                var r = Visit(context.value(0));
                var g = Visit(context.value(1));
                var b = Visit(context.value(2));
                var a = context.value(3) == null ? 1 : Visit(context.value(3));
                return new Color(
                    r is int ri ? ri : (float)r,
                    g is int gi ? gi : (float)g,
                    b is int bi ? bi : (float)b,
                    a is int ai ? ai : (float)a);
            }
            public override object VisitInt2(ConsoleParser.Int2Context context) {
                return new Vector2Int((int)Visit(context.x), (int)Visit(context.y));
            }
        }

        public static void Execute(string text) {
            var lexer = new ConsoleLexer(new AntlrInputStream(text));
            var parser = new ConsoleParser(new CommonTokenStream(lexer));
            lexer.RemoveErrorListeners();
            parser.RemoveErrorListeners();
            lexer.AddErrorListener(new ExceptionThrower<int>());
            parser.AddErrorListener(new ExceptionThrower<IToken>());
            ParseTreeWalker.Default.Walk(new Listener(), parser.input());
        }

        public static string Format(object value) {
            switch (value) {
                case bool boolean: return boolean ? "true" : "false";
                case Color color: return $"rgb {color.r} {color.g} {color.b}" + (color.a == 1 ? "" : $" {color.a}");
                case string str: {
                    var sb = new StringBuilder("\"");
                    foreach (var c in str)
                        sb.Append(charactersToEscape.TryGetValue(c, out var mnemonic) ? "\\" + mnemonic : c.ToString());
                    sb.Append("\"");
                    return sb.ToString();
                }
                case null: return "null";
                default: return value.ToString();
            }
        }

        [Command]
        public static void Print(object value) {
            Debug.Log(Format(value));
        }
    }
}