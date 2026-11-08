using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MagicLock.Radial
{
    /// <summary>
    /// Drives a layered radial menu. Each ring in the chain starting at <see cref="RootRing"/> becomes one layer,
    /// instantiated from <see cref="menuPrefab"/>; the template's style sets radius and visuals. Confirming on the last layer raises
    /// <see cref="PathSelected"/> / <see cref="onPathSelected"/> with one element per layer.
    /// Input is not handled here: call the public methods from whatever input source you use.
    /// </summary>
    public class RadialMenuController : MonoBehaviour
    {
        [Serializable]
        public class PathEvent : UnityEvent<string> { }

        [Tooltip("Template instantiated once per layer.")]
        [FormerlySerializedAs("RadialMenuPrefab")]
        public RadialMenu menuPrefab;

        [Tooltip("First ring of the menu; its NextRing chain makes the other layers. Empty = the template's Data.")]
        public RadialRing rootRing;

        [Tooltip("Parent for the layers. Empty = this GameObject.")]
        [FormerlySerializedAs("MenuCanvas")]
        public Canvas menuCanvas;

        public bool openOnStart;

        [Tooltip("Close the menu after a full path is selected. Otherwise it returns to the first layer.")]
        public bool closeOnSelect;

        [Tooltip("Raised when the last layer is confirmed, with the element names joined by '/'.")]
        public PathEvent onPathSelected = new PathEvent();

        /// <summary>Raised when the last layer is confirmed, with the selected element of every layer.</summary>
        public event Action<IReadOnlyList<RadialElement>> PathSelected;

        /// <summary>Raised when turning changes the highlighted element: (layer index, element).</summary>
        public event Action<int, RadialElement> SelectionChanged;

        public event Action Opened;
        public event Action Closed;

        private readonly List<RadialMenu> _layers = new List<RadialMenu>();
        private readonly List<RadialElement> _path = new List<RadialElement>();
        private int _index;

        public bool IsOpen { get; private set; }
        public int LayerIndex => _index;
        public IReadOnlyList<RadialMenu> Layers => _layers;
        public RadialMenu CurrentLayer => _index < _layers.Count ? _layers[_index] : null;

        /// <summary>Elements confirmed so far on the layers before the current one.</summary>
        public IReadOnlyList<RadialElement> CurrentPath => _path;

        public RadialRing RootRing => rootRing ? rootRing : menuPrefab ? menuPrefab.Data : null;

        private void Start()
        {
            if (openOnStart) Open();
        }

        private void OnDestroy() => DestroyLayers();

        public void Open()
        {
            if (_layers.Count == 0) BuildLayers();
            if (_layers.Count == 0) return;

            ResetToFirstLayer();
            if (IsOpen) return;
            IsOpen = true;
            Opened?.Invoke();
        }

        public void Close()
        {
            foreach (var layer in _layers) {
                layer.Clear();
                layer.Hide();
            }

            _path.Clear();
            _index = 0;
            if (!IsOpen) return;
            IsOpen = false;
            Closed?.Invoke();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        /// <summary>Recreate the layers, e.g. after the ring chain changed at runtime.</summary>
        public void Rebuild()
        {
            bool wasOpen = IsOpen;
            DestroyLayers();
            IsOpen = false;
            if (wasOpen) Open();
        }

        public void TurnLeft() => Turn(1);
        public void TurnRight() => Turn(-1);

        public void Turn(int steps)
        {
            if (IsOpen) CurrentLayer.Turn(steps);
        }

        /// <summary>Accept the highlighted element: open the next layer, or finish the path on the last layer.</summary>
        public void Confirm()
        {
            if (!IsOpen) return;

            RadialElement picked = CurrentLayer.SelectedElement;
            if (!picked) return;
            _path.Add(picked);

            if (_index + 1 < _layers.Count) {
                _index++;
                _layers[_index].Show();
                return;
            }

            RadialElement[] result = _path.ToArray();
            if (closeOnSelect) Close();
            else ResetToFirstLayer();

            PathSelected?.Invoke(result);
            onPathSelected.Invoke(JoinNames(result));
        }

        /// <summary>Hide the current layer and return to the previous one.</summary>
        public void Back()
        {
            if (!IsOpen || _index == 0) return;

            _layers[_index].Hide();
            _index--;
            _path.RemoveAt(_path.Count - 1);
        }

        public static string JoinNames(IReadOnlyList<RadialElement> path)
        {
            var names = new string[path.Count];
            for (int i = 0; i < path.Count; i++) names[i] = path[i].DisplayName;
            return string.Join("/", names);
        }

        private void BuildLayers()
        {
            RadialRing root = RootRing;
            if (!menuPrefab || !root) {
                Debug.LogWarning($"{name}: assign a menu prefab and a root ring before opening the radial menu.", this);
                return;
            }

            Transform parent = menuCanvas ? menuCanvas.transform : transform;
            int depth = 0;
            foreach (RadialRing ring in root.Chain()) {
                RadialMenu layer = Instantiate(menuPrefab, parent);
                layer.name = $"{menuPrefab.name} [{depth}] {ring.name}";
                layer.gameObject.SetActive(false);
                layer.Bind(ring, depth);

                int layerIndex = depth;
                layer.SelectionChanged += element => SelectionChanged?.Invoke(layerIndex, element);

                _layers.Add(layer);
                depth++;
            }
        }

        private void ResetToFirstLayer()
        {
            foreach (var layer in _layers) {
                layer.Clear();
                layer.Hide();
            }

            _path.Clear();
            _index = 0;
            _layers[0].Show();
        }

        private void DestroyLayers()
        {
            foreach (var layer in _layers) {
                if (layer) Helper.DestroySafe(layer.gameObject);
            }

            _layers.Clear();
            _path.Clear();
            _index = 0;
        }
    }
}
