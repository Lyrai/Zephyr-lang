lexer grammar testLexer;

tokens { INDENT, DEDENT }

@header {
using System.Collections.Generic;
}

@members {
Stack<int> _indents = new Stack<int>();
int _currWSCount = 0;
}

@init {
_indents.Push(0);
}

NEWLINE: '\n' {Console.WriteLine("Enetered newline mode"); if(_indents.Count == 0) _indents.Push(0);} -> skip, mode(startLine);
WS: [ \n\r] -> skip;
CLASS: 'class';
PRINT: 'print';
RETURN: 'return';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
FOR: 'for';
GET: 'get';
SET: 'set';
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
LPAR: '(';
RPAR: ')';
DOUBLE_QUOTE: '"';
INT: [0-9]+;
FLOAT: [0-9]+ DOT [0-9]+;

mode startLine;
INDENT1: ' ' { _currWSCount++; /*Console.WriteLine("Found whitespace");*/} -> skip;
EMPTY_LINE: [\r\n] {_currWSCount = 0;} -> skip, mode(DEFAULT_MODE);
ANY_LESS: {_currWSCount < _indents.Peek()}? {_indents.Pop(); _currWSCount = 0; /*Console.WriteLine("Exiting with dedent");*/} -> type(DEDENT), mode(DEFAULT_MODE);
ANY_MORE: {_currWSCount > _indents.Peek()}? {_indents.Push(_currWSCount); _currWSCount = 0; /*Console.WriteLine("Exiting with indent");*/} -> type(INDENT), mode(DEFAULT_MODE); 
ANY: {} -> skip, mode(DEFAULT_MODE);   
/*ANY: . {
if(_currWSCount < _indents.Peek()) 
{
    _indents.Pop(); 
    Console.WriteLine("Exiting with dedent");
    Emit(new CommonToken(DEDENT));
} 
else if(_currWSCount > _indents.Peek()) 
{
    _indents.Push(_currWSCount); 
    Console.WriteLine("Exiting with indent");
    Emit(new CommonToken(INDENT));
}
_currWSCount = 0;
} -> more, mode(DEFAULT_MODE);*/