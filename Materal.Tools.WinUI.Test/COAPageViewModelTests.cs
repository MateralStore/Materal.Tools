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
}
