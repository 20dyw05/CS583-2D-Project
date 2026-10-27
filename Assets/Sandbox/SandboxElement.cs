using System.Collections.Generic;
using UnityEngine;

namespace ElementalSandbox
{
    // A physics Element: a Rigidbody2D ball that carries a value.
    public class SandboxElement : MonoBehaviour
    {
        public double Value = 1;

        // Catalysts this Element has already been through. Bouncing back into
        // the same zone must not apply it twice.
        readonly HashSet<SandboxCatalyst> used = new HashSet<SandboxCatalyst>();
        SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            Refresh();
        }

        // Returns true the first time this Catalyst is used on this Element.
        public bool TryUse(SandboxCatalyst catalyst) => used.Add(catalyst);

        // Colour shows value: pale blue when cheap, gold when valuable.
        public void Refresh()
        {
            float t = Mathf.Clamp01(Mathf.Log10((float)Value + 1f) / 2f);
            sr.color = Color.Lerp(new Color(0.45f, 0.75f, 1f), new Color(1f, 0.85f, 0.2f), t);
        }

        void Update()
        {
            // "Hit the floor": anything that falls off the bottom or sides dissolves.
            Vector3 p = transform.position;
            if (p.y < -7f || Mathf.Abs(p.x) > 13f)
            {
                Destroy(gameObject);
            }
        }
    }
}
