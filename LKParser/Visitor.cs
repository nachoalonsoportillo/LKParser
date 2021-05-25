using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace LKPocParser
{

    internal class Visitor : TSqlFragmentVisitor
    {
        public Visitor()
        {
            Results = new List<ParseResults>();
        }

        public List<ParseResults> Results { get; private set; }

        private ParseResults Result
        {
            get { return Results[Results.Count - 1]; }
        }

        private int nestingLevel = 1;

        public void Traverse(TSqlFragment node)
        {
            if (nestingLevel++ == 1)
            {
                Results.Add(new ParseResults());
            };
            Debug.Assert(nestingLevel < 10, "Maximum recursion level exceeded.");
            switch (node)
            {
                case InsertStatement insSt:
                    Result.SetStatementType(StatementType.Insert);
                    Traverse(insSt.InsertSpecification);
                    break;
                case InsertSpecification ins:
                    Result.SetTargetTableFromTableRef(ins.Target);
                    if (ins.InsertSource is SelectInsertSource)
                    {
                        Traverse(((SelectInsertSource)ins.InsertSource).Select);
                    }
                    break;
                case DeleteStatement delSt:
                    Result.SetStatementType(StatementType.Delete);
                    Traverse(delSt.DeleteSpecification);
                    break;
                case UpdateStatement updSt:
                    Result.SetStatementType(StatementType.Update);
                    Traverse(updSt.UpdateSpecification);
                    break;
                case UpdateSpecification us:
                    Result.SetTargetTableFromTableRef(us.Target);
                    if (us.FromClause != null)
                    {
                        Traverse(((FromClause)us.FromClause).TableReferences[0]);
                    }
                    if ( us.Target.IsNull()
                        && Result.Alias.BinarySearch(Result.TargetTable) > 0
                        )
                    {
                        Result.SetTargetTableFromAlias();
                        Result.SetFeature(Features.UpdateTargetPointsToAlias);
                    }
                    if (us.WhereClause != null)
                    {
                        Result.SetFeature(Features.HasWhereClause);
                    }
                    break;
                case SelectStatement selSt:
                    Result.SetStatementType(StatementType.Select);
                    Result.SetTargetTableFromSchema(selSt.Into);
                    Traverse(((QuerySpecification)(selSt.QueryExpression)));
                    break;
                case CreateTableStatement createTableSt:
                    Result.SetStatementType(StatementType.CreateTable);
                    Result.SetFeature(Features.IsDDL);
                    break;
                case AlterTableStatement alterTableSt:
                    Result.SetStatementType(StatementType.AlterTable);
                    Result.SetFeature(Features.IsDDL);
                    break;
                case TruncateTableStatement truncateTableSt:
                    Result.SetStatementType(StatementType.TruncateTable);
                    Result.SetFeature(Features.IsDDL);
                    break;
                case DeclareVariableStatement declareVariableSt:
                    Result.SetStatementType(StatementType.DeclareVariable);
                    break;
                case SetVariableStatement setVariableSt:
                    Result.SetStatementType(StatementType.SetVariable);
                    break;
                case QuerySpecification qs:
                    if (qs.WhereClause != null)
                    {
                        Result.SetFeature(Features.HasWhereClause);
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
                    Result.SourceTables.Add(tr.SchemaObject.FullyQualifiedName());
                    Result.Alias.Add(string.Format("[{0}]", tr.Alias != null ? tr.Alias.Value : ""));
                    break;
                default:
                    Debug.Assert(false, "This node type is not implemented yet!!!");
                    break;
            }
            --nestingLevel;
        }
        public override void Visit(TruncateTableStatement node)
        {
            Traverse(node);
        }
        public override void Visit(CreateTableStatement node)
        {
            Traverse(node);
        }
        public override void Visit(AlterTableStatement node)
        {
            Traverse(node);
        }
        public override void Visit(DeclareVariableStatement node)
        {
            Traverse(node);
        }
        public override void Visit(SetVariableStatement node)
        {
            Traverse(node);
        }
        public override void Visit(InsertStatement node)
        {
            Traverse(node);
        }
        public override void Visit(UpdateStatement node)
        {
            Traverse(node);
        }
        public override void Visit(DeleteStatement node)
        {
            Traverse(node);
        }
        public override void Visit(SelectStatement node)
        {
            Traverse(node);
        }
    }
}
