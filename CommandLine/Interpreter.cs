using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Antlr4.Runtime;
using Butjok.CommandLine.Parsers;
using UnityEngine;

namespace Butjok.CommandLine
{
    public class Interpreter : CommandLineBaseVisitor<object>
    {
        public static readonly Dictionary<char, char> unescape = new Dictionary<char, char> {
            ['\"'] = '\"',
            ['\\'] = '\\',
            ['b'] = '\b',
            ['n'] = '\n',
            ['f'] = '\f',
            ['r'] = '\r',
            ['t'] = '\t',
        };
        public static readonly Dictionary<char, char> escape;

        private static readonly CommandLineLexer lexer = new CommandLineLexer(null);
        private static readonly CommandLineParser parser = new CommandLineParser(null);
        private static readonly Interpreter interpreter = new Interpreter();
        private static readonly StringBuilder sb = new StringBuilder();

        static Interpreter() {
            lexer.RemoveErrorListeners();
            parser.RemoveErrorListeners();
            lexer.AddErrorListener(new ExceptionThrower<int>());
            parser.AddErrorListener(new ExceptionThrower<IToken>());
            escape = unescape.ToDictionary(pair => pair.Value, pair => pair.Key);
        }
        public static object Execute(string input) {
            lexer.SetInputStream(new AntlrInputStream(input));
            parser.TokenStream = new CommonTokenStream(lexer);
            object value = null;
            foreach (var command in parser.input().command())
                value = interpreter.VisitCommand(command);
            return value;
        }
        public static object Evaluate(string input) {
            lexer.SetInputStream(new AntlrInputStream(input));
            parser.TokenStream = new CommonTokenStream(lexer);
            return interpreter.Visit(parser.value());
        }

        public override object VisitInteger(CommandLineParser.IntegerContext context) {
            return int.Parse(context.GetText());
        }
        public override object VisitBoolean(CommandLineParser.BooleanContext context) {
            return context.GetText() == "true";
        }
        public override object VisitReal(CommandLineParser.RealContext context) {
            return float.Parse(context.GetText());
        }
        public override object VisitParenthesis(CommandLineParser.ParenthesisContext context) {
            return Visit(context.value());
        }
        public override object VisitCommand(CommandLineParser.CommandContext context) {
            return Commands.Invoke(context.Identifier().GetText(), context.value().Select(Visit).ToArray());
        }
        public override object VisitInterpolation(CommandLineParser.InterpolationContext context) {
            return context.Identifier() != null
                ? Commands.Invoke(context.Identifier().GetText())   // short form without curly braces
                : VisitCommand(context.command());
        }

        public override object VisitString(CommandLineParser.StringContext context) {
            if (context.Identifier() != null)
                return context.Identifier().GetText();
            sb.Clear();
            var text = context.GetText();
            for (var i = 1; i < text.Length - 1; i++)
                sb.Append(text[i] != '\\'
                    ? text[i].ToString()
                    : unescape.TryGetValue(text[++i], out var c)
                        ? c.ToString()
                        : "\\" + text[i]);
            return sb.ToString();
        }
        
        public override object VisitUnaryExpression(CommandLineParser.UnaryExpressionContext context) {
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

        public override object VisitSummation(CommandLineParser.SummationContext context) {
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

        public override object VisitMultiplication(CommandLineParser.MultiplicationContext context) {
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

        public override object VisitJunction(CommandLineParser.JunctionContext context) {
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

        public override object VisitColor(CommandLineParser.ColorContext context) {
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

        public override object VisitInt2(CommandLineParser.Int2Context context) {
            return new Vector2Int((int)Visit(context.x), (int)Visit(context.y));
        }

        public static string Format(object value) {
            switch (value) {
                case bool boolean: return boolean ? "true" : "false";
                case Color color: return $"rgb {color.r} {color.g} {color.b}" + (color.a == 1 ? "" : $" {color.a}");
                case string str: {
                    sb.Clear();
                    sb.Append("\"");
                    foreach (var c in str)
                        sb.Append(escape.TryGetValue(c, out var mnemonic) ? "\\" + mnemonic : c.ToString());
                    sb.Append("\"");
                    return sb.ToString();
                }
                case null: return "null";
                default: return value.ToString();
            }
        }

        private class ExceptionThrower<T> : IAntlrErrorListener<T>
        {
            public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
                throw new SyntaxErrorException(msg);
            }
        }
    }

    [Serializable]
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException() { }
        public SyntaxErrorException(string message) : base(message) { }
        public SyntaxErrorException(string message, Exception inner) : base(message, inner) { }
        protected SyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}