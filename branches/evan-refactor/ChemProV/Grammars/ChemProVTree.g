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
	using ChemProV.PFD;
	using ChemProV.PFD.ProcessUnits;
	using ChemProV.PFD.Streams;
	using ChemProV.PFD.Streams.PropertiesWindow;
}
@members
{
	public List<Equation> Lines = new List<Equation>();
	public List<IPfdElement> PfdElements = new List<IPfdElement>();
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
	:	^('=' left=computation right=computation) {}//memory.Add($IDENTIFIER.text, Convert.ToInt32($computation.value));}
	;	

computation returns [int lexerType, int integerValue, int equationTokenId]
	: ^('+' left=computation right=computation) {}//$value = a+b;}
	| ^('-' left=computation right=computation) {}//$value = a-b;}
	| ^('*' left=computation right=computation) {}//$value = a*b;}
	| ^('/' left=computation right=computation) 
		{
		if(left.lexerType == IDENTIFIER && right.lexerType == INTEGER && right.integerValue == 100)
		{
			Lines[currentLineNumber].Tokens[left.equationTokenId].IsPercent = true;
		}
		}
	| IDENTIFIER
	    {
	            if (Lines.Count <= currentLineNumber)
	            {
	                Lines.Add(new Equation());
	            }
	            Variable token = new Variable($IDENTIFIER.text);
	            $equationTokenId = token.Id;
	            Lines[currentLineNumber].Tokens.Add($equationTokenId, token);
	            $lexerType = $IDENTIFIER.type;
	    }
	| INTEGER 
		{
			$lexerType = $INTEGER.type;
			Int32.TryParse($INTEGER.text, out $integerValue);
		}
	| FLOAT
		{
			$lexerType = $FLOAT.type;
		}
	;

