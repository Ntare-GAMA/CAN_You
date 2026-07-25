using System;
using System.IO;
using UnityEngine;
using VaultsOfTheElixir.Data;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Singleton. Reads and writes SaveData as JSON under
    /// Application.persistentDataPath, which works consistently across
    /// WebGL (IndexedDB-backed), PC, and mobile without extra code —
    /// this is why it was chosen over PlayerPrefs for anything beyond
    /// trivial settings (PlayerPrefs has no structured-array support and
    /// gets awkward fast for a level-status array + inventory list).
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SaveFileName = "vaultsave.json";
        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public SaveData CurrentSave { get; private set; }

        /// <summary>Whether a save file already exists on disk — used by the Main Menu to enable/disable "Continue Game".</summary>
        public bool HasSaveFile => File.Exists(SavePath);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        /// <summary>Writes CurrentSave to disk as JSON.</summary>
        public void Save()
        {
            CurrentSave.lastSavedUtc = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(CurrentSave, prettyPrint: true);

            try
            {
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to write save file: {e.Message}");
            }
        }

        /// <summary>Loads SaveData from disk, or creates a fresh default save if none exists yet.</summary>
        public void Load()
        {
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    CurrentSave = JsonUtility.FromJson<SaveData>(json);
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveManager] Save file corrupt, creating new save: {e.Message}");
                }
            }

            CurrentSave = CreateDefaultSave();
        }

        private SaveData CreateDefaultSave()
        {
            var data = new SaveData();
            data.levelStatus[0] = 1; // Vault 0 (Scorpion) available by default, rest locked (0)
            return data;
        }

        /// <summary>Wipes progress and starts fresh — used by a "New Game" confirmation.</summary>
        public void ResetSave()
        {
            CurrentSave = CreateDefaultSave();
            Save();
        }
    }
}
