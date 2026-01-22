using Materal.Tools.WinUI.Helpers;
using Materal.Utils.Crypto;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("栅栏加解密", "\uED62")]
    public sealed partial class FencePage : Page
    {
        public FencePage() => InitializeComponent();
        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                MianPanel.CipherText = FenceCrypto.Encrypt(MianPanel.PlainText);
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
                MianPanel.PlainText = FenceCrypto.Decode(MianPanel.CipherText);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
