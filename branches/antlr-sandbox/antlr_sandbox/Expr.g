grammar Expr;
options
{
	language=CSharp3;
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
	:	stat+;

stat	:	expr NEWLINE {Console.WriteLine($expr.value);}
	|	ID '=' expr NEWLINE {memory.Add($ID.text, Convert.ToInt32($expr.value));}
	|	NEWLINE
	;

expr returns [int value]
	:	e=multExpr {$value = $e.value;}
	( '+' e=multExpr {$value += $e.value;}
	| '-' e=multExpr {$value -= $e.value;}
	)*
	; //multExpr (('+' | '-') multExpr)*;

multExpr returns [int value]
	:	e=atom {$value = $e.value;} ('*' e=atom {$value *= $e.value;})
	;

atom returns [int value]
	:	INT {$value = Convert.ToInt32($INT.text);}
	|	ID
		{
			int v = (int)memory[$ID.text];
			if(v != null)
			{
				$value = v;
			}
			else
			{
				Console.WriteLine("undefined variable " + $ID.text);
			}
		}
	|	'(' expr ')' {$value = $expr.value;}
	;
