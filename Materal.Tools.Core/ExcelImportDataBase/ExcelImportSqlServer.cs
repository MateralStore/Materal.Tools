using Materal.Abstractions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Materal.Tools.Core.ExcelImportDataBase;
/// <summary>
/// Excel导入SQLServer
/// </summary>
public class ExcelImportSqlServer : ExcelImportDataBase, IExcelImportDataBase
{
    /// <inheritdoc/>
    protected override IDbConnection GetDbConnection(string connection) => new SqlConnection(connection);

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
        return $"CREATE TABLE {tableName} ({string.Join(", ", tableColumNames.Select(c => $"{c} NVARCHAR(255)"))})";
    }

    /// <inheritdoc/>
    protected override bool TableExists(IDbConnection dbConnection, string tableName)
    {
        using IDbCommand command = dbConnection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{tableName}'";
        int count = Convert.ToInt32(command.ExecuteScalar());
        return count > 0;
    }

    /// <inheritdoc/>
    protected override void Inserts(IDbConnection dbConnection, string tableName, List<List<string>> rows, ExcelImportResult result)
    {
        if (rows == null || rows.Count == 0) return;
        try
        {
            if (dbConnection is not SqlConnection sqlConnection) throw new InvalidOperationException("数据库连接必须是SqlConnection类型");
            using DataTable dataTable = new();
            using (IDbCommand command = dbConnection.CreateCommand())
            {
                command.CommandText = $"SELECT TOP 0 * FROM {tableName}";
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
            using (SqlBulkCopy bulkCopy = new(sqlConnection))
            {
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BatchSize = rows.Count;
                bulkCopy.BulkCopyTimeout = 600;
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    bulkCopy.ColumnMappings.Add(i, dataTable.Columns[i].ColumnName);
                }
                bulkCopy.WriteToServer(dataTable);
            }
            result.SuccessRows += rows.Count;
        }
        catch (Exception ex)
        {
            result.FailedRows += rows.Count;
            result.Errors.Add($"SqlBulkCopy批量插入失败: {ex.GetErrorMessage()}");
        }
    }
}
