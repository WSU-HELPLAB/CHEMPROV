/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/

using System;
using System.Collections.Generic;

using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.Validation.Rules.Adapters.Table;

namespace ChemProV.Validation.Rules.ProcessUnitRules
{
    public class HeatExchangerWithoutUtilityProcessUnitRule : GenericProcessUnitRule
    {
        // Extensive refactoring has really broken all the rules, but since they're not used 
        // at the moment I'm commenting them out. When the time comes where we want to start 
        // using the rules again, this code will have to be uncommented and altered.

        /*
        /// <summary>
        /// this stores the outgoing and incoming streams so we can set their feedback
        /// </summary>
        private List<ChemProV.PFD.Streams.AbstractStream> feedbackTarget;

        /// <summary>
        /// The first element is the table before it hit the heat exchange without utility.
        /// The second element is after it has gone through the heat exchange without utility.
        /// </summary>
        private Tuple<ITableAdapter, ITableAdapter> matchingStreams1;

        /// <summary>
        /// The first element is the table before it hit the heat exchange without utility.
        /// The second element is after it has gone through the heat exchange without utility.
        /// </summary>
        private Tuple<ITableAdapter, ITableAdapter> matchingStreams2;

        public override void CheckRule()
        {
            feedbackTarget = new List<ChemProV.PFD.Streams.AbstractStream>(target.OutgoingStreams);
            feedbackTarget.AddRange(target.IncomingStreams);

            if (target.IncomingStreams.Count > 0 || target.OutgoingStreams.Count > 0)
            {
                ValidationResult vr;
                vr = CheckIncomingStreamsMatchOutgoingStreams();
                if (vr != ValidationResult.Empty)
                {
                    ValidationResults.Add(vr);
                    return;
                    //cannot check transferEffect
                }
                vr = CheckTransferEffect();
                if (vr != ValidationResult.Empty)
                {
                    ValidationResults.Add(vr);
                    //no point in checking conservation if the transferEffect isn't true
                }
                vr = CheckSameUnits();
                if (vr != ValidationResult.Empty)
                {
                    ValidationResults.Add(vr);
                    //no point in checking conservation if the units are not the same isn't true
                }
                if (ValidationResults.Count == 0)
                {
                    vr = CheckConservation();
                    if (vr != ValidationResult.Empty)
                    {
                        ValidationResults.Add(vr);
                        return;
                    }
                }
            }
        }

        private ValidationResult CheckSameUnits()
        {
            if (matchingStreams1 != null)
            {
                if (matchingStreams1.Item1.GetUnitAtRow(0) != matchingStreams1.Item2.GetUnitAtRow(0))
                {
                    return new ValidationResult(feedbackTarget, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Inconsistant_Units, matchingStreams1.Item1.GetUnitAtRow(0), matchingStreams1.Item2.GetUnitAtRow(0)));
                }
            }
            if (matchingStreams2 != null)
            {
                if (matchingStreams2.Item1.GetUnitAtRow(0) != matchingStreams2.Item2.GetUnitAtRow(0))
                {
                    return new ValidationResult(feedbackTarget, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Inconsistant_Units, matchingStreams2.Item1.GetUnitAtRow(0), matchingStreams2.Item2.GetUnitAtRow(0)));
                }
            }
            return ValidationResult.Empty;
        }

        private ValidationResult CheckConservation()
        {
            if (matchingStreams1 != null && matchingStreams2 != null)
            {
                double stream1Enthalpy = Enthalpy(matchingStreams1.Item1, matchingStreams1.Item2);
                double stream2Enthalpy = Enthalpy(matchingStreams2.Item1, matchingStreams2.Item2);
                if (double.IsNaN(stream1Enthalpy) || double.IsNaN(stream2Enthalpy))
                {
                    return ValidationResult.Empty;
                }
                else if (stream2Enthalpy - stream1Enthalpy != 0)
                {
                    return new ValidationResult(feedbackTarget, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Unconserved_Energy));
                }
            }
            return ValidationResult.Empty;
        }

        /// <summary>
        /// This finds the Enthalpy change between the tables.  This function assumes the tables are exactly the same except
        /// for tempature column.
        /// </summary>
        /// <param name="ingoingTable"></param>
        /// <param name="outgoingTable"></param>
        /// <returns></returns>
        private double Enthalpy(ITableAdapter ingoingTable, ITableAdapter outgoingTable)
        {
            double enthaply = 0;

            double ingoingTemperature;
            double outgoingTemperature;

            try
            {
                ingoingTemperature = double.Parse(ingoingTable.GetTemperature());
                outgoingTemperature = double.Parse(outgoingTable.GetTemperature());
            }
            catch
            {
                return double.NaN;
            }

            //skip first row
            int i = 1;
            while (i < ingoingTable.GetRowCount())
            {
                Compound currentCompound = CompoundFactory.GetElementsOfCompound(ingoingTable.GetCompoundAtRow(i));
                enthaply += (currentCompound.HeatCapacity * (Math.Abs(outgoingTemperature - ingoingTemperature) - 298)) * ingoingTable.GetActuallQuantityAtRow(i);
                i++;
            }

            return enthaply;
        }

        private ValidationResult CheckTransferEffect()
        {
            if (matchingStreams1 == null || matchingStreams2 == null)
            {
                return ValidationResult.Empty;
            }

            string temperature1BeforeStr = matchingStreams1.Item1.GetTemperature();
            string temperature2BeforeStr = matchingStreams2.Item1.GetTemperature();
            string temperature1AfterStr = matchingStreams1.Item2.GetTemperature();
            string temperature2AfterStr = matchingStreams2.Item2.GetTemperature();

            try
            {
                //our table adapter converts this the temperature to Celsius before returning it.
                double temperature1Before = double.Parse(temperature1BeforeStr);
                double temperature2Before = double.Parse(temperature2BeforeStr);
                double temperature1After = double.Parse(temperature1AfterStr);
                double temperature2After = double.Parse(temperature2AfterStr);

                if (temperature1Before > temperature2Before)
                {
                    //So stream 1 is hotter than stream 2
                    //                                                    low end        high end
                    //that is if the temperature after is not inbetween temp2Before to temp1Before
                    if (temperature2Before >= temperature1After || temperature1After >= temperature1Before || temperature2Before >= temperature2After || temperature2After >= temperature1Before)
                    {
                        return new ValidationResult(feedbackTarget, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InCorrect_Temperature));
                    }
                }
                else
                {
                    //So stream 2 is hotter than stream 1
                    //                                                    low end        high end
                    //that is if the temperature after is not inbetween temp1Before to temp2Before
                    if (temperature1Before >= temperature1After || temperature1After >= temperature2Before || temperature1Before >= temperature2After || temperature2After >= temperature2Before)
                    {
                        return new ValidationResult(feedbackTarget, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.InCorrect_Temperature));
                    }
                }
            }
            catch
            {
                //got a wild so they can all be anything
                return ValidationResult.Empty;
            }

            return ValidationResult.Empty;
        }

        /// <summary>
        /// Rule: a stream that goes in must come out, everything must be the same except for tempature
        /// </summary>
        /// <returns></returns>
        private ValidationResult CheckIncomingStreamsMatchOutgoingStreams()
        {
            bool matchedFirst = false;
            bool matchedSecond = false;

            if (target.ProcessUnit.IncomingStreamCount != target.ProcessUnit.OutgoingStreamCount)
            {
                return new ValidationResult(feedbackTarget, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incoming_Outgoing_Streams_Mismatch));
            }

            foreach (ChemProV.PFD.Streams.AbstractStream incomingStream in target.IncomingStreams)
            {
                int i = 0;
                foreach (ChemProV.PFD.Streams.AbstractStream outgoingStream in target.OutgoingStreams)
                {
                    if (!(i == 0 && matchedFirst == true))
                    {
                        if (isMatch(incomingStream.Table, outgoingStream.Table))
                        {
                            if (i == 0)
                            {
                                matchingStreams1 = new Tuple<ITableAdapter, ITableAdapter>(TableAdapterFactory.CreateTableAdapter(incomingStream.Table), TableAdapterFactory.CreateTableAdapter(outgoingStream.Table));
                                matchedFirst = true;
                            }
                            else
                            {
                                matchedSecond = true;
                                matchingStreams2 = new Tuple<ITableAdapter, ITableAdapter>(TableAdapterFactory.CreateTableAdapter(incomingStream.Table), TableAdapterFactory.CreateTableAdapter(outgoingStream.Table));
                            }
                            break;
                        }
                    }
                    i++;
                }
            }
            if ((target.IncomingStreams.Count == 2 && (matchedFirst != true || matchedSecond != true)) || target.IncomingStreams.Count == 1 && matchedFirst != true)
            {
                return new ValidationResult(feedbackTarget, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Incoming_Outgoing_Streams_Mismatch));
            }
            return ValidationResult.Empty;
        }

        /// <summary>
        /// Checks to see if 2 tables match
        /// </summary>
        /// <param name="entering"></param>
        /// <param name="leaving"></param>
        /// <returns></returns>
        private bool isMatch(IPropertiesWindow entering, IPropertiesWindow leaving)
        {
            ITableAdapter enteringTA = TableAdapterFactory.CreateTableAdapter(entering);
            ITableAdapter leavingTA = TableAdapterFactory.CreateTableAdapter(leaving);
            int rowCount = enteringTA.GetRowCount();

            if (rowCount != leavingTA.GetRowCount())
            {
                return false;
            }

            //check to see if all the rows match
            for (int row = 0; row < rowCount; row++)
            {
                //If anything on each row does not match we return false not equal
                if (enteringTA.GetQuantityAtRow(row) != leavingTA.GetQuantityAtRow(row))
                {
                    return false;
                }
                if (enteringTA.GetUnitAtRow(row) != leavingTA.GetUnitAtRow(row))
                {
                    return false;
                }
                if (enteringTA.GetCompoundAtRow(row) != leavingTA.GetCompoundAtRow(row))
                {
                    return false;
                }

                //Do not check temperature on purpose it is the one thing that is allowed / must changed.

                if (enteringTA.GetTemperatureUnits() != leavingTA.GetTemperatureUnits())
                {
                    return false;
                }
            }

            //if everything is the same we return true
            return true;
        }
        */
    }
}