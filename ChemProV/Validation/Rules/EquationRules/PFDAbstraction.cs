/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using ChemProV.PFD.EquationEditor;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;
using ChemProV.PFD.Streams.PropertiesWindow;
using ChemProV.PFD.Streams.PropertiesWindow.Heat;
using ChemProV.Validation.Rules.Adapters.Table;
using ChemProV.PFD.EquationEditor.Models;

namespace ChemProV.Validation.Rules.EquationRules
{
    public class PFDAbstraction
    {
        public List<ValidationResult> CheckEquationsAgainstPFD(IEnumerable<EquationData> equations, List<IProcessUnit> processUnits, Dictionary<string, GenericTableData> dictionaryOfTableData)
        {
            if (!(new GraphConnectivity().IsConnected(processUnits)))
            {
                return new List<ValidationResult>() { new ValidationResult(equations.FirstOrDefault().EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.PFD_not_connected)) };
            }

            //Graph is connected

            Dictionary<int, List<List<IProcessUnit>>> abstractedGraphs = CreateAbstractedPFD(processUnits);

            Dictionary<List<IProcessUnit>, List<EquationData>> matchedGraphsAndEquations;

            try
            {
                matchedGraphsAndEquations = MatchEquationsToAbstractedGraphs(equations, abstractedGraphs, dictionaryOfTableData);
            }
            catch
            {
                return new List<ValidationResult>() { new ValidationResult(equations.FirstOrDefault().EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Equation_doesnt_match_PFD)) };
            }

            switch (areEquationsIndependent(matchedGraphsAndEquations))
            {
                case -1:
                    return new List<ValidationResult>() { new ValidationResult(equations.FirstOrDefault().EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Missing_equation)) };
                case 0:
                    return new List<ValidationResult>() { new ValidationResult(equations.FirstOrDefault().EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Solvable)) };
                case 1:
                    return new List<ValidationResult>() { new ValidationResult(equations.FirstOrDefault().EquationReference, ErrorMessageGenerator.GenerateMesssage(ErrorMessages.Not_Independent)) };
            }
            return new List<ValidationResult>();
        }

        public Dictionary<List<IProcessUnit>, List<EquationData>> MatchEquationsToAbstractedGraphs(IEnumerable<EquationData> equations, Dictionary<int, List<List<IProcessUnit>>> abstractedGraphs, Dictionary<string, GenericTableData> dictionaryOfTableData)
        {
            //This dictionary takes a graph and the equations  that uses it
            Dictionary<List<IProcessUnit>, List<EquationData>> matchedGraphsAndEquations = new Dictionary<List<IProcessUnit>, List<EquationData>>();
            foreach (EquationData eqData in equations)
            {
                if (eqData.Type != null && eqData.IsValid && eqData.Type.Classification != EquationTypeClassification.VariableDefinition)
                {
                    if (eqData.VariableNames.Item1[0].Count() >= 1 && eqData.VariableNames.Item2[0].Count() >= 1)
                    {
                        IStream stream1 = null;
                        int index = 0;
                        while (stream1 == null && index < eqData.VariableNames.Item1.Count)
                        {
                            if (dictionaryOfTableData.Keys.Contains(eqData.VariableNames.Item1[index]))
                            {
                                stream1 = dictionaryOfTableData[eqData.VariableNames.Item1[index]].Parent.ParentStream;
                            }
                            index++;
                        }

                        IStream stream2 = null;
                        index = 0;
                        while (stream2 == null && index < eqData.VariableNames.Item2.Count)
                        {
                            if (dictionaryOfTableData.Keys.Contains(eqData.VariableNames.Item2[index]))
                            {
                                stream2 = dictionaryOfTableData[eqData.VariableNames.Item2[index]].Parent.ParentStream;
                            }
                            index++;
                        }

                        List<IProcessUnit> graph = null;

                        if (stream1 != null && stream2 != null)
                        {
                            graph = FindGraph(stream1, stream2, abstractedGraphs);
                        }

                        if (graph != null)
                        {
                            if (isValidEquation(eqData, graph, dictionaryOfTableData))
                            {
                                if (!(matchedGraphsAndEquations.Keys.Contains(graph)))
                                {
                                    matchedGraphsAndEquations.Add(graph, new List<EquationData>());
                                }

                                matchedGraphsAndEquations[graph].Add(eqData);
                            }
                            else
                            {
                                throw new Exception("The equation was not valid");
                            }
                        }
                        else
                        {
                            //Did not find graph that the equation was referencing
                            //Bad equation?
                            throw new Exception("We have an error");
                        }
                    }
                }
            }

            return matchedGraphsAndEquations;
        }

        /// <summary>
        ///Determines if the equations are independent for each graph
        /// </summary>
        /// <param name="graphs"></param>
        /// <returns>0 correct number of equations, 1 for too many, and -1 for too few</returns>
        private int areEquationsIndependent(Dictionary<List<IProcessUnit>, List<EquationData>> graphs)
        {
            foreach (List<IProcessUnit> graph in graphs.Keys)
            {
                bool usingElements = false;
                foreach (EquationData eqData in graphs[graph])
                {
                    //Elements and Compounds cannot be mixed so if one is Element it overrides compound
                    //maybe throw an error if they try to mix em (Overall is still allowed
                    if (eqData.Type.Classification == EquationTypeClassification.Atom)
                    {
                        usingElements = true;
                    }
                }

                //If greater (as written below) then equations are not independent
                //if less then we don't have enough equations to solve for every unknown
                if (graphs[graph].Count > findNumberOfCompounds(graph, usingElements))
                {
                    //to many (not independent)
                    return 1;
                }
                else if (graphs[graph].Count < findNumberOfCompounds(graph, usingElements))
                {
                    //to few
                    return -1;
                }
            }
            return 0;
        }

        /// <summary>
        /// finds the number of unique species (heat, (elements or compounds)) that go across the APU
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="usingElements">If true finds the number of different elements used if false finds the number of different compounds used</param>
        /// <returns></returns>
        private int findNumberOfCompounds(List<IProcessUnit> graph, bool usingElements)
        {
            AbstractedProcessUnit apu = (from c in graph where c is AbstractedProcessUnit select c as AbstractedProcessUnit).FirstOrDefault();

            List<string> species = new List<string>();

            foreach (IStream stream in apu.IncomingStreams)
            {
                if (stream is HeatStream)
                {
                    if (!(species.Contains("Heat")))
                    {
                        species.Add("Heat");
                    }
                }
                else
                {
                    ITableAdapter ta = TableAdapterFactory.CreateTableAdapter(stream.Table);

                    int i = 0;
                    while (i < ta.GetRowCount())
                    {
                        if (usingElements == false)
                        {
                            string compound = ta.GetCompoundAtRow(i);
                            if (compound != "Overall")
                            {
                                if (!(species.Contains(compound)))
                                {
                                    species.Add(compound);
                                }
                            }
                        }
                        else
                        {
                            Compound c = CompoundFactory.GetElementsOfCompound(ta.GetCompoundAtRow(i));
                            foreach (Element element in c.elements.Keys)
                            {
                                if (!(species.Contains(element.Name)))
                                {
                                    species.Add(element.Name);
                                }
                            }
                        }
                        i++;
                    }
                }
            }

            return species.Count;
        }

        private bool isValidEquation(EquationData eqData, List<IProcessUnit> graph, Dictionary<string, GenericTableData> dictionaryOfTableData)
        {
            if (eqData.Type.Classification == EquationTypeClassification.Total || eqData.Type.Classification == EquationTypeClassification.Energy || eqData.Type.Classification == EquationTypeClassification.Compound || eqData.Type.Classification == EquationTypeClassification.Atom)
            {
                AbstractedProcessUnit apu = (from c in graph where c is AbstractedProcessUnit select c as AbstractedProcessUnit).FirstOrDefault();
                bool lhsIsIncomming = true;

                if ((from c in apu.OutgoingStreams
                     where c.Table == dictionaryOfTableData[eqData.VariableNames.Item1[0]].Parent
                     select c).FirstOrDefault() != null)
                {
                    lhsIsIncomming = false;
                }

                if (eqData.Type.Classification == EquationTypeClassification.Total)
                {
                    if (lhsIsIncomming)
                    {
                        if (!(allVariablesExistAndAllStreamsUsed(eqData.VariableNames.Item1, apu.IncomingStreams, dictionaryOfTableData)))
                        {
                            return false;
                        }
                        if (!(allVariablesExistAndAllStreamsUsed(eqData.VariableNames.Item2, apu.OutgoingStreams, dictionaryOfTableData)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!(allVariablesExistAndAllStreamsUsed(eqData.VariableNames.Item1, apu.OutgoingStreams, dictionaryOfTableData)))
                        {
                            return false;
                        }
                        if (!(allVariablesExistAndAllStreamsUsed(eqData.VariableNames.Item2, apu.IncomingStreams, dictionaryOfTableData)))
                        {
                            return false;
                        }
                    }
                }
            }

                //Not overall
            else
            {
            }
            return true;
        }

        private bool allVariablesExistAndAllStreamsUsed(List<string> variableNames, IList<IStream> streams, Dictionary<string, GenericTableData> dictionaryOfTableData)
        {
            bool[] allstreamsUsed = new bool[streams.Count];
            foreach (string s in variableNames)
            {
                IStream stream = (from c in streams where c.Table == dictionaryOfTableData[s].Parent select c).FirstOrDefault();
                if (stream == null)
                {
                    return false;
                }

                allstreamsUsed[streams.IndexOf(stream)] = true;
            }

            //Heat streams don't count for this so set them all to true
            foreach (IStream stream in streams)
            {
                if (stream.Table is HeatStreamPropertiesWindow)
                {
                    allstreamsUsed[streams.IndexOf(stream)] = true;
                }
            }

            foreach (bool b in allstreamsUsed)
            {
                if (b == false)
                {
                    return false;
                }
            }

            return true;
        }

        private List<IProcessUnit> FindGraph(IStream stream1, IStream stream2, Dictionary<int, List<List<IProcessUnit>>> abstractedGraphs)
        {
            foreach (KeyValuePair<int, List<List<IProcessUnit>>> levels in abstractedGraphs)
            {
                foreach (List<IProcessUnit> graph in levels.Value)
                {
                    AbstractedProcessUnit apu = (from c in graph
                                                 where c is AbstractedProcessUnit
                                                 select c as AbstractedProcessUnit).FirstOrDefault();

                    bool foundFirstStream = (from c in apu.OutgoingStreams
                                             where c.Table == stream1.Table
                                             select c).FirstOrDefault() != null;

                    bool foundSecondStream = (from c in apu.OutgoingStreams
                                              where c.Table == stream2.Table
                                              select c).FirstOrDefault() != null;

                    if (foundFirstStream == false)
                    {
                        foundFirstStream = (from c in apu.IncomingStreams
                                            where c.Table == stream1.Table
                                            select c).FirstOrDefault() != null;
                    }
                    if (foundSecondStream == false)
                    {
                        foundSecondStream = (from c in apu.IncomingStreams
                                             where c.Table == stream2.Table
                                             select c).FirstOrDefault() != null;
                    }

                    if (foundFirstStream && foundSecondStream)
                    {
                        return graph;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// This assumes the graph is connected and will create several graphs each with exactly 1 abstract process unit
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public Dictionary<int, List<List<IProcessUnit>>> CreateAbstractedPFD(List<IProcessUnit> nodes)
        {
            //CollectionOfGraphs is a List of Levels (key) and the graphs at that level (value)
            Dictionary<int, List<List<IProcessUnit>>> CollectionOfGraphs = new Dictionary<int, List<List<IProcessUnit>>>();

            CreateAbstractedGraphs(nodes, CollectionOfGraphs, 0);

            return CollectionOfGraphs;
        }

        public void CreateAbstractedGraphs(List<IProcessUnit> graph, Dictionary<int, List<List<IProcessUnit>>> collectionOfGraphs, int level)
        {
            //This list will be inserted into graphsAtThisLevel once it has been built (aka the end of this function)
            List<List<IProcessUnit>> newGraphs = new List<List<IProcessUnit>>();

            AbstractedProcessUnit oldApu = (from c in graph where c is AbstractedProcessUnit select c as AbstractedProcessUnit).FirstOrDefault();

            if (oldApu == null)
            {
                //if no abstract process units exist yet we are at the base level so each process unit becomes an abstract
                foreach (IProcessUnit pu in graph)
                {
                    if (!(pu is TemporaryProcessUnit))
                    {
                        List<IProcessUnit> newGraph = CreateNewGraph(graph);
                        IProcessUnit newPU = newGraph[graph.IndexOf(pu)];
                        AbstractedProcessUnit newApu = new AbstractedProcessUnit();
                        foreach (IStream stream in newPU.IncomingStreams)
                        {
                            newApu.IncomingStreams.Add(stream);
                            stream.Destination = newApu;
                        }
                        foreach (IStream stream in newPU.OutgoingStreams)
                        {
                            newApu.OutgoingStreams.Add(stream);
                            stream.Source = newApu;
                        }

                        newGraph.Remove(newPU);
                        newGraph.Add(newApu);

                        //no need to check for duplicates since we are just replacing one process unit at a time
                        newGraphs.Add(newGraph);
                    }
                }
            }
            else
            {
                //since an APU exists then we got to add to it
                //We need only worry about the outgoing streams otherwise we would get twice as many duplicates
                //because we are building up every possible graph there must be one that has an APU before us
                //and when that merges with PU after it, it will look the same as the graph we would form now
                //if this apu merged with the ones behind it
                foreach (IStream stream in oldApu.OutgoingStreams)
                {
                    //if destination is not a TPU
                    if (!(stream.Destination is TemporaryProcessUnit))
                    {
                        List<IProcessUnit> newGraph = CreateNewGraph(graph);
                        IProcessUnit newDestination = newGraph[graph.IndexOf(stream.Destination)];
                        AbstractedProcessUnit newApu = (from c in newGraph where c is AbstractedProcessUnit select c as AbstractedProcessUnit).FirstOrDefault();
                        int i = 0;

                        //attach all outgoing Streams from oldApu
                        while (i < newApu.OutgoingStreams.Count)
                        {
                            IStream s = newApu.OutgoingStreams[i];
                            if (s.Destination == newDestination)
                            {
                                //do not need to increment when we delete something
                                newApu.OutgoingStreams.RemoveAt(i);
                            }
                            else
                            {
                                i++;
                            }
                        }

                        //the APU gets all of the newDestinations outgoing streams
                        foreach (IStream s in newDestination.OutgoingStreams)
                        {
                            newApu.OutgoingStreams.Add(s);
                            s.Source = newApu;
                        }

                        //the APU gets all of the newDestinations incoming streams
                        foreach (IStream s in newDestination.IncomingStreams)
                        {
                            if (s.Source != newApu)
                            {
                                newApu.IncomingStreams.Add(s);
                                s.Destination = newApu;
                            }
                        }

                        //then we remove the process unit purposefully removing all links
                        newGraph.Remove(newDestination);

                        //check for duplicates
                        if (isRedundent(newGraph, newGraphs) == false)
                        {
                            newGraphs.Add(newGraph);
                        }
                    }
                }
            }

            if (collectionOfGraphs.Keys.Contains(level))
            {
                collectionOfGraphs[level].AddRange(newGraphs);
            }
            else
            {
                collectionOfGraphs.Add(level, newGraphs);
            }

            //The base case of just one non temporary process unit will create no graphs so this for each will exit before it does anything
            foreach (List<IProcessUnit> g in newGraphs)
            {
                CreateAbstractedGraphs(g, collectionOfGraphs, level + 1);
            }
        }

        public bool isRedundent(List<IProcessUnit> graph, List<List<IProcessUnit>> graphs)
        {
            foreach (List<IProcessUnit> oldGraph in graphs)
            {
                if (!isEqual(graph, oldGraph))
                {
                    return true;
                }
            }
            return false;
        }

        public bool isEqual(List<IProcessUnit> lhs, List<IProcessUnit> rhs)
        {
            //since we just added an abstracted process unit and the rest should be the same if the streams on the abstract pu is the same
            //then we can safely say it is the same graph if they are not the same then they cannot be the same graph

            AbstractedProcessUnit lhsApu = (from c in lhs
                                            where c is AbstractedProcessUnit
                                            select c as AbstractedProcessUnit).FirstOrDefault();

            AbstractedProcessUnit rhsApu = (from c in rhs
                                            where c is AbstractedProcessUnit
                                            select c as AbstractedProcessUnit).FirstOrDefault();

            if (lhsApu == null || rhsApu == null)
            {
                //maybe throw an exception since this should never happen
                return false;
            }
            if (!(AreStreamSetsEqual(lhsApu.IncomingStreams, rhsApu.IncomingStreams)))
            {
                return false;
            }

            if (!(AreStreamSetsEqual(lhsApu.OutgoingStreams, rhsApu.OutgoingStreams)))
            {
                return false;
            }
            return true;
        }

        public bool AreStreamSetsEqual(IList<IStream> lhs, IList<IStream> rhs)
        {
            foreach (IStream s in lhs)
            {
                int i = 0;
                bool found = false;
                while (i < rhs.Count)
                {
                    if (rhs[i].Table == s)
                    {
                        found = true;
                        break;
                    }
                    i++;
                }
                if (!found)
                {
                    return false;
                }
            }
            return true;
        }

        public void SetStreams(bool incoming, IProcessUnit pu, ICollection<IStream> streams)
        {
            if (incoming)
            {
                pu.IncomingStreams.Clear();
                foreach (IStream s in streams)
                {
                    pu.IncomingStreams.Add(s);
                }
            }
            else
            {
                pu.OutgoingStreams.Clear();
                foreach (IStream s in streams)
                {
                    pu.OutgoingStreams.Add(s);
                }
            }
        }

        public List<IProcessUnit> CreateNewGraph(List<IProcessUnit> graph)
        {
            List<IProcessUnit> newGraph = new List<IProcessUnit>();
            foreach (IProcessUnit pu in graph)
            {
                IProcessUnit newPu;
                if (pu is AbstractedProcessUnit)
                {
                    newPu = new AbstractedProcessUnit();
                }

                else
                {
                    newPu = ProcessUnitFactory.ProcessUnitFromUnitType(ProcessUnitFactory.GetProcessUnitType(pu));
                }
                newGraph.Add(newPu);
            }
            foreach (IProcessUnit pu in graph)
            {
                //we draw all the outgoing streams as every incoming stream must also be an outgoing stream
                //we will draw all the streams but have no duplicates
                foreach (IStream stream in pu.OutgoingStreams)
                {
                    IStream newStream = StreamFactory.StreamFromStreamType(StreamFactory.StreamTypeFromStream(stream));
                    newStream.Table = stream.Table;
                    newStream.Source = newGraph[graph.IndexOf(stream.Source)];
                    newStream.Destination = newGraph[graph.IndexOf(stream.Destination)];
                    newStream.Source.OutgoingStreams.Add(newStream);
                    newStream.Destination.IncomingStreams.Add(newStream);
                }
            }

            return newGraph;
        }
    }
}