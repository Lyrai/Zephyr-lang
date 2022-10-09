//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.10.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:/Users/x355x/Desktop/zephyr/Zephyr/grammars\testLexer.g4 by ANTLR 4.10.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.10.1")]
[System.CLSCompliant(false)]
public partial class testLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		STRING_LITERAL=1, WS=2, CLASS=3, END=4, PRINT=5, RETURN=6, IF=7, ELSE=8, 
		WHILE=9, FOR=10, GET=11, SET=12, FN=13, LET=14, PROPERTY=15, ID=16, ARROW=17, 
		SEMICOLON=18, COLON=19, COMMA=20, DOT=21, LBRACE=22, RBRACE=23, ASSIGN=24, 
		EQUAL=25, NOT_EQUAL=26, GREATER_EQUAL=27, GREATER=28, LESS_EQUAL=29, LESS=30, 
		PLUS=31, MINUS=32, DIVIDE=33, MULTIPLY=34, NOT=35, LPAR=36, RPAR=37, INT=38, 
		FLOAT=39, DOUBLE_QUOTE=40;
	public const int
		StringLiteral=1;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE", "StringLiteral"
	};

	public static readonly string[] ruleNames = {
		"WS", "CLASS", "END", "PRINT", "RETURN", "IF", "ELSE", "WHILE", "FOR", 
		"GET", "SET", "FN", "LET", "PROPERTY", "ID", "ARROW", "SEMICOLON", "COLON", 
		"COMMA", "DOT", "LBRACE", "RBRACE", "ASSIGN", "EQUAL", "NOT_EQUAL", "GREATER_EQUAL", 
		"GREATER", "LESS_EQUAL", "LESS", "PLUS", "MINUS", "DIVIDE", "MULTIPLY", 
		"NOT", "LPAR", "RPAR", "DOUBLE_QUOTE", "INT", "FLOAT", "CLOSING_QUOTE", 
		"ANY"
	};


	public testLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public testLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, "'class'", "'end'", "'print'", "'return'", "'if'", "'else'", 
		"'while'", "'for'", "'get'", "'set'", "'fn'", "'let'", "'property'", null, 
		"'->'", "';'", "':'", "','", "'.'", "'{'", "'}'", "'='", "'=='", "'!='", 
		"'>='", "'>'", "'<='", "'<'", "'+'", "'-'", "'/'", "'*'", "'!'", "'('", 
		"')'", null, null, "'\"'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "STRING_LITERAL", "WS", "CLASS", "END", "PRINT", "RETURN", "IF", 
		"ELSE", "WHILE", "FOR", "GET", "SET", "FN", "LET", "PROPERTY", "ID", "ARROW", 
		"SEMICOLON", "COLON", "COMMA", "DOT", "LBRACE", "RBRACE", "ASSIGN", "EQUAL", 
		"NOT_EQUAL", "GREATER_EQUAL", "GREATER", "LESS_EQUAL", "LESS", "PLUS", 
		"MINUS", "DIVIDE", "MULTIPLY", "NOT", "LPAR", "RPAR", "INT", "FLOAT", 
		"DOUBLE_QUOTE"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "testLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static testLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,40,237,6,-1,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,
		6,7,6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,
		2,14,7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,
		2,21,7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,
		2,28,7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,
		2,35,7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,1,0,1,0,1,
		0,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,2,1,2,1,2,1,3,1,3,1,3,1,3,1,3,1,3,
		1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,5,1,5,1,5,1,6,1,6,1,6,1,6,1,6,1,7,1,7,1,
		7,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,10,1,10,1,10,1,10,1,11,
		1,11,1,11,1,12,1,12,1,12,1,12,1,13,1,13,1,13,1,13,1,13,1,13,1,13,1,13,
		1,13,1,14,1,14,5,14,156,8,14,10,14,12,14,159,9,14,1,15,1,15,1,15,1,16,
		1,16,1,17,1,17,1,18,1,18,1,19,1,19,1,20,1,20,1,21,1,21,1,22,1,22,1,23,
		1,23,1,23,1,24,1,24,1,24,1,25,1,25,1,25,1,26,1,26,1,27,1,27,1,27,1,28,
		1,28,1,29,1,29,1,30,1,30,1,31,1,31,1,32,1,32,1,33,1,33,1,34,1,34,1,35,
		1,35,1,36,1,36,1,36,1,36,1,36,1,37,4,37,214,8,37,11,37,12,37,215,1,38,
		4,38,219,8,38,11,38,12,38,220,1,38,1,38,4,38,225,8,38,11,38,12,38,226,
		1,39,1,39,1,39,1,39,1,39,1,40,1,40,1,40,1,40,0,0,41,2,2,4,3,6,4,8,5,10,
		6,12,7,14,8,16,9,18,10,20,11,22,12,24,13,26,14,28,15,30,16,32,17,34,18,
		36,19,38,20,40,21,42,22,44,23,46,24,48,25,50,26,52,27,54,28,56,29,58,30,
		60,31,62,32,64,33,66,34,68,35,70,36,72,37,74,40,76,38,78,39,80,0,82,0,
		2,0,1,4,3,0,9,10,13,13,32,32,3,0,65,90,95,95,97,122,4,0,49,57,65,90,95,
		95,97,122,1,0,48,57,239,0,2,1,0,0,0,0,4,1,0,0,0,0,6,1,0,0,0,0,8,1,0,0,
		0,0,10,1,0,0,0,0,12,1,0,0,0,0,14,1,0,0,0,0,16,1,0,0,0,0,18,1,0,0,0,0,20,
		1,0,0,0,0,22,1,0,0,0,0,24,1,0,0,0,0,26,1,0,0,0,0,28,1,0,0,0,0,30,1,0,0,
		0,0,32,1,0,0,0,0,34,1,0,0,0,0,36,1,0,0,0,0,38,1,0,0,0,0,40,1,0,0,0,0,42,
		1,0,0,0,0,44,1,0,0,0,0,46,1,0,0,0,0,48,1,0,0,0,0,50,1,0,0,0,0,52,1,0,0,
		0,0,54,1,0,0,0,0,56,1,0,0,0,0,58,1,0,0,0,0,60,1,0,0,0,0,62,1,0,0,0,0,64,
		1,0,0,0,0,66,1,0,0,0,0,68,1,0,0,0,0,70,1,0,0,0,0,72,1,0,0,0,0,74,1,0,0,
		0,0,76,1,0,0,0,0,78,1,0,0,0,1,80,1,0,0,0,1,82,1,0,0,0,2,84,1,0,0,0,4,88,
		1,0,0,0,6,94,1,0,0,0,8,98,1,0,0,0,10,104,1,0,0,0,12,111,1,0,0,0,14,114,
		1,0,0,0,16,119,1,0,0,0,18,125,1,0,0,0,20,129,1,0,0,0,22,133,1,0,0,0,24,
		137,1,0,0,0,26,140,1,0,0,0,28,144,1,0,0,0,30,153,1,0,0,0,32,160,1,0,0,
		0,34,163,1,0,0,0,36,165,1,0,0,0,38,167,1,0,0,0,40,169,1,0,0,0,42,171,1,
		0,0,0,44,173,1,0,0,0,46,175,1,0,0,0,48,177,1,0,0,0,50,180,1,0,0,0,52,183,
		1,0,0,0,54,186,1,0,0,0,56,188,1,0,0,0,58,191,1,0,0,0,60,193,1,0,0,0,62,
		195,1,0,0,0,64,197,1,0,0,0,66,199,1,0,0,0,68,201,1,0,0,0,70,203,1,0,0,
		0,72,205,1,0,0,0,74,207,1,0,0,0,76,213,1,0,0,0,78,218,1,0,0,0,80,228,1,
		0,0,0,82,233,1,0,0,0,84,85,7,0,0,0,85,86,1,0,0,0,86,87,6,0,0,0,87,3,1,
		0,0,0,88,89,5,99,0,0,89,90,5,108,0,0,90,91,5,97,0,0,91,92,5,115,0,0,92,
		93,5,115,0,0,93,5,1,0,0,0,94,95,5,101,0,0,95,96,5,110,0,0,96,97,5,100,
		0,0,97,7,1,0,0,0,98,99,5,112,0,0,99,100,5,114,0,0,100,101,5,105,0,0,101,
		102,5,110,0,0,102,103,5,116,0,0,103,9,1,0,0,0,104,105,5,114,0,0,105,106,
		5,101,0,0,106,107,5,116,0,0,107,108,5,117,0,0,108,109,5,114,0,0,109,110,
		5,110,0,0,110,11,1,0,0,0,111,112,5,105,0,0,112,113,5,102,0,0,113,13,1,
		0,0,0,114,115,5,101,0,0,115,116,5,108,0,0,116,117,5,115,0,0,117,118,5,
		101,0,0,118,15,1,0,0,0,119,120,5,119,0,0,120,121,5,104,0,0,121,122,5,105,
		0,0,122,123,5,108,0,0,123,124,5,101,0,0,124,17,1,0,0,0,125,126,5,102,0,
		0,126,127,5,111,0,0,127,128,5,114,0,0,128,19,1,0,0,0,129,130,5,103,0,0,
		130,131,5,101,0,0,131,132,5,116,0,0,132,21,1,0,0,0,133,134,5,115,0,0,134,
		135,5,101,0,0,135,136,5,116,0,0,136,23,1,0,0,0,137,138,5,102,0,0,138,139,
		5,110,0,0,139,25,1,0,0,0,140,141,5,108,0,0,141,142,5,101,0,0,142,143,5,
		116,0,0,143,27,1,0,0,0,144,145,5,112,0,0,145,146,5,114,0,0,146,147,5,111,
		0,0,147,148,5,112,0,0,148,149,5,101,0,0,149,150,5,114,0,0,150,151,5,116,
		0,0,151,152,5,121,0,0,152,29,1,0,0,0,153,157,7,1,0,0,154,156,7,2,0,0,155,
		154,1,0,0,0,156,159,1,0,0,0,157,155,1,0,0,0,157,158,1,0,0,0,158,31,1,0,
		0,0,159,157,1,0,0,0,160,161,5,45,0,0,161,162,5,62,0,0,162,33,1,0,0,0,163,
		164,5,59,0,0,164,35,1,0,0,0,165,166,5,58,0,0,166,37,1,0,0,0,167,168,5,
		44,0,0,168,39,1,0,0,0,169,170,5,46,0,0,170,41,1,0,0,0,171,172,5,123,0,
		0,172,43,1,0,0,0,173,174,5,125,0,0,174,45,1,0,0,0,175,176,5,61,0,0,176,
		47,1,0,0,0,177,178,5,61,0,0,178,179,5,61,0,0,179,49,1,0,0,0,180,181,5,
		33,0,0,181,182,5,61,0,0,182,51,1,0,0,0,183,184,5,62,0,0,184,185,5,61,0,
		0,185,53,1,0,0,0,186,187,5,62,0,0,187,55,1,0,0,0,188,189,5,60,0,0,189,
		190,5,61,0,0,190,57,1,0,0,0,191,192,5,60,0,0,192,59,1,0,0,0,193,194,5,
		43,0,0,194,61,1,0,0,0,195,196,5,45,0,0,196,63,1,0,0,0,197,198,5,47,0,0,
		198,65,1,0,0,0,199,200,5,42,0,0,200,67,1,0,0,0,201,202,5,33,0,0,202,69,
		1,0,0,0,203,204,5,40,0,0,204,71,1,0,0,0,205,206,5,41,0,0,206,73,1,0,0,
		0,207,208,5,34,0,0,208,209,1,0,0,0,209,210,6,36,1,0,210,211,6,36,2,0,211,
		75,1,0,0,0,212,214,7,3,0,0,213,212,1,0,0,0,214,215,1,0,0,0,215,213,1,0,
		0,0,215,216,1,0,0,0,216,77,1,0,0,0,217,219,7,3,0,0,218,217,1,0,0,0,219,
		220,1,0,0,0,220,218,1,0,0,0,220,221,1,0,0,0,221,222,1,0,0,0,222,224,3,
		40,19,0,223,225,7,3,0,0,224,223,1,0,0,0,225,226,1,0,0,0,226,224,1,0,0,
		0,226,227,1,0,0,0,227,79,1,0,0,0,228,229,5,34,0,0,229,230,1,0,0,0,230,
		231,6,39,3,0,231,232,6,39,4,0,232,81,1,0,0,0,233,234,9,0,0,0,234,235,1,
		0,0,0,235,236,6,40,1,0,236,83,1,0,0,0,6,0,1,157,215,220,226,5,6,0,0,3,
		0,0,2,1,0,7,1,0,2,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}