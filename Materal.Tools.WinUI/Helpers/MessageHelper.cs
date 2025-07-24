using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Helpers
{
    public static class MessageHelper
    {
        /// <summary>
        /// 显示消息对话框
        /// </summary>
        /// <param name="message">消息内容</param>
        public static async Task ShowMessageAsync(this Control control, string message)
        {
            ContentDialog dialog = new()
            {
                Title = "提示",
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = control.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
