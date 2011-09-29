/*
Copyright 2010, 2011 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Microsoft Reciprocal License (Ms-RL).
Consult "LICENSE.txt" included in this package for the complete Ms-RL license.
*/
﻿using System.Collections.Generic;
using System.Linq;
using ChemProV.PFD.ProcessUnits;
using ChemProV.PFD.Streams;

namespace ChemProV.Validation.Rules.EquationRules
{
    public class GraphConnectivity
    {
        public bool IsConnected(List<IProcessUnit> nodes)
        {
            //we select a source (random element)
            TemporaryProcessUnit source = (from c in nodes
                                           where c is TemporaryProcessUnit
                                           && (c as TemporaryProcessUnit).OutgoingStreams.Count > 0
                                           select c as TemporaryProcessUnit).FirstOrDefault();

            int numberVisited = 0;
            bool[] visited = new bool[nodes.Count];

            //if this is true then we have an empty graph
            if (source != null)
            {
                //go through all its adjacent nodes marking them as we go
                numberVisited = MarkNode(source, nodes, visited, numberVisited);
                numberVisited = MarkAllAdjacent(source, nodes, visited, numberVisited);
            }
            //if everything has been marked everything is connected otherwise it is not
            if (numberVisited == visited.Count())
            {
                return true;
            }

            return false;
        }

        private int MarkNode(IProcessUnit node, List<IProcessUnit> nodes, bool[] visited, int numberVisited)
        {
            visited[nodes.IndexOf(node)] = true;
            return ++numberVisited;
        }

        private bool HasNotBeenVisited(IProcessUnit node, List<IProcessUnit> nodes, bool[] visited)
        {
            return !visited[nodes.IndexOf(node)];
        }

        /// <summary>
        /// This is Depth first search, this 'marks' the nodes because if we have a loop then we wont detect it unless
        /// we mark them as we go so we can make sure not to re-iterate over each one
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodes"></param>
        /// <param name="visited"></param>
        private int MarkAllAdjacent(IProcessUnit node, List<IProcessUnit> nodes, bool[] visited, int numberVisited)
        {
            foreach (IProcessUnit decendent in GetAllAdjacent(node))
            {
                //if this has already be visited then so has all its children so we do nothing
                if (HasNotBeenVisited(decendent, nodes, visited))
                {
                    numberVisited = MarkNode(decendent, nodes, visited, numberVisited);
                    numberVisited = MarkAllAdjacent(decendent, nodes, visited, numberVisited);
                }
            }
            return numberVisited;
        }

        private IEnumerable<IProcessUnit> GetAllAdjacent(IProcessUnit node)
        {
            List<IProcessUnit> streams = new List<IProcessUnit>();
            foreach (IStream stream in node.OutgoingStreams)
            {
                streams.Add(stream.Destination);
            }
            foreach (IStream stream in node.IncomingStreams)
            {
                streams.Add(stream.Source);
            }

            return streams.AsEnumerable();
        }
    }
}