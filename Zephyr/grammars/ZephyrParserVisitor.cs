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
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="ZephyrParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public interface IZephyrParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgram([NotNull] ZephyrParser.ProgramContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.statementList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatementList([NotNull] ZephyrParser.StatementListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] ZephyrParser.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.decl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDecl([NotNull] ZephyrParser.DeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.classBodyDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassBodyDecl([NotNull] ZephyrParser.ClassBodyDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.typedVarDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypedVarDecl([NotNull] ZephyrParser.TypedVarDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.classDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDecl([NotNull] ZephyrParser.ClassDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.classBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassBody([NotNull] ZephyrParser.ClassBodyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.printStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrintStmt([NotNull] ZephyrParser.PrintStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.returnStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStmt([NotNull] ZephyrParser.ReturnStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.compound"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompound([NotNull] ZephyrParser.CompoundContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.ifStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStmt([NotNull] ZephyrParser.IfStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.whileStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileStmt([NotNull] ZephyrParser.WhileStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.forStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForStmt([NotNull] ZephyrParser.ForStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.funcDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncDecl([NotNull] ZephyrParser.FuncDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.funcParameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncParameters([NotNull] ZephyrParser.FuncParametersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.funcArguments"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFuncArguments([NotNull] ZephyrParser.FuncArgumentsContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVarDecl([NotNull] ZephyrParser.VarDeclContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.assignExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignExpr([NotNull] ZephyrParser.AssignExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.equality"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEquality([NotNull] ZephyrParser.EqualityContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.arrayInitializer"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayInitializer([NotNull] ZephyrParser.ArrayInitializerContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.arrayType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayType([NotNull] ZephyrParser.ArrayTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitType([NotNull] ZephyrParser.TypeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.factor"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFactor([NotNull] ZephyrParser.FactorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.call"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCall([NotNull] ZephyrParser.CallContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimary([NotNull] ZephyrParser.PrimaryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="ZephyrParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLiteral([NotNull] ZephyrParser.LiteralContext context);
}
