grammar TestL;
options
{
	language=CSharp3;
	output=AST;
	ASTLabelType=CommonTree;
}

@header
{
	using System.Collections;
	using System;
}
@members
{
	Hashtable memory = new Hashtable();
}
IDENTIFIER
	:	('a'..'z'|'A'..'Z')+ | 'a'..'z'+ '0'..'9' ('a'..'z'|'0'..'9')*;
CONSTANT
	:	'0'..'9'+;


NEWLINE	:	'\r'? '\n';
WS	:	(' '|'\t'|'\n'|'\r')+ {Skip();};
public prog	
	:	(balance {Console.WriteLine($balance.tree==null?"null":$balance.tree.ToStringTree());} )+
	;

balance	:	computation NEWLINE 		-> computation
	|	IDENTIFIER '=' computation NEWLINE 	-> ^('=' IDENTIFIER computation)
	|	NEWLINE 		-> 
	;

computation
	:	term (('+'^ | '-'^) term)*;

term 
	:	atom(('*'^ | '/'^) atom)*
	;

atom returns [int value]
	:	CONSTANT
	|	IDENTIFIER
	|	'('! computation ')'!
	
	;
