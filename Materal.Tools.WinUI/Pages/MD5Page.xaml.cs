using Materal.Extensions;
using Materal.Tools.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("MD5加密", "\uED62")]
    public sealed partial class MD5Page : Page
    {
        public MD5Page() => InitializeComponent();

        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                if (Is32ToggleButton.IsChecked is null) return;
                MianPanel.CipherText = Is32ToggleButton.IsChecked.Value ? MianPanel.PlainText.ToMd5_32Encode() : MianPanel.PlainText.ToMd5_16Encode();
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
