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
    public class Interpreter : CommandLineBaseVisitor<dynamic>
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
        public static object Execute(string text) {
            lexer.SetInputStream(new AntlrInputStream(text));
            parser.TokenStream = new CommonTokenStream(lexer);
            var value = parser.input().value();
            return value == null ? null : interpreter.Visit(value);
        }

        public override dynamic VisitNull(CommandLineParser.NullContext context) {
            return null;
        }
        public override dynamic VisitInteger(CommandLineParser.IntegerContext context) {
            return int.Parse(context.GetText());
        }
        public override dynamic VisitBoolean(CommandLineParser.BooleanContext context) {
            return context.GetText() == "true";
        }
        public override dynamic VisitReal(CommandLineParser.RealContext context) {
            return float.Parse(context.GetText());
        }
        public override dynamic VisitParenthesis(CommandLineParser.ParenthesisContext context) {
            return Visit(context.value());
        }
        public override dynamic VisitCommand(CommandLineParser.CommandContext context) {
            return context.Identifier() != null
                ? Commands.Invoke(context.Identifier().GetText(), context.value().Select(Visit).Cast<object>().ToArray())
                : Visit(context.value(0));
        }

        public override dynamic VisitString(CommandLineParser.StringContext context) {
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

        public const string unsupported = "Operators are not supported with Api Compatibility Level .NET Standard 2.0. Please change the Api Compatibility Level to .NET 4.x in Edit > Project Settings > Player > Other Settings > Configuration.";
        
        public override dynamic VisitUnaryExpression(CommandLineParser.UnaryExpressionContext context) {
#if !NET_4_6
            throw new NotSupportedException(unsupported);
#else
            var value = Visit(context.value());
            return context.@operator.Text switch {
                "-" => -value,
                "!" => !value,
                "~" => ~value,
                _ => throw new ArgumentOutOfRangeException(context.@operator.Text)
            };
#endif
        }

        public override dynamic VisitSummation(CommandLineParser.SummationContext context) {
#if !NET_4_6
            throw new NotSupportedException(unsupported);
#else
            var lhs = Visit(context.value(0));
            var rhs = Visit(context.value(1));
            return context.@operator.Text switch {
                "+" => lhs + rhs,
                "-" => lhs - rhs,
                _ => throw new ArgumentOutOfRangeException(context.@operator.Text)
            };
#endif
        }

        public override dynamic VisitMultiplication(CommandLineParser.MultiplicationContext context) {
#if !NET_4_6
            throw new NotSupportedException(unsupported);
#else
            var lhs = Visit(context.value(0));
            var rhs = Visit(context.value(1));
            return context.@operator.Text switch {
                "*" => lhs * rhs,
                "/" => lhs / rhs,
                "%" => lhs % rhs,
                _ => throw new ArgumentOutOfRangeException(context.@operator.Text)
            };
#endif
        }

        public override dynamic VisitJunction(CommandLineParser.JunctionContext context) {
#if !NET_4_6
            throw new NotSupportedException(unsupported);
#else
            var lhs = Visit(context.value(0));
            var rhs = Visit(context.value(1));
            return context.@operator.Text switch {
                "&&" => lhs && rhs,
                "||" => lhs || rhs,
                _ => throw new ArgumentOutOfRangeException(context.@operator.Text)
            };
#endif
        }

        public override dynamic VisitColor(CommandLineParser.ColorContext context) {
            var r = (object)Visit(context.value(0));
            var g = (object)Visit(context.value(1));
            var b = (object)Visit(context.value(2));
            var a = context.value(3) == null ? 1 : (object)Visit(context.value(3));
            return new Color(
                r is int ri ? ri : (float)r,
                g is int gi ? gi : (float)g,
                b is int bi ? bi : (float)b,
                a is int ai ? ai : (float)a);
        }

        public override dynamic VisitInt2(CommandLineParser.Int2Context context) {
            return new Vector2Int((int)(object)Visit(context.x), (int)(object)Visit(context.y));
        }
        public override dynamic VisitInt3(CommandLineParser.Int3Context context) {
            return new Vector3Int((int)(object)Visit(context.x), (int)(object)Visit(context.y), (int)(object)Visit(context.z));
        }
        public override dynamic VisitFloat2(CommandLineParser.Float2Context context) {
            var x = (object)Visit(context.x);
            var y = (object)Visit(context.y);
            return new Vector2(
                x is int xi ? xi : (float)x,
                y is int yi ? yi : (float)y);
        }
        public override dynamic VisitFloat3(CommandLineParser.Float3Context context) {
            var x = (object)Visit(context.x);
            var y = (object)Visit(context.y);
            var z = (object)Visit(context.z);
            return new Vector3(
                x is int xi ? xi : (float)x,
                y is int yi ? yi : (float)y,
                z is int zi ? zi : (float)z);
        }

        public static string Format(object value) {
            switch (value) {
                case bool boolean: return boolean ? "true" : "false";
                case Color color: return $"rgb {color.r} {color.g} {color.b}" + (color.a == 1 ? "" : $" {color.a}");
                case string str: {
                    sb.Clear();
                    sb.Append("\"");
                    foreach (var c in str)
                        if (escape.TryGetValue(c, out var mnemonic)) {
                            sb.Append(mnemonic);
                            sb.Append('\\');
                        }
                        else sb.Append(c);
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