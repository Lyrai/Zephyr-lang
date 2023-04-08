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

typedVarDecl: Name=ID ':' Type=ID;

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

funcDecl: 'fn' Name=ID (':' funcParameters | '!') ('->' Type=ID)?
    Body=statementList END;

funcParameters: Parameters+=typedVarDecl (COMMA Paramters+=typedVarDecl)*;
funcArguments: Args+=equality (',' Args+=equality)*;

varDecl: 'let' Name=ID (':' (Type=ID | ArrayType=arrayType))? (ASSIGN assignExpr)?;

//propertyDecl: 'property' typedVarDecl (('get' statementList END) ('set' statementList END)? | ('set' statementList END) ('get' statementList END)?) ;

assignExpr: Lhs=equality (ASSIGN Rhs=equality)?;

equality:
      '(' Inner=equality ')'
    | Left=equality Op=(DIVIDE | MULTIPLY) Right=equality
    | Left=equality Op=(PLUS | MINUS) Right=equality
    | Left=equality Op=(GREATER_EQUAL | GREATER | LESS_EQUAL | LESS) Right=equality
    | Left=equality Op=(EQUAL | NOT_EQUAL) Right=equality
    | factor
;

indexer: ID LBRACKET equality RBRACKET;
arrayInitializer: LBRACKET Exprs+=equality (',' Exprs+=equality)* RBRACKET;
arrayType: '[' ID ']';

factor: Op=(MINUS | PLUS | NOT) factor | call;
call: call ((':' funcArguments ';'?) | '!' | DOT ID) | primary;
primary: literal | ID | indexer | compound | ifStmt | arrayInitializer;
literal: StringLit=STRING_LITERAL | Int=INT | Float=FLOAT | True='true' | False='false';
