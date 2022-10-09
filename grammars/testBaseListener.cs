//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from test.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419


using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ItestListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class testBaseListener : ItestListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.program"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterProgram([NotNull] test.ProgramContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.program"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitProgram([NotNull] test.ProgramContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.statementList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStatementList([NotNull] test.StatementListContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.statementList"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStatementList([NotNull] test.StatementListContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStatement([NotNull] test.StatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.statement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStatement([NotNull] test.StatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.decl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDecl([NotNull] test.DeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.decl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDecl([NotNull] test.DeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.classBodyDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterClassBodyDecl([NotNull] test.ClassBodyDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.classBodyDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitClassBodyDecl([NotNull] test.ClassBodyDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.typedVarDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTypedVarDecl([NotNull] test.TypedVarDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.typedVarDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTypedVarDecl([NotNull] test.TypedVarDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.classDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterClassDecl([NotNull] test.ClassDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.classDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitClassDecl([NotNull] test.ClassDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.classBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterClassBody([NotNull] test.ClassBodyContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.classBody"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitClassBody([NotNull] test.ClassBodyContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.printStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPrintStmt([NotNull] test.PrintStmtContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.printStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPrintStmt([NotNull] test.PrintStmtContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.returnStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterReturnStmt([NotNull] test.ReturnStmtContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.returnStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitReturnStmt([NotNull] test.ReturnStmtContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.compound"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCompound([NotNull] test.CompoundContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.compound"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCompound([NotNull] test.CompoundContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.ifStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIfStmt([NotNull] test.IfStmtContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.ifStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIfStmt([NotNull] test.IfStmtContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.whileStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterWhileStmt([NotNull] test.WhileStmtContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.whileStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitWhileStmt([NotNull] test.WhileStmtContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.forStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterForStmt([NotNull] test.ForStmtContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.forStmt"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitForStmt([NotNull] test.ForStmtContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.funcDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFuncDecl([NotNull] test.FuncDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.funcDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFuncDecl([NotNull] test.FuncDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.funcParameters"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFuncParameters([NotNull] test.FuncParametersContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.funcParameters"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFuncParameters([NotNull] test.FuncParametersContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.funcArguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFuncArguments([NotNull] test.FuncArgumentsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.funcArguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFuncArguments([NotNull] test.FuncArgumentsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVarDecl([NotNull] test.VarDeclContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.varDecl"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVarDecl([NotNull] test.VarDeclContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.assignExpr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAssignExpr([NotNull] test.AssignExprContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.assignExpr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAssignExpr([NotNull] test.AssignExprContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.equality"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEquality([NotNull] test.EqualityContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.equality"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEquality([NotNull] test.EqualityContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.comparison"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterComparison([NotNull] test.ComparisonContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.comparison"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitComparison([NotNull] test.ComparisonContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExpression([NotNull] test.ExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExpression([NotNull] test.ExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.term"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTerm([NotNull] test.TermContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.term"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTerm([NotNull] test.TermContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.factor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFactor([NotNull] test.FactorContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.factor"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFactor([NotNull] test.FactorContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.call"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCall([NotNull] test.CallContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.call"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCall([NotNull] test.CallContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.primary"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPrimary([NotNull] test.PrimaryContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.primary"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPrimary([NotNull] test.PrimaryContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLiteral([NotNull] test.LiteralContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.literal"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLiteral([NotNull] test.LiteralContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="test.type"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterType([NotNull] test.TypeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="test.type"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitType([NotNull] test.TypeContext context) { }

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
