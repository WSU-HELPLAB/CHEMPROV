using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.Validation
{
    /// <summary>
    /// Contains a list constants that correspond to a particular error message 
    /// </summary>
    public enum ErrorMessages { 

                                //ProcessUnit Rules
                                Individual_Flowrate_Mismatch,
                                Missing_Incoming_Compounds,
                                Missing_Outgoing_Compounds,
                                Overall_Flowrate_Mismatch,
                                Overall_Units_Mismatch,

                                //Reactor Rules
                                Not_In_Moles,

                                //Table Rules
                                Sum_Does_Not_Equal_Total_Quantity,
                                Inconsistant_Units,

                                //Equation Rules
                                Equation_Variable_Not_In_Tables,
                                Incorrect_Use_Of_Percent,
                                More_Than_One_Compound,
                                More_Than_One_Unit,
                                Unused_Variables,
                                    
                                //Solvabilty Rules
                                Quadratic_Equation,
                                Equations_and_Unknowns,
                                Not_Independent,
                                Solveable,
                                Insuffcient_infomation
                              };
    public static class ErrorMessageGenerator
    {
        public static string GenerateMesssage(ErrorMessages message, params object[] list)
        {
            string resultMessage = "";
            switch (message)
            {
                case ErrorMessages.Overall_Flowrate_Mismatch:
                    resultMessage = "Overall mass balance across the process unit connected to this stream is not "
                                  + "satisfied.  Make sure that the quantities of all incoming and outgoing streams "
                                  + "match.";
                    break;

                case ErrorMessages.Missing_Incoming_Compounds:
                    resultMessage = "Incoming stream(s) contains {0} which are NOT specified in the outgoing "
                                  + "stream(s).  Make sure that every compound that enters a processing unit "
                                  + "also leaves that unit";

                    //concat on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

                case ErrorMessages.Missing_Outgoing_Compounds:
                    resultMessage = "Outgoing stream(s) contains {0} which are NOT specified in the incoming "
                                  + "stream(s).  Make sure that every compound that leaves a processing unit "
                                  + "also enters that unit";

                    //concat on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

               case ErrorMessages.Sum_Does_Not_Equal_Total_Quantity:
                    resultMessage = "The sum of the quantities of all individual compounds is not equal to the quantity of the overall stream, or 100%.  " +
                                            "Make sure that the quantites of all individual compounds add up to the quantity of overall stream, or 100%";
                    break;
                case ErrorMessages.Inconsistant_Units:
                    resultMessage = "Units associated with individual quantities do not match the units associated with the overall stream or are not in %. Make sure units are consistent or are in %";
                    break;

                case ErrorMessages.Equation_Variable_Not_In_Tables:
                    resultMessage = "The following terms are undefined: {0}";

                    //concat on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

                case ErrorMessages.Incorrect_Use_Of_Percent:
                    resultMessage = "Improper use of percentages.  Check that % from different streams are not added, equated or multiplied by other %s";
                    break;

                case ErrorMessages.More_Than_One_Compound:
                    resultMessage = "This material balance contains terms that involve more than one compound and is not an overall balance.  Any material balance, other than the overall balance, must involve only a single chemical compound";
                    break;
                case ErrorMessages.More_Than_One_Unit:
                    resultMessage = "Units associated with individual quantities do not match the units associated with the overall stream, or are not in %.  Make sure units are consistent, or in %";
                    break;

                case ErrorMessages.Unused_Variables:
                    resultMessage = "These unkown(s) are not used in the equations {0}.\n Each unkown must be used in at least one equations";
                    
                    //concat on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

                case ErrorMessages.Quadratic_Equation:
                    resultMessage = "You have specified Qudratic equation(s).\n Quadratic equation are not yet supported by the system.\n Please modify the equation to make it linear.";
                    break;

                case ErrorMessages.Equations_and_Unknowns:
                    resultMessage = "You have {0} valid equations(s) and {1} unkown(s).\n The number of independent equations and the number of unknowns should be the same";

                    //concat on the list of missing materials
                    resultMessage = String.Format(resultMessage, list[0] as string, list[1] as string);
                    //resultMessage = String.Format(resultMessage, two as string);
                    break;

                case ErrorMessages.Not_Independent:
                    resultMessage = "The set of equations that you have created is NOT solvable\nThe equations that you have written are not independent of each other";
                    break;

                case ErrorMessages.Solveable:
                    resultMessage = "Congratulations! The set of equations that you have created is solvable.";
                    break;

                case ErrorMessages.Insuffcient_infomation:
                    resultMessage = "Insuffcient information to check";
                    break;

                case ErrorMessages.Individual_Flowrate_Mismatch:
                    resultMessage = "Overall mass balance is satisfied.  One or more of the individual mass balances across "
                                  + "the processing unit attached to this stream is NOT satisfied.  Check the amount "
                                  + "of each compound entering and leaving the process unit.";
                    break;

                case ErrorMessages.Overall_Units_Mismatch:
                    resultMessage = "Units of all stream(s) entering or leaving the process unit connected to this stream "
                                  + "do not match.  Make sure that the units of all stream(s) connected to the process "
                                  + "unit match.";
                    break;

                case ErrorMessages.Not_In_Moles:
                    resultMessage = "Units going to and from a reactor must be in moles or moles per minute";
                    break;

                default:
                    resultMessage = "There are no messages, for me to poop on!";
                    break;
            }
            return resultMessage;
        }
    }
}
