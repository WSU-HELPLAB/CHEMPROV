// $ANTLR 3.4 D:\\acarter\\code\\antlr_sandbox\\T.g 2011-11-28 10:45:33

import org.antlr.runtime.*;
import java.util.Stack;
import java.util.List;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked"})
public class TParser extends Parser {
    public static final String[] tokenNames = new String[] {
        "<invalid>", "<EOR>", "<DOWN>", "<UP>", "ID", "WS", "';'", "'call'"
    };

    public static final int EOF=-1;
    public static final int T__6=6;
    public static final int T__7=7;
    public static final int ID=4;
    public static final int WS=5;

    // delegates
    public Parser[] getDelegates() {
        return new Parser[] {};
    }

    // delegators


    public TParser(TokenStream input) {
        this(input, new RecognizerSharedState());
    }
    public TParser(TokenStream input, RecognizerSharedState state) {
        super(input, state);
    }

    public String[] getTokenNames() { return TParser.tokenNames; }
    public String getGrammarFileName() { return "D:\\acarter\\code\\antlr_sandbox\\T.g"; }



    // $ANTLR start "r"
    // D:\\acarter\\code\\antlr_sandbox\\T.g:2:1: r : 'call' ID ';' ;
    public final void r() throws RecognitionException {
        Token ID1=null;

        try {
            // D:\\acarter\\code\\antlr_sandbox\\T.g:2:3: ( 'call' ID ';' )
            // D:\\acarter\\code\\antlr_sandbox\\T.g:2:6: 'call' ID ';'
            {
            match(input,7,FOLLOW_7_in_r10); 

            ID1=(Token)match(input,ID,FOLLOW_ID_in_r12); 

            match(input,6,FOLLOW_6_in_r14); 

            System.out.println("invoke"+(ID1!=null?ID1.getText():null));

            }

        }
        catch (RecognitionException re) {
            reportError(re);
            recover(input,re);
        }

        finally {
        	// do for sure before leaving
        }
        return ;
    }
    // $ANTLR end "r"

    // Delegated rules


 

    public static final BitSet FOLLOW_7_in_r10 = new BitSet(new long[]{0x0000000000000010L});
    public static final BitSet FOLLOW_ID_in_r12 = new BitSet(new long[]{0x0000000000000040L});
    public static final BitSet FOLLOW_6_in_r14 = new BitSet(new long[]{0x0000000000000002L});

}