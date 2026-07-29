using Materal.Utils.CloudStorage.Tencent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Materal.Tools.COSWatcher;

/// <summary>
/// 程序入口。
/// </summary>
internal static class Program
{
    /// <summary>
    /// 启动目录监听服务。
    /// </summary>
    /// <returns>进程退出码。</returns>
    public static async Task<int> Main()
    {
        bool isDevelopment = IsDevelopment();
        string configurationFileName = isDevelopment ? "appsettings.Development.json" : "appsettings.json";
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false);
        if (isDevelopment)
        {
            configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true);
        }
        IConfiguration configuration = configurationBuilder.Build();
        TencentCloudStorageConfig cloudStorageConfig = configuration.GetSection("TencentCloudStorage").Get<TencentCloudStorageConfig>() ?? new();
        COSWatcherOptions watcherOptions = configuration.GetSection("COSWatcher").Get<COSWatcherOptions>() ?? new();
        if (!cloudStorageConfig.IsOK)
        {
            Console.Error.WriteLine($"请在 {configurationFileName} 的 TencentCloudStorage 节点中填写 AppID、SecretID 和 SecretKey。");
            return 1;
        }
        if (watcherOptions.WatchDirectories.Count == 0)
        {
            Console.Error.WriteLine($"请在 {configurationFileName} 的 COSWatcher:WatchDirectories 中至少填写一个目标目录。");
            return 1;
        }
        COSWatchDirectoryOptions? invalidWatchDirectory = watcherOptions.WatchDirectories.FirstOrDefault(directory => string.IsNullOrWhiteSpace(directory.WatchDirectory) || !Directory.Exists(directory.WatchDirectory));
        if (invalidWatchDirectory is not null)
        {
            Console.Error.WriteLine("COSWatcher:WatchDirectories 中存在未填写或不存在的目标目录。");
            return 1;
        }

        ServiceCollection services = new();
        services.Configure<TencentCloudStorageConfig>(configuration.GetSection("TencentCloudStorage"));
        services.AddTencentCloudStorage();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        TencentCloudStorageService cloudStorageService = serviceProvider.GetRequiredService<TencentCloudStorageService>();
        using COSWatcher watcher = new(cloudStorageService, watcherOptions);
        using CancellationTokenSource cancellationTokenSource = new();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        foreach (COSWatchDirectoryOptions watchDirectory in watcherOptions.WatchDirectories)
        {
            Console.WriteLine($"正在监听目录: {watchDirectory.WatchDirectory} -> {watchDirectory.KeyPrefix}");
        }
        Console.WriteLine("按 Ctrl+C 停止监听。");
        await watcher.RunAsync(cancellationTokenSource.Token);
        return 0;
    }

    private static bool IsDevelopment()
    {
        string? environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase);
    }
}
