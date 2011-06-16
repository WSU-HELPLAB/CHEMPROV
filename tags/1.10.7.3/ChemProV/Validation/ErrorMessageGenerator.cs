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
    public enum ErrorMessages
    {

        ////////////////////////////////////////////ProcessUnit Rules////////////////////////////////////////////
        /// <summary>
        /// ProcessUnit Rules: If the flowrate of any specific compound going in is not the same of that same compound going out
        /// </summary>
        Individual_Flowrate_Mismatch,
        /// <summary>
        /// ProcessUnit Rules: If there are compounds going out that are not going in
        /// </summary>
        Missing_Incoming_Compounds,
        /// <summary>
        /// ProcessUnit Rules: If there are compounds going in that are not going out
        /// </summary>
        Missing_Outgoing_Compounds,
        /// <summary>
        /// ProcessUnit Rules: If the overal flowrate going in is not the same as the flowrate going out
        /// </summary>
        Overall_Flowrate_Mismatch,
        /// <summary>
        /// ProcessUnit Rules: If all the Overall units are not the same
        /// </summary>
        Overall_Units_Mismatch,
        /// <summary>
        /// ProcessUnit Rules: If the conservation of energy is not upheld.
        /// </summary>
        Unconserved_Energy,
        ////////////////////////////////////////////Reactor Rules////////////////////////////////////////////
        /// <summary>
        /// Reactor Rules: If the compounds going in are not all in moles
        /// </summary>
        Not_In_Moles,

        ///////////////////////////////Heat Exchanger Without Utility Rules//////////////////////////////////
        /// <summary>
        /// Heat Exchanger Without Utility Rules: If the compounds going in are not all in moles
        /// </summary>
        Incoming_Outgoing_Streams_Mismatch,
        /// <summary>
        /// Heat Exchanger Without Utility Rules: If the temperature of the outgoing streams does not fall within the range of the temperatures of the out going streams
        /// </summary>
        InCorrect_Temperature,
        ////////////////////////////////////////////Table Rules////////////////////////////////////////////
        /// <summary>
        /// Table Rules: if the sum of the quantity of the individual compounds does not equal the quantity of the overal compound
        /// </summary>
        Sum_Does_Not_Equal_Total_Quantity,
        /// <summary>
        /// Table Rules: If the units for the table are not all the same
        /// </summary>
        Inconsistant_Units,
        /// <summary>
        /// Table Rules: If the table labels are not all unique
        /// </summary>
        NonUniqueNames,

        ////////////////////////////////////////////Chemical Equation Rules////////////////////////////////////////////
        /// <summary>
        /// Equation Rules: If the variables used are not labels in the tables
        /// </summary>
        Equation_Variable_Not_In_Tables,
        /// <summary>
        /// Equation Rules: If percents are not divided by 100 and then multipied by the Overall Compound
        /// </summary>
        Incorrect_Use_Of_Percent,
        /// <summary>
        /// Equation Rules: If there is more than one compound used and it is not the sum of an overal
        /// </summary>
        More_Than_One_Compound,
        /// <summary>
        /// Equation Rules: If there is more than one type of unit used in the equation
        /// </summary>
        More_Than_One_Unit,
        /// <summary>
        /// Equation Rules: This is if there are any unused variables in the equations
        /// </summary>
        Unused_Variables,
        /// <summary>
        /// Equation Rules: This is if the abbrivation used does not match another abbrivation or the compound refered by a label
        /// </summary>
        Incorrect_Abbrv,
        //////////////////////////////////////////////Heat Equation Rules//////////////////////////////////////////////

        /// <summary>
        /// Equation Rules: This is if a Heat equation isn't in the correct format
        /// </summary>
        InValid_Heat_Equation,




        ////////////////////////////////////////////Solvabilty Rules////////////////////////////////////////////
        /// <summary>
        /// Solvability Rules: If there are two unkowns being multiplied together.
        /// </summary>
        Quadratic_Equation,
        /// <summary>
        /// Solvability Rules: If there number of equations and the number of unkowns are not the same.
        /// </summary>
        Equations_and_Unknowns,
        /// <summary>
        /// Solvability Rules: If the equations are not independent from each other
        /// </summary>
        Not_Independent,
        /// <summary>
        /// Solvability Rules: If the equations are indeed solvable
        /// </summary>
        Solvable,
        /// <summary>
        /// If there is not enough infomation to check the equations.
        /// </summary>
        Insuffcient_infomation
    };
    /// <summary>
    /// This class only has the one static member Generatemessage
    /// </summary>
    public static class ErrorMessageGenerator
    {
        /// <summary>
        /// This generates an error messaged bassed on the message and list passed in
        /// </summary>
        /// <param name="message">the rule that was broken</param>
        /// <param name="list">not needed, if passed must be an array of strings which maybe used when making the message that is return</param>
        /// <returns>a message for the rule that was broken</returns>
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
                    resultMessage = "Improper use of percentages in the equation.  When using percents in equations it must be in this format: the row label / 100 * overall Label for that table.  Example: m11 / 100 * M1";
                    break;

                case ErrorMessages.More_Than_One_Compound:
                    resultMessage = "This material balance contains terms that involve more than one compound or element and is not an overall balance.  Any material balance, other than the overall balance, must involve only a single chemical compound or element";
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

                case ErrorMessages.Solvable:
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

                case ErrorMessages.NonUniqueNames:
                    resultMessage = "These Labels appear more than once {0}.  Each label name may only appear once";

                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;
                case ErrorMessages.Incoming_Outgoing_Streams_Mismatch:
                    resultMessage = "The stream that enter a heat exchagned without utility must leave unchanged except for temperature";

                   break;
                case ErrorMessages.InCorrect_Temperature:
                   resultMessage = "The temperature of a leaving stream must be in the range of the temperatures of the incoming streams";
                   break;
                case ErrorMessages.Unconserved_Energy:
                   resultMessage = "Energy is not being conserved across a Process Unit";
                   break;
                case ErrorMessages.InValid_Heat_Equation:
                   resultMessage = "This heat equation is not in a valid format.  Must be either Enthropy = Q or Sum Enthropy in = Sum of Enthropy out. Enthropy must be written as Hf?? + Hc?? * Temp - 298 * moles. Where the ?? is the abbriviation of the current compound";
                   break;
                case ErrorMessages.Incorrect_Abbrv:
                   resultMessage = "The abbrivation {0} does not match the compound used or another abbrivation";
                   
                    //concat on the list of missing materials
                   resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                   break;
                default:
                    resultMessage = "There are no messages, for me to poop on!";
                    break;
            }
            return resultMessage;
        }
    }
}
