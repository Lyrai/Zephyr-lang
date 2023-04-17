parser grammar ZephyrParser;

options { tokenVocab = ZephyrLexer; }

program: statementList EOF;
statementList: (statement ';'?)*;
statement: 
    (decl
    | printStmt 
    | returnStmt 
    | compound
    | ifStmt
    | whileStmt
    | forStmt
    | assignExpr
    | useStmt
);

decl: 
    (classDecl
    | funcDecl
    | varDecl
);

classBodyDecl:
    (funcDecl
//    | propertyDecl
    | typedVarDecl
    | classDecl
);

typedVarDecl: Name=ID Type=type;
optionallyTypedVarDecl: Name=ID (Type=type)?;
useStmt: USE Namespace=namespace;
namespace: ID ('.' ID)*;

classDecl: 'class' Name=ID ('<' Base=ID)? classBody 'end';
classBody: (classBodyDecl)*;

printStmt: 'print' equality;

returnStmt: 'return' equality?;

compound: '{' statementList '}';

ifStmt: 'if' Condition=equality 
    ThenBranch=statement 
    ('else' 
    ElseBranch=statement)?;

whileStmt: 'while' Condition=equality Body=statement;

forStmt: 'for' Initializer=varDecl COMMA Condition=equality COMMA PostAction=assignExpr 
    Body=statement;

funcDecl: 'fn' Name=ID (':' funcParameters | '!') ('->' Type=type)?
    Body=statement;

funcParameters: Parameters+=typedVarDecl (COMMA Paramters+=typedVarDecl)*;
funcArguments: Args+=factor (',' Args+=factor)*;

varDecl: 'let' optionallyTypedVarDecl (ASSIGN assignExpr)?;

//propertyDecl: 'property' typedVarDecl (('get' statementList END) ('set' statementList END)? | ('set' statementList END) ('get' statementList END)?) ;

assignExpr: Lhs=equality (ASSIGN Rhs=equality)?;

equality: 
    Expr=equality LBRACKET Index=equality RBRACKET
    | Left=equality Op=(DIVIDE | MULTIPLY) Right=equality
    | Left=equality Op=(PLUS | MINUS) Right=equality
    | Left=equality Op=(GREATER_EQUAL | GREATER | LESS_EQUAL | LESS) Right=equality
    | Left=equality Op=(EQUAL | NOT_EQUAL) Right=equality
    | factor
    | Caller=equality Callee=ID Call=call?
    | Callee=ID Call=call?
;

arrayInitializer: LBRACKET Exprs+=equality (',' Exprs+=equality)* RBRACKET;
arrayType: '[' ID ']';
lambda: '[' Params+=optionallyTypedVarDecl (',' Params+=optionallyTypedVarDecl)* '|' statement ']';

type: ID | arrayType;

factor: Op=(MINUS | PLUS | NOT) factor | primary;
call: (':' funcArguments ';'? ) | '!';
primary: literal | arrayInitializer | ID | compound | ifStmt | lambda | namespace | '(' Inner=equality ')';
literal: StringLit=STRING_LITERAL | Int=INT | Float=FLOAT | True='true' | False='false';
