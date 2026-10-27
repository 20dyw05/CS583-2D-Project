using UnityEngine;

namespace ElementalSandbox
{
    // A trigger zone that turns any Element entering it into mana.
    public class SandboxTransmutator : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            SandboxElement e = other.GetComponent<SandboxElement>();
            if (e == null) return;

            PhysicsSandbox.Instance.AddMana(e.Value);
            other.enabled = false; // make sure it can't be counted twice this frame
            Destroy(e.gameObject);
        }
    }
}
