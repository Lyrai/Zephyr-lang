grammar test;

Program: (Statement SEMICOLON?)*;
StatementList: (INDENT Statement SEMICOLON?)*;
Statement: 
    INDENT (Decl 
    | ClassDecl 
    | PrintStmt 
    | ReturnStmt 
    | Compound
    | IfStmt
    | WhileStmt
    | ForStmt
    | AssignExpr
);

Decl: 
    FuncDecl
    | PropertyDecl
    | VarDecl
;

ClassDecl: 'class' Id ('<' Id)? COLON ClassBody;
ClassBody: (INDENT (ClassDecl | Decl))*;

PrintStmt: 'print' AssignExpr;

ReturnStmt: 'return' (SEMICOLON | AssignExpr);

Compound: COLON StatementList;

IfStmt: 'if' AssignExpr 
    Statement 
    ('else' 
    Statement);

WhileStmt: 'while' AssignExpr Statement;

ForStmt: 'for' (VarDecl | Equality)? COMMA Equality COMMA AssignExpr 
    Statement;

FuncDecl: Type Id '(' FuncParameters ')' 
    Statement;

FuncParameters: VarDecl (COMMA VarDecl)*;

VarDecl: Type Id (ASSIGN Equality);

PropertyDecl: Type Id COLON ('get' Statement)? ('set' Statement);

AssignExpr: Id Equality (ASSIGN AssignExpr);
Equality: Comparison ((EQUAL | NOT_EQUAL) Comparison)*;
Comparison: Expression (
    (GREATER_EQUAL | GREATER | LESS_EQUAL | LESS)
    Expression)*;
Expression: Term ((PLUS | MINUS) Term)*;
Term: Factor ((DIVIDE | MULTIPLY) Factor)*;
Factor: (MINUS | PLUS | NOT) Factor | Call;
Call: Primary (('(' (')' | Equality (COMMA Equality)* ')')) | DOT Id)*;
Primary: Literal | Id | '(' Equality ')';
Literal: '"'.*?'"' | [0-9]+ (DOT [0-9]+);
Type: Id; 

INDENT: '\t' | ' '{4};
Id: [a-zA-Z_][1-9a-zA-Z_]*;
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
WS: [ \r\n] -> skip;