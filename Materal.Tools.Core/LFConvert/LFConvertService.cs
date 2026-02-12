
using Microsoft.Extensions.Logging;

namespace Materal.Tools.Core.LFConvert
{
    /// <summary>
    /// LF转换服务
    /// </summary>
    public class LFConvertService(ILogger<LFConvertService>? logger = null) : ILFConvertService
    {
        /// <summary>
        /// CRLF转换为LF
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ToolsException"></exception>
        public async Task CRLFToLFAsync(string path, LFConvertOptions? options = null)
        {
            logger?.LogInformation($"转换开始...");
            options ??= new();
            FileInfo fileInfo = new(path);
            if (fileInfo.Exists)
            {
                await CRLFToLFAsync(fileInfo);
            }
            else
            {
                DirectoryInfo directoryInfo = new(path);
                if (directoryInfo.Exists)
                {
                    await CRLFToLFAsync(directoryInfo, options);
                }
                else
                {
                    throw new ToolsException($"路径{path}不存在");
                }
            }
            logger?.LogInformation($"转换完成");
        }
        /// <summary>
        /// LF转换为CRLF
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ToolsException"></exception>
        public async Task LFToCRLFAsync(string path, LFConvertOptions? options = null)
        {
            logger?.LogInformation($"转换开始...");
            options ??= new();
            FileInfo fileInfo = new(path);
            if (fileInfo.Exists)
            {
                await LFToCRLFAsync(fileInfo);
            }
            else
            {
                DirectoryInfo directoryInfo = new(path);
                if (directoryInfo.Exists)
                {
                    await LFToCRLFAsync(directoryInfo, options);
                }
                else
                {
                    throw new ToolsException($"路径{path}不存在");
                }
            }
            logger?.LogInformation($"转换完成");
        }
        /// <summary>
        /// CRLF转换为LF
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ToolsException"></exception>
        private async Task CRLFToLFAsync(DirectoryInfo directoryInfo, LFConvertOptions options)
        {
            if (!directoryInfo.Exists) throw new ToolsException("文件夹不存在");
            IEnumerable<FileInfo> fileInfos = directoryInfo.GetFiles().Where(options.Filter);
            foreach (FileInfo fileInfo in fileInfos)
            {
                await CRLFToLFAsync(fileInfo);
            }
            if (options.Recursive)
            {
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
                foreach (DirectoryInfo subDirectoryInfo in directoryInfos)
                {
                    await CRLFToLFAsync(subDirectoryInfo, options);
                }
            }
        }
        /// <summary>
        /// LF转换为CRLF
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ToolsException"></exception>
        private async Task LFToCRLFAsync(DirectoryInfo directoryInfo, LFConvertOptions options)
        {
            if (!directoryInfo.Exists) throw new ToolsException("文件夹不存在");
            IEnumerable<FileInfo> fileInfos = directoryInfo.GetFiles().Where(options.Filter);
            foreach (FileInfo fileInfo in fileInfos)
            {
                await LFToCRLFAsync(fileInfo);
            }
            if (options.Recursive)
            {
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
                foreach (DirectoryInfo subDirectoryInfo in directoryInfos)
                {
                    await LFToCRLFAsync(subDirectoryInfo, options);
                }
            }
        }
        /// <summary>
        /// CRLF转换为LF
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private async Task CRLFToLFAsync(FileInfo fileInfo)
        {
            if (!fileInfo.Exists) throw new FileNotFoundException($"文件{fileInfo.FullName}不存在");
            logger?.LogInformation($"正在转换{fileInfo.FullName}为LF");
#if NET
            byte[] bytes = await File.ReadAllBytesAsync(fileInfo.FullName);
#else
            byte[] bytes = File.ReadAllBytes(fileInfo.FullName);
            await Task.CompletedTask;
#endif
            // 将CRLF(0x0D 0x0A)替换为LF(0x0A)
            var result = new List<byte>();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i < bytes.Length - 1 && bytes[i] == 0x0D && bytes[i + 1] == 0x0A)
                {
                    result.Add(0x0A);
                    i++; // 跳过下一个字节(0x0A)
                }
                else
                {
                    result.Add(bytes[i]);
                }
            }
            logger?.LogInformation($"转换{fileInfo.FullName}成功");
#if NET
            await File.WriteAllBytesAsync(fileInfo.FullName, result.ToArray());
#else
            File.WriteAllBytes(fileInfo.FullName, result.ToArray());
            await Task.CompletedTask;
#endif
        }
        /// <summary>
        /// LF转换为CRLF
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private async Task LFToCRLFAsync(FileInfo fileInfo)
        {
            if (!fileInfo.Exists) throw new FileNotFoundException($"文件{fileInfo.FullName}不存在");
            logger?.LogInformation($"正在转换{fileInfo.FullName}为CRLF");
#if NET
            byte[] bytes = await File.ReadAllBytesAsync(fileInfo.FullName);
#else
            byte[] bytes = File.ReadAllBytes(fileInfo.FullName);
#endif
            // 将LF(0x0A)替换为CRLF(0x0D 0x0A)，但避免重复替换
            var result = new List<byte>();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0x0A)
                {
                    // 检查前面是否已经是CR，避免重复替换
                    if (i > 0 && bytes[i - 1] == 0x0D)
                    {
                        result.Add(0x0A);
                    }
                    else
                    {
                        result.Add(0x0D);
                        result.Add(0x0A);
                    }
                }
                else
                {
                    result.Add(bytes[i]);
                }
            }
            logger?.LogInformation($"转换{fileInfo.FullName}成功");
#if NET
            await File.WriteAllBytesAsync(fileInfo.FullName, result.ToArray());
#else
            File.WriteAllBytes(fileInfo.FullName, result.ToArray());
            await Task.CompletedTask;
#endif
        }
    }
}
