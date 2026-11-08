using System.Collections.Generic;
using UnityEngine;

namespace MagicLock.Radial
{
    /// <summary>
    /// Authoring data for one layer of a radial menu. Rings form a chain through <see cref="NextRing"/>;
    /// each ring in the chain becomes one layer at runtime. Runtime code only reads these assets.
    /// </summary>
    [CreateAssetMenu(fileName = "RadialRing", menuName = "RadialMenu/Ring", order = 1)]
    public class RadialRing : ScriptableObject
    {
        public List<RadialElement> Elements = new List<RadialElement>();
        public RadialRing NextRing;
        public string MenuName;

        [Tooltip("Per-ring changes to the menu style.")]
        public RadialRingOverrides overrides = new RadialRingOverrides();

        /// <summary>This ring followed by every ring reachable through <see cref="NextRing"/>. Stops at a cycle.</summary>
        public IEnumerable<RadialRing> Chain()
        {
            var visited = new HashSet<RadialRing>();
            for (RadialRing ring = this; ring; ring = ring.NextRing) {
                if (!visited.Add(ring)) {
                    Debug.LogWarning($"Radial ring chain loops back to '{ring.name}'. Ignoring the rest of the chain.", this);
                    yield break;
                }

                yield return ring;
            }
        }
    }
}
