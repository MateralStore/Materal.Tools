using Materal.Utils.Excel;
using NPOI.SS.UserModel;
using System.Data;

namespace Materal.Tools.Core.ExcelImportDataBase;

/// <summary>
/// Excel导入数据库基类
/// </summary>
public abstract class ExcelImportDataBase : IExcelImportDataBase
{
    /// <inheritdoc/>
    public virtual ExcelImportResult Import(string filePath, string connection, ExcelImportOptions options)
    {
        ExcelImportResult result = new();
        IWorkbook workbook = ExcelHelper.ReadExcelToWorkbook(filePath);
        ISheet sheet = workbook.GetSheet(options.SheetName) ?? throw new ToolsException($"工作表{options.SheetName}不存在");
        IRow row = sheet.GetRow(0) ?? throw new ToolsException($"工作表{options.SheetName}没有列头");
        List<string> columNames = new(row.LastCellNum);
        for (int i = 0; i < row.LastCellNum; i++)
        {
            columNames.Add(row.GetCell(i).StringCellValue);
        }
        string createTableSQL = GetCreateTableSQL(options.TableName, columNames, options.ColumnMapping);
        IDbConnection dbConnection = GetDbConnection(connection);
        dbConnection.Open();
        try
        {
            if (!TableExists(dbConnection, options.TableName))
            {
                CreateTable(dbConnection, createTableSQL);
            }
            List<List<string>> batchValues = new(options.BatchSize);
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                row = sheet.GetRow(i);
                if (row == null) continue;
                List<string> values = new(row.LastCellNum);
                for (int j = 0; j < row.LastCellNum; j++)
                {
                    values.Add(row.GetCell(j).StringCellValue);
                }
                batchValues.Add(values);
                if (batchValues.Count == options.BatchSize)
                {
                    Inserts(dbConnection, options.TableName, batchValues, result);
                    batchValues = new(options.BatchSize);
                }
            }
            Inserts(dbConnection, options.TableName, batchValues, result);
            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
            return result;
        }
        finally
        {
            dbConnection.Close();
        }
    }
    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="dbConnection"></param>
    /// <param name="tableName"></param>
    /// <param name="rows"></param>
    /// <param name="result"></param>
    protected abstract void Inserts(IDbConnection dbConnection, string tableName, List<List<string>> rows, ExcelImportResult result);
    /// <summary>
    /// 表是否存在
    /// </summary>
    /// <param name="dbConnection"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    protected abstract bool TableExists(IDbConnection dbConnection, string tableName);
    /// <summary>
    /// 创建表
    /// </summary>
    /// <param name="dbConnection"></param>
    /// <param name="createTableSQL"></param>
    protected virtual void CreateTable(IDbConnection dbConnection, string createTableSQL)
    {
        using IDbCommand command = dbConnection.CreateCommand();
        command.CommandText = createTableSQL;
        command.ExecuteNonQuery();
    }
    /// <summary>
    /// 获取数据库连接
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    protected abstract IDbConnection GetDbConnection(string connection);
    /// <summary>
    /// 获取创建表SQL
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnNames"></param>
    /// <param name="columnMapping"></param>
    /// <returns></returns>
    protected abstract string GetCreateTableSQL(string tableName, List<string> columnNames, Dictionary<string, string>? columnMapping = null);
}
