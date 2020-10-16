using System.Reflection;
using System;
using Dapper;
using Npgsql;
using DatabaseDumper.Models;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DatabaseDumper
{
    class Program
    {
        private static string connectionString = "Host=localhost;Username=northwind_user;Password=thewindisblowing;Database=northwind";
        private static string constraintQuery = 
        @"SELECT conrelid::regclass::text AS table_from, conname as constraint_name,pg_get_constraintdef(oid) as constraint
        FROM   pg_constraint
        WHERE  contype IN ('f', 'p ')
        AND    connamespace = 'public'::regnamespace  -- your schema here
        ORDER  BY conrelid::regclass::text, contype DESC;";
        private const int maxRows = 1000;
        private static StringBuilder outputBuilder = new StringBuilder(); 


        static void Main(string[] args)
        {
            Console.WriteLine("Pass connection string");
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            string sql = "select * from information_schema.tables";
            using var connection = new NpgsqlConnection(connectionString);
            
            var tableInfoes = connection.Query<TableInfo>("select * from information_schema.tables").ToList();
            var columnInfoes = connection.Query<ColumnInfo>("select * from information_schema.columns").ToList();
            var constraintInfoes = connection.Query<ConstraintInfo>(constraintQuery).ToList();

            foreach (var tableInfo in tableInfoes)
            {
                tableInfo.Columns = columnInfoes.Where(x => x.TableName == tableInfo.TableName).ToList();

                var referencedTablesNames = constraintInfoes
                    .Where(x => x.TableFrom == tableInfo.TableName)
                    .Where(x => x.ForeignKey != null)
                    .Select(x => x.ForeignKey.ReferencedTable);

                tableInfo.References = tableInfoes.Where(x => referencedTablesNames.Contains(x.TableName)).ToList();

                var foreignReferences = constraintInfoes
                    .Where(x => x.ForeignKey != null)
                    .Where(x => x.ForeignKey.ReferencedTable == tableInfo.TableName)
                    .Select(x => x.TableFrom);

                tableInfo.ReferencedBy = tableInfoes.Where(x => foreignReferences.Contains(x.TableName)).ToList();
            }
            var alreadyInserted = new List<string>();

            while(true)
            { 
                var availableToInsert = tableInfoes
                    .Where(x => x.TableSchema == "public")
                    .Where(x => !alreadyInserted.Contains(x.TableName))
                    .Where(x => !x.References.Where(r => !alreadyInserted.Contains(r.TableName)).Any())
                    .ToList();

                var availableToInsertWithSelfReference = tableInfoes
                    .Where(x => x.TableSchema == "public")
                    .Where(x => !alreadyInserted.Contains(x.TableName))
                    .Where(x => !x.References.Where(r => !alreadyInserted.Contains(r.TableName)).Where(r => r.TableName != x.TableName).Any())
                    .Where(x => !availableToInsert.Select(a => a.TableName).Contains(x.TableName))
                    .ToList();

                if (!availableToInsert.Any() && !availableToInsertWithSelfReference.Any())
                    break;

                foreach (var insertTable in availableToInsert)
                {
                    InsertValues(insertTable, connection);
                    alreadyInserted.Add(insertTable.TableName);
                }

                foreach (var insertTable in availableToInsertWithSelfReference)
                {
                    InsertValuesSelfRefrenced(insertTable, connection);
                    alreadyInserted.Add(insertTable.TableName);
                }

            }

            var a = 1;
        }

        private static void InsertValuesSelfRefrenced(TableInfo table, NpgsqlConnection connection)
        {
            var tableName = $"{table.TableSchema}.\"{table.TableName}\"";
            var columns = table.Columns.OrderBy(c => c.OrdinalPosition).Select(c => c.ColumnName);
            var columnNames = string.Join(",", columns.Select(c => $"\"{c}\""));

            var valuesRaw = connection.Query($"select {columnNames} from {tableName} limit {maxRows}").ToList().Select(x => (IDictionary<string, object>)x).ToList();

            for (int i = 0; i < valuesRaw.Count; i++)
            {
                var values = string.Join(",", columns.Select(c => {
                    var rawValue = valuesRaw[i][c];
                    if (rawValue is null)
                        return "null";
                    if (rawValue is string)
                        return $"'{rawValue}'";
                    return rawValue.ToString();
                }
                ));
                var insertStatement = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values});";
                outputBuilder.AppendLine(insertStatement);
            }
            var text = outputBuilder.ToString();
        }

        private static void InsertValues(TableInfo table, NpgsqlConnection connection)
        {
            var tableName = $"{table.TableSchema}.\"{table.TableName}\"";
            var columns = table.Columns.OrderBy(c => c.OrdinalPosition).Select(c => c.ColumnName);
            var columnNames = string.Join(",", columns.Select(c => $"\"{c}\""));

            var valuesRaw = connection.Query($"select {columnNames} from {tableName} limit {maxRows}").ToList().Select(x => (IDictionary<string, object>)x).ToList();

            for (int i = 0; i < valuesRaw.Count; i++)
            {
                var values = string.Join(",", columns.Select(c => {
                    var rawValue = valuesRaw[i][c];
                    if (rawValue is null)
                        return "null";
                    if (rawValue is string)
                        return $"'{rawValue}'";
                    return rawValue.ToString();
                    }
                ));
                var insertStatement = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values});";
                outputBuilder.AppendLine(insertStatement);
            }
            var text = outputBuilder.ToString();
        }
    }
}
