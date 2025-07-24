using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Materal.Tools.WinUI.Controls
{
    public sealed partial class FileSelector : UserControl
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(FileSelector), new PropertyMetadata(string.Empty));
        /// <summary>
        /// 密文
        /// </summary>
        public string FilePath { get => (string)GetValue(FilePathProperty); set => SetValue(FilePathProperty, value); }
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(FileSelector), new PropertyMetadata(string.Empty));
        /// <summary>
        /// 密文
        /// </summary>
        public IList<string>? FileTypeFilters { get => (IList<string>?)GetValue(FileTypeFilterProperty); set => SetValue(FileTypeFilterProperty, value); }
        public static readonly DependencyProperty FileTypeFilterProperty = DependencyProperty.Register(nameof(FileTypeFilters), typeof(IList<string>), typeof(FileSelector), new PropertyMetadata(null));
        public event EventHandler<StorageFile>? FileChanged;
        public FileSelector()
        {
            InitializeComponent();
            FileTypeFilters = [];
        }
        private async void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile? file = await SelectFileAsync();
            if (file == null) return;
            FilePath = file.Path;
            FileChanged?.Invoke(this, file);
        }
        /// <summary>
        /// 选择密钥文件
        /// </summary>
        /// <returns>选择的文件</returns>
        private async Task<StorageFile?> SelectFileAsync()
        {
            FileOpenPicker openPicker = new()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            if (FileTypeFilters is not null && FileTypeFilters.Count > 0)
            {
                foreach (string fileTypeFilter in FileTypeFilters)
                {
                    openPicker.FileTypeFilter.Add(fileTypeFilter);
                }
            }
            nint hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(openPicker, hwnd);
            return await openPicker.PickSingleFileAsync();
        }
    }
}