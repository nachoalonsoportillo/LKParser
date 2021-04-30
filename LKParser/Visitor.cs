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
            HasWhereClause = false;
        }
        public HashSet<string> SourceTables { get; set; }
        public string TargetTable { get; set; }

        public bool HasWhereClause { get; set; }

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
                case InsertStatement insSt:
                    Traverse(insSt.InsertSpecification, ++nestingLevel);
                    break;
                case InsertSpecification ins:
                    if (ins.Target is NamedTableReference)
                    {
                        TargetTable = QualifiedName(((NamedTableReference)(ins.Target)).SchemaObject);
                    }
                    if (ins.InsertSource is SelectInsertSource)
                    {
                        Traverse(((SelectInsertSource)ins.InsertSource).Select, ++nestingLevel);
                    }
                    break;
                case UpdateStatement updSt:
                    Traverse(updSt.UpdateSpecification, ++nestingLevel);
                    break;
                case UpdateSpecification us:
                    if (us.Target is NamedTableReference)
                    {
                        TargetTable = QualifiedName(((NamedTableReference)(us.Target)).SchemaObject);
                    }
                    if (us.FromClause != null)
                    {
                        Traverse(((FromClause)us.FromClause).TableReferences[0], ++nestingLevel);
                    }
                    if (us.WhereClause != null)
                    {
                        HasWhereClause = true;
                    }
                    break;
                case SelectStatement selSt:
                    if (selSt.Into != null)
                    {
                        TargetTable = QualifiedName(selSt.Into);
                    }
                    Traverse(((QuerySpecification)(selSt.QueryExpression)), ++nestingLevel);
                    break;
                case QuerySpecification qs:
                    if (qs.WhereClause != null)
                    {
                        HasWhereClause = true;
                    }
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
                case NamedTableReference nt:
                    SourceTables.Add(QualifiedName(nt.SchemaObject));
                    break;
                default:
                    Debug.Assert(false, "This node type is not implemented yet!!!");
                    break;
            }
            --nestingLevel;
        }

        public override void Visit(InsertStatement node)
        {
            Traverse(node, 1);
        }

        public override void Visit(UpdateStatement node)
        {
            Traverse(node, 1);
        }

        public override void Visit(SelectStatement node)
        {
            Traverse(node, 1);
        }
    }

}
