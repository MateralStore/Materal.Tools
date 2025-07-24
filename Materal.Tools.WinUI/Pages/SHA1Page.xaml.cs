using Materal.Extensions;
using Materal.Tools.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("SHA1加密", "\uED62")]
    public sealed partial class SHA1Page : Page
    {
        public SHA1Page() => InitializeComponent();

        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                MianPanel.CipherText = MianPanel.PlainText.ToSHA1_40Encode();
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
