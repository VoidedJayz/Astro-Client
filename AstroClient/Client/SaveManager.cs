using AstroClient.Systems;
using NAudio.SoundFont;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroClient.Client
{
    public class GameData
    {
        public int? CoinCount { get; set; }
        public int? TimeLeft { get; set; }
        public int? QuotaAmount { get; set; }
    }

    internal class SaveManager
    {
        // Important
        public static Dictionary<string, string> saveMap = new Dictionary<string, string>();
        public static PresetLibrary presetLibrary = new PresetLibrary();
        public static string LocalLowPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\";
        public static string DefaultSaveDirectory = Directory.GetCurrentDirectory() + "\\GameBackups\\";
        public static string GameSavePath = LocalLowPath + "\\ZeekerssRBLX\\Lethal Company\\";
        public static string Password = "lcslime14a5";
        public static string Save1 = "LCSaveFile1";
        public static string Save2 = "LCSaveFile2";
        public static string Save3 = "LCSaveFile3";
        public static void Start()
        {
            try
            {
                LogSystem.Log("Starting Save System...");
                saveMap.Add("Save 1", Save1);
                saveMap.Add("Save 2", Save2);
                saveMap.Add("Save 3", Save3);
                presetLibrary.AddPreset("Titan Capable", "TitanReady");
                presetLibrary.AddPreset("Astros Personal Save", "AstrosSave");
                if (!Directory.Exists(DefaultSaveDirectory))
                {
                    Directory.CreateDirectory(DefaultSaveDirectory);
                }
                LogSystem.Log("Save System Started!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error starting save system. Check logs for details.");
                LogSystem.ReportError("Error starting save system: " + ex);
            }
        }
        public static List<string> GetStats(string save)
        {
            try
            {
                LogSystem.Log("Getting Save Stats...");
                var loadedSave = Decrypt(Password, File.ReadAllBytes(GameSavePath + saveMap[save]));
                var data = JsonConvert.DeserializeObject<dynamic>(loadedSave);
                // save OG Data
                var OG_CoinCount = data.GroupCredits.value;
                var OG_TimeLeft = data.DeadlineTime.value;
                var OG_QuotaAmount = data.ProfitQuota.value;
                LogSystem.Log("Save Stats: " + OG_CoinCount + " " + OG_TimeLeft + " " + OG_QuotaAmount);
                return new List<string>()
                {
                    OG_CoinCount.ToString(),
                    OG_TimeLeft.ToString(),
                    OG_QuotaAmount.ToString()
                };
            }
            catch (Exception ex)
            {
                LogSystem.ReportError("Error getting save stats: " + ex);
                return new List<string>()
                {
                    "???",
                    "???",
                    "???"
                };
            }
        }
        public static void ModifyGameData(string SaveToModify, GameData newData)
        {
            try
            {
                LogSystem.Log("Modifying Game Data...");
                var loadedSave = Decrypt(Password, File.ReadAllBytes(GameSavePath + saveMap[SaveToModify]));
                var data = JsonConvert.DeserializeObject<dynamic>(loadedSave);
                // save OG Data
                var OG_CoinCount = data.GroupCredits.value;
                var OG_TimeLeft = data.DeadlineTime.value;
                var OG_QuotaAmount = data.ProfitQuota.value;
                LogSystem.Log("Original Save Values: " + OG_CoinCount + " " + OG_TimeLeft + " " + OG_QuotaAmount);

                // Modify the data, if the data is null then remain the old data
                data.GroupCredits.value = newData.CoinCount ?? OG_CoinCount;
                data.DeadlineTime.value = newData.TimeLeft ?? OG_TimeLeft;
                data.ProfitQuota.value = newData.QuotaAmount ?? OG_QuotaAmount;
                LogSystem.Log("New Save Values: " + data.GroupCredits.value + " " + data.DeadlineTime.value + " " + data.ProfitQuota.value);

                // Finish
                var modifiedJsonData = JsonConvert.SerializeObject(data);
                var encryptedData = Encrypt(Password, modifiedJsonData);
                File.WriteAllBytes(GameSavePath + saveMap[SaveToModify], encryptedData);
                LogSystem.Log("Game Data Modified!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error modifying game data. Check logs for details.");
                LogSystem.ReportError("Error modifying game data: " + ex);
            }
        }
        public static void ExportSave(string SaveToExport)
        {
            try
            {
                LogSystem.Log("Exporting Save Data...");
                var path = GameSavePath + saveMap[SaveToExport];
                var downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\";
                ConsoleSystem.AnimatedText($"Would you like to name the save?");
                ConsoleSystem.AnimatedText($"If you do not enter a name, the save will be named Exported_LCSaveFile");
                string fileName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = "Exported_LCSaveFile";
                }
                FileSystem.CopyFile(path, downloads + fileName);
                LogSystem.Log("Save Data Exported!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error exporting save data. Check logs for details.");
                LogSystem.ReportError("Error exporting save data: " + ex);
            }
        }
        public static void ImportSave(string SaveToImport, string filePath)
        {
            try
            {
                LogSystem.Log("Requesting Save Location...");
                string PathToSave = "";
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Title = "Select the save file to import";
                        openFileDialog.Filter = "All files (*.*)|*.*";

                        DialogResult result = openFileDialog.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
                        {
                            PathToSave = openFileDialog.FileName;
                            ConsoleSystem.AnimatedText($"Selected file: {PathToSave}");
                            LogSystem.Log($"User selected file path: {PathToSave}");
                        }
                        else
                        {
                            ConsoleSystem.AnimatedText("No file was selected.");
                            LogSystem.Log("User did not select a file path.");
                        }
                    }
                }
                else
                {
                    ConsoleSystem.AnimatedText($"You entered: {filePath}");
                    LogSystem.Log($"User entered folder path: {filePath}");
                }
                LogSystem.Log("Importing Save Data...");
                if (!FileSystem.FileExists(PathToSave))
                {
                    LogSystem.Log("Save File Does Not Exist!");
                    ConsoleSystem.AnimatedText("Save File Does Not Exist, Please Try Again!");
                    Task.Delay(2000).Wait();
                    MenuManager.SavesMenu();
                    return;
                }
                FileSystem.CopyFile(PathToSave, GameSavePath + saveMap[SaveToImport]);
                LogSystem.Log("Save Data Imported!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error importing save data. Check logs for details.");
                LogSystem.ReportError("Error importing save data: " + ex);
            }
        }
        public static void DeleteSave(string SaveToDelete)
        {
            LogSystem.Log("Deleting Save Data...");
            try
            {
                FileSystem.DeleteFile(GameSavePath + saveMap[SaveToDelete]);
                LogSystem.Log("Save Data Deleted!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error deleting save data. Check logs for details.");
                LogSystem.ReportError("Error deleting save data: " + ex);
            }
        }
        public static void DeleteAllSaves()
        {
            LogSystem.Log("Deleting All Save Data...");
            try
            {
                FileSystem.DeleteDirectory(GameSavePath);
                LogSystem.Log("All Save Data Deleted!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error deleting all save data. Check logs for details.");
                LogSystem.ReportError("Error deleting all save data: " + ex);
            }
        }
        private static string Decrypt(string password, byte[] data)
        {
            LogSystem.Log("Decrypting Save Data...");
            try
            {
                byte[] IV = new byte[16];
                Array.Copy(data, IV, 16);
                byte[] dataToDecrypt = new byte[data.Length - 16];
                Array.Copy(data, 16, dataToDecrypt, 0, dataToDecrypt.Length);

                using (Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(password, IV, 100, HashAlgorithmName.SHA1))
                using (Aes decAlg = Aes.Create())
                {
                    decAlg.Mode = CipherMode.CBC;
                    decAlg.Padding = PaddingMode.PKCS7;
                    decAlg.Key = k2.GetBytes(16);
                    decAlg.IV = IV;

                    using (MemoryStream decryptionStreamBacking = new MemoryStream())
                    using (CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        decrypt.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                        decrypt.FlushFinalBlock();
                        LogSystem.Log("Save Data Decrypted!");

                        return new UTF8Encoding(true).GetString(decryptionStreamBacking.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error decrypting save data. Check logs for details.");
                LogSystem.ReportError("Error decrypting save data: " + ex);
                return null; // Handle decryption error accordingly
            }
        }
        private static byte[] Encrypt(string password, string data)
        {
            LogSystem.Log("Encrypting Save Data...");
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.KeySize = 128;

                    // Generate a random IV
                    aesAlg.GenerateIV();

                    using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, aesAlg.IV, 100, HashAlgorithmName.SHA1))
                    {
                        aesAlg.Key = keyDerivation.GetBytes(16); // 128-bit key

                        using (MemoryStream encryptedStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(encryptedStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                                cryptoStream.Write(dataBytes, 0, dataBytes.Length);
                            }

                            byte[] iv = aesAlg.IV; // Get IV
                            byte[] encryptedData = encryptedStream.ToArray(); // Get encrypted data

                            // Combine IV and encrypted data
                            byte[] ivAndEncryptedData = new byte[iv.Length + encryptedData.Length];
                            Array.Copy(iv, 0, ivAndEncryptedData, 0, iv.Length);
                            Array.Copy(encryptedData, 0, ivAndEncryptedData, iv.Length, encryptedData.Length);
                            LogSystem.Log("Save Data Encrypted!");
                            return ivAndEncryptedData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error encrypting save data. Check logs for details.");
                LogSystem.ReportError("Error encrypting save data: " + ex);
                return null; // Handle encryption error accordingly
            }
        }
    }

    public class PresetLibrary
    {
        public List<Preset> presets = new List<Preset>();
        public void AddPreset(string name, string serverName)
        {
            presets.Add(new Preset(name, serverName));
        }
        public void ImportPreset(string SaveToImport, string presetName)
        {
            try
            {
                LogSystem.Log("Importing Save Data from Server...");

                Preset preset = presets.Find(p => p.Name == presetName);

                if (preset == null)
                {
                    LogSystem.Log("Preset not found in the library.");
                    ConsoleSystem.AnimatedText("Preset not found in the library.");
                    return;
                }

                string serverUrl = $"https://astroswrld.club/Client/Presets/{preset.ResourcePath}";

                using (WebClient webClient = new())
                {
                    string tempFilePath = "temp";
                    webClient.DownloadFile(serverUrl, tempFilePath);
                    FileSystem.CopyFile(tempFilePath, SaveManager.GameSavePath + SaveManager.saveMap[SaveToImport]);
                    FileSystem.DeleteFile(tempFilePath);
                    LogSystem.Log("Save Data Imported from Server!");
                }
            }
            catch (Exception ex)
            {
                ConsoleSystem.AnimatedText("Error importing save data from server.");
                ConsoleSystem.AnimatedText("Press any key to continue...");
                LogSystem.ReportError("Error importing save data from server: " + ex);
                Console.ReadKey();
            }
        }
    }

    public class Preset
    {
        public string Name { get; }
        public string ResourcePath { get; }
        public Preset(string name, string serverName)
        {
            Name = name;
            ResourcePath = serverName;
        }
    }


}
