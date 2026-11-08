using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MagicLock.Radial
{
    public class RadialButton : MonoBehaviour
    {
        public Image Icon;
        public Image Background;

        private Vector3 _targetScale = Vector3.one;
        private Coroutine _popIn;

        /// <summary>Scale the button rests at. Applied immediately unless the pop-in animation is running.</summary>
        public void SetScale(float scale)
        {
            _targetScale = Vector3.one * scale;
            if (_popIn == null) transform.localScale = _targetScale;
        }

        /// <summary>Hide the button until <see cref="Anim"/> pops it in.</summary>
        public void HideForPopIn() => transform.localScale = Vector3.zero;

        /// <summary>Grow from zero to the target scale over 1 / <paramref name="speed"/> seconds.</summary>
        public void Anim(float speed = 8f)
        {
            if (_popIn != null) StopCoroutine(_popIn);
            _popIn = null;

            if (isActiveAndEnabled && speed > 0f) _popIn = StartCoroutine(AnimateButtonIn(speed));
            else transform.localScale = _targetScale;
        }

        private IEnumerator AnimateButtonIn(float speed)
        {
            float t = 0f;
            while (t < 1f) {
                t = Mathf.Min(1f, t + Time.deltaTime * speed);
                transform.localScale = _targetScale * t;
                yield return null;
            }

            _popIn = null;
        }

        private void OnDisable()
        {
            // A coroutine stopped mid-animation would otherwise leave the button shrunk.
            _popIn = null;
            transform.localScale = _targetScale;
        }
    }
}
