using Materal.Tools.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Pages;

/// <summary>
/// 授权证书
/// </summary>
[Menu("授权证书", "\uE71D")]
public sealed partial class COAPage : Page
{
    public COAPageViewModel ViewModel { get; } = new();
    /// <summary>
    /// 授权证书
    /// </summary>
    public COAPage() => InitializeComponent();
}
