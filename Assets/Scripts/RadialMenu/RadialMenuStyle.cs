using System;
using UnityEngine;

namespace MagicLock.Radial
{
    /// <summary>A value that is only used when <see cref="enabled"/> is set; otherwise the inherited value applies.</summary>
    [Serializable]
    public struct Optional<T>
    {
        public bool enabled;
        public T value;

        public Optional(T value)
        {
            enabled = true;
            this.value = value;
        }

        public T Or(T fallback) => enabled ? value : fallback;
    }

    /// <summary>
    /// Menu-wide look and feel. Each ring can override parts of it (<see cref="RadialRingOverrides"/>), and each
    /// element can override its button visuals (<see cref="RadialElementOverrides"/>).
    /// Override fields use the same names as the fields here.
    /// </summary>
    [Serializable]
    public class RadialMenuStyle
    {
        [Header("Layout")]
        [Tooltip("Radius of the first layer.")]
        public float radius = 100f;
        [Tooltip("Radius added for each layer further out.")]
        public float layerRadiusStep = 100f;
        [Tooltip("Angle in degrees where the selected element sits. 0 = right, 90 = top.")]
        public float selectedAngle = 90f;
        [Tooltip("Lay the following elements out clockwise from the selected one.")]
        public bool clockwise;

        [Header("Buttons")]
        public RadialButton buttonPrefab;
        [Tooltip("Background used by elements without their own Circle sprite. Empty = keep the button prefab's sprite.")]
        public Sprite defaultBackground;
        public float buttonScale = 1f;
        [Tooltip("Extra scale multiplier for the selected button.")]
        public float selectedScale = 1f;
        [Tooltip("Optional decoration spawned behind each ring and scaled to its radius.")]
        public GameObject lineCircle;

        [Header("Colors")]
        public Color selectedColor = new Color(1f, 1f, 1f, 0.75f);
        public Color unselectedColor = new Color(1f, 1f, 1f, 0.5f);
        public Color iconColor = Color.white;
        public Color selectedIconColor = Color.white;

        [Header("Animation")]
        [Tooltip("Seconds a turn takes.")]
        public float turnTime = 1f;
        [Tooltip("Speed multiplier applied when another turn comes in while one is still animating.")]
        public float rushMultiplier = 5f;
        [Tooltip("Seconds between buttons popping in.")]
        public float spawnStagger = 0.06f;
        [Tooltip("Pop-in speed; the animation lasts 1 / speed seconds.")]
        public float popInSpeed = 8f;

        public RadialMenuStyle Clone() => (RadialMenuStyle)MemberwiseClone();

        /// <summary>A copy of this style with a ring's overrides applied. The ring's radius is resolved separately.</summary>
        public RadialMenuStyle With(RadialRingOverrides o)
        {
            RadialMenuStyle s = Clone();
            if (o == null) return s;

            s.selectedAngle = o.selectedAngle.Or(s.selectedAngle);
            s.clockwise = o.clockwise.Or(s.clockwise);
            s.buttonPrefab = o.buttonPrefab.Or(s.buttonPrefab);
            s.defaultBackground = o.defaultBackground.Or(s.defaultBackground);
            s.buttonScale = o.buttonScale.Or(s.buttonScale);
            s.selectedScale = o.selectedScale.Or(s.selectedScale);
            s.lineCircle = o.lineCircle.Or(s.lineCircle);
            s.selectedColor = o.selectedColor.Or(s.selectedColor);
            s.unselectedColor = o.unselectedColor.Or(s.unselectedColor);
            s.iconColor = o.iconColor.Or(s.iconColor);
            s.selectedIconColor = o.selectedIconColor.Or(s.selectedIconColor);
            s.turnTime = o.turnTime.Or(s.turnTime);
            s.rushMultiplier = o.rushMultiplier.Or(s.rushMultiplier);
            s.spawnStagger = o.spawnStagger.Or(s.spawnStagger);
            s.popInSpeed = o.popInSpeed.Or(s.popInSpeed);
            return s;
        }

        /// <summary>Radius of the layer at <paramref name="depth"/>, honouring a ring radius override.</summary>
        public float RadiusFor(int depth, RadialRingOverrides o)
        {
            float computed = radius + layerRadiusStep * depth;
            return o != null ? o.radius.Or(computed) : computed;
        }
    }

    /// <summary>Per-ring overrides of <see cref="RadialMenuStyle"/>.</summary>
    [Serializable]
    public class RadialRingOverrides
    {
        [Tooltip("Absolute radius of this ring, instead of the menu radius plus the layer step.")]
        public Optional<float> radius;
        public Optional<float> selectedAngle;
        public Optional<bool> clockwise;
        public Optional<RadialButton> buttonPrefab;
        public Optional<Sprite> defaultBackground;
        public Optional<float> buttonScale;
        public Optional<float> selectedScale;
        public Optional<GameObject> lineCircle;
        public Optional<Color> selectedColor;
        public Optional<Color> unselectedColor;
        public Optional<Color> iconColor;
        public Optional<Color> selectedIconColor;
        public Optional<float> turnTime;
        public Optional<float> rushMultiplier;
        public Optional<float> spawnStagger;
        public Optional<float> popInSpeed;

        public RadialRingOverrides Clone() => (RadialRingOverrides)MemberwiseClone();
    }

    /// <summary>Per-element overrides of the (ring-resolved) <see cref="RadialMenuStyle"/>.</summary>
    [Serializable]
    public class RadialElementOverrides
    {
        public Optional<RadialButton> buttonPrefab;
        public Optional<float> buttonScale;
        public Optional<float> selectedScale;
        public Optional<Color> selectedColor;
        public Optional<Color> unselectedColor;
        public Optional<Color> iconColor;
        public Optional<Color> selectedIconColor;

        public RadialElementOverrides Clone() => (RadialElementOverrides)MemberwiseClone();
    }

    /// <summary>Final visuals of one button after menu, ring and element settings are combined.</summary>
    public struct RadialElementVisual
    {
        public RadialButton buttonPrefab;
        public Sprite background;
        public Sprite icon;
        public float scale;
        public float selectedScale;
        public Color selectedColor;
        public Color unselectedColor;
        public Color iconColor;
        public Color selectedIconColor;

        public static RadialElementVisual Resolve(RadialMenuStyle ringStyle, Sprite circle, Sprite icon,
            RadialElementOverrides o)
        {
            o ??= new RadialElementOverrides();
            return new RadialElementVisual
            {
                buttonPrefab = o.buttonPrefab.Or(ringStyle.buttonPrefab),
                background = circle ? circle : ringStyle.defaultBackground,
                icon = icon,
                scale = o.buttonScale.Or(ringStyle.buttonScale),
                selectedScale = o.selectedScale.Or(ringStyle.selectedScale),
                selectedColor = o.selectedColor.Or(ringStyle.selectedColor),
                unselectedColor = o.unselectedColor.Or(ringStyle.unselectedColor),
                iconColor = o.iconColor.Or(ringStyle.iconColor),
                selectedIconColor = o.selectedIconColor.Or(ringStyle.selectedIconColor),
            };
        }

        public static RadialElementVisual Resolve(RadialMenuStyle ringStyle, RadialElement element) =>
            Resolve(ringStyle, element.Circle, element.Icon, element.overrides);
    }
}
