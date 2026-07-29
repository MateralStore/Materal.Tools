using Materal.Utils.CloudStorage.Tencent;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Materal.Tools.COSWatcher;

/// <summary>
/// 监听文件创建事件并上传到腾讯云 COS。
/// </summary>
internal sealed class COSWatcher : IDisposable
{
    private readonly TencentCloudStorageService _cloudStorageService;
    private readonly COSWatcherOptions _options;
    private readonly List<FileSystemWatcher> _fileSystemWatchers = [];
    private readonly Channel<UploadItem> _fileQueue = Channel.CreateUnbounded<UploadItem>();
    private readonly ConcurrentDictionary<string, byte> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 初始化监听器。
    /// </summary>
    /// <param name="cloudStorageService">腾讯云 COS 服务。</param>
    /// <param name="options">监听配置。</param>
    public COSWatcher(TencentCloudStorageService cloudStorageService, COSWatcherOptions options)
    {
        _cloudStorageService = cloudStorageService;
        _options = options;
        foreach (COSWatchDirectoryOptions watchDirectory in options.WatchDirectories)
        {
            FileSystemWatcher fileSystemWatcher = new(watchDirectory.WatchDirectory)
            {
                IncludeSubdirectories = watchDirectory.IncludeSubdirectories,
                EnableRaisingEvents = true
            };
            fileSystemWatcher.Created += (_, eventArgs) => Enqueue(eventArgs.FullPath, watchDirectory);
            fileSystemWatcher.Renamed += (_, eventArgs) => Enqueue(eventArgs.FullPath, watchDirectory);
            fileSystemWatcher.Error += FileSystemWatcher_Error;
            _fileSystemWatchers.Add(fileSystemWatcher);
        }
    }

    /// <summary>
    /// 开始处理监听到的文件，直至取消。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (UploadItem uploadItem in _fileQueue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await UploadAsync(uploadItem, cancellationToken);
                }
                finally
                {
                    _pendingFiles.TryRemove(uploadItem.PendingKey, out _);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (FileSystemWatcher fileSystemWatcher in _fileSystemWatchers)
        {
            fileSystemWatcher.Dispose();
        }
    }

    private static void FileSystemWatcher_Error(object sender, ErrorEventArgs eventArgs)
    {
        Console.Error.WriteLine($"目录监听发生错误: {eventArgs.GetException().Message}");
    }

    private void Enqueue(string filePath, COSWatchDirectoryOptions watchDirectory)
    {
        string pendingKey = $"{watchDirectory.WatchDirectory}|{filePath}";
        if (Directory.Exists(filePath) || !_pendingFiles.TryAdd(pendingKey, 0))
        {
            return;
        }
        UploadItem uploadItem = new(filePath, watchDirectory, pendingKey);
        if (!_fileQueue.Writer.TryWrite(uploadItem))
        {
            _pendingFiles.TryRemove(pendingKey, out _);
            Console.Error.WriteLine($"无法将文件加入上传队列: {filePath}");
        }
    }

    private async Task UploadAsync(UploadItem uploadItem, CancellationToken cancellationToken)
    {
        string key = CreateKey(uploadItem);
        int retryCount = Math.Max(0, _options.RetryCount);
        for (int attempt = 0; attempt <= retryCount; attempt++)
        {
            try
            {
                await WaitForFileReadyAsync(uploadItem.FilePath, cancellationToken);
                await _cloudStorageService.UploadObjectByKeyAsync(uploadItem.FilePath, key);
                Console.WriteLine($"上传成功: {uploadItem.FilePath} -> {key}");
                return;
            }
            catch (Exception exception) when (attempt < retryCount)
            {
                Console.Error.WriteLine($"上传失败，将进行第 {attempt + 1} 次重试: {uploadItem.FilePath}{Environment.NewLine}{exception.Message}");
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _options.RetryDelaySeconds)), cancellationToken);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine($"上传失败，已放弃: {uploadItem.FilePath}{Environment.NewLine}{exception}");
            }
        }
    }

    private async Task WaitForFileReadyAsync(string filePath, CancellationToken cancellationToken)
    {
        await Task.Delay(Math.Max(0, _options.FileReadyDelayMilliseconds), cancellationToken);
        using FileStream _ = new(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
    }

    private static string CreateKey(UploadItem uploadItem)
    {
        string relativePath = Path.GetRelativePath(uploadItem.WatchDirectory.WatchDirectory, uploadItem.FilePath).Replace(Path.DirectorySeparatorChar, '/');
        string keyPrefix = uploadItem.WatchDirectory.KeyPrefix.Trim('/');
        return string.IsNullOrWhiteSpace(keyPrefix) ? relativePath : $"{keyPrefix}/{relativePath}";
    }

    private sealed record UploadItem(string FilePath, COSWatchDirectoryOptions WatchDirectory, string PendingKey);
}
