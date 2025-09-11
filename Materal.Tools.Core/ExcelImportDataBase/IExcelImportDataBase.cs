namespace Materal.Tools.Core.ExcelImportDataBase;

/// <summary>
/// Excel导入数据库接口
/// </summary>
public interface IExcelImportDataBase
{
    /// <summary>
    /// 读取Excel开始事件
    /// </summary>
    event EventHandler<ExcelImportResult>? ReadExcelStarted;
    /// <summary>
    /// 读取Excel完成事件
    /// </summary>
    event EventHandler<ExcelImportResult>? ReadExcelCompleted;
    /// <summary>
    /// 数据库验证事件
    /// </summary>
    event EventHandler<ExcelImportResult>? DatabaseValidation;
    /// <summary>
    /// 导入开始事件
    /// </summary>
    event EventHandler<ExcelImportResult>? ImportStarted;
    /// <summary>
    /// 导入进度变化事件
    /// </summary>
    event EventHandler<ExcelImportResult>? ImportProgressChanged;
    /// <summary>
    /// 导入完成事件
    /// </summary>
    event EventHandler<ExcelImportResult>? ImportCompleted;
    /// <summary>
    /// 从Excel文件导入数据到数据库
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="connection">数据库连接</param>
    /// <param name="options">导入选项</param>
    /// <returns>导入结果</returns>
    ExcelImportResult Import(string filePath, string connection, ExcelImportOptions options);
}
