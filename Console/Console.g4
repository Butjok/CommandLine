grammar Console;

input: command? EOF;
command: Identifier argument*;
argument: value|Identifier ':' value;
value
    : Integer #integer
    | (True|False) #boolean
    | Real #real
    | (Identifier|String) #string
    | Interpolation (Identifier | LeftCurlyBrace command RightCurlyBrace) #interpolation
    | LeftParenthesis value RightParenthesis #parenthesis
    | operator=(Minus|Exclamation) value #unaryExpression
    | value operator=(Asterisk|Slash|Percent) value #multiplication
    | value operator=(Plus|Minus) value #summation
    | value operator=(DoubleAmpersand|DoubleVerticalBar) value #junction
    | Rgb r=value g=value b=value a=value? #color
    | Int2 x=value y=value #int2
    //| LeftSquareBracket value* RightSquareBracket #
    ;

True:'true';
False:'false';
Rgb:'rgb';
Int2:'int2';
Interpolation:'$';
LeftCurlyBrace: '{';
RightCurlyBrace: '}';
LeftParenthesis: '(';
RightParenthesis: ')';
LeftSquareBracket: '[';
RightSquareBracket: ']';
Minus: '-';
Exclamation: '!';
Asterisk: '*';
Slash: '/';
Percent: '%';
Plus: '+';
DoubleAmpersand: '&&';
DoubleVerticalBar: '||';
Identifier: [a-zA-Z_][a-zA-Z_0-9]* ('.' [a-zA-Z_][a-zA-Z_0-9]*)*;

String: '"' ('\\' ["\\bfnrt]  | ~ ["\\\u0000-\u001F])* '"';

Real: '-'? (INT '.' INT | '.' INT | INT '.'); 
Integer: '-'? INT;
Whitespace: [ \r\n\t]+ -> channel(HIDDEN);
fragment INT: [0-9]+;