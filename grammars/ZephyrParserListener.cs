//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from ZephyrParser.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="ZephyrParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public interface IZephyrParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterProgram([NotNull] ZephyrParser.ProgramContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitProgram([NotNull] ZephyrParser.ProgramContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.statementList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStatementList([NotNull] ZephyrParser.StatementListContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.statementList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStatementList([NotNull] ZephyrParser.StatementListContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStatement([NotNull] ZephyrParser.StatementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStatement([NotNull] ZephyrParser.StatementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDecl([NotNull] ZephyrParser.DeclContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDecl([NotNull] ZephyrParser.DeclContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.classBodyDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassBodyDecl([NotNull] ZephyrParser.ClassBodyDeclContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.classBodyDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassBodyDecl([NotNull] ZephyrParser.ClassBodyDeclContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.typedVarDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTypedVarDecl([NotNull] ZephyrParser.TypedVarDeclContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.typedVarDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTypedVarDecl([NotNull] ZephyrParser.TypedVarDeclContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.classDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassDecl([NotNull] ZephyrParser.ClassDeclContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.classDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassDecl([NotNull] ZephyrParser.ClassDeclContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.classBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClassBody([NotNull] ZephyrParser.ClassBodyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.classBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClassBody([NotNull] ZephyrParser.ClassBodyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.printStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrintStmt([NotNull] ZephyrParser.PrintStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.printStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrintStmt([NotNull] ZephyrParser.PrintStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.returnStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterReturnStmt([NotNull] ZephyrParser.ReturnStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.returnStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitReturnStmt([NotNull] ZephyrParser.ReturnStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.compound"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompound([NotNull] ZephyrParser.CompoundContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.compound"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompound([NotNull] ZephyrParser.CompoundContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.ifStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIfStmt([NotNull] ZephyrParser.IfStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.ifStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIfStmt([NotNull] ZephyrParser.IfStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.whileStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterWhileStmt([NotNull] ZephyrParser.WhileStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.whileStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitWhileStmt([NotNull] ZephyrParser.WhileStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.forStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterForStmt([NotNull] ZephyrParser.ForStmtContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.forStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitForStmt([NotNull] ZephyrParser.ForStmtContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.funcDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncDecl([NotNull] ZephyrParser.FuncDeclContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.funcDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncDecl([NotNull] ZephyrParser.FuncDeclContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.funcParameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncParameters([NotNull] ZephyrParser.FuncParametersContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.funcParameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncParameters([NotNull] ZephyrParser.FuncParametersContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.funcArguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFuncArguments([NotNull] ZephyrParser.FuncArgumentsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.funcArguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFuncArguments([NotNull] ZephyrParser.FuncArgumentsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVarDecl([NotNull] ZephyrParser.VarDeclContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVarDecl([NotNull] ZephyrParser.VarDeclContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.assignExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAssignExpr([NotNull] ZephyrParser.AssignExprContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.assignExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAssignExpr([NotNull] ZephyrParser.AssignExprContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.equality"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEquality([NotNull] ZephyrParser.EqualityContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.equality"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEquality([NotNull] ZephyrParser.EqualityContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFactor([NotNull] ZephyrParser.FactorContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFactor([NotNull] ZephyrParser.FactorContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCall([NotNull] ZephyrParser.CallContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCall([NotNull] ZephyrParser.CallContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrimary([NotNull] ZephyrParser.PrimaryContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrimary([NotNull] ZephyrParser.PrimaryContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLiteral([NotNull] ZephyrParser.LiteralContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLiteral([NotNull] ZephyrParser.LiteralContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ZephyrParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterType([NotNull] ZephyrParser.TypeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ZephyrParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitType([NotNull] ZephyrParser.TypeContext context);
}
