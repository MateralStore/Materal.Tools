using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Materal.COA;
using Materal.COA.Generator;
using Materal.Utils.Extensions;
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
        public partial DateTimeOffset EndDate { get; set; } = GetCertificateExpirationTime(DateTimeOffset.Now.AddYears(1));
        /// <summary>
        /// 缓冲天数
        /// </summary>
        [ObservableProperty]
        public partial int GraceDays { get; set; } = 30;
        /// <summary>
        /// 缓冲天数输入值
        /// </summary>
        [ObservableProperty]
        public partial double GraceDaysInput { get; set; } = 30;
        /// <summary>
        /// 消息
        /// </summary>
        [ObservableProperty]
        public partial string Message { get; set; } = string.Empty;
        [RelayCommand(CanExecute = nameof(CanHandleCertificate))]
        private void WriteCertificateFile()
        {
            CertificateGeneratorService certificateGeneratorService = new();
            string baseDirectory = typeof(CertificateGeneratorService).Assembly.GetDirectoryPath().TrimEnd('\\');
            string certificateName = CertificateName.Trim();
            baseDirectory = Path.Combine(baseDirectory, "MateralCertificates", certificateName);
            CertificateFileResult result = certificateGeneratorService.GenerateToFile(baseDirectory, CreateCertificateOptions());
            ExplorerHelper.OpenExplorer(result.Certificate.FullName);
            Message = "证书已签发";
        }
        [RelayCommand(CanExecute = nameof(CanHandleCertificate))]
        private void VerifyAuthorization()
        {
            string baseDirectory = typeof(CertificateVerificationService).Assembly.GetDirectoryPath().TrimEnd('\\');
            string certificateName = CertificateName.Trim();
            string privateKeyPath = Path.Combine(baseDirectory, "MateralCertificates", certificateName, "private.key");
            if (!File.Exists(privateKeyPath))
            {
                Message = "证书不存在";
                return;
            }
            string certificatePath = Path.Combine(baseDirectory, "MateralCertificates", certificateName, "MateralCertificate.cer");
            try
            {
                CertificateVerificationService certificateVerificationService = new();
                CertificateVerificationResult xmlResult = certificateVerificationService.VerifyDetail(certificatePath, privateKeyPath, certificateName);
                string privateKeyPEMPath = Path.Combine(baseDirectory, "MateralCertificates", certificateName, "private.pem");
                CertificateVerificationResult pemResult = certificateVerificationService.VerifyDetail(certificatePath, privateKeyPEMPath, certificateName);
                if (!IsSameVerificationResult(xmlResult, pemResult))
                {
                    Message = "证书验证失败，PEM和XML验证结果不一致";
                }
                else
                {
                    Message = GetVerificationMessage(xmlResult);
                }
            }
            catch
            {
                Message = "不是有效的证书";
            }
        }
        public CertificateOptions CreateCertificateOptions() => new()
        {
            ProjectName = CertificateName.Trim(),
            ExpirationTime = GetCertificateExpirationTime(EndDate),
            GraceDays = GraceDays
        };
        public static DateTimeOffset GetCertificateExpirationTime(DateTimeOffset selectedDate)
            => new(selectedDate.Year, selectedDate.Month, selectedDate.Day, 23, 59, 59, selectedDate.Offset);
        public static string GetVerificationMessage(CertificateVerificationResult result)
        {
            if (!result.IsCertificateReadable || result.ExpirationTime is null)
            {
                return "证书不存在";
            }
            if (!result.IsProjectMatched)
            {
                return result.ErrorMessage ?? "项目名称不匹配";
            }
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (now <= result.ExpirationTime.Value)
            {
                return $"证书验证成功，到期时间:{result.ExpirationTime.Value:yyyy/MM/dd}，缓冲天数:{result.GraceDays ?? 30}";
            }
            if (result.GraceEndTime is not null && now <= result.GraceEndTime.Value)
            {
                return $"证书已进入缓冲期，到期时间:{result.ExpirationTime.Value:yyyy/MM/dd}，缓冲截止:{result.GraceEndTime.Value:yyyy/MM/dd}";
            }
            return $"证书已过期:{result.ExpirationTime.Value:yyyy/MM/dd}";
        }
        private static bool IsSameVerificationResult(CertificateVerificationResult first, CertificateVerificationResult second)
            => first.IsCertificateReadable == second.IsCertificateReadable &&
            first.IsProjectMatched == second.IsProjectMatched &&
            first.ProjectName == second.ProjectName &&
            first.ExpirationTime == second.ExpirationTime &&
            first.GraceDays == second.GraceDays &&
            first.GraceEndTime == second.GraceEndTime;
        private bool CanHandleCertificate() => !string.IsNullOrWhiteSpace(CertificateName) && GraceDays >= 0;
        partial void OnCertificateNameChanged(string value)
        {
            WriteCertificateFileCommand.NotifyCanExecuteChanged();
            VerifyAuthorizationCommand.NotifyCanExecuteChanged();
        }
        partial void OnGraceDaysChanged(int value)
        {
            if (Math.Abs(GraceDaysInput - value) > 0.1)
            {
                GraceDaysInput = value;
            }
            WriteCertificateFileCommand.NotifyCanExecuteChanged();
            VerifyAuthorizationCommand.NotifyCanExecuteChanged();
        }
        partial void OnGraceDaysInputChanged(double value)
        {
            int graceDays = double.IsFinite(value) ? Convert.ToInt32(Math.Max(0, Math.Floor(value))) : 0;
            if (GraceDays != graceDays)
            {
                GraceDays = graceDays;
            }
        }
    }
}
