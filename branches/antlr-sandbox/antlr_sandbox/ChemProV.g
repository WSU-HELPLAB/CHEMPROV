grammar ChemProV;
options
{
	language=CSharp3;
}
//Ignore whitespace?
WS	:	 (' '|'\n'|'\r')+ {$channel=HIDDEN;};

//constants can be either constants or whole numbers
Constant:	'0'..'9'+ | '0'..'9'* '.' '0'..'9'+;

//mathematical operators
Op	:	'+' | '-' | '*' | '/';

//mathematical variables or streams of chemicals/elements
Identifier 
	:	'a'..'z'+ | 'a'..'z'+ '0'..'9' ('a'..'z'|'0'..'9')+;	
term	:	Identifier | Constant;
computation
	:	(term Op)+ term;
materialBalance
	:	computation '=' computation | 
		computation '=' term | 
		Identifier '=' computation | 
		Identifier '=' Constant;