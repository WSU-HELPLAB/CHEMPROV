tree grammar ChemProVTree;

options
{
	language=CSharp3;
	tokenVocab=ChemProV;
	ASTLabelType=CommonTree;
}
@header
{
	using System.Collections;
	using System;
	using System.Collections.Generic;
	using AntlrExpressionTest;
}
@members
{
	public List<ChemProVLine> lines = new List<ChemProVLine>();
	public Dictionary<string, EquationVariable> varList = new Dictionary<string, EquationVariable>();
	private int currentLineNumber = 0;
	
}
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
	| ^('/' left=computation right=computation) {Console.WriteLine("left: " + left.token + " right: " + right.value);}//$value = a/b;}
	| IDENTIFIER
	    {
	            if (lines.Count <= currentLineNumber)
	            {
	                lines.Add(new ChemProVLine());
	            }
	            if(!varList.ContainsKey($IDENTIFIER.text))
	            {
	            	varList.add($IDENTIFIER.text, new EquationVariable(){
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

