using Materal.COA.Generator;
using Materal.Extensions;
using Materal.Tools.Core;
using Materal.Tools.WinUI.Helpers;
using Materal.Utils.Crypto;
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
                MianPanel.CipherText = RsaCrypto.Encrypt(MianPanel.PlainText, _publicKeyContent);
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
                MianPanel.CipherText = RsaCrypto.Decrypt(MianPanel.CipherText, _privateKeyContent);
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
            string baseDirectory = typeof(CertificateGeneratorService).Assembly.GetDirectoryPath().TrimEnd('\\');
            baseDirectory = Path.Combine(baseDirectory, "Temp", "RSAKey");
            CertificateGeneratorService certificateGeneratorService = new();
            certificateGeneratorService.GenerateToFile(baseDirectory, new()
            {
                ProjectName = "RSAKey",
                ExpirationTime = DateTimeOffset.UtcNow,
            });
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
