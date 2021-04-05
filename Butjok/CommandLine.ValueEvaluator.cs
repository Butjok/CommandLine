using UnityEngine;

namespace Butjok {
    public partial class CommandLine {
        
        private class ValueEvaluator : CommandLineBaseVisitor<object> {

            public CommandLine CommandLine;

            public override object VisitVariable(CommandLineParser.VariableContext context) {
                var commandName = context.GetText();
                var found = CommandLine._commands.TryGetValue(commandName, out var command);
                Check.That(found, commandName.ToString);
                Check.That(command.Type.IsVariable(), commandName.ToString);
                return command.Value;
            }
            public override object VisitBoolean(CommandLineParser.BooleanContext context) {
                return ParseBoolean(context.GetText());
            }
            public override object VisitInteger(CommandLineParser.IntegerContext context) {
                return ParseInteger(context.GetText());
            }
            public override object VisitReal(CommandLineParser.RealContext context) {
                return ParseFloat(context.GetText());
            }
            public override object VisitString(CommandLineParser.StringContext context) {
                return ParseString(context.GetText());
            }
            public override object VisitInt2(CommandLineParser.Int2Context context) {
                return new Vector2Int((int) Visit(context.integer(0)), (int) Visit(context.integer(1)));
            }
            public override object VisitShortHexRgbColor(CommandLineParser.ShortHexRgbColorContext context) {
                return ParseHexColor(context.GetText());
            }
            public override object VisitShortHexRgbaColor(CommandLineParser.ShortHexRgbaColorContext context) {
                return ParseHexColor(context.GetText());
            }
            public override object VisitLongHexRgbColor(CommandLineParser.LongHexRgbColorContext context) {
                return ParseHexColor(context.GetText());
            }
            public override object VisitLongHexRgbaColor(CommandLineParser.LongHexRgbaColorContext context) {
                return ParseHexColor(context.GetText());
            }
            public override object VisitColorComponent(CommandLineParser.ColorComponentContext context) {
                return context.integer() != null ? (int) Visit(context.integer()) / 255f : Visit(context.real());
            }
            public override object VisitRgbColor(CommandLineParser.RgbColorContext context) {
                var r = (float) Visit(context.colorComponent(0));
                var g = (float) Visit(context.colorComponent(1));
                var b = (float) Visit(context.colorComponent(2));
                return new Color(r, g, b);
            }
            public override object VisitRgbaColor(CommandLineParser.RgbaColorContext context) {
                var r = (float) Visit(context.colorComponent(0));
                var g = (float) Visit(context.colorComponent(1));
                var b = (float) Visit(context.colorComponent(2));
                var a = (float) Visit(context.colorComponent(3));
                return new Color(r, g, b, a);
            }
        }
    }
}