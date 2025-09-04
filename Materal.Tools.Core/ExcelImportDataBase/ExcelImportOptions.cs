namespace Materal.Tools.Core.ExcelImportDataBase;

/// <summary>
/// Excel导入选项
/// </summary>
public class ExcelImportOptions
{
    /// <summary>
    /// 工作表名称
    /// </summary>
    public string SheetName { get; set; } = "Sheet1";

    /// <summary>
    /// 列映射配置（Excel列名 -> 表列名）
    /// </summary>
    public Dictionary<string, string>? ColumnMapping { get; set; } = null;

    /// <summary>
    /// 批处理大小
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// 表名
    /// </summary>
    public string TableName { get; set; } = string.Empty;
}
