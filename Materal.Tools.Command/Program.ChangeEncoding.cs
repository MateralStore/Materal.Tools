using Materal.Tools.Core.ChangeEncoding;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using SubCommand = System.CommandLine.Command;

namespace Materal.Tools.Command
{
    public partial class Program
    {
        /// <summary>
        /// 添加更改编码命令
        /// </summary>
        /// <param name="rootCommand"></param>
        [AddSubCommand]
        public void AddChangeEncodingCommand(RootCommand rootCommand)
        {
            SubCommand command = new("ChangeEncoding", "更改文件编码");
            Option<string?> pathOption = new("--Path") { Description = "指定路径" };
            pathOption.Aliases.Add("-p");
            command.Options.Add(pathOption);

            Option<bool> recursiveOption = new("--Recursive") { Description = "递归", DefaultValueFactory = _ => true };
            recursiveOption.Aliases.Add("-r");
            command.Options.Add(recursiveOption);

            Option<string?> writeEncodingOption = new("--WriteEncoding") { Description = "写入编码" };
            writeEncodingOption.Aliases.Add("-write");
            writeEncodingOption.AcceptOnlyFromAmong("UTF-8", "GBK", "其他编码");
            command.Options.Add(writeEncodingOption);

            Option<string?> readEncodingOption = new("--ReadEncoding") { Description = "写入编码,不传则自动识别" };
            readEncodingOption.Aliases.Add("-read");
            readEncodingOption.AcceptOnlyFromAmong("UTF-8", "GBK", "其他编码");
            command.Options.Add(readEncodingOption);

            Option<string> filterOption = new("--Filter") { Description = "过滤正则表达式", DefaultValueFactory = _ => "^.+$" };
            filterOption.Aliases.Add("-f");
            filterOption.AcceptOnlyFromAmong("^.+\\.cs$", "^.+\\.xml$", "其他正则");
            command.Options.Add(filterOption);

            command.SetAction(parseResult => ChangeEncodingAsync(
                parseResult.GetValue(pathOption),
                parseResult.GetValue(recursiveOption),
                parseResult.GetValue(writeEncodingOption),
                parseResult.GetValue(readEncodingOption),
                parseResult.GetValue(filterOption) ?? "^.+$"));
            rootCommand.Subcommands.Add(command);
        }
        private async Task ChangeEncodingAsync(string? path, bool recursive, string? writeEncoding, string? readEncoding, string filter)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Environment.CurrentDirectory;
            }
            IChangeEncodingService service = _serviceProvider.GetRequiredService<IChangeEncodingService>();
            ChangeEncodingOptions options = new()
            {
                Recursive = Convert.ToBoolean(recursive),
                Filter = fileInfo => new Regex(filter).Match(fileInfo.Name).Success,
            };
            if (!string.IsNullOrWhiteSpace(writeEncoding))
            {
                options.WriteEncoding = Encoding.GetEncoding(writeEncoding);
            }
            if (!string.IsNullOrWhiteSpace(readEncoding))
            {
                options.ReadEncoding = Encoding.GetEncoding(readEncoding);
            }
            await service.ChangeEncodingAsync(path, options);
        }
    }
}
