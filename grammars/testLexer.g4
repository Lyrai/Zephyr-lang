﻿lexer grammar testLexer;

tokens { STRING_LITERAL }

WS: [ \r\t\n] -> skip;
CLASS: 'class';
END: 'end';
PRINT: 'print';
RETURN: 'return';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
FOR: 'for';
GET: 'get';
SET: 'set';
FN: 'fn';
LET: 'let';
TRUE: 'true';
FALSE: 'false';
ID: [a-zA-Z_][1-9a-zA-Z_]*;
ARROW: '->';
SEMICOLON: ';';
COLON: ':';
COMMA: ',';
DOT: '.';
LBRACE: '{';
RBRACE: '}';
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
LPAR: '(';
RPAR: ')';
DOUBLE_QUOTE: '"' -> more, mode(StringLiteral);
INT: [0-9]+;
FLOAT: [0-9]+ DOT [0-9]+;

mode StringLiteral;
CLOSING_QUOTE: '"' -> type(STRING_LITERAL), mode(DEFAULT_MODE);
ANY: . -> more;