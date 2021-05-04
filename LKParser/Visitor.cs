using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;

namespace LKPocParser
{
    [Flags]
    public enum Features
    {
        None = 0,
        HasWhereClause = 1,
        UpdateTargetPointsToAlias = 2
    }
    internal class Visitor : TSqlFragmentVisitor
    {
        public Visitor()
        {
            SourceTables = new ArrayList();
            Alias = new ArrayList();
            TargetTable = "";
            StatementFeatures = Features.None;
        }
        public ArrayList SourceTables { get; set; }
        public ArrayList Alias { get; set; }
        public string TargetTable { get; set; }

        internal Features StatementFeatures;

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
                    if (
                        us.Target is NamedTableReference
                        && ((NamedTableReference)(us.Target)).SchemaObject.ServerIdentifier == null
                        && ((NamedTableReference)(us.Target)).SchemaObject.DatabaseIdentifier == null
                        && ((NamedTableReference)(us.Target)).SchemaObject.SchemaIdentifier == null
                        && Alias.BinarySearch(TargetTable) > 0
                        )
                    {
                        TargetTable = SourceTables[Alias.BinarySearch(TargetTable)].ToString();
                        FlagsHelper.Set(ref StatementFeatures, Features.UpdateTargetPointsToAlias);
                    }
                    if (us.WhereClause != null)
                    {
                        FlagsHelper.Set(ref StatementFeatures, Features.HasWhereClause);
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
                        FlagsHelper.Set(ref StatementFeatures, Features.HasWhereClause);
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
                    Alias.Add(string.Format("[{0}]", nt.Alias != null ? nt.Alias.Value : ""));
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
