using System.Text.Json;

namespace Materal.Tools.WinUI.Models
{
    public class DatabaseConfig
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        public static string GetConfigFilePath()
        {
            string tempPath = Path.GetTempPath();
            return Path.Combine(tempPath, "MateralTools_ExcelImportDataBase_Config.json");
        }

        public static DatabaseConfig? LoadConfig()
        {
            string configPath = GetConfigFilePath();
            if (!File.Exists(configPath))
                return null;

            try
            {
                string json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<DatabaseConfig>(json);
            }
            catch
            {
                return null;
            }
        }

        public void SaveConfig()
        {
            try
            {
                string configPath = GetConfigFilePath();
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);
            }
            catch
            {
                // 忽略保存错误
            }
        }
    }
}
