using Materal.COA;
using Materal.Extensions;
using Materal.Tools.Core;
using Materal.Tools.WinUI.Helpers;
using Materal.Utils.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("RSA加解密", "\uED62")]
    public sealed partial class RSAPage : Page
    {
        private string _publicKeyContent = string.Empty;
        private string _privateKeyContent = string.Empty;
        public RSAPage() => InitializeComponent();
        private async void EncryptionControl_EncryptButtonClick(object sender, string e)
        {
            try
            {
                if (string.IsNullOrEmpty(_publicKeyContent)) throw new ToolsException("请先选择公钥文件");
                if (string.IsNullOrEmpty(MianPanel.PlainText)) throw new ToolsException("请输入需要加密的明文");
                if (_publicKeyContent.Contains("-----BEGIN PUBLIC KEY-----"))
                {
#if NET8_0_OR_GREATER
                    MianPanel.CipherText = MianPanel.PlainText.ToRSAEncodePEM(_publicKeyContent);
#else
                    throw new ToolsException("当前版本不支持PEM格式的密钥，请使用XML格式的密钥");
#endif
                }
                else
                {
                    MianPanel.CipherText = MianPanel.PlainText.ToRSAEncode(_publicKeyContent);
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"加密失败：{ex.Message}");
            }
        }
        private async void EncryptionControl_DecryptButtonClick(object sender, string e)
        {
            try
            {
                if (string.IsNullOrEmpty(_privateKeyContent)) throw new ToolsException("请先选择私钥文件");
                if (string.IsNullOrEmpty(MianPanel.CipherText)) throw new ToolsException("请输入需要解密的密文");
                // 判断是否为PEM格式的私钥
                if (_privateKeyContent.Contains("-----BEGIN PRIVATE KEY-----"))
                {
#if NET8_0_OR_GREATER
                    MianPanel.PlainText = MianPanel.CipherText.RSADecodePEM(_privateKeyContent);
#else
                    throw new ToolsException("当前版本不支持PEM格式的密钥，请使用XML格式的密钥");
#endif
                }
                else
                {
                    MianPanel.PlainText = MianPanel.CipherText.RSADecode(_privateKeyContent);
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync($"解密失败：{ex.Message}");
            }
        }
        private async void PublicKeyFileSelector_FileChanged(object sender, StorageFile file) => _publicKeyContent = await FileIO.ReadTextAsync(file);
        private async void PrivateKeyFileSelector_FileChanged(object sender, StorageFile file) => _privateKeyContent = await FileIO.ReadTextAsync(file);
        private void CreateKey_Click(object sender, RoutedEventArgs e)
        {
            string baseDirectory = typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('\\');
            baseDirectory = Path.Combine(baseDirectory, "Temp", "RSAKey");
            COAHost.WriteCertificateFile("RSAKey", DateTime.Now, baseDirectory);
            string certificatePath = Path.Combine(baseDirectory, "MateralCertificate.cer");
            string publicKeyPath = Path.Combine(baseDirectory, "public.key");
            string privateKeyPath = Path.Combine(baseDirectory, "private.key");
            if (File.Exists(certificatePath))
            {
                File.Delete(certificatePath);
            }
            PublicKeyFileSelector.FilePath = publicKeyPath;
            _publicKeyContent = File.ReadAllText(publicKeyPath);
            PrivateKeyFileSelector.FilePath = privateKeyPath;
            _privateKeyContent = File.ReadAllText(privateKeyPath);
            ExplorerHelper.OpenExplorer(privateKeyPath);
        }
    }
}
