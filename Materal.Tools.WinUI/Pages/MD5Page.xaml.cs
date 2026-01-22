using Materal.Tools.WinUI.Helpers;
using Materal.Utils.Crypto;
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
                MianPanel.CipherText = Is32ToggleButton.IsChecked.Value ? MD5Crypto.Hash32(MianPanel.PlainText) : MD5Crypto.Hash16(MianPanel.PlainText);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
