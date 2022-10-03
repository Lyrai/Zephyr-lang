grammar test;

@parser::members {
    int nestingLevel = 0;
}

program: (statement SEMICOLON?)* EOF;
statementList: (INDENT statement SEMICOLON?)*;
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

classDecl: 'class' ID ('<' ID)? COLON {nestingLevel++;} classBody;
classBody: (indent[1] (classDecl | decl))+;

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

indent[int i] returns [int r]: {i > 0}? INDENT indent[i - 1] | {$r = $i};

//NEWLINE: [\n]+;
//EMPTY: [\n];
WS: [ \n\r] -> skip;
INDENT: '    ';
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


//WS1: ' ' -> skip;
