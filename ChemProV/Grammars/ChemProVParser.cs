//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 3.4
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// $ANTLR 3.4 E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g 2012-02-22 12:12:02

// The variable 'variable' is assigned but its value is never used.
#pragma warning disable 219
// Unreachable code detected.
#pragma warning disable 162
// Missing XML comment for publicly visible type or member 'Type_or_Member'
#pragma warning disable 1591
// CLS compliance checking will not be performed on 'type' because it is not visible from outside this assembly.
#pragma warning disable 3019


	using System.Collections;
	using System;


using System.Collections.Generic;
using Antlr.Runtime;
using Antlr.Runtime.Misc;


using Antlr.Runtime.Tree;
using RewriteRuleITokenStream = Antlr.Runtime.Tree.RewriteRuleTokenStream;

namespace  ChemProV.Grammars 
{
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "3.4")]
[System.CLSCompliant(false)]
public partial class ChemProVParser : Antlr.Runtime.Parser
{
	internal static readonly string[] tokenNames = new string[] {
		"<invalid>", "<EOR>", "<DOWN>", "<UP>", "FLOAT", "IDENTIFIER", "INTEGER", "NEWLINE", "VAR", "WS", "'('", "')'", "'*'", "'+'", "'-'", "'/'", "'='", "'let'"
	};
	public const int EOF=-1;
	public const int T__10=10;
	public const int T__11=11;
	public const int T__12=12;
	public const int T__13=13;
	public const int T__14=14;
	public const int T__15=15;
	public const int T__16=16;
	public const int T__17=17;
	public const int FLOAT=4;
	public const int IDENTIFIER=5;
	public const int INTEGER=6;
	public const int NEWLINE=7;
	public const int VAR=8;
	public const int WS=9;

	public ChemProVParser(ITokenStream input)
		: this(input, new RecognizerSharedState())
	{
	}
	public ChemProVParser(ITokenStream input, RecognizerSharedState state)
		: base(input, state)
	{
		ITreeAdaptor treeAdaptor = default(ITreeAdaptor);
		CreateTreeAdaptor(ref treeAdaptor);
		TreeAdaptor = treeAdaptor ?? new CommonTreeAdaptor();
		OnCreated();
	}
	// Implement this function in your helper file to use a custom tree adaptor
	partial void CreateTreeAdaptor(ref ITreeAdaptor adaptor);

	private ITreeAdaptor adaptor;

	public ITreeAdaptor TreeAdaptor
	{
		get
		{
			return adaptor;
		}

		set
		{
			this.adaptor = value;
		}
	}

	public override string[] TokenNames { get { return ChemProVParser.tokenNames; } }
	public override string GrammarFileName { get { return "E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g"; } }




	partial void OnCreated();
	partial void EnterRule(string ruleName, int ruleIndex);
	partial void LeaveRule(string ruleName, int ruleIndex);

	#region Rules
	partial void EnterRule_program();
	partial void LeaveRule_program();

	// $ANTLR start "program"
	// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:38:8: public program : ( line )+ ;
	[GrammarRule("program")]
	public AstParserRuleReturnScope<CommonTree, IToken> program()
	{
		EnterRule_program();
		EnterRule("program", 1);
		TraceIn("program", 1);
		AstParserRuleReturnScope<CommonTree, IToken> retval = new AstParserRuleReturnScope<CommonTree, IToken>();
		retval.Start = (IToken)input.LT(1);

		CommonTree root_0 = default(CommonTree);

		AstParserRuleReturnScope<CommonTree, IToken> line1 = default(AstParserRuleReturnScope<CommonTree, IToken>);

		try { DebugEnterRule(GrammarFileName, "program");
		DebugLocation(38, 1);
		try
		{
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:39:2: ( ( line )+ )
			DebugEnterAlt(1);
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:39:4: ( line )+
			{
			root_0 = (CommonTree)adaptor.Nil();

			DebugLocation(39, 4);
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:39:4: ( line )+
			int cnt1=0;
			try { DebugEnterSubRule(1);
			while (true)
			{
				int alt1=2;
				try { DebugEnterDecision(1, false);
				int LA1_0 = input.LA(1);

				if (((LA1_0>=FLOAT && LA1_0<=NEWLINE)||LA1_0==10||LA1_0==17))
				{
					alt1 = 1;
				}


				} finally { DebugExitDecision(1); }
				switch (alt1)
				{
				case 1:
					DebugEnterAlt(1);
					// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:39:5: line
					{
					DebugLocation(39, 5);
					PushFollow(Follow._line_in_program188);
					line1=line();
					PopFollow();

					adaptor.AddChild(root_0, line1.Tree);
					DebugLocation(39, 10);


					}
					break;

				default:
					if (cnt1 >= 1)
						goto loop1;

					EarlyExitException eee1 = new EarlyExitException( 1, input );
					DebugRecognitionException(eee1);
					throw eee1;
				}
				cnt1++;
			}
			loop1:
				;

			} finally { DebugExitSubRule(1); }


			}

			retval.Stop = (IToken)input.LT(-1);

			retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
			adaptor.SetTokenBoundaries(retval.Tree, retval.Start, retval.Stop);

		}
		catch (RecognitionException re)
		{
			ReportError(re);
			Recover(input,re);
		retval.Tree = (CommonTree)adaptor.ErrorNode(input, retval.Start, input.LT(-1), re);

		}
		finally
		{
			TraceOut("program", 1);
			LeaveRule("program", 1);
			LeaveRule_program();
		}
		DebugLocation(40, 1);
		} finally { DebugExitRule(GrammarFileName, "program"); }
		return retval;

	}
	// $ANTLR end "program"

	partial void EnterRule_line();
	partial void LeaveRule_line();

	// $ANTLR start "line"
	// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:42:1: line : ( variable | balance );
	[GrammarRule("line")]
	private AstParserRuleReturnScope<CommonTree, IToken> line()
	{
		EnterRule_line();
		EnterRule("line", 2);
		TraceIn("line", 2);
		AstParserRuleReturnScope<CommonTree, IToken> retval = new AstParserRuleReturnScope<CommonTree, IToken>();
		retval.Start = (IToken)input.LT(1);

		CommonTree root_0 = default(CommonTree);

		AstParserRuleReturnScope<CommonTree, IToken> variable2 = default(AstParserRuleReturnScope<CommonTree, IToken>);
		AstParserRuleReturnScope<CommonTree, IToken> balance3 = default(AstParserRuleReturnScope<CommonTree, IToken>);

		try { DebugEnterRule(GrammarFileName, "line");
		DebugLocation(42, 1);
		try
		{
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:43:2: ( variable | balance )
			int alt2=2;
			try { DebugEnterDecision(2, false);
			int LA2_0 = input.LA(1);

			if ((LA2_0==17))
			{
				alt2 = 1;
			}
			else if (((LA2_0>=FLOAT && LA2_0<=NEWLINE)||LA2_0==10))
			{
				alt2 = 2;
			}
			else
			{
				NoViableAltException nvae = new NoViableAltException("", 2, 0, input);
				DebugRecognitionException(nvae);
				throw nvae;
			}
			} finally { DebugExitDecision(2); }
			switch (alt2)
			{
			case 1:
				DebugEnterAlt(1);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:43:4: variable
				{
				root_0 = (CommonTree)adaptor.Nil();

				DebugLocation(43, 4);
				PushFollow(Follow._variable_in_line206);
				variable2=variable();
				PopFollow();

				adaptor.AddChild(root_0, variable2.Tree);

				}
				break;
			case 2:
				DebugEnterAlt(2);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:44:4: balance
				{
				root_0 = (CommonTree)adaptor.Nil();

				DebugLocation(44, 4);
				PushFollow(Follow._balance_in_line211);
				balance3=balance();
				PopFollow();

				adaptor.AddChild(root_0, balance3.Tree);

				}
				break;

			}
			retval.Stop = (IToken)input.LT(-1);

			retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
			adaptor.SetTokenBoundaries(retval.Tree, retval.Start, retval.Stop);

		}
		catch (RecognitionException re)
		{
			ReportError(re);
			Recover(input,re);
		retval.Tree = (CommonTree)adaptor.ErrorNode(input, retval.Start, input.LT(-1), re);

		}
		finally
		{
			TraceOut("line", 2);
			LeaveRule("line", 2);
			LeaveRule_line();
		}
		DebugLocation(45, 1);
		} finally { DebugExitRule(GrammarFileName, "line"); }
		return retval;

	}
	// $ANTLR end "line"

	partial void EnterRule_variable();
	partial void LeaveRule_variable();

	// $ANTLR start "variable"
	// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:47:1: variable : 'let' IDENTIFIER '=' computation NEWLINE -> ^( VAR IDENTIFIER computation ) ;
	[GrammarRule("variable")]
	private AstParserRuleReturnScope<CommonTree, IToken> variable()
	{
		EnterRule_variable();
		EnterRule("variable", 3);
		TraceIn("variable", 3);
		AstParserRuleReturnScope<CommonTree, IToken> retval = new AstParserRuleReturnScope<CommonTree, IToken>();
		retval.Start = (IToken)input.LT(1);

		CommonTree root_0 = default(CommonTree);

		IToken string_literal4 = default(IToken);
		IToken IDENTIFIER5 = default(IToken);
		IToken char_literal6 = default(IToken);
		IToken NEWLINE8 = default(IToken);
		AstParserRuleReturnScope<CommonTree, IToken> computation7 = default(AstParserRuleReturnScope<CommonTree, IToken>);

		CommonTree string_literal4_tree = default(CommonTree);
		CommonTree IDENTIFIER5_tree = default(CommonTree);
		CommonTree char_literal6_tree = default(CommonTree);
		CommonTree NEWLINE8_tree = default(CommonTree);
		RewriteRuleITokenStream stream_NEWLINE=new RewriteRuleITokenStream(adaptor,"token NEWLINE");
		RewriteRuleITokenStream stream_17=new RewriteRuleITokenStream(adaptor,"token 17");
		RewriteRuleITokenStream stream_IDENTIFIER=new RewriteRuleITokenStream(adaptor,"token IDENTIFIER");
		RewriteRuleITokenStream stream_16=new RewriteRuleITokenStream(adaptor,"token 16");
		RewriteRuleSubtreeStream stream_computation=new RewriteRuleSubtreeStream(adaptor,"rule computation");
		try { DebugEnterRule(GrammarFileName, "variable");
		DebugLocation(47, 1);
		try
		{
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:48:2: ( 'let' IDENTIFIER '=' computation NEWLINE -> ^( VAR IDENTIFIER computation ) )
			DebugEnterAlt(1);
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:48:4: 'let' IDENTIFIER '=' computation NEWLINE
			{
			DebugLocation(48, 4);
			string_literal4=(IToken)Match(input,17,Follow._17_in_variable222);  
			stream_17.Add(string_literal4);

			DebugLocation(48, 10);
			IDENTIFIER5=(IToken)Match(input,IDENTIFIER,Follow._IDENTIFIER_in_variable224);  
			stream_IDENTIFIER.Add(IDENTIFIER5);

			DebugLocation(48, 21);
			char_literal6=(IToken)Match(input,16,Follow._16_in_variable226);  
			stream_16.Add(char_literal6);

			DebugLocation(48, 25);
			PushFollow(Follow._computation_in_variable228);
			computation7=computation();
			PopFollow();

			stream_computation.Add(computation7.Tree);
			DebugLocation(48, 37);
			NEWLINE8=(IToken)Match(input,NEWLINE,Follow._NEWLINE_in_variable230);  
			stream_NEWLINE.Add(NEWLINE8);



			{
			// AST REWRITE
			// elements: IDENTIFIER, computation
			// token labels: 
			// rule labels: retval
			// token list labels: 
			// rule list labels: 
			// wildcard labels: 
			retval.Tree = root_0;
			RewriteRuleSubtreeStream stream_retval=new RewriteRuleSubtreeStream(adaptor,"rule retval",retval!=null?retval.Tree:null);

			root_0 = (CommonTree)adaptor.Nil();
			// 48:45: -> ^( VAR IDENTIFIER computation )
			{
				DebugLocation(48, 48);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:48:48: ^( VAR IDENTIFIER computation )
				{
				CommonTree root_1 = (CommonTree)adaptor.Nil();
				DebugLocation(48, 50);
				root_1 = (CommonTree)adaptor.BecomeRoot((CommonTree)adaptor.Create(VAR, "VAR"), root_1);

				DebugLocation(48, 54);
				adaptor.AddChild(root_1, stream_IDENTIFIER.NextNode());
				DebugLocation(48, 65);
				adaptor.AddChild(root_1, stream_computation.NextTree());

				adaptor.AddChild(root_0, root_1);
				}

			}

			retval.Tree = root_0;
			}

			}

			retval.Stop = (IToken)input.LT(-1);

			retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
			adaptor.SetTokenBoundaries(retval.Tree, retval.Start, retval.Stop);

		}
		catch (RecognitionException re)
		{
			ReportError(re);
			Recover(input,re);
		retval.Tree = (CommonTree)adaptor.ErrorNode(input, retval.Start, input.LT(-1), re);

		}
		finally
		{
			TraceOut("variable", 3);
			LeaveRule("variable", 3);
			LeaveRule_variable();
		}
		DebugLocation(49, 1);
		} finally { DebugExitRule(GrammarFileName, "variable"); }
		return retval;

	}
	// $ANTLR end "variable"

	partial void EnterRule_balance();
	partial void LeaveRule_balance();

	// $ANTLR start "balance"
	// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:51:1: balance : ( computation '=' computation NEWLINE -> ^( '=' computation computation ) | NEWLINE ->);
	[GrammarRule("balance")]
	private AstParserRuleReturnScope<CommonTree, IToken> balance()
	{
		EnterRule_balance();
		EnterRule("balance", 4);
		TraceIn("balance", 4);
		AstParserRuleReturnScope<CommonTree, IToken> retval = new AstParserRuleReturnScope<CommonTree, IToken>();
		retval.Start = (IToken)input.LT(1);

		CommonTree root_0 = default(CommonTree);

		IToken char_literal10 = default(IToken);
		IToken NEWLINE12 = default(IToken);
		IToken NEWLINE13 = default(IToken);
		AstParserRuleReturnScope<CommonTree, IToken> computation9 = default(AstParserRuleReturnScope<CommonTree, IToken>);
		AstParserRuleReturnScope<CommonTree, IToken> computation11 = default(AstParserRuleReturnScope<CommonTree, IToken>);

		CommonTree char_literal10_tree = default(CommonTree);
		CommonTree NEWLINE12_tree = default(CommonTree);
		CommonTree NEWLINE13_tree = default(CommonTree);
		RewriteRuleITokenStream stream_NEWLINE=new RewriteRuleITokenStream(adaptor,"token NEWLINE");
		RewriteRuleITokenStream stream_16=new RewriteRuleITokenStream(adaptor,"token 16");
		RewriteRuleSubtreeStream stream_computation=new RewriteRuleSubtreeStream(adaptor,"rule computation");
		try { DebugEnterRule(GrammarFileName, "balance");
		DebugLocation(51, 1);
		try
		{
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:52:2: ( computation '=' computation NEWLINE -> ^( '=' computation computation ) | NEWLINE ->)
			int alt3=2;
			try { DebugEnterDecision(3, false);
			int LA3_0 = input.LA(1);

			if (((LA3_0>=FLOAT && LA3_0<=INTEGER)||LA3_0==10))
			{
				alt3 = 1;
			}
			else if ((LA3_0==NEWLINE))
			{
				alt3 = 2;
			}
			else
			{
				NoViableAltException nvae = new NoViableAltException("", 3, 0, input);
				DebugRecognitionException(nvae);
				throw nvae;
			}
			} finally { DebugExitDecision(3); }
			switch (alt3)
			{
			case 1:
				DebugEnterAlt(1);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:52:4: computation '=' computation NEWLINE
				{
				DebugLocation(52, 4);
				PushFollow(Follow._computation_in_balance253);
				computation9=computation();
				PopFollow();

				stream_computation.Add(computation9.Tree);
				DebugLocation(52, 16);
				char_literal10=(IToken)Match(input,16,Follow._16_in_balance255);  
				stream_16.Add(char_literal10);

				DebugLocation(52, 20);
				PushFollow(Follow._computation_in_balance257);
				computation11=computation();
				PopFollow();

				stream_computation.Add(computation11.Tree);
				DebugLocation(52, 32);
				NEWLINE12=(IToken)Match(input,NEWLINE,Follow._NEWLINE_in_balance259);  
				stream_NEWLINE.Add(NEWLINE12);



				{
				// AST REWRITE
				// elements: 16, computation, computation
				// token labels: 
				// rule labels: retval
				// token list labels: 
				// rule list labels: 
				// wildcard labels: 
				retval.Tree = root_0;
				RewriteRuleSubtreeStream stream_retval=new RewriteRuleSubtreeStream(adaptor,"rule retval",retval!=null?retval.Tree:null);

				root_0 = (CommonTree)adaptor.Nil();
				// 52:41: -> ^( '=' computation computation )
				{
					DebugLocation(52, 44);
					// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:52:44: ^( '=' computation computation )
					{
					CommonTree root_1 = (CommonTree)adaptor.Nil();
					DebugLocation(52, 46);
					root_1 = (CommonTree)adaptor.BecomeRoot(stream_16.NextNode(), root_1);

					DebugLocation(52, 50);
					adaptor.AddChild(root_1, stream_computation.NextTree());
					DebugLocation(52, 62);
					adaptor.AddChild(root_1, stream_computation.NextTree());

					adaptor.AddChild(root_0, root_1);
					}

				}

				retval.Tree = root_0;
				}

				}
				break;
			case 2:
				DebugEnterAlt(2);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:53:4: NEWLINE
				{
				DebugLocation(53, 4);
				NEWLINE13=(IToken)Match(input,NEWLINE,Follow._NEWLINE_in_balance275);  
				stream_NEWLINE.Add(NEWLINE13);



				{
				// AST REWRITE
				// elements: 
				// token labels: 
				// rule labels: retval
				// token list labels: 
				// rule list labels: 
				// wildcard labels: 
				retval.Tree = root_0;
				RewriteRuleSubtreeStream stream_retval=new RewriteRuleSubtreeStream(adaptor,"rule retval",retval!=null?retval.Tree:null);

				root_0 = (CommonTree)adaptor.Nil();
				// 53:16: ->
				{
					DebugLocation(54, 2);
					root_0 = null;
				}

				retval.Tree = root_0;
				}

				}
				break;

			}
			retval.Stop = (IToken)input.LT(-1);

			retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
			adaptor.SetTokenBoundaries(retval.Tree, retval.Start, retval.Stop);

		}
		catch (RecognitionException re)
		{
			ReportError(re);
			Recover(input,re);
		retval.Tree = (CommonTree)adaptor.ErrorNode(input, retval.Start, input.LT(-1), re);

		}
		finally
		{
			TraceOut("balance", 4);
			LeaveRule("balance", 4);
			LeaveRule_balance();
		}
		DebugLocation(54, 1);
		} finally { DebugExitRule(GrammarFileName, "balance"); }
		return retval;

	}
	// $ANTLR end "balance"

	partial void EnterRule_computation();
	partial void LeaveRule_computation();

	// $ANTLR start "computation"
	// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:56:1: computation : term ( ( '+' ^| '-' ^) term )* ;
	[GrammarRule("computation")]
	private AstParserRuleReturnScope<CommonTree, IToken> computation()
	{
		EnterRule_computation();
		EnterRule("computation", 5);
		TraceIn("computation", 5);
		AstParserRuleReturnScope<CommonTree, IToken> retval = new AstParserRuleReturnScope<CommonTree, IToken>();
		retval.Start = (IToken)input.LT(1);

		CommonTree root_0 = default(CommonTree);

		IToken char_literal15 = default(IToken);
		IToken char_literal16 = default(IToken);
		AstParserRuleReturnScope<CommonTree, IToken> term14 = default(AstParserRuleReturnScope<CommonTree, IToken>);
		AstParserRuleReturnScope<CommonTree, IToken> term17 = default(AstParserRuleReturnScope<CommonTree, IToken>);

		CommonTree char_literal15_tree = default(CommonTree);
		CommonTree char_literal16_tree = default(CommonTree);
		try { DebugEnterRule(GrammarFileName, "computation");
		DebugLocation(56, 29);
		try
		{
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:57:2: ( term ( ( '+' ^| '-' ^) term )* )
			DebugEnterAlt(1);
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:57:4: term ( ( '+' ^| '-' ^) term )*
			{
			root_0 = (CommonTree)adaptor.Nil();

			DebugLocation(57, 4);
			PushFollow(Follow._term_in_computation293);
			term14=term();
			PopFollow();

			adaptor.AddChild(root_0, term14.Tree);
			DebugLocation(57, 9);
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:57:9: ( ( '+' ^| '-' ^) term )*
			try { DebugEnterSubRule(5);
			while (true)
			{
				int alt5=2;
				try { DebugEnterDecision(5, false);
				int LA5_0 = input.LA(1);

				if (((LA5_0>=13 && LA5_0<=14)))
				{
					alt5 = 1;
				}


				} finally { DebugExitDecision(5); }
				switch ( alt5 )
				{
				case 1:
					DebugEnterAlt(1);
					// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:57:10: ( '+' ^| '-' ^) term
					{
					DebugLocation(57, 10);
					// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:57:10: ( '+' ^| '-' ^)
					int alt4=2;
					try { DebugEnterSubRule(4);
					try { DebugEnterDecision(4, false);
					int LA4_0 = input.LA(1);

					if ((LA4_0==13))
					{
						alt4 = 1;
					}
					else if ((LA4_0==14))
					{
						alt4 = 2;
					}
					else
					{
						NoViableAltException nvae = new NoViableAltException("", 4, 0, input);
						DebugRecognitionException(nvae);
						throw nvae;
					}
					} finally { DebugExitDecision(4); }
					switch (alt4)
					{
					case 1:
						DebugEnterAlt(1);
						// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:57:11: '+' ^
						{
						DebugLocation(57, 14);
						char_literal15=(IToken)Match(input,13,Follow._13_in_computation297); 
						char_literal15_tree = (CommonTree)adaptor.Create(char_literal15);
						root_0 = (CommonTree)adaptor.BecomeRoot(char_literal15_tree, root_0);

						}
						break;
					case 2:
						DebugEnterAlt(2);
						// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:57:18: '-' ^
						{
						DebugLocation(57, 21);
						char_literal16=(IToken)Match(input,14,Follow._14_in_computation302); 
						char_literal16_tree = (CommonTree)adaptor.Create(char_literal16);
						root_0 = (CommonTree)adaptor.BecomeRoot(char_literal16_tree, root_0);

						}
						break;

					}
					} finally { DebugExitSubRule(4); }

					DebugLocation(57, 24);
					PushFollow(Follow._term_in_computation306);
					term17=term();
					PopFollow();

					adaptor.AddChild(root_0, term17.Tree);

					}
					break;

				default:
					goto loop5;
				}
			}

			loop5:
				;

			} finally { DebugExitSubRule(5); }


			}

			retval.Stop = (IToken)input.LT(-1);

			retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
			adaptor.SetTokenBoundaries(retval.Tree, retval.Start, retval.Stop);

		}
		catch (RecognitionException re)
		{
			ReportError(re);
			Recover(input,re);
		retval.Tree = (CommonTree)adaptor.ErrorNode(input, retval.Start, input.LT(-1), re);

		}
		finally
		{
			TraceOut("computation", 5);
			LeaveRule("computation", 5);
			LeaveRule_computation();
		}
		DebugLocation(57, 29);
		} finally { DebugExitRule(GrammarFileName, "computation"); }
		return retval;

	}
	// $ANTLR end "computation"

	partial void EnterRule_term();
	partial void LeaveRule_term();

	// $ANTLR start "term"
	// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:59:1: term : atom ( ( '*' ^| '/' ^) atom )* ;
	[GrammarRule("term")]
	private AstParserRuleReturnScope<CommonTree, IToken> term()
	{
		EnterRule_term();
		EnterRule("term", 6);
		TraceIn("term", 6);
		AstParserRuleReturnScope<CommonTree, IToken> retval = new AstParserRuleReturnScope<CommonTree, IToken>();
		retval.Start = (IToken)input.LT(1);

		CommonTree root_0 = default(CommonTree);

		IToken char_literal19 = default(IToken);
		IToken char_literal20 = default(IToken);
		AstParserRuleReturnScope<CommonTree, IToken> atom18 = default(AstParserRuleReturnScope<CommonTree, IToken>);
		AstParserRuleReturnScope<CommonTree, IToken> atom21 = default(AstParserRuleReturnScope<CommonTree, IToken>);

		CommonTree char_literal19_tree = default(CommonTree);
		CommonTree char_literal20_tree = default(CommonTree);
		try { DebugEnterRule(GrammarFileName, "term");
		DebugLocation(59, 1);
		try
		{
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:60:2: ( atom ( ( '*' ^| '/' ^) atom )* )
			DebugEnterAlt(1);
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:60:4: atom ( ( '*' ^| '/' ^) atom )*
			{
			root_0 = (CommonTree)adaptor.Nil();

			DebugLocation(60, 4);
			PushFollow(Follow._atom_in_term318);
			atom18=atom();
			PopFollow();

			adaptor.AddChild(root_0, atom18.Tree);
			DebugLocation(60, 9);
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:60:9: ( ( '*' ^| '/' ^) atom )*
			try { DebugEnterSubRule(7);
			while (true)
			{
				int alt7=2;
				try { DebugEnterDecision(7, false);
				int LA7_0 = input.LA(1);

				if ((LA7_0==12||LA7_0==15))
				{
					alt7 = 1;
				}


				} finally { DebugExitDecision(7); }
				switch ( alt7 )
				{
				case 1:
					DebugEnterAlt(1);
					// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:60:10: ( '*' ^| '/' ^) atom
					{
					DebugLocation(60, 10);
					// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:60:10: ( '*' ^| '/' ^)
					int alt6=2;
					try { DebugEnterSubRule(6);
					try { DebugEnterDecision(6, false);
					int LA6_0 = input.LA(1);

					if ((LA6_0==12))
					{
						alt6 = 1;
					}
					else if ((LA6_0==15))
					{
						alt6 = 2;
					}
					else
					{
						NoViableAltException nvae = new NoViableAltException("", 6, 0, input);
						DebugRecognitionException(nvae);
						throw nvae;
					}
					} finally { DebugExitDecision(6); }
					switch (alt6)
					{
					case 1:
						DebugEnterAlt(1);
						// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:60:11: '*' ^
						{
						DebugLocation(60, 14);
						char_literal19=(IToken)Match(input,12,Follow._12_in_term322); 
						char_literal19_tree = (CommonTree)adaptor.Create(char_literal19);
						root_0 = (CommonTree)adaptor.BecomeRoot(char_literal19_tree, root_0);

						}
						break;
					case 2:
						DebugEnterAlt(2);
						// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:60:18: '/' ^
						{
						DebugLocation(60, 21);
						char_literal20=(IToken)Match(input,15,Follow._15_in_term327); 
						char_literal20_tree = (CommonTree)adaptor.Create(char_literal20);
						root_0 = (CommonTree)adaptor.BecomeRoot(char_literal20_tree, root_0);

						}
						break;

					}
					} finally { DebugExitSubRule(6); }

					DebugLocation(60, 24);
					PushFollow(Follow._atom_in_term331);
					atom21=atom();
					PopFollow();

					adaptor.AddChild(root_0, atom21.Tree);

					}
					break;

				default:
					goto loop7;
				}
			}

			loop7:
				;

			} finally { DebugExitSubRule(7); }


			}

			retval.Stop = (IToken)input.LT(-1);

			retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
			adaptor.SetTokenBoundaries(retval.Tree, retval.Start, retval.Stop);

		}
		catch (RecognitionException re)
		{
			ReportError(re);
			Recover(input,re);
		retval.Tree = (CommonTree)adaptor.ErrorNode(input, retval.Start, input.LT(-1), re);

		}
		finally
		{
			TraceOut("term", 6);
			LeaveRule("term", 6);
			LeaveRule_term();
		}
		DebugLocation(61, 1);
		} finally { DebugExitRule(GrammarFileName, "term"); }
		return retval;

	}
	// $ANTLR end "term"

	partial void EnterRule_atom();
	partial void LeaveRule_atom();

	// $ANTLR start "atom"
	// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:63:1: atom : ( INTEGER | FLOAT | IDENTIFIER | '(' ! computation ')' !);
	[GrammarRule("atom")]
	private AstParserRuleReturnScope<CommonTree, IToken> atom()
	{
		EnterRule_atom();
		EnterRule("atom", 7);
		TraceIn("atom", 7);
		AstParserRuleReturnScope<CommonTree, IToken> retval = new AstParserRuleReturnScope<CommonTree, IToken>();
		retval.Start = (IToken)input.LT(1);

		CommonTree root_0 = default(CommonTree);

		IToken INTEGER22 = default(IToken);
		IToken FLOAT23 = default(IToken);
		IToken IDENTIFIER24 = default(IToken);
		IToken char_literal25 = default(IToken);
		IToken char_literal27 = default(IToken);
		AstParserRuleReturnScope<CommonTree, IToken> computation26 = default(AstParserRuleReturnScope<CommonTree, IToken>);

		CommonTree INTEGER22_tree = default(CommonTree);
		CommonTree FLOAT23_tree = default(CommonTree);
		CommonTree IDENTIFIER24_tree = default(CommonTree);
		CommonTree char_literal25_tree = default(CommonTree);
		CommonTree char_literal27_tree = default(CommonTree);
		try { DebugEnterRule(GrammarFileName, "atom");
		DebugLocation(63, 1);
		try
		{
			// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:64:2: ( INTEGER | FLOAT | IDENTIFIER | '(' ! computation ')' !)
			int alt8=4;
			try { DebugEnterDecision(8, false);
			switch (input.LA(1))
			{
			case INTEGER:
				{
				alt8 = 1;
				}
				break;
			case FLOAT:
				{
				alt8 = 2;
				}
				break;
			case IDENTIFIER:
				{
				alt8 = 3;
				}
				break;
			case 10:
				{
				alt8 = 4;
				}
				break;
			default:
				{
					NoViableAltException nvae = new NoViableAltException("", 8, 0, input);
					DebugRecognitionException(nvae);
					throw nvae;
				}
			}

			} finally { DebugExitDecision(8); }
			switch (alt8)
			{
			case 1:
				DebugEnterAlt(1);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:64:4: INTEGER
				{
				root_0 = (CommonTree)adaptor.Nil();

				DebugLocation(64, 4);
				INTEGER22=(IToken)Match(input,INTEGER,Follow._INTEGER_in_atom344); 
				INTEGER22_tree = (CommonTree)adaptor.Create(INTEGER22);
				adaptor.AddChild(root_0, INTEGER22_tree);

				}
				break;
			case 2:
				DebugEnterAlt(2);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:65:6: FLOAT
				{
				root_0 = (CommonTree)adaptor.Nil();

				DebugLocation(65, 6);
				FLOAT23=(IToken)Match(input,FLOAT,Follow._FLOAT_in_atom351); 
				FLOAT23_tree = (CommonTree)adaptor.Create(FLOAT23);
				adaptor.AddChild(root_0, FLOAT23_tree);

				}
				break;
			case 3:
				DebugEnterAlt(3);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:66:4: IDENTIFIER
				{
				root_0 = (CommonTree)adaptor.Nil();

				DebugLocation(66, 4);
				IDENTIFIER24=(IToken)Match(input,IDENTIFIER,Follow._IDENTIFIER_in_atom356); 
				IDENTIFIER24_tree = (CommonTree)adaptor.Create(IDENTIFIER24);
				adaptor.AddChild(root_0, IDENTIFIER24_tree);

				}
				break;
			case 4:
				DebugEnterAlt(4);
				// E:\\dolivares\\ChemProV\\ChemProV\\Grammars\\ChemProV.g:67:4: '(' ! computation ')' !
				{
				root_0 = (CommonTree)adaptor.Nil();

				DebugLocation(67, 7);
				char_literal25=(IToken)Match(input,10,Follow._10_in_atom361); 
				DebugLocation(67, 9);
				PushFollow(Follow._computation_in_atom364);
				computation26=computation();
				PopFollow();

				adaptor.AddChild(root_0, computation26.Tree);
				DebugLocation(67, 24);
				char_literal27=(IToken)Match(input,11,Follow._11_in_atom366); 

				}
				break;

			}
			retval.Stop = (IToken)input.LT(-1);

			retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
			adaptor.SetTokenBoundaries(retval.Tree, retval.Start, retval.Stop);

		}
		catch (RecognitionException re)
		{
			ReportError(re);
			Recover(input,re);
		retval.Tree = (CommonTree)adaptor.ErrorNode(input, retval.Start, input.LT(-1), re);

		}
		finally
		{
			TraceOut("atom", 7);
			LeaveRule("atom", 7);
			LeaveRule_atom();
		}
		DebugLocation(68, 1);
		} finally { DebugExitRule(GrammarFileName, "atom"); }
		return retval;

	}
	// $ANTLR end "atom"
	#endregion Rules


	#region Follow sets
	private static class Follow
	{
		public static readonly BitSet _line_in_program188 = new BitSet(new ulong[]{0x204F2UL});
		public static readonly BitSet _variable_in_line206 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _balance_in_line211 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _17_in_variable222 = new BitSet(new ulong[]{0x20UL});
		public static readonly BitSet _IDENTIFIER_in_variable224 = new BitSet(new ulong[]{0x10000UL});
		public static readonly BitSet _16_in_variable226 = new BitSet(new ulong[]{0x470UL});
		public static readonly BitSet _computation_in_variable228 = new BitSet(new ulong[]{0x80UL});
		public static readonly BitSet _NEWLINE_in_variable230 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _computation_in_balance253 = new BitSet(new ulong[]{0x10000UL});
		public static readonly BitSet _16_in_balance255 = new BitSet(new ulong[]{0x470UL});
		public static readonly BitSet _computation_in_balance257 = new BitSet(new ulong[]{0x80UL});
		public static readonly BitSet _NEWLINE_in_balance259 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _NEWLINE_in_balance275 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _term_in_computation293 = new BitSet(new ulong[]{0x6002UL});
		public static readonly BitSet _13_in_computation297 = new BitSet(new ulong[]{0x470UL});
		public static readonly BitSet _14_in_computation302 = new BitSet(new ulong[]{0x470UL});
		public static readonly BitSet _term_in_computation306 = new BitSet(new ulong[]{0x6002UL});
		public static readonly BitSet _atom_in_term318 = new BitSet(new ulong[]{0x9002UL});
		public static readonly BitSet _12_in_term322 = new BitSet(new ulong[]{0x470UL});
		public static readonly BitSet _15_in_term327 = new BitSet(new ulong[]{0x470UL});
		public static readonly BitSet _atom_in_term331 = new BitSet(new ulong[]{0x9002UL});
		public static readonly BitSet _INTEGER_in_atom344 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _FLOAT_in_atom351 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _IDENTIFIER_in_atom356 = new BitSet(new ulong[]{0x2UL});
		public static readonly BitSet _10_in_atom361 = new BitSet(new ulong[]{0x470UL});
		public static readonly BitSet _computation_in_atom364 = new BitSet(new ulong[]{0x800UL});
		public static readonly BitSet _11_in_atom366 = new BitSet(new ulong[]{0x2UL});
	}
	#endregion Follow sets
}

} // namespace  ChemProV.Grammars 