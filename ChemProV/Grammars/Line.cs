using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemProV.Grammars
{
    public class ChemProVLine
    {
        public string RawTreeForm { get; set; }
        public List<string> VariablesUsed { get; set; }

        public ChemProVLine()
        {
            VariablesUsed = new List<string>();
        }
    }
}
