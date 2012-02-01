grammar T;
options
{
	language=CSharp3;
}
r	:	 'call' ID ';' {System.out.println("invoke"+$ID.text);};
ID	:	 'a'..'z'+;
WS	:	 (' '|'\n'|'\r')+ {$channel=HIDDEN;};