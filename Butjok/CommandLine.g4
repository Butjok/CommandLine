grammar CommandLine;

colorScheme: style* EOF;
style: string color boolean boolean; 

input: statement* EOF;
statement: noOperation	| command;
noOperation: Semicolon;
command: Name value*;
	
value
	: boolean
	| integer
	| real
	| string
	| int2
	| color
	| variable
	;
	
variable: Name;
boolean: True|False; 
integer: Integer;
real: Real | integer;
string:  DoubleQuotedString;
int2: Vector2Int LeftParenthesis integer Comma integer RightParenthesis;
color: shortHexRgbColor | longHexRgbColor | shortHexRgbaColor | longHexRgbaColor | rgbColor | rgbaColor;
shortHexRgbColor: ShortHexRgbColor;
shortHexRgbaColor: ShortHexRgbaColor;
longHexRgbColor: LongHexRgbColor;
longHexRgbaColor: LongHexRgbaColor;

// Integer value 0-255 or float value 0-1
colorComponent: integer | real;

// RGB color: Color(colorComponent, colorComponent, colorComponent)
rgbColor: Color LeftParenthesis colorComponent Comma colorComponent Comma colorComponent RightParenthesis;
// RGBA color: Color(colorComponent, colorComponent, colorComponent, colorComponent)
rgbaColor: Color LeftParenthesis colorComponent Comma colorComponent Comma colorComponent Comma colorComponent RightParenthesis;

Semicolon: ';';
LeftParenthesis: '(';
RightParenthesis: ')';
Comma: ',';
Vector2Int: 'Vector2Int';
Color: 'Color';
True: 'true';
False: 'false';
Integer: '-'? [0-9]+;
Real: '-'? ([0-9]* '.' [0-9]+ | [0-9]+ '.');
DoubleQuotedString: '"' ('\\' ["\\nrt] | ~["\\\u0000-\u001F])* '"';
Name: Word ('.' Word)*;
ShortHexRgbColor: '#' HexadecimalDigit HexadecimalDigit HexadecimalDigit;
ShortHexRgbaColor: '#' HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit;
LongHexRgbColor: '#' HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit;
LongHexRgbaColor: '#' HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit HexadecimalDigit;

fragment Word: [a-zA-Z_][a-zA-Z_0-9]*;
fragment HexadecimalDigit: [0-9a-fA-F];

SingleLineComment: '//' ~[\r\n]* -> channel(HIDDEN);
BlockComment: '/*' .*? '*/' -> channel(HIDDEN);
Whitespace: [ \r\n\t]+ -> channel(HIDDEN);
