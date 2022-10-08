parser grammar test;

options { tokenVocab = testLexer; }

program: statementList EOF;
statementList: (statement SEMICOLON?)*;
statement: 
    (decl 
    | classDecl 
    | printStmt 
    | returnStmt 
    | compound
    | ifStmt
    | whileStmt
    | forStmt
    | assignExpr
);

decl: type ID
    (funcDecl
    //| COLON (INDENT 'get' statement)? (INDENT 'set' statement)?
    | propertyDecl
    | varDecl
);

varDecl1: type ID;

classDecl: 'class' ID ('<' ID)? COLON INDENT classBody DEDENT;
classBody: ((classDecl | decl))+;

printStmt: 'print' assignExpr;

returnStmt: 'return' (SEMICOLON | assignExpr);

compound: COLON INDENT statementList DEDENT;

ifStmt: 'if' assignExpr 
    statement 
    ('else' 
    statement);

whileStmt: 'while' assignExpr statement;

forStmt: 'for' (varDecl | equality)? COMMA equality COMMA assignExpr 
    statement;

funcDecl: '(' funcParameters? ')' 
    statement;

funcParameters: varDecl1 (COMMA varDecl1)*;

varDecl: (ASSIGN equality)?;

propertyDecl: COLON ('get' statement)? ('set' statement);

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
literal: '"'.*?'"' | INT | FLOAT;
type: ID;
