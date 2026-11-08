using UnityEngine;

namespace MagicLock.Radial
{
    /// <summary>One selectable entry on a <see cref="RadialRing"/>.</summary>
    [CreateAssetMenu(fileName = "RadialElement", menuName = "RadialMenu/Element", order = 2)]
    public class RadialElement : ScriptableObject
    {
        public string Name;
        public Sprite Circle;
        public Sprite Icon;

        [Tooltip("Per-element changes to the menu and ring style.")]
        public RadialElementOverrides overrides = new RadialElementOverrides();

        /// <summary><see cref="Name"/>, or the asset name when it is empty.</summary>
        public string DisplayName => string.IsNullOrEmpty(Name) ? name : Name;
    }
}
