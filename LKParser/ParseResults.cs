using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LKPocParser
{
    [Flags]
    public enum Features
    {
        None = 0,
        HasWhereClause = 1,
        UpdateTargetPointsToAlias = 2
    }

    public class ParseResults
    {
        private Features features;

        public ParseResults()
        {
            SourceTables = new ArrayList();
            features = new Features();
            Alias = new ArrayList();
            TargetTable = string.Empty;
        }

        internal void SetTargetTableFromAlias()
        {
            TargetTable = SourceTables[Alias.BinarySearch(TargetTable)].ToString();
        }

        internal void SetTargetTableFromSchema(SchemaObjectName schemaObjectName)
        {
            TargetTable = schemaObjectName != null ? schemaObjectName.FullyQualifiedName() : "";
        }

        internal void SetTargetTableFromTableRef(TableReference target)
        {
            if (target is NamedTableReference)
            {
                var tableRef = target as NamedTableReference;
                TargetTable = tableRef.SchemaObject.FullyQualifiedName();
            }
        }

        public string TargetTable { get; private set; }
        public ArrayList SourceTables { get; private set; }
        public Features StatementFeatures { get { return features; } }
        public ArrayList Alias { get; private set; }

        internal void SetFeature(Features feature)
        {
            FlagsHelper.Set(ref features, feature);
        }
    }
}
