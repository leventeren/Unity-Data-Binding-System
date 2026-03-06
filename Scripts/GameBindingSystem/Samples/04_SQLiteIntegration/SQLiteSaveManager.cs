using System.Collections.Generic;
using UnityEngine;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Dirty flag + batch save manager for SQLite integration.
    /// Tracks which fields have changed and allows batch saving.
    /// 
    /// Replace SimulateLoad/SimulateSave with actual SQLite operations.
    /// </summary>
    public class SQLiteSaveManager
    {
        private readonly HashSet<string> _dirtyFields = new HashSet<string>();

        /// <summary>Whether any field has been marked as dirty.</summary>
        public bool IsDirty => _dirtyFields.Count > 0;

        /// <summary>Mark a field as dirty (changed but not yet saved).</summary>
        public void MarkDirty(string fieldName)
        {
            _dirtyFields.Add(fieldName);
            Debug.Log($"[SaveManager] Field '{fieldName}' marked dirty. Total dirty: {_dirtyFields.Count}");
        }

        /// <summary>Get all dirty field names.</summary>
        public HashSet<string> GetDirtyFields() => new HashSet<string>(_dirtyFields);

        /// <summary>Clear all dirty flags (call after successful save).</summary>
        public void ClearDirty()
        {
            _dirtyFields.Clear();
        }

        /// <summary>Check if a specific field is dirty.</summary>
        public bool IsFieldDirty(string fieldName) => _dirtyFields.Contains(fieldName);

        // =============================================
        // SIMULATED DATABASE OPERATIONS
        // Replace these with your actual SQLite code
        // =============================================

        private PlayerSimData _storedData = new PlayerSimData();

        /// <summary>
        /// Simulate loading data from SQLite.
        /// REPLACE WITH: var row = db.Table&lt;PlayerRow&gt;().First();
        /// </summary>
        public PlayerSimData SimulateLoad()
        {
            return new PlayerSimData
            {
                Name = _storedData.Name,
                Gold = _storedData.Gold,
                Level = _storedData.Level,
                Health = _storedData.Health
            };
        }

        /// <summary>
        /// Simulate saving data to SQLite.
        /// REPLACE WITH:
        ///   if (dirtyFields.Contains("Gold"))
        ///       db.Execute("UPDATE Player SET Gold = ?", data.Gold);
        ///   // ... etc for each dirty field
        /// 
        /// OR for simplicity:
        ///   db.Execute("UPDATE Player SET Name=?, Gold=?, Level=?, Health=?",
        ///       data.Name, data.Gold, data.Level, data.Health);
        /// </summary>
        public void SimulateSave(PlayerSimData data, HashSet<string> dirtyFields)
        {
            // Partial save: only update dirty fields
            if (dirtyFields.Contains("Name")) _storedData.Name = data.Name;
            if (dirtyFields.Contains("Gold")) _storedData.Gold = data.Gold;
            if (dirtyFields.Contains("Level")) _storedData.Level = data.Level;
            if (dirtyFields.Contains("Health")) _storedData.Health = data.Health;

            Debug.Log($"[SaveManager] Saved to DB: Name={_storedData.Name}, Gold={_storedData.Gold}, " +
                      $"Level={_storedData.Level}, Health={_storedData.Health}");
        }
    }
}
