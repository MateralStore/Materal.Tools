namespace Materal.Tools.COSWatcher;

/// <summary>
/// COS 目录监听配置。
/// </summary>
internal sealed class COSWatcherOptions
{
    /// <summary>
    /// 要监听的本地目录列表。
    /// </summary>
    public List<COSWatchDirectoryOptions> WatchDirectories { get; set; } = [];
    /// <summary>
    /// 上传失败后的最大重试次数。
    /// </summary>
    public int RetryCount { get; set; } = 3;
    /// <summary>
    /// 重试间隔秒数。
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;
    /// <summary>
    /// 文件写入完成后再上传的等待时间（毫秒）。
    /// </summary>
    public int FileReadyDelayMilliseconds { get; set; } = 1000;
}

/// <summary>
/// 单个目录的监听配置。
/// </summary>
internal sealed class COSWatchDirectoryOptions
{
    /// <summary>
    /// 要监听的本地目录。
    /// </summary>
    public string WatchDirectory { get; set; } = string.Empty;
    /// <summary>
    /// COS 对象键前缀。
    /// </summary>
    public string KeyPrefix { get; set; } = "uploads";
    /// <summary>
    /// 是否监听子目录。
    /// </summary>
    public bool IncludeSubdirectories { get; set; } = true;
}
