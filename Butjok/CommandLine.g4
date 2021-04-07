grammar CommandLine;

// Style files

styles: style* EOF;
style: (string | Identifier) color boolean boolean boolean; 

// Commands grammar.

commands: statement* EOF;
statement: noOperation	| command;
noOperation: Semicolon;
command: Identifier value*;
	
value
	: boolean
	| integer
	| real
	| string
	| int2
	| color
	| variable
	| null
	;
	
variable: Identifier;
null: Null;
boolean: True | False; 
integer: Integer;
real: Real | integer;
string:  DoubleQuotedString;
int2: Vector2Int LeftParenthesis integer Comma integer RightParenthesis;
color: shortHexRgbColor | longHexRgbColor | shortHexRgbaColor | longHexRgbaColor | rgbColor | rgbaColor;
shortHexRgbColor: ShortHexRgbColor;
shortHexRgbaColor: ShortHexRgbaColor;
longHexRgbColor: LongHexRgbColor;
longHexRgbaColor: LongHexRgbaColor;

colorComponent: integer | real;
rgbColor: Color LeftParenthesis? colorComponent Comma? colorComponent Comma? colorComponent RightParenthesis?;
rgbaColor: Color LeftParenthesis? colorComponent Comma? colorComponent Comma? colorComponent Comma? colorComponent RightParenthesis?;

// Tokens.

Null: 'null';
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
Identifier: Word ('.' Word)*;
ShortHexRgbColor: '#' Hex Hex Hex;
ShortHexRgbaColor: '#' Hex Hex Hex Hex;
LongHexRgbColor: '#' Hex Hex Hex Hex Hex Hex;
LongHexRgbaColor: '#' Hex Hex Hex Hex Hex Hex Hex Hex;

fragment Word: [a-zA-Z_][a-zA-Z_0-9]*;
fragment Hex: [0-9a-fA-F];

// These go into HIDDEN channel so they don't disappear and we get them after lexer does its job.
SingleLineComment: '//' ~[\r\n]* -> channel(HIDDEN);
BlockComment: '/*' .*? '*/' -> channel(HIDDEN);
Whitespace: [ \r\n\t]+ -> channel(HIDDEN);
