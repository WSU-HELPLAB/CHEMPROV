// $ANTLR 3.4 D:\\acarter\\code\\antlr_sandbox\\T.g 2011-11-28 10:45:33

import org.antlr.runtime.*;
import java.util.Stack;
import java.util.List;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked"})
public class TLexer extends Lexer {
    public static final int EOF=-1;
    public static final int T__6=6;
    public static final int T__7=7;
    public static final int ID=4;
    public static final int WS=5;

    // delegates
    // delegators
    public Lexer[] getDelegates() {
        return new Lexer[] {};
    }

    public TLexer() {} 
    public TLexer(CharStream input) {
        this(input, new RecognizerSharedState());
    }
    public TLexer(CharStream input, RecognizerSharedState state) {
        super(input,state);
    }
    public String getGrammarFileName() { return "D:\\acarter\\code\\antlr_sandbox\\T.g"; }

    // $ANTLR start "T__6"
    public final void mT__6() throws RecognitionException {
        try {
            int _type = T__6;
            int _channel = DEFAULT_TOKEN_CHANNEL;
            // D:\\acarter\\code\\antlr_sandbox\\T.g:2:6: ( ';' )
            // D:\\acarter\\code\\antlr_sandbox\\T.g:2:8: ';'
            {
            match(';'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally {
        	// do for sure before leaving
        }
    }
    // $ANTLR end "T__6"

    // $ANTLR start "T__7"
    public final void mT__7() throws RecognitionException {
        try {
            int _type = T__7;
            int _channel = DEFAULT_TOKEN_CHANNEL;
            // D:\\acarter\\code\\antlr_sandbox\\T.g:3:6: ( 'call' )
            // D:\\acarter\\code\\antlr_sandbox\\T.g:3:8: 'call'
            {
            match("call"); 



            }

            state.type = _type;
            state.channel = _channel;
        }
        finally {
        	// do for sure before leaving
        }
    }
    // $ANTLR end "T__7"

    // $ANTLR start "ID"
    public final void mID() throws RecognitionException {
        try {
            int _type = ID;
            int _channel = DEFAULT_TOKEN_CHANNEL;
            // D:\\acarter\\code\\antlr_sandbox\\T.g:3:4: ( ( 'a' .. 'z' )+ )
            // D:\\acarter\\code\\antlr_sandbox\\T.g:3:7: ( 'a' .. 'z' )+
            {
            // D:\\acarter\\code\\antlr_sandbox\\T.g:3:7: ( 'a' .. 'z' )+
            int cnt1=0;
            loop1:
            do {
                int alt1=2;
                int LA1_0 = input.LA(1);

                if ( ((LA1_0 >= 'a' && LA1_0 <= 'z')) ) {
                    alt1=1;
                }


                switch (alt1) {
            	case 1 :
            	    // D:\\acarter\\code\\antlr_sandbox\\T.g:
            	    {
            	    if ( (input.LA(1) >= 'a' && input.LA(1) <= 'z') ) {
            	        input.consume();
            	    }
            	    else {
            	        MismatchedSetException mse = new MismatchedSetException(null,input);
            	        recover(mse);
            	        throw mse;
            	    }


            	    }
            	    break;

            	default :
            	    if ( cnt1 >= 1 ) break loop1;
                        EarlyExitException eee =
                            new EarlyExitException(1, input);
                        throw eee;
                }
                cnt1++;
            } while (true);


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally {
        	// do for sure before leaving
        }
    }
    // $ANTLR end "ID"

    // $ANTLR start "WS"
    public final void mWS() throws RecognitionException {
        try {
            int _type = WS;
            int _channel = DEFAULT_TOKEN_CHANNEL;
            // D:\\acarter\\code\\antlr_sandbox\\T.g:4:4: ( ( ' ' | '\\n' | '\\r' )+ )
            // D:\\acarter\\code\\antlr_sandbox\\T.g:4:7: ( ' ' | '\\n' | '\\r' )+
            {
            // D:\\acarter\\code\\antlr_sandbox\\T.g:4:7: ( ' ' | '\\n' | '\\r' )+
            int cnt2=0;
            loop2:
            do {
                int alt2=2;
                int LA2_0 = input.LA(1);

                if ( (LA2_0=='\n'||LA2_0=='\r'||LA2_0==' ') ) {
                    alt2=1;
                }


                switch (alt2) {
            	case 1 :
            	    // D:\\acarter\\code\\antlr_sandbox\\T.g:
            	    {
            	    if ( input.LA(1)=='\n'||input.LA(1)=='\r'||input.LA(1)==' ' ) {
            	        input.consume();
            	    }
            	    else {
            	        MismatchedSetException mse = new MismatchedSetException(null,input);
            	        recover(mse);
            	        throw mse;
            	    }


            	    }
            	    break;

            	default :
            	    if ( cnt2 >= 1 ) break loop2;
                        EarlyExitException eee =
                            new EarlyExitException(2, input);
                        throw eee;
                }
                cnt2++;
            } while (true);


            _channel=HIDDEN;

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally {
        	// do for sure before leaving
        }
    }
    // $ANTLR end "WS"

    public void mTokens() throws RecognitionException {
        // D:\\acarter\\code\\antlr_sandbox\\T.g:1:8: ( T__6 | T__7 | ID | WS )
        int alt3=4;
        switch ( input.LA(1) ) {
        case ';':
            {
            alt3=1;
            }
            break;
        case 'c':
            {
            int LA3_2 = input.LA(2);

            if ( (LA3_2=='a') ) {
                int LA3_5 = input.LA(3);

                if ( (LA3_5=='l') ) {
                    int LA3_6 = input.LA(4);

                    if ( (LA3_6=='l') ) {
                        int LA3_7 = input.LA(5);

                        if ( ((LA3_7 >= 'a' && LA3_7 <= 'z')) ) {
                            alt3=3;
                        }
                        else {
                            alt3=2;
                        }
                    }
                    else {
                        alt3=3;
                    }
                }
                else {
                    alt3=3;
                }
            }
            else {
                alt3=3;
            }
            }
            break;
        case 'a':
        case 'b':
        case 'd':
        case 'e':
        case 'f':
        case 'g':
        case 'h':
        case 'i':
        case 'j':
        case 'k':
        case 'l':
        case 'm':
        case 'n':
        case 'o':
        case 'p':
        case 'q':
        case 'r':
        case 's':
        case 't':
        case 'u':
        case 'v':
        case 'w':
        case 'x':
        case 'y':
        case 'z':
            {
            alt3=3;
            }
            break;
        case '\n':
        case '\r':
        case ' ':
            {
            alt3=4;
            }
            break;
        default:
            NoViableAltException nvae =
                new NoViableAltException("", 3, 0, input);

            throw nvae;

        }

        switch (alt3) {
            case 1 :
                // D:\\acarter\\code\\antlr_sandbox\\T.g:1:10: T__6
                {
                mT__6(); 


                }
                break;
            case 2 :
                // D:\\acarter\\code\\antlr_sandbox\\T.g:1:15: T__7
                {
                mT__7(); 


                }
                break;
            case 3 :
                // D:\\acarter\\code\\antlr_sandbox\\T.g:1:20: ID
                {
                mID(); 


                }
                break;
            case 4 :
                // D:\\acarter\\code\\antlr_sandbox\\T.g:1:23: WS
                {
                mWS(); 


                }
                break;

        }

    }


 

}