using Materal.Tools.WinUI.Helpers;
using Materal.Utils.Crypto;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("二进制加解密", "\uED62")]
    public sealed partial class BinaryPage : Page
    {
        public BinaryPage() => InitializeComponent();
        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                MianPanel.CipherText = BaseCrypto.EncodeBinary(MianPanel.PlainText);
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
                MianPanel.PlainText = BaseCrypto.DecodeBinary(MianPanel.CipherText);
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
