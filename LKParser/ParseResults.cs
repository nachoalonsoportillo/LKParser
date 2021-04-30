using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKPocParser
{
    public class ParseResults
    {
        public string TargetTable { get; set; }
        public HashSet<string> SourceTables { get; set; }
        public bool HasWhereClause { get; set; }
    }
}
