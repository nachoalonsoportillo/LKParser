using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using System.Diagnostics;

namespace LKPocParser
{
    public class Parser
    {
        public List<ParseResults> Parse(string procText)
        {
            var parser = new TSql150Parser(true);
            var textReader = new StringReader(procText);
            var sqlTree = parser.Parse(textReader, out var errors);
            if (errors.Count > 0)
            {
                throw new ParserException(errors);
            }
            var visitor = new Visitor();
            sqlTree.Accept(visitor);

            TSqlScript sqlScript = (TSqlScript)sqlTree;
            Debug.Assert(sqlScript.Batches.Count == 1, "Currently our visitor doesn't support multiple batches.");
            Debug.Assert(visitor.Results.Count == sqlScript.Batches[0].Statements.Count, "Statements processed by visitor don't match statements counted by parser.");

            int i = 0;
            foreach (var stm in sqlScript.Batches[0].Statements)
            {
                visitor.Results[i++].SetStatementText(procText.Substring(stm.StartOffset, stm.FragmentLength));
            }
            return visitor.Results;
        }
    }
}
