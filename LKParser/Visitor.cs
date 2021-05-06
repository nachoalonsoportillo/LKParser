using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;

namespace LKPocParser
{

    internal class Visitor : TSqlFragmentVisitor
    {
        public Visitor()
        {
            Results = new ParseResults();
        }

        public ParseResults Results { get; private set; }

        private int nestingLevel = 1;

        public void Traverse(TSqlFragment node)
        {
            ++nestingLevel;
            Debug.Assert(nestingLevel < 10, "Maximum recursion level exceeded.");
            switch (node)
            {
                case InsertStatement insSt:
                    Traverse(insSt.InsertSpecification);
                    break;
                case InsertSpecification ins:
                    Results.SetTargetTableFromTableRef(ins.Target);
                    if (ins.InsertSource is SelectInsertSource)
                    {
                        Traverse(((SelectInsertSource)ins.InsertSource).Select);
                    }
                    break;
                case UpdateStatement updSt:
                    Traverse(updSt.UpdateSpecification);
                    break;
                case UpdateSpecification us:
                    Results.SetTargetTableFromTableRef(us.Target);
                    if (us.FromClause != null)
                    {
                        Traverse(((FromClause)us.FromClause).TableReferences[0]);
                    }
                    if ( us.Target.IsNull()
                        && Results.Alias.BinarySearch(Results.TargetTable) > 0
                        )
                    {
                        Results.SetTargetTableFromAlias();
                        Results.SetFeature(Features.UpdateTargetPointsToAlias);
                    }
                    if (us.WhereClause != null)
                    {
                        Results.SetFeature(Features.HasWhereClause);
                    }
                    break;
                case SelectStatement selSt:
                    Results.SetTargetTableFromSchema(selSt.Into);
                    Traverse(((QuerySpecification)(selSt.QueryExpression)));
                    break;
                case QuerySpecification qs:
                    if (qs.WhereClause != null)
                    {
                        Results.SetFeature(Features.HasWhereClause);
                    }
                    Traverse(qs.FromClause.TableReferences[0]);
                    break;
                case UnqualifiedJoin uj:
                    Traverse(uj.FirstTableReference);
                    Traverse(uj.SecondTableReference);
                    break;
                case QualifiedJoin qj:
                    Traverse(qj.FirstTableReference);
                    Traverse(qj.SecondTableReference);
                    break;
                case BinaryQueryExpression bqs:
                    Traverse(bqs.FirstQueryExpression);
                    Traverse(bqs.SecondQueryExpression);
                    break;
                case NamedTableReference tr:
                    Results.SourceTables.Add(tr.SchemaObject.FullyQualifiedName());
                    Results.Alias.Add(string.Format("[{0}]", tr.Alias != null ? tr.Alias.Value : ""));
                    break;
                default:
                    Debug.Assert(false, "This node type is not implemented yet!!!");
                    break;
            }
            --nestingLevel;
        }

        public override void Visit(InsertStatement node)
        {
            Traverse(node);
        }

        public override void Visit(UpdateStatement node)
        {
            Traverse(node);
        }

        public override void Visit(SelectStatement node)
        {
            Traverse(node);
        }
       
    }
}
