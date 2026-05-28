using Materal.COA;
using Materal.Tools.WinUI.ViewModels;

namespace Materal.Tools.WinUI.Test;

[TestClass]
public sealed class COAPageViewModelTests
{
    [TestMethod]
    public void Constructor_ShouldDefaultEndDateToFutureDate()
    {
        COAPageViewModel viewModel = new();

        Assert.IsTrue(viewModel.EndDate > DateTimeOffset.Now);
    }

    [TestMethod]
    public void Constructor_ShouldDefaultGraceDaysToThirty()
    {
        COAPageViewModel viewModel = new();

        Assert.AreEqual(30, viewModel.GraceDays);
    }

    [TestMethod]
    public void GetCertificateExpirationTime_ShouldUseEndOfSelectedDay()
    {
        DateTimeOffset selectedDate = new(2026, 5, 25, 0, 0, 0, TimeSpan.FromHours(8));

        DateTimeOffset expirationTime = COAPageViewModel.GetCertificateExpirationTime(selectedDate);

        Assert.AreEqual(new DateTimeOffset(2026, 5, 25, 23, 59, 59, TimeSpan.FromHours(8)), expirationTime);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    public void Commands_ShouldNotExecute_WhenCertificateNameIsBlank(string certificateName)
    {
        COAPageViewModel viewModel = new()
        {
            CertificateName = certificateName
        };

        Assert.IsFalse(viewModel.WriteCertificateFileCommand.CanExecute(null));
        Assert.IsFalse(viewModel.VerifyAuthorizationCommand.CanExecute(null));
    }

    [TestMethod]
    public void CreateCertificateOptions_ShouldUseGraceDays()
    {
        COAPageViewModel viewModel = new()
        {
            CertificateName = " TestProject ",
            EndDate = new DateTimeOffset(2026, 5, 25, 0, 0, 0, TimeSpan.FromHours(8)),
            GraceDays = 0
        };

        Materal.COA.Generator.CertificateOptions options = viewModel.CreateCertificateOptions();

        Assert.AreEqual("TestProject", options.ProjectName);
        Assert.AreEqual(0, options.GraceDays);
        Assert.AreEqual(new DateTimeOffset(2026, 5, 25, 23, 59, 59, TimeSpan.FromHours(8)), options.ExpirationTime);
    }

    [TestMethod]
    public void GraceDaysInput_ShouldNormalizeToNonNegativeInteger()
    {
        COAPageViewModel viewModel = new()
        {
            GraceDaysInput = 7.9
        };

        Assert.AreEqual(7, viewModel.GraceDays);

        viewModel.GraceDaysInput = double.NaN;

        Assert.AreEqual(0, viewModel.GraceDays);
    }

    [TestMethod]
    public void GetVerificationMessage_ShouldShowGraceState()
    {
        CertificateVerificationResult result = new()
        {
            IsCertificateReadable = true,
            IsProjectMatched = true,
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(-1),
            GraceDays = 30,
            GraceEndTime = DateTimeOffset.UtcNow.AddDays(29)
        };

        string message = COAPageViewModel.GetVerificationMessage(result);

        Assert.IsTrue(message.StartsWith("证书已进入缓冲期"));
    }
}
