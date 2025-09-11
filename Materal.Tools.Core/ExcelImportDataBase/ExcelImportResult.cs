namespace Materal.Tools.Core.ExcelImportDataBase;

/// <summary>
/// Excel导入结果
/// </summary>
public class ExcelImportResult
{
    /// <summary>
    /// 总行数
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// 成功导入行数
    /// </summary>
    public int SuccessRows { get; set; }

    /// <summary>
    /// 失败行数
    /// </summary>
    public int FailedRows { get; set; }

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// 导入是否成功
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;
}
