using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLock.Radial
{
    /// <summary>
    /// One ring of a radial menu. Spawns a <see cref="RadialButton"/> per element of its <see cref="RadialRing"/>
    /// and rotates them so the selected element sits at <see cref="RadialMenuStyle.selectedAngle"/>.
    /// Visuals come from <see cref="style"/>, overridden by the ring and then by each element.
    /// All state here is per instance; the ring and element assets are only read.
    /// </summary>
    public class RadialMenu : MonoBehaviour
    {
        [Tooltip("Ring shown by this menu. A RadialMenuController replaces it per layer through Bind().")]
        public RadialRing Data;

        [Tooltip("Menu-wide look and feel. Rings and elements can override parts of it.")]
        public RadialMenuStyle style = new RadialMenuStyle();

        /// <summary>Raised when turning changes the selected element.</summary>
        public event Action<RadialElement> SelectionChanged;

        private readonly List<RadialElement> _elements = new List<RadialElement>();
        private readonly List<RadialElementVisual> _visuals = new List<RadialElementVisual>();
        private readonly List<RadialButton> _buttons = new List<RadialButton>();
        private float[] _startAngles = Array.Empty<float>();
        private float[] _currentAngles = Array.Empty<float>();
        private float[] _targetAngles = Array.Empty<float>();
        private RadialMenuStyle _ringStyle;
        private GameObject _lineCircleInstance;
        private Coroutine _turnRoutine;
        private Coroutine _spawnRoutine;
        private float _turnTimer;
        private bool _rushing;
        private bool _built;
        private int _selected;
        private int _depth;

        public bool IsBuilt => _built;
        public int Depth => _depth;
        public int SelectedIndex => _selected;
        public RadialElement SelectedElement => _selected < _elements.Count ? _elements[_selected] : null;

        /// <summary>The elements that got a button, in display order.</summary>
        public IReadOnlyList<RadialElement> Elements => _elements;

        /// <summary><see cref="style"/> with this ring's overrides applied.</summary>
        public RadialMenuStyle EffectiveStyle => style.With(Data ? Data.overrides : null);

        public float Radius => style.RadiusFor(_depth, Data ? Data.overrides : null);

        /// <summary>
        /// Use <paramref name="ring"/> as layer <paramref name="depth"/> (0 = innermost). Clears existing buttons;
        /// they are rebuilt on the next Show().
        /// </summary>
        public void Bind(RadialRing ring, int depth = 0)
        {
            Clear();
            Data = ring;
            _depth = depth;
        }

        /// <summary>Activate the menu, building its buttons if needed.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            if (!_built) Build();
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>Destroy the spawned buttons and reset the selection. The ring data is kept.</summary>
        public void Clear()
        {
            StopRoutines();

            foreach (var button in _buttons) {
                if (button) Helper.DestroySafe(button.gameObject);
            }

            _buttons.Clear();
            _elements.Clear();
            _visuals.Clear();
            Helper.DestroySafe(_lineCircleInstance);
            _lineCircleInstance = null;
            _startAngles = _currentAngles = _targetAngles = Array.Empty<float>();
            _selected = 0;
            _built = false;
        }

        /// <summary>Rotate by <paramref name="steps"/> elements. Positive selects the next element.</summary>
        public void Turn(int steps)
        {
            int count = _elements.Count;
            if (!_built || count == 0 || steps == 0) return;

            int previous = _selected;
            _selected = ((_selected + steps) % count + count) % count;

            for (int i = 0; i < count; i++) {
                _startAngles[i] = _currentAngles[i];
                _targetAngles[i] = AngleFor(i);
            }

            RefreshVisuals();

            if (!isActiveAndEnabled) {
                SnapToTarget();
            }
            else {
                // A turn arriving mid-animation retargets from where the buttons are now and speeds up.
                _rushing = _turnRoutine != null;
                _turnTimer = 0f;
                if (_turnRoutine == null) _turnRoutine = StartCoroutine(TurnAnimation());
            }

            if (_selected != previous) SelectionChanged?.Invoke(SelectedElement);
        }

        private void Build()
        {
            Clear();
            _built = true;
            _ringStyle = EffectiveStyle;
            float radius = Radius;

            if (Data) {
                foreach (var element in Data.Elements) {
                    if (!element) continue;

                    RadialElementVisual visual = RadialElementVisual.Resolve(_ringStyle, element);
                    if (!visual.buttonPrefab) {
                        Debug.LogWarning($"{name}: no button prefab for element '{element.name}'. " +
                                         "Set one on the menu style, the ring or the element.", this);
                        continue;
                    }

                    _elements.Add(element);
                    _visuals.Add(visual);
                }
            }

            int count = _elements.Count;
            _startAngles = new float[count];
            _currentAngles = new float[count];
            _targetAngles = new float[count];

            if (_ringStyle.lineCircle) {
                _lineCircleInstance = Instantiate(_ringStyle.lineCircle, transform);
                _lineCircleInstance.transform.localScale = new Vector3(radius, radius) * 0.5f;
            }

            if (count == 0) return;

            float step = 360f / count;
            for (int i = 0; i < count; i++) {
                RadialElement element = _elements[i];
                RadialElementVisual visual = _visuals[i];
                RadialButton prefab = visual.buttonPrefab;

                RadialButton button = Instantiate(prefab, transform, false);
                button.name = $"Button:{element.name}";

                float angle = AngleFor(i);
                _startAngles[i] = _currentAngles[i] = _targetAngles[i] = angle;
                button.transform.localPosition = Helper.PolarToCart(angle, radius);

                if (button.Background && visual.background) button.Background.sprite = visual.background;
                if (button.Icon) {
                    button.Icon.sprite = visual.icon;
                    button.Icon.enabled = visual.icon;

                    if (button.Background) {
                        float iconDist = Vector3.Distance(prefab.Icon.transform.position,
                            prefab.Background.transform.position);
                        button.Icon.transform.localPosition = button.Background.transform.localPosition +
                                                              Quaternion.AngleAxis(i * step, Vector3.forward) *
                                                              Vector3.up * iconDist;
                    }
                }

                _buttons.Add(button);
            }

            RefreshVisuals();
            foreach (var button in _buttons) button.HideForPopIn();
            _spawnRoutine = StartCoroutine(PopIn());
        }

        private IEnumerator PopIn()
        {
            foreach (var button in _buttons) {
                button.Anim(_ringStyle.popInSpeed);
                if (_ringStyle.spawnStagger > 0f) yield return new WaitForSeconds(_ringStyle.spawnStagger);
            }

            _spawnRoutine = null;
        }

        private IEnumerator TurnAnimation()
        {
            float turnTime = _ringStyle.turnTime;
            while (_turnTimer < turnTime) {
                _turnTimer += Time.deltaTime * (_rushing ? _ringStyle.rushMultiplier : 1f);
                float t = Mathf.Clamp01(_turnTimer / turnTime);

                for (int i = 0; i < _currentAngles.Length; i++) {
                    _currentAngles[i] = Mathf.LerpAngle(_startAngles[i], _targetAngles[i], t);
                }

                ApplyAngles();
                yield return null;
            }

            _turnRoutine = null;
            SnapToTarget();
        }

        private float AngleFor(int index)
        {
            float step = 360f / _elements.Count * (index - _selected);
            return NormalizeAngle(_ringStyle.selectedAngle + (_ringStyle.clockwise ? -step : step));
        }

        private static float NormalizeAngle(float a) => (a % 360f + 360f) % 360f;

        private void SnapToTarget()
        {
            Array.Copy(_targetAngles, _currentAngles, _targetAngles.Length);
            ApplyAngles();
            _rushing = false;
        }

        private void ApplyAngles()
        {
            float radius = Radius;
            for (int i = 0; i < _buttons.Count; i++) {
                _buttons[i].transform.localPosition = Helper.PolarToCart(_currentAngles[i], radius);
            }
        }

        private void RefreshVisuals()
        {
            for (int i = 0; i < _buttons.Count; i++) {
                RadialButton button = _buttons[i];
                RadialElementVisual visual = _visuals[i];
                bool selected = i == _selected;

                if (button.Background) button.Background.color = selected ? visual.selectedColor : visual.unselectedColor;
                if (button.Icon) button.Icon.color = selected ? visual.selectedIconColor : visual.iconColor;
                button.SetScale(visual.scale * (selected ? visual.selectedScale : 1f));
            }
        }

        private void StopRoutines()
        {
            if (_turnRoutine != null) StopCoroutine(_turnRoutine);
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
            _turnRoutine = _spawnRoutine = null;
        }

        private void OnDisable()
        {
            // Coroutines die with the GameObject; finish their work so the menu is correct when shown again.
            bool wasSpawning = _spawnRoutine != null;
            StopRoutines();
            if (wasSpawning) {
                foreach (var button in _buttons) {
                    if (button) button.Anim(0f);
                }
            }

            SnapToTarget();
        }
    }
}
