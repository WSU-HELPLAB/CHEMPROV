grammar ChemProV;

options
{
	language=CSharp3;
	output=AST;
	ASTLabelType=CommonTree;
}

tokens {
  VAR;   // variable definition
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

//IDs can be either standard variable names (ex: 'foo') or a string followed by a number followed by one or more numbers or characters
//(ex: 'M1' or 'm23' or 'm5Ab'
IDENTIFIER
	:	('a'..'z'|'A'..'Z')+ | ('a'..'z'|'A'..'Z')+ '0'..'9' ('a'..'z'|'A'..'Z'|'0'..'9')*;
	
//Allow both integer and floating-point constants
INTEGER
	:	'0'..'9'+;
FLOAT
	:	'0'..'9'+ '.' '0'..'9'+;
	
NEWLINE	:	'\r'? '\n';
WS	:	(' '|'\t'|'\n'|'\r')+ {Skip();};
public program	
	:	(line {Console.WriteLine($line.tree==null?"null":$line.tree.ToStringTree());} )+	
	;

line	
	:	variable
	|	balance
	;

variable
	:	'let' IDENTIFIER '=' computation NEWLINE -> ^(VAR IDENTIFIER computation)
	;

balance		
	:	computation '=' computation NEWLINE 	-> ^('=' computation computation)
	|	NEWLINE 				-> 
	;

computation
	:	term (('+'^ | '-'^) term)*;

term 
	:	atom (('*'^ | '/'^) atom)*
	;

atom
	:	INTEGER
	|   FLOAT
	|	IDENTIFIER
	|	'('! computation ')'!
	;
