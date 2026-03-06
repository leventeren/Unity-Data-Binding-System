using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 09: List Filtering & Search
    /// Demonstrates how to take a master BindableList, bind a Search query to it,
    /// and display only the filtered results dynamically as the user types.
    /// </summary>
    public class ListFilteringSample : MonoBehaviour
    {
        [Serializable]
        public class Employee
        {
            public string Name;
            public string Department;
            public int ID;
        }

        [Header("Data")]
        public Bindable<string> SearchQuery = new("");
        public BindableList<Employee> AllEmployees = new();

        [Header("UI References")]
        [SerializeField] private TMP_InputField _searchInput;
        [SerializeField] private Transform _contentParent;
        [SerializeField] private GameObject _employeePrefab; // Needs 1 TMP_Text for Name, 1 for Dept
        [SerializeField] private TMP_Text _resultCountText;

        void Start()
        {
            // Populate dummy data
            AllEmployees.Add(new Employee { ID = 101, Name = "Alice Smith", Department = "Engineering" });
            AllEmployees.Add(new Employee { ID = 102, Name = "Bob Jones", Department = "Marketing" });
            AllEmployees.Add(new Employee { ID = 103, Name = "Charlie Brown", Department = "Engineering" });
            AllEmployees.Add(new Employee { ID = 104, Name = "Diana Prince", Department = "HR" });
            AllEmployees.Add(new Employee { ID = 105, Name = "Evan Wright", Department = "Sales" });
            AllEmployees.Add(new Employee { ID = 106, Name = "Fiona Gallagher", Department = "Marketing" });

            // 1. Two-way bind the search input to our query string
            if (_searchInput != null)
                _searchInput.BindTextTwoWay(SearchQuery);

            // 2. React to SearchQuery OR List Changes to regenerate the UI
            // We can derive a filtered list snapshot whenever the search query or master list changes.
            Action RefreshList = () => 
            {
                ClearUI();
                
                string query = SearchQuery.Value.ToLowerInvariant();
                
                // Filter the list based on query
                var filtered = AllEmployees.Where(emp => 
                    string.IsNullOrEmpty(query) || 
                    emp.Name.ToLowerInvariant().Contains(query) || 
                    emp.Department.ToLowerInvariant().Contains(query)
                ).ToList();

                // Update UI count
                if (_resultCountText != null)
                    _resultCountText.text = $"Found {filtered.Count} employees";

                // Spawn UI items
                foreach (var emp in filtered)
                {
                    SpawnEmployeeUI(emp);
                }
            };

            // Bind to triggers
            SearchQuery.OnChanged += RefreshList;
            AllEmployees.OnChanged += RefreshList;

            // Initial build
            RefreshList();
        }

        private void SpawnEmployeeUI(Employee emp)
        {
            if (_employeePrefab == null || _contentParent == null) return;

            var go = Instantiate(_employeePrefab, _contentParent);
            var texts = go.GetComponentsInChildren<TMP_Text>();
            
            if (texts.Length > 0) texts[0].text = emp.Name;
            if (texts.Length > 1) texts[1].text = emp.Department;
        }

        private void ClearUI()
        {
            if (_contentParent == null) return;
            for (int i = _contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_contentParent.GetChild(i).gameObject);
            }
        }

        // Test Method
        [ContextMenu("Add Random Developer")]
        public void AddRandomDev()
        {
            string[] names = { "George", "Hannah", "Ian", "Julia" };
            AllEmployees.Add(new Employee { 
                ID = UnityEngine.Random.Range(200, 999), 
                Name = names[UnityEngine.Random.Range(0, names.Length)] + " Tech", 
                Department = "Engineering" 
            });
        }
    }
}
