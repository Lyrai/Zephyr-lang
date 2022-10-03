grammar test;

program: (statement SEMICOLON?)* EOF;
statementList: (INDENT statement SEMICOLON?)*;
statement: 
    INDENT (decl 
    | classDecl 
    | printStmt 
    | returnStmt 
    | compound
    | ifStmt
    | whileStmt
    | forStmt
    | assignExpr
);

decl: 
    funcDecl
    | propertyDecl
    | varDecl
;

classDecl: 'class' ID ('<' ID)? COLON classBody;
classBody: (INDENT (classDecl | decl))*;

printStmt: 'print' assignExpr;

returnStmt: 'return' (SEMICOLON | assignExpr);

compound: COLON statementList;

ifStmt: 'if' assignExpr 
    statement 
    ('else' 
    statement);

whileStmt: 'while' assignExpr statement;

forStmt: 'for' (varDecl | equality)? COMMA equality COMMA assignExpr 
    statement;

funcDecl: type ID '(' funcParameters ')' 
    statement;

funcParameters: varDecl (COMMA varDecl)*;

varDecl: type ID (ASSIGN equality);

propertyDecl: type ID COLON ('get' statement)? ('set' statement);

assignExpr: ID equality (ASSIGN assignExpr);
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

INDENT: '\t' | ' '{4};
ID: [a-zA-Z_][1-9a-zA-Z_]*;
COLON: ':';
SEMICOLON: ';';
COMMA: ',';
DOT: '.';
ASSIGN: '=';
EQUAL: '==';
NOT_EQUAL: '!=';
GREATER_EQUAL: '>=';
GREATER: '>';
LESS_EQUAL: '<=';
LESS: '<';
PLUS: '+';
MINUS: '-';
DIVIDE: '/';
MULTIPLY: '*';
NOT: '!';
INT: [0-9]+;
FLOAT: [0-9]+ DOT [0-9]+;
WS: [ \r\n] -> skip;
