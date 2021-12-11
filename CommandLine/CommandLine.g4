grammar CommandLine;

input: (command (Semicolon command)*)? EOF;
command: Identifier value*;
value
    : (True | False) #boolean
    | Integer #integer
    | Real #real
    | (Identifier | String) #string
    | Interpolation (Identifier | LeftCurlyBrace command RightCurlyBrace) #interpolation
    | LeftParenthesis value RightParenthesis #parenthesis
    | operator=(Minus | Exclamation) value #unaryExpression
    | value operator=(Asterisk | ForwardSlash | Percent) value #multiplication
    | value operator=(Plus | Minus) value #summation
    | value operator=(DoubleAmpersand | DoubleVerticalBar) value #junction
    | Rgb r=value g=value b=value a=value? #color
    | Int2 x=value y=value #int2
    ;

Asterisk:           '*';
DoubleAmpersand:    '&&';
DoubleVerticalBar:  '||';
Exclamation:        '!';
False:              'false';
ForwardSlash:       '/';
Int2:               'int2';
Interpolation:      '$';
LeftCurlyBrace:     '{';
LeftParenthesis:    '(';
LeftSquareBracket:  '[';
Minus:              '-';
Percent:            '%';
Plus:               '+';
Rgb:                'rgb';
RightCurlyBrace:    '}';
RightParenthesis:   ')';
RightSquareBracket: ']';
Semicolon:          ';';
True:               'true';

Identifier: [a-zA-Z_][a-zA-Z_0-9]* ('.' [a-zA-Z_][a-zA-Z_0-9]*)*;
Integer: '-'? INT;
Real: '-'? (INT '.' INT | '.' INT | INT '.'); 
String: '"' ('\\' ["\\bfnrt]  | ~ ["\\\u0000-\u001F])* '"';

Whitespace: [ \r\n\t]+ -> channel(HIDDEN);
fragment INT: [0-9]+;