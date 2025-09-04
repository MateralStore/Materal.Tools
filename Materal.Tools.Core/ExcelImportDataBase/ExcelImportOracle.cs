#if NET
using Materal.Abstractions;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Materal.Tools.Core.ExcelImportDataBase
{
    /// <summary>
    /// Excel导入Oracle
    /// </summary>
    public class ExcelImportOracle : ExcelImportDataBase, IExcelImportDataBase
    {
        /// <inheritdoc/>
        protected override IDbConnection GetDbConnection(string connection) => new OracleConnection(connection);

        /// <inheritdoc/>
        protected override string GetCreateTableSQL(string tableName, List<string> columnNames, Dictionary<string, string>? columnMapping = null)
        {
            List<string> tableColumNames = new(columnNames.Count);
            foreach (string columnName in columnNames)
            {
                if (columnMapping is not null && columnMapping.TryGetValue(columnName, out string? mappingColumnName))
                {
                    tableColumNames.Add(mappingColumnName);
                }
                else
                {
                    tableColumNames.Add(columnName);
                }
            }
            return $"CREATE TABLE \"{tableName}\" ({string.Join(", ", tableColumNames.Select(c => $"\"{c}\" NVARCHAR2(255)"))})";
        }

        /// <inheritdoc/>
        protected override bool TableExists(IDbConnection dbConnection, string tableName)
        {
            using IDbCommand command = dbConnection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM all_tables WHERE table_name = '{tableName}'";
            object? result = command.ExecuteScalar();
            int count = 0;
            if (result is not null and decimal decimalResult)
            {
                count = Convert.ToInt32(decimalResult);
            }
            return count > 0;
        }

        /// <inheritdoc/>
        protected override void Inserts(IDbConnection dbConnection, string tableName, List<List<string>> rows, ExcelImportResult result)
        {
            if (rows == null || rows.Count == 0) return;
            try
            {
                if (dbConnection is not OracleConnection oracleConnection) throw new InvalidOperationException("数据库连接必须是OracleConnection类型");
                using DataTable dataTable = new();
                using (IDbCommand command = dbConnection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM \"{tableName}\" WHERE 1=0";
                    using IDataReader reader = command.ExecuteReader();
                    dataTable.Load(reader);
                }
                foreach (List<string> row in rows)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < Math.Min(row.Count, dataTable.Columns.Count); i++)
                    {
                        dataRow[i] = row[i] ?? string.Empty;
                    }
                    dataTable.Rows.Add(dataRow);
                }
                using (OracleBulkCopy bulkCopy = new(oracleConnection))
                {
                    bulkCopy.DestinationTableName = $"\"{tableName}\"";
                    bulkCopy.BatchSize = rows.Count;
                    bulkCopy.BulkCopyTimeout = 600;
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        bulkCopy.ColumnMappings.Add(i, $"\"{dataTable.Columns[i].ColumnName}\"");
                    }
                    bulkCopy.WriteToServer(dataTable);
                }
                result.SuccessRows += rows.Count;
            }
            catch (Exception ex)
            {
                result.FailedRows += rows.Count;
                result.Errors.Add($"OracleBulkCopy批量插入失败: {ex.GetErrorMessage()}");
            }
        }
    }
}
#endif