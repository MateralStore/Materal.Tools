using Materal.Tools.WinUI.Helpers;
using Materal.Utils.Crypto;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("SHA256加密", "\uED62")]
    public sealed partial class SHA256Page : Page
    {
        public SHA256Page() => InitializeComponent();

        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                MianPanel.CipherText = SHA256Crypto.Hash(MianPanel.PlainText);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
