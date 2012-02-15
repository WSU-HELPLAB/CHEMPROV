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
	using System.Collections.Generic;
}
@members
{
	public List<ChemProVLine> lines = new List<ChemProVLine>();
	private int currentLineNumber = 0;
}
@namespace { ChemProV.Grammars }
public program	
	:	 (line 
	{
		currentLineNumber++;
	})+
	;

line	
	:	variable {}
	|	balance {}
	;

variable
	: ^(VAR IDENTIFIER computation)
	;

balance	
	:	^('=' computation computation) {}//memory.Add($IDENTIFIER.text, Convert.ToInt32($computation.value));}
	;	

computation returns [int token, int value]
	: ^('+' left=computation right=computation) {}//$value = a+b;}
	| ^('-' left=computation right=computation) {}//$value = a-b;}
	| ^('*' left=computation right=computation) {}//$value = a*b;}
	| ^('/' left=computation right=computation) {}//$value = a/b;}
	| IDENTIFIER
	    {
	            if (lines.Count <= currentLineNumber)
	            {
	                lines.Add(new ChemProVLine());
	            }
	            lines[currentLineNumber].VariablesUsed.Add($IDENTIFIER.text);
	            $token = $IDENTIFIER.type;
	    }
	| INTEGER 
		{
			$token = $INTEGER.type;
			Int32.TryParse($INTEGER.text, out $value);
		}
	| FLOAT
		{
			$token = $FLOAT.type;
		}
	;

