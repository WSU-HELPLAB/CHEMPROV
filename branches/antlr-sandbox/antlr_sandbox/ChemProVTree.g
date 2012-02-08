tree grammar ChemProVTree;

options
{
	language=CSharp3;
	tokenVocab=ChemProv;
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
public program	
	:	 line+;

line	:	variable
	|	balance
	;

variable
	: ^(VAR IDENTIFIER computation)
	;

balance	
	:	^('=' computation computation) {}//memory.Add($IDENTIFIER.text, Convert.ToInt32($computation.value));}
	;	

computation
	: ^('+' a=computation b=computation) {}//$value = a+b;}
	| ^('-' a=computation b=computation) {}//$value = a-b;}
	| ^('*' a=computation b=computation) {}//$value = a*b;}
	| ^('/' a=computation b=computation) {}//$value = a/b;}
	| IDENTIFIER
	    /*{
	    	
		    int v = (int)memory[$IDENTIFIER.text];
		    if(v != null)
		    {
		    	$value = v;
		    }
		    else
		    {
		    	Console.WriteLine("undefined variable " + $IDENTIFIER.text);
		    }
		    
	    }*/
	| CONSTANT {}
	;

