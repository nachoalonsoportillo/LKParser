using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using LKPocParser;

public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void Parser (SqlString textToParse, SqlString label)
    {
        ValidateArguments((string)textToParse, (string)label);
        try
        {
            Parser pParser = new Parser();
            List<ParseResults> r = pParser.Parse((string)textToParse);
            r.ForEach(delegate (ParseResults results)
            {
                LogResultsInTable((string)textToParse, (string)label, results);
            });
        }
        catch (ParserException e)
        {
            SqlContext.Pipe.Send("Specific Error: " + e.Message);
        }
        catch (Exception e)
        {
            SqlContext.Pipe.Send("Generic Error: " + e.Message);
        }
    }

    private static void ValidateArguments(string textToParse, string label)
    {
        if (string.IsNullOrEmpty(textToParse))
        {
            throw new ArgumentException("textToParse cannot be null nor empty.");
        }
        if (string.IsNullOrEmpty(label) || label.Trim().Length > 200)
        {
            throw new ArgumentException("label cannot be null, empty, nor its can exceed 200 characters.");
        }
    }
    private static void LogResultsInTable(string textToParse, string label, ParseResults results)
    {
        using (SqlConnection sqlConnection = new SqlConnection("context connection = true"))
        {
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(
                "INSERT INTO TrackingParser (InputText, StatementText, Label, Alias, TargetTable, SourceTables, Features) VALUES (@InputText, @StatementText, @Label, @Alias, @TargetTable, @SourceTables, @Features)", sqlConnection);
            SqlParameter[] cmdParameters =
            {
                new SqlParameter
                {
                    ParameterName = "@InputText",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = -1,
                    Value = textToParse
                },
                new SqlParameter
                {
                    ParameterName = "@StatementText",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = -1,
                    Value = results.StatementText
                },
                new SqlParameter
                {
                    ParameterName = "@Label",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = 200,
                    Value = label
                },
                new SqlParameter
                {
                    ParameterName = "@Alias",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = -1,
                    Value = string.Join(",", results.Alias.ToArray())
                },
                new SqlParameter
                {
                    ParameterName = "@TargetTable",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = -1,
                    Value = results.TargetTable
                },
                new SqlParameter
                {
                    ParameterName = "@SourceTables",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.NVarChar,
                    Size = -1,
                    Value = string.Join(",", results.SourceTables.ToArray())
                },
                new SqlParameter
                {
                    ParameterName = "@Features",
                    Direction = ParameterDirection.Input,
                    SqlDbType = SqlDbType.SmallInt,
                    Value = results.StatementFeatures
                }
            };
            sqlCommand.Parameters.AddRange(cmdParameters);
            sqlCommand.ExecuteNonQuery();
        }
    }
}
