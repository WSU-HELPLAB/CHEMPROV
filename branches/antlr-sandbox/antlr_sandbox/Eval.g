tree grammar Eval;
options
{
	language=CSharp3;
	tokenVocab=EasyExpr;
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
expr returns [int value]
	: ^('+' a=expr b=expr) {$value = a+b;}
	| ^('-' a=expr b=expr) {$value = a-b;}
	| ^('*' a=expr b=expr) {$value = a*b;}
	| ID
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
	| INT {$value = Convert.ToInt32($INT.text);}
	;

public prog	:	 stat+;
stat	:	expr {Console.WriteLine($expr.value);}
	|	^('=' ID expr) {memory.Add($ID.text, Convert.ToInt32($expr.value));}
	;