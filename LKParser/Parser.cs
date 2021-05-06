﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;


namespace LKPocParser
{
    public class Parser
    {
        public ParseResults Parse(string procText)
        {
            var parser = new TSql150Parser(true);
            var textReader = new StringReader(procText);
            var sqlTree = parser.Parse(textReader, out var errors);
            if (errors.Count > 0)
            {
                ParserException e = new ParserException(errors);
                throw e;
            }
            var visitor = new Visitor();
            sqlTree.Accept(visitor);
            return visitor.Results;
        }
    }
}
