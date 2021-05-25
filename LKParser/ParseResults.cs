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
        UpdateTargetPointsToAlias = 2,
        IsDDL = 4
    }

    [Flags]
    public enum StatementType
    {
        Undefined = 0,
        Select = 1,
        Insert = 2,
        Update = 3,
        Delete = 4,
        CreateTable = 5,
        AlterTable = 6,
        DeclareVariable = 7,
        SetVariable = 8,
        TruncateTable = 9,
        DropTable = 20,
    }

    public class ParseResults
    {
        private Features features;
        private StatementType statementType;

        public ParseResults()
        {
            statementType = new StatementType();
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

        internal void SetStatementText(string text)
        {
            StatementText = text;
        } 
        public string StatementText { get; private set; }
        public string TargetTable { get; private set; }
        public ArrayList SourceTables { get; private set; }
        public Features StatementFeatures { get { return features; } }
        public ArrayList Alias { get; private set; }

        internal void SetFeature(Features feature)
        {
            FlagsHelper.Set(ref features, feature);
        }

        internal void SetStatementType(StatementType type)
        {
            FlagsHelper.Set(ref statementType, type);
        }
    }
}
