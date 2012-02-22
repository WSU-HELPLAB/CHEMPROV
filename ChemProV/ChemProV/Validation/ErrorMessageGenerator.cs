/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;

namespace ChemProV.Validation
{
    /// <summary>
    /// Contains a list constants that correspond to a particular error message
    /// </summary>
    public enum ErrorMessages
    {
        ////////////////////////////////////////////ProcessUnit Rules////////////////////////////////////////////
        /// <summary>
        /// ProcessUnit Rules: If the flow rate of any specific compound going in is not the same of that same compound going out
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
        /// ProcessUnit Rules: If the overall flow rate going in is not the same as the flow rate going out
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
        /// Table Rules: if the sum of the quantity of the individual compounds does not equal the quantity of the overall compound
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

        ////////////////////////////////////////////User Defined Equation Rules////////////////////////////////////////////

        /// <summary>
        /// This is used if the the variable definition is in the incorrect format
        /// </summary>
        Variable_Defination_Incorrect_Format,

        ////////////////////////////////////////////Chemical Equation Rules////////////////////////////////////////////

        /// <summary>
        /// If the compound that is supposed to be used is not used at all
        /// </summary>
        Compound_Not_Used,
        /// <summary>
        /// Equation Rules: If the variables used are not labels in the tables
        /// </summary>
        Equation_Variable_Not_In_Tables,
        /// <summary>
        /// Equation Rules: If percents are not divided by 100 and then multiplied by the Overall Compound
        /// </summary>
        Incorrect_Use_Of_Percent,
        /// <summary>
        /// Equation Rules: If the equation was specified as an Overall but it is not
        /// </summary>
        Not_Overall,
        /// <summary>
        /// Equation Rules: If there is more than one compound used and it is not the sum of an overall
        /// </summary>
        More_Than_One_Compound,
        /// <summary>
        /// Equation Rules: There is more than one element used and it is not the sum of an overall
        /// </summary>
        More_Than_One_Element,
        /// <summary>
        /// Equation Rules: This is if there are any unused variables in the equations
        /// </summary>
        Unused_Variables,
        /// <summary>
        /// Equation Rules: This is if the abbreviation used does not match another abbreviation or the compound referred by a label
        /// </summary>
        Incorrect_Abbrv,
        //////////////////////////////////////////////Heat Equation Rules//////////////////////////////////////////////

        /// <summary>
        /// Equation Rules: This is if a Heat equation isn't in the correct format
        /// </summary>
        InValid_Heat_Equation,

        Unknown_Constant,

        ////////////////////////////////////////////Solvability Rules////////////////////////////////////////////
        /// <summary>
        /// Solvability Rules: If there are two unknowns being multiplied together.
        /// </summary>
        Quadratic_Equation,
        /// <summary>
        /// Solvability Rules: If there number of equations and the number of unknowns are not the same.
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
        /// Equation does not match abstracted graph.
        /// </summary>
        Equation_doesnt_match_PFD,
        /// <summary>
        /// If there is not enough information to check the equations.
        /// </summary>
        Insuffcient_infomation,
        /// <summary>
        /// If the pfd is not connected
        /// </summary>
        PFD_not_connected,
        /// <summary>
        /// If there are missing equations for a graph
        /// </summary>
        Missing_equation
    };

    /// <summary>
    /// This class only has the one static member GenerateMessage
    /// </summary>
    public static class ErrorMessageGenerator
    {
        /// <summary>
        /// This generates an error messaged based on the message and list passed in
        /// </summary>
        /// <param name="message">the rule that was broken</param>
        /// <param name="list">not needed, if passed must be an array of strings which maybe used when making the message that is return</param>
        /// <returns>a message for the rule that was broken</returns>
        public static string GenerateMesssage(ErrorMessages message, params object[] list)
        {
            string resultMessage = "";
            string[] args = list as string[];
            switch (message)
            {
                case ErrorMessages.Compound_Not_Used:
                    resultMessage = "The compound that was expected was not used";

                    break;

                case ErrorMessages.Overall_Flowrate_Mismatch:
                    resultMessage = "Overall mass balance across the process unit connected to this "
                                  + "stream is not satisfied.  Make sure that the quantities of all incoming "
                                  + "and outgoing streams match";
                    break;

                case ErrorMessages.Missing_Incoming_Compounds:
                    resultMessage = "Incoming stream(s) contains {0} which is (are) NOT specified in the outgoing stream(s).  Make sure that every compound that enters a processing unit also leaves that unit";

                    //concatenate on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

                case ErrorMessages.Missing_Outgoing_Compounds:
                    resultMessage = "Outgoing stream(s) contains {0} which is (are) NOT specified in the incoming stream(s).  Make sure that every compound that leaves a processing unit also enters that unit";

                    //concatenate on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

                case ErrorMessages.Sum_Does_Not_Equal_Total_Quantity:
                    resultMessage = "The sum of the quantities of all individual compounds is not equal to the quantity of the overall stream, or 100%.  Make sure that the quantities of all individual compounds add up to the quantity of overall stream, or 100%";
                    break;
                case ErrorMessages.Inconsistant_Units:
                    resultMessage = "The units: \"{0}\" and \"{1}\" where used together, which is invalid";

                    resultMessage = String.Format(resultMessage, list[0], list[1]);
                    break;

                case ErrorMessages.Equation_Variable_Not_In_Tables:
                    resultMessage = "The following terms are undefined: {0}";

                    //concatenate on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

                case ErrorMessages.Incorrect_Use_Of_Percent:
                    resultMessage = "Improper use of percentages in the equation.  Percentages must appear in equations in the following format: the row label / 100 * overall label for that table.  Example: m11 / 100 * M1";
                    break;

                case ErrorMessages.Not_Overall:
                    resultMessage = "This equation is not a summation of the overalls.  Only the overall row can be used in this equation";
                    break;
                case ErrorMessages.More_Than_One_Compound:
                    resultMessage = "This material balance must contain only one compound.  The compound expect was {0}, but the compound found was {1}";
                    resultMessage = String.Format(resultMessage, list[0], list[1]);
                    break;
                case ErrorMessages.More_Than_One_Element:
                    resultMessage = "This material balance must contain only one element.  The element expect was {0}, but the compound {1} was found which does not contain the specified element";
                    resultMessage = String.Format(resultMessage, list[0], list[1]);
                    break;
                case ErrorMessages.Unused_Variables:
                    resultMessage = "Warning: These unknown(s) are not used in the equations {0}.\n Each unknown must be used in at least one equation";

                    //concatenate on the list of missing materials
                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;

                case ErrorMessages.Quadratic_Equation:
                    resultMessage = "You have specified at least one quadratic equation,\n quadratic equations are not yet supported.\n Please modify your equations to make them linear";
                    break;

                case ErrorMessages.Equations_and_Unknowns:
                    resultMessage = "You have {0} valid equations(s) and {1} unknown(s).\n The number of independent equations and the number of unknowns should be the same";

                    //concatenate on the list of missing materials
                    resultMessage = String.Format(resultMessage, list[0] as string, list[1] as string);
                    //resultMessage = String.Format(resultMessage, two as string);
                    break;

                case ErrorMessages.Not_Independent:
                    resultMessage = "The set of equations that you have created is NOT solvable.\nThe equations that you have written are not independent of each other";
                    break;

                case ErrorMessages.Solvable:
                    resultMessage = "Congratulations! The set of equations that you have created is solvable";
                    break;

                case ErrorMessages.Insuffcient_infomation:
                    resultMessage = "Insufficient information to check";
                    break;

                case ErrorMessages.Individual_Flowrate_Mismatch:
                    resultMessage = "Overall mass balance is satisfied.  One or more of the individual mass balances across the processing unit attached to this stream are NOT satisfied.  Check the amount of each compound entering and leaving the process unit.";
                    break;

                case ErrorMessages.Overall_Units_Mismatch:
                    resultMessage = "Overall mass balance across the process unit connected to this stream is not satisfied.  Make sure that the quantities of all incoming and outgoing streams match";
                    break;

                case ErrorMessages.Not_In_Moles:
                    resultMessage = "Units going to and from a reactor must be in moles or moles per second";
                    break;

                case ErrorMessages.NonUniqueNames:
                    resultMessage = "These labels appear more than once: {0}.  Each label name may appear only once";

                    resultMessage = String.Format(resultMessage, String.Join(", ", list as string[]));
                    break;
                case ErrorMessages.Incoming_Outgoing_Streams_Mismatch:
                    resultMessage = "Streams that enter and leave a heat exchanger must exactly match except for temperature";

                    break;
                case ErrorMessages.InCorrect_Temperature:
                    resultMessage = "The temperature of an outgoing stream must be in the range of the temperatures of the incoming streams";
                    break;
                case ErrorMessages.Unconserved_Energy:
                    resultMessage = "Energy is not being conserved across a process unit";
                    break;
                case ErrorMessages.InValid_Heat_Equation:
                    resultMessage = "This heat equation is not in a valid format.  The valid format is Enthalpy equals Q or Sum of Enthalpy in equals Sum of Enthalpy out. Enthalpy must be written as Hf?? + Cp?? * Temp - 25 * moles, where the ?? is the abbreviation of the current compound";
                    break;
                case ErrorMessages.Incorrect_Abbrv:
                    resultMessage = "The abbreviation {0} does not match the compound used or another abbreviation";

                    //concatenate on the list of missing materials
                    resultMessage = String.Format(resultMessage);
                    break;
                case ErrorMessages.Unknown_Constant:
                    resultMessage = "The constant(s) used in this equation have no actual value and cannot be used";
                    break;
                case ErrorMessages.PFD_not_connected:
                    resultMessage = "One or more process units or streams are not connected to the rest of the PFD.  Please make sure everything is connected properly and delete any unnecessary process units or streams";
                    break;
                case ErrorMessages.Missing_equation:
                    resultMessage = "There are more independent equations that you can write at this level of abstraction";
                    break;

                case ErrorMessages.Equation_doesnt_match_PFD:
                    resultMessage = "The equation does not match the PFD, or it is in a format that ChemProV cannot recognize try to re-write it";
                    break;
                case ErrorMessages.Variable_Defination_Incorrect_Format:
                    resultMessage = "The Variable Definition is not in the correct format please change it so the variable label = some expression";
                    break;
                default:
                    resultMessage = "There are no messages, for me to poop on!";
                    break;
            }
            return resultMessage;
        }
    }
}