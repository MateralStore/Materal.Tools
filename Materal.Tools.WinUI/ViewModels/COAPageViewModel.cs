using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Materal.COA;
using Materal.Extensions;
using Materal.Utils.Windows;

namespace Materal.Tools.WinUI.ViewModels
{
    public partial class COAPageViewModel : ObservableObject
    {
        /// <summary>
        /// 证书名称
        /// </summary>
        [ObservableProperty]
        public partial string CertificateName { get; set; } = string.Empty;
        /// <summary>
        /// 证书名称
        /// </summary>
        [ObservableProperty]
        public partial DateTimeOffset EndDate { get; set; } = DateTime.Now.Date.ToDateTimeOffset();
        /// <summary>
        /// 消息
        /// </summary>
        [ObservableProperty]
        public partial string Message { get; set; } = string.Empty;
        [RelayCommand]
        private void WriteCertificateFile()
        {
            COAHost.WriteCertificateFile(CertificateName, EndDate);
            string baseDirectory = typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('\\');
            string privateKeyPath = Path.Combine(baseDirectory, "MateralCertificates", CertificateName);
            ExplorerHelper.OpenExplorer(privateKeyPath);
            Message = "证书已签发";
        }
        [RelayCommand]
        private void VerifyAuthorization()
        {
            string baseDirectory = typeof(COAHost).Assembly.GetDirectoryPath().TrimEnd('\\');
            string privateKeyPath = Path.Combine(baseDirectory, "MateralCertificates", CertificateName, "private.key");
            if (!File.Exists(privateKeyPath))
            {
                Message = "证书不存在";
                return;
            }
            string privateKey = File.ReadAllText(privateKeyPath);
            string certificatePath = Path.Combine(baseDirectory, "MateralCertificates", CertificateName, "MateralCertificate.cer");
            try
            {
                if (COAHost.VerifyAuthorization(CertificateName, privateKey, certificatePath, out DateTimeOffset? endDate))
                {
                    string privateKeyPEMPath = Path.Combine(baseDirectory, "MateralCertificates", CertificateName, "private.pem");
                    string privateKeyPEM = File.ReadAllText(privateKeyPEMPath);
                    if (COAHost.VerifyAuthorizationPEM(CertificateName, privateKeyPEM, certificatePath, out DateTimeOffset? endDate2))
                    {
                        if (endDate2 is null || endDate != endDate2)
                        {
                            Message = $"证书验证失败，PEM和XML验证结果不一致";
                        }
                        else
                        {
                            Message = $"证书验证成功，到期时间:{endDate2.Value:yyyy/MM/dd}";
                        }
                    }
                    else
                    {
                        if (endDate2 is null)
                        {
                            Message = "证书不存在";
                        }
                        else
                        {
                            Message = $"证书已过期:{endDate2.Value:yyyy/MM/dd}";
                        }
                    }
                }
                else
                {
                    if (endDate is null)
                    {
                        Message = "证书不存在";
                    }
                    else
                    {
                        Message = $"证书已过期:{endDate.Value:yyyy/MM/dd}";
                    }
                }
            }
            catch
            {
                Message = "不是有效的证书";
            }
        }
    }
}
