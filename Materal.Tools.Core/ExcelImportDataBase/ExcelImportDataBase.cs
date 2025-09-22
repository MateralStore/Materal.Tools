using Materal.Abstractions;
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
    public event EventHandler<ExcelImportResult>? ImportCompleted;
    /// <inheritdoc/>
    public event EventHandler<ExcelImportResult>? ReadExcelStarted;
    /// <inheritdoc/>
    public event EventHandler<ExcelImportResult>? ReadExcelCompleted;
    /// <inheritdoc/>
    public event EventHandler<ExcelImportResult>? ImportStarted;
    /// <inheritdoc/>
    public event EventHandler<ExcelImportResult>? ImportProgressChanged;
    /// <inheritdoc/>
    public event EventHandler<ExcelImportResult>? DatabaseValidation;

    /// <inheritdoc/>
    public virtual ExcelImportResult Import(string filePath, string connection, ExcelImportOptions options)
    {
        ExcelImportResult result = new();
        try
        {
            ReadExcelStarted?.Invoke(this, result);
            IWorkbook workbook = ExcelHelper.ReadExcelToWorkbook(filePath);
            ISheet? sheet = workbook.GetSheet(options.SheetName);
            if (sheet is null)
            {
                result.Errors.Add($"工作表{options.SheetName}不存在");
                return result;
            }
            IRow? row = sheet.GetRow(0);
            if (row is null)
            {
                result.Errors.Add($"工作表{options.SheetName}没有列头");
                return result;
            }
            result.TotalRows = sheet.LastRowNum - 1;
            List<string> columNames = new(row.LastCellNum);
            for (int i = 0; i < row.LastCellNum; i++)
            {
                columNames.Add(row.GetCell(i).StringCellValue);
            }
            ReadExcelCompleted?.Invoke(this, result);
            DatabaseValidation?.Invoke(this, result);
            try
            {
                string createTableSQL = GetCreateTableSQL(options.TableName, columNames, options.ColumnMapping);
                IDbConnection dbConnection = GetDbConnection(connection);
                dbConnection.Open();
                try
                {
                    if (!TableExists(dbConnection, options.TableName))
                    {
                        CreateTable(dbConnection, createTableSQL);
                    }
                    ImportStarted?.Invoke(this, result);
                    List<List<string>> batchValues = new(options.BatchSize);
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        row = sheet.GetRow(i);
                        if (row == null) continue;
                        List<string> values = new(row.LastCellNum);
                        for (int j = 0; j < row.LastCellNum; j++)
                        {
                            ICell cell = row.GetCell(j);
                            if (cell is null)
                            {
                                values.Add(string.Empty);
                                continue;
                            }
                            if (cell.CellType == CellType.String)
                            {
                                values.Add(cell.StringCellValue);
                            }
                            else if (cell.CellType == CellType.Numeric)
                            {
                                values.Add(cell.NumericCellValue.ToString());
                            }
                            else if (cell.CellType == CellType.Boolean)
                            {
                                values.Add(cell.BooleanCellValue ? "1" : "0");
                            }
                            else if (cell.CellType == CellType.Formula)
                            {
                                try
                                {
                                    values.Add(cell.StringCellValue);
                                }
                                catch
                                {
                                    values.Add(cell.NumericCellValue.ToString());
                                }
                            }
                            else
                            {
                                values.Add(string.Empty);
                            }
                        }
                        batchValues.Add(values);
                        if (batchValues.Count == options.BatchSize)
                        {
                            Inserts(dbConnection, options.TableName, batchValues, result);
                            ImportProgressChanged?.Invoke(this, result);
                            batchValues = new(options.BatchSize);
                        }
                    }
                    Inserts(dbConnection, options.TableName, batchValues, result);
                    ImportProgressChanged?.Invoke(this, result);
                    return result;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(ex.GetErrorMessage());
                    return result;
                }
                finally
                {
                    dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.GetErrorMessage());
                return result;
            }
        }
        finally
        {
            ImportCompleted?.Invoke(this, result);
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
