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
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="test"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public interface ItestVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgram([NotNull] test.ProgramContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.statementList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatementList([NotNull] test.StatementListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] test.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDecl([NotNull] test.DeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.classBodyDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassBodyDecl([NotNull] test.ClassBodyDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.typedVarDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypedVarDecl([NotNull] test.TypedVarDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.classDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDecl([NotNull] test.ClassDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.classBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassBody([NotNull] test.ClassBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.printStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrintStmt([NotNull] test.PrintStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.returnStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStmt([NotNull] test.ReturnStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.compound"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompound([NotNull] test.CompoundContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.ifStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStmt([NotNull] test.IfStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.whileStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileStmt([NotNull] test.WhileStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.forStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForStmt([NotNull] test.ForStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.funcDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncDecl([NotNull] test.FuncDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.funcParameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncParameters([NotNull] test.FuncParametersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.funcArguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncArguments([NotNull] test.FuncArgumentsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarDecl([NotNull] test.VarDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.assignExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignExpr([NotNull] test.AssignExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.equality"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEquality([NotNull] test.EqualityContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.comparison"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComparison([NotNull] test.ComparisonContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] test.ExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.term"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTerm([NotNull] test.TermContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFactor([NotNull] test.FactorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCall([NotNull] test.CallContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimary([NotNull] test.PrimaryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLiteral([NotNull] test.LiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="test.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType([NotNull] test.TypeContext context);
}