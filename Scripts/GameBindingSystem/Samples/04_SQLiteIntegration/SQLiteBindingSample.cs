using System;
using System.Collections.Generic;
using UnityEngine;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 04: SQLite Integration — Save Manager with Dirty Flag + Batch Save
    /// 
    /// This demonstrates the PATTERN for integrating Bindables with SQLite (or any persistence layer).
    /// It does NOT depend on any specific SQLite library — replace the SimulatedDB calls
    /// with your actual SQLite implementation.
    /// 
    /// KEY CONCEPTS:
    /// - Dirty flag: marks data as changed without immediately saving
    /// - Batch save: periodically saves all dirty data in one operation
    /// - Thread safety: all Bindable.Value assignments happen on main thread
    /// 
    /// SETUP:
    /// 1. Attach this script to a GameObject
    /// 2. Optionally assign UI references
    /// 3. Play → modify values → see "dirty" flag in console → auto-save occurs
    /// </summary>
    public class SQLiteBindingSample : MonoBehaviour
    {
        [Header("Reactive Data (synced with DB)")]
        public Bindable<string> PlayerName = new("Player");
        public Bindable<int> Gold = new(0);
        public Bindable<int> Level = new(1);
        public Bindable<float> Health = new(100f);

        [Header("Save Settings")]
        [SerializeField] private float _autoSaveInterval = 5f;

        private SQLiteSaveManager _saveManager;

        void Start()
        {
            _saveManager = new SQLiteSaveManager();

            // Load from "database"
            LoadFromDatabase();

            // Register dirty tracking — when any value changes, mark as dirty
            PlayerName.OnChanged += () => _saveManager.MarkDirty("PlayerName");
            Gold.OnChanged += () => _saveManager.MarkDirty("Gold");
            Level.OnChanged += () => _saveManager.MarkDirty("Level");
            Health.OnChanged += () => _saveManager.MarkDirty("Health");

            // Auto-save at interval
            this.BindInterval(_autoSaveInterval, () =>
            {
                if (_saveManager.IsDirty)
                {
                    SaveToDatabase();
                }
            });
        }

        public void LoadFromDatabase()
        {
            // Simulate loading from SQLite
            // Replace with: var row = db.Table<PlayerRow>().First();
            var data = _saveManager.SimulateLoad();

            PlayerName.Value = data.Name;
            Gold.Value = data.Gold;
            Level.Value = data.Level;
            Health.Value = data.Health;

            _saveManager.ClearDirty();
            Debug.Log("[SQLiteSample] Loaded from database");
        }

        public void SaveToDatabase()
        {
            var dirtyFields = _saveManager.GetDirtyFields();

            // Simulate saving to SQLite
            // Replace with: db.Execute("UPDATE Player SET ...", values);
            _saveManager.SimulateSave(new PlayerSimData
            {
                Name = PlayerName.Value,
                Gold = Gold.Value,
                Level = Level.Value,
                Health = Health.Value
            }, dirtyFields);

            _saveManager.ClearDirty();
            Debug.Log($"[SQLiteSample] Saved {dirtyFields.Count} dirty fields to database");
        }

        // --- Test Methods ---
        [ContextMenu("Add Gold +100")]
        public void AddGold() => Gold.Value += 100;

        [ContextMenu("Level Up")]
        public void LevelUp() => Level.Value++;

        [ContextMenu("Take Damage -20")]
        public void TakeDamage() => Health.Value = Mathf.Max(0, Health.Value - 20f);

        [ContextMenu("Force Save")]
        public void ForceSave() => SaveToDatabase();

        [ContextMenu("Force Load")]
        public void ForceLoad() => LoadFromDatabase();

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _saveManager.IsDirty)
                SaveToDatabase();
        }

        void OnApplicationQuit()
        {
            if (_saveManager.IsDirty)
                SaveToDatabase();
        }
    }

    /// <summary>
    /// Simulated player data structure (replace with your SQLite table class).
    /// </summary>
    [Serializable]
    public class PlayerSimData
    {
        public string Name = "Player";
        public int Gold = 0;
        public int Level = 1;
        public float Health = 100f;
    }
}
