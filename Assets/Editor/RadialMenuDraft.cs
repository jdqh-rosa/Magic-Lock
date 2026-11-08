using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MagicLock.Radial.Editor
{
    /// <summary>
    /// In-memory working copy edited by <see cref="RadialMenuCreator"/>. Nothing touches project assets until the
    /// creator saves; <c>source</c> fields remember which existing assets a save should update.
    /// </summary>
    public class RadialMenuDraft : ScriptableObject
    {
        public string menuName = "NewRadialMenu";
        [Tooltip("Parent folder for a new menu. The menu gets its own subfolder named after it.")]
        public string folder = "Assets/RadialMenus";
        public GameObject sourcePrefab;
        public RadialMenuStyle style = new RadialMenuStyle();
        public List<LayerDraft> layers = new List<LayerDraft>();

        public static RadialMenuDraft CreateNew()
        {
            var draft = CreateInstance<RadialMenuDraft>();
            draft.hideFlags = HideFlags.DontSave;
            draft.style.buttonPrefab = FindDefaultButtonPrefab();

            var layer = new LayerDraft { assetName = "Layer1" };
            for (int i = 1; i <= 4; i++) {
                layer.elements.Add(new ElementDraft { assetName = $"Element{i}", displayName = $"Element {i}" });
            }

            draft.layers.Add(layer);
            return draft;
        }

        public static RadialMenuDraft FromPrefab(GameObject prefab, RadialMenu menu)
        {
            var draft = CreateInstance<RadialMenuDraft>();
            draft.hideFlags = HideFlags.DontSave;
            draft.sourcePrefab = prefab;
            draft.menuName = prefab.name;

            string prefabDir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefab))?.Replace('\\', '/');
            draft.folder = Path.GetDirectoryName(prefabDir)?.Replace('\\', '/') ?? "Assets";
            draft.style = menu.style != null ? menu.style.Clone() : new RadialMenuStyle();

            if (menu.Data) {
                foreach (RadialRing ring in menu.Data.Chain()) {
                    var layer = new LayerDraft
                    {
                        source = ring,
                        assetName = ring.name,
                        menuName = ring.MenuName,
                        overrides = ring.overrides != null ? ring.overrides.Clone() : new RadialRingOverrides(),
                    };

                    foreach (RadialElement element in ring.Elements.Where(e => e)) {
                        layer.elements.Add(new ElementDraft
                        {
                            source = element,
                            assetName = element.name,
                            displayName = element.Name,
                            circle = element.Circle,
                            icon = element.Icon,
                            overrides = element.overrides != null
                                ? element.overrides.Clone()
                                : new RadialElementOverrides(),
                        });
                    }

                    draft.layers.Add(layer);
                }
            }

            return draft;
        }

        /// <summary>Forget the source assets so the next save creates a new, independent menu.</summary>
        public void Detach()
        {
            sourcePrefab = null;
            menuName += " Copy";
            foreach (LayerDraft layer in layers) {
                layer.source = null;
                foreach (ElementDraft element in layer.elements) element.source = null;
            }
        }

        private static RadialButton FindDefaultButtonPrefab()
        {
            IEnumerable<string> guids = AssetDatabase.FindAssets("RadialButton t:Prefab")
                .Concat(AssetDatabase.FindAssets("t:Prefab"));
            foreach (string guid in guids) {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (go && go.TryGetComponent(out RadialButton button)) return button;
            }

            return null;
        }
    }

    [Serializable]
    public class LayerDraft
    {
        public string assetName = "Layer";
        public string menuName;
        public RadialRing source;
        public RadialRingOverrides overrides = new RadialRingOverrides();
        public List<ElementDraft> elements = new List<ElementDraft>();

        public LayerDraft CloneAsNew() => new LayerDraft
        {
            assetName = assetName + " Copy",
            menuName = menuName,
            overrides = overrides.Clone(),
            elements = elements.Select(e => e.CloneAsNew()).ToList(),
        };
    }

    [Serializable]
    public class ElementDraft
    {
        public string assetName = "Element";
        [Tooltip("Name reported in the selected path. Empty = the asset name.")]
        public string displayName;
        [Tooltip("Background sprite. Empty = the style's default background, then the button prefab's sprite.")]
        public Sprite circle;
        public Sprite icon;
        public RadialElement source;
        public RadialElementOverrides overrides = new RadialElementOverrides();

        public ElementDraft CloneAsNew() => new ElementDraft
        {
            assetName = assetName + " Copy",
            displayName = displayName,
            circle = circle,
            icon = icon,
            overrides = overrides.Clone(),
        };
    }
}
