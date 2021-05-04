using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKPocParser
{
    public class ParseResults
    {
        public string TargetTable { get; set; }
        public ArrayList SourceTables { get; set; }
        public Features StatementFeatures { get; set; }
    }
}
