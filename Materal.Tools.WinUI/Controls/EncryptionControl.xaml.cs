using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Materal.Tools.WinUI.Controls
{
    public sealed partial class EncryptionControl : UserControl
    {
        /// <summary>
        /// 是否能解密
        /// </summary>
        public bool CanDecode { get => (bool)GetValue(CanDecodeProperty); set => SetValue(CanDecodeProperty, value); }
        public static readonly DependencyProperty CanDecodeProperty = DependencyProperty.Register(nameof(CanDecode), typeof(bool), typeof(EncryptionControl), new PropertyMetadata(true));
        /// <summary>
        /// 明文
        /// </summary>
        public string PlainText { get => (string)GetValue(PlainTextProperty); set => SetValue(PlainTextProperty, value); }
        public static readonly DependencyProperty PlainTextProperty = DependencyProperty.Register(nameof(PlainText), typeof(string), typeof(EncryptionControl), new PropertyMetadata(string.Empty));
        /// <summary>
        /// 密文
        /// </summary>
        public string CipherText { get => (string)GetValue(CipherTextProperty); set => SetValue(CipherTextProperty, value); }
        public static readonly DependencyProperty CipherTextProperty = DependencyProperty.Register(nameof(CipherText), typeof(string), typeof(EncryptionControl), new PropertyMetadata(string.Empty));
        /// <summary>
        /// 操作面板内容
        /// </summary>
        public object? OperationPanelContent { get => GetValue(OperationPanelContentProperty); set => SetValue(OperationPanelContentProperty, value); }
        public static readonly DependencyProperty OperationPanelContentProperty = DependencyProperty.Register(nameof(OperationPanelContent), typeof(object), typeof(EncryptionControl), new PropertyMetadata(null));
        /// <summary>
        /// 加密按钮点击事件
        /// </summary>
        public event EventHandler<string>? EncryptButtonClick;
        /// <summary>
        /// 解密按钮点击事件
        /// </summary>
        public event EventHandler<string>? DecryptButtonClick;
        public EncryptionControl() => InitializeComponent();
        private void EncryptButton_Click(object sender, RoutedEventArgs e) => EncryptButtonClick?.Invoke(sender, PlainText);
        private void DecryptButton_Click(object sender, RoutedEventArgs e) => DecryptButtonClick?.Invoke(sender, CipherText);
    }
}
