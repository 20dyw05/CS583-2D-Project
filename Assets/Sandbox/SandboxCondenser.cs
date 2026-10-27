using UnityEngine;

namespace ElementalSandbox
{
    // Drops a new Element below itself on a timer.
    public class SandboxCondenser : MonoBehaviour
    {
        public float Interval = 0.6f;
        public bool Running = true;

        float timer;

        void Update()
        {
            if (!Running) return;

            timer += Time.deltaTime;
            while (timer >= Interval)
            {
                timer -= Interval;
                // A tiny random nudge so drops aren't perfectly identical.
                Vector3 spawn = transform.position + new Vector3(Random.Range(-0.04f, 0.04f), -0.6f, 0f);
                PhysicsSandbox.Instance.SpawnElement(spawn);
            }
        }
    }
}
