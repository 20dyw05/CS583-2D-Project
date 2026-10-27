using UnityEngine;

namespace ElementalSandbox
{
    public enum CatalystKind { Multiply, AddUnder20 }

    // An invisible-ish trigger zone that changes Elements passing through it.
    public class SandboxCatalyst : MonoBehaviour
    {
        public CatalystKind Kind = CatalystKind.Multiply;
        public double Amount = 2;

        SpriteRenderer body;
        Color baseColor;
        float flash;

        void Start()
        {
            body = GetComponentInChildren<SpriteRenderer>();
            baseColor = body.color;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            SandboxElement e = other.GetComponent<SandboxElement>();
            if (e == null || !e.TryUse(this)) return;

            if (Kind == CatalystKind.Multiply)
            {
                e.Value *= Amount;
            }
            else if (e.Value < 20)
            {
                e.Value += Amount;
            }

            e.Refresh();
            flash = 1f;
        }

        void Update()
        {
            // Brief glow each time the Catalyst fires.
            if (body == null) return;
            flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 4f);
            Color c = baseColor;
            c.a = Mathf.Lerp(baseColor.a, 0.8f, flash);
            body.color = c;
        }
    }
}
