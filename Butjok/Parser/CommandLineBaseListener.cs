//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from CommandLine.g4 by ANTLR 4.9.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Butjok {

using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ICommandLineListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.1")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class CommandLineBaseListener : ICommandLineListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.styles"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStyles([NotNull] CommandLineParser.StylesContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.styles"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStyles([NotNull] CommandLineParser.StylesContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.style"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStyle([NotNull] CommandLineParser.StyleContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.style"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStyle([NotNull] CommandLineParser.StyleContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.commands"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCommands([NotNull] CommandLineParser.CommandsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.commands"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCommands([NotNull] CommandLineParser.CommandsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStatement([NotNull] CommandLineParser.StatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStatement([NotNull] CommandLineParser.StatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.noOperation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNoOperation([NotNull] CommandLineParser.NoOperationContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.noOperation"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNoOperation([NotNull] CommandLineParser.NoOperationContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCommand([NotNull] CommandLineParser.CommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCommand([NotNull] CommandLineParser.CommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterValue([NotNull] CommandLineParser.ValueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitValue([NotNull] CommandLineParser.ValueContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.variable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariable([NotNull] CommandLineParser.VariableContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.variable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariable([NotNull] CommandLineParser.VariableContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.null"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNull([NotNull] CommandLineParser.NullContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.null"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNull([NotNull] CommandLineParser.NullContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.boolean"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBoolean([NotNull] CommandLineParser.BooleanContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.boolean"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBoolean([NotNull] CommandLineParser.BooleanContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.integer"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInteger([NotNull] CommandLineParser.IntegerContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.integer"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInteger([NotNull] CommandLineParser.IntegerContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.real"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterReal([NotNull] CommandLineParser.RealContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.real"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitReal([NotNull] CommandLineParser.RealContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.string"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterString([NotNull] CommandLineParser.StringContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.string"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitString([NotNull] CommandLineParser.StringContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.int2"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInt2([NotNull] CommandLineParser.Int2Context context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.int2"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInt2([NotNull] CommandLineParser.Int2Context context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.color"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterColor([NotNull] CommandLineParser.ColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.color"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitColor([NotNull] CommandLineParser.ColorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.shortHexRgbColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterShortHexRgbColor([NotNull] CommandLineParser.ShortHexRgbColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.shortHexRgbColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitShortHexRgbColor([NotNull] CommandLineParser.ShortHexRgbColorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.shortHexRgbaColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterShortHexRgbaColor([NotNull] CommandLineParser.ShortHexRgbaColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.shortHexRgbaColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitShortHexRgbaColor([NotNull] CommandLineParser.ShortHexRgbaColorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.longHexRgbColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLongHexRgbColor([NotNull] CommandLineParser.LongHexRgbColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.longHexRgbColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLongHexRgbColor([NotNull] CommandLineParser.LongHexRgbColorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.longHexRgbaColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLongHexRgbaColor([NotNull] CommandLineParser.LongHexRgbaColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.longHexRgbaColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLongHexRgbaColor([NotNull] CommandLineParser.LongHexRgbaColorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.colorComponent"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterColorComponent([NotNull] CommandLineParser.ColorComponentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.colorComponent"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitColorComponent([NotNull] CommandLineParser.ColorComponentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.rgbColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRgbColor([NotNull] CommandLineParser.RgbColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.rgbColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRgbColor([NotNull] CommandLineParser.RgbColorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="CommandLineParser.rgbaColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRgbaColor([NotNull] CommandLineParser.RgbaColorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="CommandLineParser.rgbaColor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRgbaColor([NotNull] CommandLineParser.RgbaColorContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace Butjok
