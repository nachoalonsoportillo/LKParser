using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;

namespace LKPocParser
{

    internal class Visitor : TSqlFragmentVisitor
    {
        public Visitor()
        {
            SourceTables = new HashSet<string>();
            TargetTable = "";
        }
        public HashSet<string> SourceTables { get; set; }
        public string TargetTable { get; set; }
        private string QualifiedName(SchemaObjectName name)
        {
            bool AnyPartSet = false;
            string result = "";
            if (name.ServerIdentifier != null)
            {
                AnyPartSet = true;
                result = string.Format("[{0}].", name.ServerIdentifier.Value);
            }

            if (name.DatabaseIdentifier != null && !String.IsNullOrEmpty(name.DatabaseIdentifier.Value))
            {
                AnyPartSet = true;
                result += string.Format("[{0}].", name.DatabaseIdentifier.Value);
            }
            else if (AnyPartSet)
            {
                result += ".";
            }

            if (name.SchemaIdentifier != null && !String.IsNullOrEmpty(name.SchemaIdentifier.Value))
            {
                AnyPartSet = true;
                result += string.Format("[{0}].", name.SchemaIdentifier.Value);
            }
            else if (AnyPartSet)
            {
                result += ".";
            }

            result += string.Format("[{0}]", name.BaseIdentifier.Value);

            return result;
        }
        public void Traverse(TSqlFragment node, int nestingLevel)
        {
            Debug.Assert(nestingLevel < 10, "Maximum recursion level exceeded.");
            switch (node)
            {
                case NamedTableReference nt:
                    SourceTables.Add(QualifiedName(nt.SchemaObject));
                    break;

                case QuerySpecification qs:
                    Traverse(qs.FromClause.TableReferences[0], ++nestingLevel);

                    break;
                case UnqualifiedJoin uj:
                    Traverse(uj.FirstTableReference, ++nestingLevel);
                    Traverse(uj.SecondTableReference, ++nestingLevel);
                    break;
                case QualifiedJoin qj:
                    Traverse(qj.FirstTableReference, ++nestingLevel);
                    Traverse(qj.SecondTableReference, ++nestingLevel);
                    break;
                case BinaryQueryExpression bqs:
                    Traverse(bqs.FirstQueryExpression, ++nestingLevel);
                    Traverse(bqs.SecondQueryExpression, ++nestingLevel);
                    break;
                default:
                    Debug.Assert(false, "This node type is not implemented yet!!!");
                    break;
            }
            --nestingLevel;
        }

        public override void Visit(InsertStatement node)
        {
            if (node.InsertSpecification.Target is NamedTableReference)
            {
                TargetTable = QualifiedName(((NamedTableReference)(node.InsertSpecification.Target)).SchemaObject);
            }
            if (node.InsertSpecification.InsertSource is SelectInsertSource)
            {
                Traverse(((SelectInsertSource)node.InsertSpecification.InsertSource).Select, 1);
            }
        }

        public override void Visit(UpdateStatement node)
        {
            if (node.UpdateSpecification.Target is NamedTableReference)
            {
                TargetTable = QualifiedName(((NamedTableReference)(node.UpdateSpecification.Target)).SchemaObject);
            }
            if (node.UpdateSpecification.FromClause != null)
            {
                Traverse(((FromClause)node.UpdateSpecification.FromClause).TableReferences[0], 1);
            }
        }

        public override void Visit(SelectStatement node)
        {
            if (node.Into != null)
            {
                TargetTable = QualifiedName(node.Into);
            }
            Traverse(((QuerySpecification)(node.QueryExpression)), 1);
        }
    }

}
