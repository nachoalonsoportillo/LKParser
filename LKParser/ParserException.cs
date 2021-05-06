using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LKPocParser
{
    public class ParserException : Exception
    {
        public IList<ParseError> Errors { get; private set; }

        public ParserException(IList<ParseError> errors)
        {
            Errors = errors;
        }

    }
}
