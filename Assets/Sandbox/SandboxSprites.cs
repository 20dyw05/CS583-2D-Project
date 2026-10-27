using UnityEngine;

namespace ElementalSandbox
{
    // Builds simple placeholder sprites in code, so the sandbox needs no image files.
    public static class SandboxSprites
    {
        static Sprite square, circle, chevron;

        // A 1 x 1 unit white square.
        public static Sprite Square
        {
            get
            {
                if (square == null)
                {
                    Texture2D tex = MakeTexture(4, (x, y, n) => 1f);
                    tex.filterMode = FilterMode.Point;
                    square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
                }
                return square;
            }
        }

        // A white circle, 1 unit across, with a soft edge.
        public static Sprite Circle
        {
            get
            {
                if (circle == null)
                {
                    Texture2D tex = MakeTexture(64, (x, y, n) =>
                    {
                        float r = n / 2f;
                        float dx = x + 0.5f - r, dy = y + 0.5f - r;
                        return Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                    });
                    circle = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
                }
                return circle;
            }
        }

        // A white triangle pointing right, 1 unit across. Flip it to point left.
        public static Sprite Chevron
        {
            get
            {
                if (chevron == null)
                {
                    Texture2D tex = MakeTexture(32, (x, y, n) =>
                    {
                        float v = Mathf.Abs((y + 0.5f) / n * 2f - 1f); // 0 in the middle, 1 at top/bottom
                        return (x + 0.5f) / n <= 1f - v ? 1f : 0f;
                    });
                    chevron = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
                }
                return chevron;
            }
        }

        static Texture2D MakeTexture(int n, System.Func<int, int, int, float> alpha)
        {
            Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    pixels[y * n + x] = new Color(1f, 1f, 1f, alpha(x, y, n));
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
