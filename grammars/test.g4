parser grammar test;

options { tokenVocab = testLexer; }

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
//    | propertyDecl
    | varDecl
);

classBodyDecl:
    (funcDecl
//    | propertyDecl
    | typedVarDecl
    | classDecl
);

typedVarDecl: ID ':' type;

classDecl: 'class' ID ('<' ID)? classBody 'end';
classBody: (classBodyDecl)*;

printStmt: 'print' assignExpr;

returnStmt: 'return' assignExpr?;

compound: '{' statementList '}';

ifStmt: 'if' assignExpr 
    statement 
    ('else' 
    statement)?;

whileStmt: 'while' assignExpr statement;

forStmt: 'for' varDecl COMMA equality COMMA assignExpr 
    statement;

funcDecl: 'fn' ID '(' funcParameters? ')' ('->' type)?
    statementList END;

funcParameters: typedVarDecl (COMMA typedVarDecl)*;

varDecl: 'let' ID ':' type (ASSIGN assignExpr)?;

//propertyDecl: 'property' typedVarDecl (('get' statementList END) ('set' statementList END)? | ('set' statementList END) ('get' statementList END)?) ;

//expression: equality (ASSIGN expression)?;
//
//equality: 
//      equality (EQUAL | NOT_EQUAL) equality # eq
//    | equality (GREATER_EQUAL | GREATER | LESS_EQUAL | LESS) equality # cmp
//    | equality (PLUS | MINUS) equality # add
//    | equality (DIVIDE | MULTIPLY) equality # mul
//    | factor # fact
//;
assignExpr: equality (ASSIGN assignExpr)?;
equality: comparison ((EQUAL | NOT_EQUAL) comparison)*;
comparison: expression (
    (GREATER_EQUAL | GREATER | LESS_EQUAL | LESS)
    expression)*;
expression: term ((PLUS | MINUS) term)*;
term: factor ((DIVIDE | MULTIPLY) factor)*;
factor: (MINUS | PLUS | NOT) factor | call;
call: primary (('(' (')' | equality (COMMA equality)* ')')) | DOT ID)*;
primary: literal | ID | '(' equality ')';
literal: STRING_LITERAL | INT | FLOAT;
type: ID;
