tree grammar Eval2;
options
{
	language=CSharp3;
	tokenVocab=TestL;
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
computation returns [int value]
	: ^('+' a=computation b=computation) {$value = a+b;}
	| ^('-' a=computation b=computation) {$value = a-b;}
	| ^('*' a=computation b=computation) {$value = a*b;}
	| ^('/' a=computation b=computation) {$value = a/b;}
	| IDENTIFIER
	    {
		    int v = (int)memory[$IDENTIFIER.text];
		    if(v != null)
		    {
		    	$value = v;
		    }
		    else
		    {
		    	Console.WriteLine("undefined variable " + $IDENTIFIER.text);
		    }
	    }
	| CONSTANT {$value = Convert.ToInt32($CONSTANT.text);}
	;

public prog	:	 balance+;
balance	:	computation {Console.WriteLine($computation.value);}
	|	^('=' IDENTIFIER computation) {memory.Add($IDENTIFIER.text, Convert.ToInt32($computation.value));}
	;