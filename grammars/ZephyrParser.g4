parser grammar ZephyrParser;

options { tokenVocab = ZephyrLexer; }

program: statementList EOF;
statementList: (statement SEMICOLON?)*;
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

printStmt: 'print' assignExpr;

returnStmt: 'return' assignExpr?;

compound: '{' statementList '}';

ifStmt: 'if' Condition=assignExpr 
    ThenBranch=statement 
    ('else' 
    ElseBranch=statement)?;

whileStmt: 'while' Condition=assignExpr Body=statement;

forStmt: 'for' Initializer=varDecl COMMA Condition=equality COMMA PostAction=assignExpr 
    Body=statement;

funcDecl: 'fn' Name=ID '(' funcParameters? ')' ('->' Type=ID)?
    Body=statementList END;

funcParameters: Parameters+=typedVarDecl (COMMA Paramters+=typedVarDecl)*;
funcArguments: Args+=equality (',' Args+=equality)*;

varDecl: 'let' Name=ID (':' Type=ID)? (ASSIGN assignExpr)?;

//propertyDecl: 'property' typedVarDecl (('get' statementList END) ('set' statementList END)? | ('set' statementList END) ('get' statementList END)?) ;

assignExpr: equality (ASSIGN assignExpr)?;

equality:
      '(' Inner=equality ')'
    | Left=equality Op=(EQUAL | NOT_EQUAL) Right=equality
    | Left=equality Op=(GREATER_EQUAL | GREATER | LESS_EQUAL | LESS) Right=equality
    | Left=equality Op=(PLUS | MINUS) Right=equality
    | Left=equality Op=(DIVIDE | MULTIPLY) Right=equality
    | factor
;

factor: Op=(MINUS | PLUS | NOT) factor | call;
call: call (('(' funcArguments? ')') | DOT ID) | primary;
primary: literal | ID;
literal: StringLit=STRING_LITERAL | Int=INT | Float=FLOAT | True='true' | False='false';
