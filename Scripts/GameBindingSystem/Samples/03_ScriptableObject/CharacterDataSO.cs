using UnityEngine;

namespace BindingSystem.Samples
{
    /// <summary>
    /// ScriptableObject containing Bindable character data.
    /// Can be shared across multiple UI views and scene objects.
    /// 
    /// CREATE: Right-click in Project → Create → BindingSystem → CharacterData
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "BindingSystem/CharacterData")]
    public class CharacterDataSO : ScriptableObject
    {
        [Header("Identity")]
        public Bindable<string> CharacterName = new("Hero");
        public Bindable<Sprite> Portrait;

        [Header("Stats")]
        public Bindable<int> Level = new(1);
        public Bindable<float> Health = new(100f);
        public Bindable<float> MaxHealth = new(100f);
        public Bindable<float> Mana = new(50f);
        public Bindable<float> MaxMana = new(50f);
        public Bindable<int> Gold = new(0);

        [Header("Status")]
        public Bindable<bool> IsAlive = new(true);
    }
}
