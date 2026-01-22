using Materal.Tools.WinUI.Helpers;
using Materal.Utils.Crypto;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("Base64加解密", "\uED62")]
    public sealed partial class Base64Page : Page
    {
        public Base64Page() => InitializeComponent();
        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                MianPanel.CipherText = Base64Crypto.Encode(MianPanel.PlainText);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
        private async void MianPanel_DecryptButtonClick(object sender, string e)
        {
            try
            {
                MianPanel.PlainText = Base64Crypto.Decode(MianPanel.CipherText);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
