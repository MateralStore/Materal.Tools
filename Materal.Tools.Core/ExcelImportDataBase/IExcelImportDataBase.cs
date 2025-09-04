namespace Materal.Tools.Core.ExcelImportDataBase;

/// <summary>
/// Excel导入数据库接口
/// </summary>
public interface IExcelImportDataBase
{
    /// <summary>
    /// 从Excel文件导入数据到数据库
    /// </summary>
    /// <param name="filePath">Excel文件路径</param>
    /// <param name="connection">数据库连接</param>
    /// <param name="options">导入选项</param>
    /// <returns>导入结果</returns>
    ExcelImportResult Import(string filePath, string connection, ExcelImportOptions options);
}
