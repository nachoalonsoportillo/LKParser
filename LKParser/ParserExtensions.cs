using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKPocParser
{
    public static class ParserExtensions
    {
        public static bool IsNull(this TableReference tableRef )
        {
            return tableRef is NamedTableReference
                       && ((NamedTableReference)(tableRef)).SchemaObject.ServerIdentifier == null
                       && ((NamedTableReference)(tableRef)).SchemaObject.DatabaseIdentifier == null
                       && ((NamedTableReference)(tableRef)).SchemaObject.SchemaIdentifier == null;
        }

        public static string FormatPart(this string part, bool lastpart=false)
        {
            if (part == null)
                return null;

            var formattedPart = $"[{part}]";
            return !lastpart ? $"{formattedPart}." : formattedPart;
        }

        public static string FullyQualifiedName(this SchemaObjectName schemaObjectName)
        {
            string serverIdentifier = schemaObjectName.ServerIdentifier?.Value;
            string databaseIdentifier = schemaObjectName.DatabaseIdentifier?.Value;
            string schemaIdentifier = schemaObjectName.SchemaIdentifier?.Value;
            string baseIdentifier = schemaObjectName.BaseIdentifier?.Value;

            string result = $"{serverIdentifier.FormatPart()}{databaseIdentifier.FormatPart()}{schemaIdentifier.FormatPart()}{baseIdentifier.FormatPart(true)}";

            return result;
        }

    }
}
