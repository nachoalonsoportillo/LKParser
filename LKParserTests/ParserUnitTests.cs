using System;
using Xunit;
using LKPocParser;

namespace LKParserTests
{
    public class ParserUnitTests
    {
        [Fact]
        public void InsertSelectFrom1Table()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("INSERT INTO tablaUno SELECT * FROM tablaDos as t2 WHERE t2.c3 = 55");
            Assert.Equal("[tablaUno]", r.TargetTable);
            Assert.Single(r.SourceTables);
            Assert.Contains("[tablaDos]", r.SourceTables);
        }
        [Fact]
        public void InsertSelectFrom2CrossJoinedTables()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("INSERT INTO tablaUno SELECT * FROM tablaDos CROSS JOIN tablaTres");
            Assert.Equal("[tablaUno]", r.TargetTable);
            Assert.Equal(2, r.SourceTables.Count);
            Assert.Contains("[tablaDos]", r.SourceTables);
            Assert.Contains("[tablaTres]", r.SourceTables);
        }
        [Fact]
        public void InsertSelectFrom3UnionAllTables()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("INSERT INTO tablaUno SELECT * FROM tablaDos UNION ALL SELECT * FROM tablaTres UNION ALL SELECT * FROM tablaCuatro");
            Assert.Equal("[tablaUno]", r.TargetTable);
            Assert.Equal(3, r.SourceTables.Count);
            Assert.Contains("[tablaDos]", r.SourceTables);
            Assert.Contains("[tablaTres]", r.SourceTables);
            Assert.Contains("[tablaCuatro]", r.SourceTables);
        }
        [Fact]
        public void InsertValues()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("INSERT INTO tablaUno VALUES (1, 2, 3)");
            Assert.Equal("[tablaUno]", r.TargetTable);
            Assert.Empty(r.SourceTables);
        }
        [Fact]
        public void UpdateSet()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("UPDATE tablaUno SET c1 = 1, c2 = 2");
            Assert.Equal("[tablaUno]", r.TargetTable);
            Assert.Empty(r.SourceTables);
        }
        [Fact]
        public void UpdateSelectFrom2InnerJoinTables()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("UPDATE tablaUno set columna1 = 3 FROM tablaUno INNER JOIN tablaDos on tablaUno.ccolumna1= tablaDos.columna1");
            Assert.Equal("[tablaUno]", r.TargetTable);
            Assert.Equal(2, r.SourceTables.Count);
            Assert.Contains("[tablaUno]", r.SourceTables);
            Assert.Contains("[tablaDos]", r.SourceTables);
        }
        [Fact]
        public void SelectFrom3LeftJoinTables()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("SELECT * FROM myserver.mydatabase.dbo.tablaUno LEFT JOIN tablaDos ON tablaUno.c1 = tablaDos.c1 LEFT JOIN tablaTres on tablaDos.columna1 = tablaTres.columna1");
            Assert.Empty(r.TargetTable);
            Assert.Equal(3, r.SourceTables.Count);
            Assert.Contains("[myserver].[mydatabase].[dbo].[tablaUno]", r.SourceTables);
            Assert.Contains("[tablaDos]", r.SourceTables);
            Assert.Contains("[tablaTres]", r.SourceTables);
        }
        [Fact]
        public void SelectInto()
        {
            Parser pParser = new Parser();
            ParseResults r = pParser.Parse("SELECT * INTO tablaUno FROM tablaDos");
            Assert.Equal("[tablaUno]", r.TargetTable);
            Assert.Single(r.SourceTables);
            Assert.Contains("[tablaDos]", r.SourceTables);
        }

    }
}
