// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.IO;
using UnityEngine;

// Serialisasi JSON ke persistentDataPath/save_0.json (Windows: AppData/LocalLow/Politeknik Negeri Batam/Pulau Monster).
public class SaveService
{
    public const int CurrentVersion = 1;

    // Diisi TitleInstaller saat Lanjutkan; WorldInstaller memakai posisinya lalu mengosongkannya.
    public SaveData PendingLoad { get; set; }

    // Tantangan tugas kelompok: dua slot, save_0.json dan save_1.json. Slot dipilih di layar judul.
    public int Slot { get; set; }

    public string PathOf(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public string FilePath => PathOf(Slot);

    public bool HasSave() => File.Exists(FilePath);

    public bool HasSave(int slot) => File.Exists(PathOf(slot));

    public bool Save(SaveData data)
    {
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"SaveService: gagal menyimpan ({e.Message})");
            return false;
        }
    }

    public SaveData Load()
    {
        try
        {
            if (!HasSave()) return null;
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            return data != null && data.version == CurrentVersion ? data : null;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"SaveService: gagal membaca ({e.Message})");
            return null;
        }
    }

    public void Delete()
    {
        if (HasSave()) File.Delete(FilePath);
    }
}
