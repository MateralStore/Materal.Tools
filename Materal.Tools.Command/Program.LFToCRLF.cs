using Materal.Tools.Core.LFConvert;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.RegularExpressions;
using SubCommand = System.CommandLine.Command;

namespace Materal.Tools.Command
{
    public partial class Program
    {
        /// <summary>
        /// 添加LF转换CRLF命令
        /// </summary>
        /// <param name="rootCommand"></param>
        [AddSubCommand]
        public void AddLFToCRLFCommand(RootCommand rootCommand)
        {
            SubCommand command = new("LFToCRLF", "LF转换CRLF");

            Option<string?> pathOption = new("--Path") { Description = "指定路径" };
            pathOption.Aliases.Add("-p");
            command.Options.Add(pathOption);

            Option<bool> recursiveOption = new("--Recursive") { Description = "递归", DefaultValueFactory = _ => true };
            recursiveOption.Aliases.Add("-r");
            command.Options.Add(recursiveOption);

            Option<string> filterOption = new("--Filter") { Description = "过滤正则表达式", DefaultValueFactory = _ => "^.+$" };
            filterOption.Aliases.Add("-f");
            filterOption.AcceptOnlyFromAmong("^.+\\.cs$", "^.+\\.xml$", "其他正则");
            command.Options.Add(filterOption);

            command.SetAction(parseResult => LFToCRLFAsync(
                parseResult.GetValue(pathOption),
                parseResult.GetValue(recursiveOption),
                parseResult.GetValue(filterOption) ?? "^.+$"));
            rootCommand.Subcommands.Add(command);
        }
        private async Task LFToCRLFAsync(string? path, bool recursive, string filter)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Environment.CurrentDirectory;
            }
            ILFConvertService service = _serviceProvider.GetRequiredService<ILFConvertService>();
            LFConvertOptions options = new()
            {
                Recursive = Convert.ToBoolean(recursive),
                Filter = fileInfo => new Regex(filter).Match(fileInfo.Name).Success,
            };
            await service.LFToCRLFAsync(path, options);
        }
    }
}
