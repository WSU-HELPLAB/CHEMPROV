grammar EasyExpr;
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
ID	:	('a'..'z'|'A'..'Z')+;
INT	:	'0'..'9'+;
NEWLINE	:	'\r'? '\n';
WS	:	(' '|'\t'|'\n'|'\r')+ {Skip();};
public prog	
	:	(stat {Console.WriteLine($stat.tree==null?"null":$stat.tree.ToStringTree());} )+
	;

stat	:	expr NEWLINE 		-> expr
	|	ID '=' expr NEWLINE 	-> ^('=' ID expr)
	|	NEWLINE 		-> 
	;

expr
	: multExpr (('+'^ | '-'^) multExpr)*;

multExpr 
	:	atom('*'^ atom)*
	;

atom returns [int value]
	:	INT
	|	ID
	|	'('! expr ')'!
	
	;
