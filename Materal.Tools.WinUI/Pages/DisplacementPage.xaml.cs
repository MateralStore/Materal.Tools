using Materal.Extensions;
using Materal.Tools.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("位移加解密", "\uED62")]
    public sealed partial class DisplacementPage : Page
    {
        public DisplacementPage() => InitializeComponent();
        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                MianPanel.CipherText = MianPanel.PlainText.ToDisplacementEncode(Convert.ToInt32(DigitSlider.Value));
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
                MianPanel.PlainText = MianPanel.CipherText.DisplacementDecode(Convert.ToInt32(DigitSlider.Value));
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
    }
}
