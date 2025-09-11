using Materal.Tools.Core.ExcelImportDataBase;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Materal.Tools.WinUI.Pages
{
    [Menu("Excel导入数据库", "\uED62")]
    public sealed partial class ExcelImportDataBase : Page
    {
        public ExcelImportDataBase() => InitializeComponent();

        private async void BrowseExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xls");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                ExcelFilePathTextBox.Text = file.Path;
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证输入
            if (!ValidateInput()) return;
            // 显示进度条
            ImportProgressBar.Visibility = Visibility.Visible;
            ImportProgressBar.IsIndeterminate = true;
            ImportButton.IsEnabled = false;
            try
            {
                ExcelImportOptions options = GetExcelImportOptions();
                IExcelImportDataBase importer = SqlServerRadioButton.IsChecked == true
                    ? new ExcelImportSqlServer()
                    : new ExcelImportOracle();
                string filePath = ExcelFilePathTextBox.Text;
                string connectionString = ConnectionStringTextBox.Text;
                Task.Run(() =>
                {
                    importer.DatabaseValidation += Importer_DatabaseValidation;
                    importer.ImportCompleted += Importer_ImportCompleted;
                    importer.ImportProgressChanged += Importer_ImportProgressChanged;
                    importer.ReadExcelCompleted += Importer_ReadExcelCompleted;
                    importer.ImportStarted += Importer_ImportStarted;
                    importer.ReadExcelStarted += Importer_ReadExcelStarted;
                    importer.Import(filePath, connectionString, options);
                });
            }
            catch (Exception ex)
            {
                ResultTextBlock.Text = $"导入过程中发生错误: {ex.Message}";
                ResultTextBlock.Visibility = Visibility.Visible;
                ErrorTextBox.Text = ex.ToString();
                ErrorTextBox.Visibility = Visibility.Visible;
            }
            finally
            {
                // 隐藏进度条
                ImportProgressBar.IsIndeterminate = false;
                ImportProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void Importer_ReadExcelStarted(object? sender, ExcelImportResult e) => DispatcherQueue.TryEnqueue(() =>
        {
            ResultTextBlock.Text = "开始读取Excel文件...";
            ResultTextBlock.Visibility = Visibility.Visible;
            ErrorTextBox.Visibility = Visibility.Collapsed;
        });

        private void Importer_ImportStarted(object? sender, ExcelImportResult e) => DispatcherQueue.TryEnqueue(() =>
        {
            ResultTextBlock.Text = "开始导入...";
            ResultTextBlock.Visibility = Visibility.Visible;
            ErrorTextBox.Visibility = Visibility.Collapsed;
        });

        private void Importer_ReadExcelCompleted(object? sender, ExcelImportResult e) => DispatcherQueue.TryEnqueue(() =>
        {
            ResultTextBlock.Text = "读取Excel文件成功";
            ResultTextBlock.Visibility = Visibility.Visible;
            ErrorTextBox.Visibility = Visibility.Collapsed;
        });

        private void Importer_ImportProgressChanged(object? sender, ExcelImportResult e) => DispatcherQueue.TryEnqueue(() =>
        {
            ResultTextBlock.Text = $"正在导入{e.SuccessRows + e.FailedRows}/{e.TotalRows},成功:{e.SuccessRows},失败:{e.FailedRows}";
            ResultTextBlock.Visibility = Visibility.Visible;
            if (e.Errors.Count != 0)
            {
                ErrorTextBox.Text = string.Join("\n\n", e.Errors);
                ErrorTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorTextBox.Visibility = Visibility.Collapsed;
            }
        });

        private void Importer_ImportCompleted(object? sender, ExcelImportResult e) => DispatcherQueue.TryEnqueue(() =>
        {
            DisplayResult(e);
            ImportButton.IsEnabled = true;
        });

        private void Importer_DatabaseValidation(object? sender, ExcelImportResult e) => DispatcherQueue.TryEnqueue(() =>
        {
            ResultTextBlock.Text = "正在验证数据库....";
            ResultTextBlock.Visibility = Visibility.Visible;
            ErrorTextBox.Visibility = Visibility.Collapsed;
        });

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ExcelFilePathTextBox.Text))
            {
                ResultTextBlock.Text = "请选择Excel文件";
                ResultTextBlock.Visibility = Visibility.Visible;
                return false;
            }

            if (string.IsNullOrWhiteSpace(ConnectionStringTextBox.Text))
            {
                ResultTextBlock.Text = "请输入数据库连接字符串";
                ResultTextBlock.Visibility = Visibility.Visible;
                return false;
            }

            if (string.IsNullOrWhiteSpace(TableNameTextBox.Text))
            {
                ResultTextBlock.Text = "请输入表名";
                ResultTextBlock.Visibility = Visibility.Visible;
                return false;
            }

            if (string.IsNullOrWhiteSpace(SheetNameTextBox.Text))
            {
                ResultTextBlock.Text = "请输入工作表名称";
                ResultTextBlock.Visibility = Visibility.Visible;
                return false;
            }

            return true;
        }

        private ExcelImportOptions GetExcelImportOptions()
        {
            var options = new ExcelImportOptions
            {
                TableName = TableNameTextBox.Text,
                SheetName = SheetNameTextBox.Text,
                BatchSize = (int)BatchSizeNumberBox.Value
            };

            // 解析列映射
            if (!string.IsNullOrWhiteSpace(ColumnMappingTextBox.Text))
            {
                options.ColumnMapping = [];
                string[] lines = ColumnMappingTextBox.Text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(["->"], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        options.ColumnMapping[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }

            return options;
        }

        private void DisplayResult(ExcelImportResult result)
        {
            ResultTextBlock.Text = $"导入完成！成功: {result.SuccessRows} 行，失败: {result.FailedRows} 行";
            ResultTextBlock.Visibility = Visibility.Visible;
            if (result.Errors.Count != 0)
            {
                ErrorTextBox.Text = string.Join("\n\n", result.Errors);
                ErrorTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SqlServerRadioButton.IsChecked = true;
            ConnectionStringTextBox.Text = "";
            ExcelFilePathTextBox.Text = "";
            TableNameTextBox.Text = "";
            SheetNameTextBox.Text = "Sheet1";
            BatchSizeNumberBox.Value = 100;
            ColumnMappingTextBox.Text = "";
            ResultTextBlock.Visibility = Visibility.Collapsed;
            ErrorTextBox.Visibility = Visibility.Collapsed;
        }
    }
}
