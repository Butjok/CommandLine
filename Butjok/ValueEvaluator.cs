using System;
using UnityEngine;

namespace Butjok {
    public class ValueEvaluator : CommandLineBaseVisitor<object> {

        private readonly Func<string, object> _getVariableValue;

        public ValueEvaluator(Func<string, object> getVariableValue = null) {
            _getVariableValue = getVariableValue;
        }
        public override object VisitVariable(CommandLineParser.VariableContext context) {
            Assert.That(_getVariableValue != null);
            return _getVariableValue(context.GetText());
        }
        public override object VisitBoolean(CommandLineParser.BooleanContext context) {
            return Parse.Boolean(context.GetText());
        }
        public override object VisitInteger(CommandLineParser.IntegerContext context) {
            return Parse.Integer(context.GetText());
        }
        public override object VisitReal(CommandLineParser.RealContext context) {
            return Parse.Float(context.GetText());
        }
        public override object VisitString(CommandLineParser.StringContext context) {
            return Parse.String(context.GetText());
        }
        public override object VisitInt2(CommandLineParser.Int2Context context) {
            return new Vector2Int((int) Visit(context.integer(0)), (int) Visit(context.integer(1)));
        }
        public override object VisitShortHexRgbColor(CommandLineParser.ShortHexRgbColorContext context) {
            return Parse.HexColor(context.GetText());
        }
        public override object VisitShortHexRgbaColor(CommandLineParser.ShortHexRgbaColorContext context) {
            return Parse.HexColor(context.GetText());
        }
        public override object VisitLongHexRgbColor(CommandLineParser.LongHexRgbColorContext context) {
            return Parse.HexColor(context.GetText());
        }
        public override object VisitLongHexRgbaColor(CommandLineParser.LongHexRgbaColorContext context) {
            return Parse.HexColor(context.GetText());
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