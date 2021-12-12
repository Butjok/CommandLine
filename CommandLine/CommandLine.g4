grammar CommandLine;

input: value? EOF;
value
    : Identifier value* #command
    | Null #null
    | (True | False) #boolean
    | Integer #integer
    | Real #real
    | String #string
    | LeftParenthesis value RightParenthesis #parenthesis
    | operator=(Minus | Exclamation | Tilde) value #unaryExpression
    | value operator=(Asterisk | ForwardSlash | Percent) value #multiplication
    | value operator=(Plus | Minus) value #summation
    | value operator=(DoubleAmpersand | DoubleVerticalBar) value #junction
    | Rgb r=value g=value b=value a=value? #color
    | Int2 x=value y=value #int2
    | Int3 x=value y=value z=value #int3
    | Float2 x=value y=value #float2
    | Float3 x=value y=value z=value #float3
    ;

Asterisk:           '*';
DoubleAmpersand:    '&&';
DoubleVerticalBar:  '||';
Exclamation:        '!';
False:              'false';
Float2:             'float2';
Float3:             'float3';
ForwardSlash:       '/';
Int2:               'int2';
Int3:               'int3';
LeftParenthesis:    '(';
LeftSquareBracket:  '[';
Minus:              '-';
Null:               'null';
Percent:            '%';
Plus:               '+';
Rgb:                'rgb';
RightParenthesis:   ')';
RightSquareBracket: ']';
True:               'true';
Tilde:              '~';

Identifier: [a-zA-Z_][a-zA-Z_0-9]* ('.' [a-zA-Z_][a-zA-Z_0-9]*)*;
Integer: '-'? INT;
Real: '-'? (INT '.' INT | '.' INT | INT '.'); 
String: '"' ('\\' ["\\bfnrt]  | ~ ["\\\u0000-\u001F])* '"';

Whitespace: [ \r\n\t]+ -> channel(HIDDEN);
fragment INT: [0-9]+;