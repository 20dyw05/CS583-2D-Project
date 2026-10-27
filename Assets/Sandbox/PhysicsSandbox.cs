using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ElementalSandbox
{
    // Exploratory prototype: physics-based Elements on conveyors, side view.
    // Builds the whole level in code when Play starts. Nothing here depends on
    // the main game's scripts, and the main game doesn't depend on this.
    public class PhysicsSandbox : MonoBehaviour
    {
        [Header("Conveyors")]
        [SerializeField] float conveyorSpeed = 2.5f;
        [Tooltip("How hard conveyors push, 0 to 1. Low = slippery, high = grippy.")]
        [SerializeField, Range(0f, 1f)] float conveyorForceScale = 0.4f;

        [Header("Elements")]
        [SerializeField] float dropInterval = 0.6f;
        [SerializeField] float elementRadius = 0.18f;
        [SerializeField] float gravityScale = 1.5f;
        [SerializeField, Range(0f, 1f)] float elementFriction = 0.4f;
        [SerializeField, Range(0f, 1f)] float elementBounciness = 0.15f;
        [SerializeField] bool elementsCollide = true;

        public static PhysicsSandbox Instance { get; private set; }

        public double Mana { get; private set; }

        // The prototype borrows Unity's built-in "Water" layer (4) for Elements,
        // so it can switch Element-to-Element collisions on and off.
        const int ElementLayer = 4;

        Camera cam;
        Material spriteMaterial;
        PhysicsMaterial2D platformMaterial, elementMaterial;
        Transform layoutRoot, elementsRoot;
        SandboxCondenser condenser;

        readonly HashSet<Transform> draggables = new HashSet<Transform>();
        readonly Dictionary<SurfaceEffector2D, int> conveyors = new Dictionary<SurfaceEffector2D, int>();
        readonly Dictionary<Transform, string> labels = new Dictionary<Transform, string>();

        Transform dragging;
        Vector3 dragOffset;
        string lastSale = "";
        float lastSaleTimer;
        GUIStyle hudStyle, labelStyle;

        // ---------------------------------------------------------------- setup

        void Awake()
        {
            Instance = this;
            SetUpCamera();
            SetUpMaterials();

            layoutRoot = new GameObject("Layout").transform;
            elementsRoot = new GameObject("Elements").transform;

            BuildLayout();
            ApplyElementCollisions();
        }

        void OnDestroy()
        {
            // Put the borrowed layer back to normal when Play stops.
            Physics2D.IgnoreLayerCollision(ElementLayer, ElementLayer, false);
            if (Instance == this) Instance = null;
        }

        void SetUpCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.11f, 0.13f, 0.19f);
        }

        void SetUpMaterials()
        {
            // Unlit, so the sandbox doesn't need any 2D lights.
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null) spriteMaterial = new Material(shader);

            platformMaterial = new PhysicsMaterial2D("Platform") { friction = 0.6f, bounciness = 0f };
            elementMaterial = new PhysicsMaterial2D("Element") { friction = elementFriction, bounciness = elementBounciness };
        }

        // The level. Every block can be dragged around while playing.
        void BuildLayout()
        {
            Color purple = new Color(0.6f, 0.2f, 0.95f);
            Color gold = new Color(1f, 0.7f, 0.1f);
            Color teal = new Color(0.2f, 0.9f, 0.8f);
            Color pink = new Color(1f, 0.4f, 0.7f);
            Color stone = new Color(0.32f, 0.34f, 0.4f);

            // Condenser, top left. It's a trigger so Elements don't bounce off it.
            GameObject cond = MakeBlock("Condenser", new Vector2(-7f, 5.4f), new Vector2(1f, 0.7f), purple, 3);
            cond.GetComponent<BoxCollider2D>().isTrigger = true;
            condenser = cond.AddComponent<SandboxCondenser>();
            condenser.Interval = dropInterval;
            labels[cond.transform] = "Condenser";

            // Floor 3: runs right.
            MakeConveyor("Conveyor A", new Vector2(-3.5f, 3.2f), 9f, +1);
            labels[MakeCatalyst("x2 Catalyst", new Vector2(-5f, 3.9f), new Vector2(0.7f, 1.1f), teal,
                CatalystKind.Multiply, 2).transform] = "x2 (once)";
            labels[MakeHump("Hump", new Vector2(-2.5f, 3.2f), 0.3f, stone).transform] = "Hump";

            // Floor 2: runs left. Elements fall onto it from the end of floor 3.
            MakeConveyor("Conveyor B", new Vector2(2.5f, 0.8f), 10f, -1);
            labels[MakeBlock("Low ceiling", new Vector2(0.5f, 1.8f), new Vector2(0.4f, 0.6f), stone).transform] = "Low ceiling";
            labels[MakeCatalyst("+3 Catalyst", new Vector2(-1.2f, 1.45f), new Vector2(0.7f, 1f), pink,
                CatalystKind.AddUnder20, 3).transform] = "+3 (under 20)";

            // Floor 1: runs right, off the end into the Transmutator.
            MakeConveyor("Conveyor C", new Vector2(-0.5f, -1.8f), 12f, +1);

            GameObject trans = MakeBlock("Transmutator", new Vector2(7f, -2.9f), new Vector2(2f, 1.6f),
                new Color(gold.r, gold.g, gold.b, 0.55f), 2);
            trans.GetComponent<BoxCollider2D>().isTrigger = true;
            trans.AddComponent<SandboxTransmutator>();
            labels[trans.transform] = "Transmutator";

            MakeBlock("Back wall", new Vector2(8.3f, -2.6f), new Vector2(0.3f, 2.4f), stone);
        }

        // ------------------------------------------------------------- builders

        // A solid block: an unscaled root with the collider, and a scaled child
        // with the sprite, so children like arrows don't get stretched.
        GameObject MakeBlock(string name, Vector2 center, Vector2 size, Color color, int order = 0)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(layoutRoot, false);
            root.transform.position = center;

            BoxCollider2D col = root.AddComponent<BoxCollider2D>();
            col.size = size;
            col.sharedMaterial = platformMaterial;

            AddSprite(root.transform, SandboxSprites.Square, size, color, order);
            draggables.Add(root.transform);
            return root;
        }

        GameObject MakeHump(string name, Vector2 center, float radius, Color color)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(layoutRoot, false);
            root.transform.position = center;

            CircleCollider2D col = root.AddComponent<CircleCollider2D>();
            col.radius = radius;
            col.sharedMaterial = platformMaterial;

            AddSprite(root.transform, SandboxSprites.Circle, Vector2.one * radius * 2f, color, 1);
            draggables.Add(root.transform);
            return root;
        }

        void MakeConveyor(string name, Vector2 center, float width, int dir)
        {
            GameObject root = MakeBlock(name, center, new Vector2(width, 0.25f), new Color(0.42f, 0.45f, 0.52f));
            root.GetComponent<BoxCollider2D>().usedByEffector = true;

            // Unity's built-in conveyor belt: pushes whatever touches the surface.
            SurfaceEffector2D effector = root.AddComponent<SurfaceEffector2D>();
            effector.speed = conveyorSpeed * dir;
            effector.forceScale = conveyorForceScale;
            conveyors[effector] = dir;

            // Arrows showing which way it runs.
            int count = Mathf.Max(1, Mathf.FloorToInt(width / 1.2f));
            for (int i = 0; i < count; i++)
            {
                SpriteRenderer arrow = AddSprite(root.transform, SandboxSprites.Chevron, Vector2.one * 0.18f,
                    new Color(0.2f, 0.22f, 0.27f), 1);
                arrow.transform.localPosition = new Vector3(-width / 2f + (i + 0.5f) * width / count, 0f, 0f);
                arrow.flipX = dir < 0;
            }
        }

        GameObject MakeCatalyst(string name, Vector2 center, Vector2 size, Color color, CatalystKind kind, double amount)
        {
            GameObject root = MakeBlock(name, center, size, new Color(color.r, color.g, color.b, 0.3f), 2);
            root.GetComponent<BoxCollider2D>().isTrigger = true;

            SandboxCatalyst catalyst = root.AddComponent<SandboxCatalyst>();
            catalyst.Kind = kind;
            catalyst.Amount = amount;
            return root;
        }

        SpriteRenderer AddSprite(Transform parent, Sprite sprite, Vector2 size, Color color, int order)
        {
            GameObject go = new GameObject("Sprite");
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (spriteMaterial != null) sr.sharedMaterial = spriteMaterial;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        // -------------------------------------------------------- game actions

        public void SpawnElement(Vector3 position)
        {
            GameObject go = new GameObject("Element");
            go.layer = ElementLayer;
            go.transform.SetParent(elementsRoot, false);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * elementRadius * 2f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SandboxSprites.Circle;
            if (spriteMaterial != null) sr.sharedMaterial = spriteMaterial;
            sr.sortingOrder = 5;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f; // local units; the scale makes it elementRadius in the world
            col.sharedMaterial = elementMaterial;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = gravityScale;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            go.AddComponent<SandboxElement>();
        }

        public void AddMana(double amount)
        {
            Mana += amount;
            lastSale = "+" + amount.ToString("0.##");
            lastSaleTimer = 1f;
        }

        void ApplyElementCollisions()
        {
            Physics2D.IgnoreLayerCollision(ElementLayer, ElementLayer, !elementsCollide);
        }

        void SetConveyorSpeed(float speed)
        {
            conveyorSpeed = Mathf.Clamp(speed, 0f, 8f);
            foreach (KeyValuePair<SurfaceEffector2D, int> pair in conveyors)
            {
                pair.Key.speed = conveyorSpeed * pair.Value;
            }
        }

        void ResetSandbox()
        {
            Mana = 0;
            lastSale = "";
            for (int i = elementsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(elementsRoot.GetChild(i).gameObject);
            }
        }

        // ---------------------------------------------------------------- input

#if ENABLE_INPUT_SYSTEM
        static bool Pressed(Key k) => Keyboard.current != null && Keyboard.current[k].wasPressedThisFrame;
        static Vector2 MouseScreen => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        static bool LeftPressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        static bool LeftHeld => Mouse.current != null && Mouse.current.leftButton.isPressed;
        static bool RightPressed => Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        const Key KUp = Key.UpArrow, KDown = Key.DownArrow, KLeft = Key.LeftArrow, KRight = Key.RightArrow,
                  KSpace = Key.Space, KC = Key.C, KR = Key.R;
#else
        static bool Pressed(KeyCode k) => Input.GetKeyDown(k);
        static Vector2 MouseScreen => Input.mousePosition;
        static bool LeftPressed => Input.GetMouseButtonDown(0);
        static bool LeftHeld => Input.GetMouseButton(0);
        static bool RightPressed => Input.GetMouseButtonDown(1);
        const KeyCode KUp = KeyCode.UpArrow, KDown = KeyCode.DownArrow, KLeft = KeyCode.LeftArrow,
                      KRight = KeyCode.RightArrow, KSpace = KeyCode.Space, KC = KeyCode.C, KR = KeyCode.R;
#endif

        void Update()
        {
            Vector2 screen = MouseScreen;
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
            world.z = 0f;

            // Drag blocks around with the left mouse button.
            if (LeftPressed)
            {
                Collider2D hit = Physics2D.OverlapPoint(world, ~(1 << ElementLayer));
                if (hit != null && draggables.Contains(hit.transform))
                {
                    dragging = hit.transform;
                    dragOffset = dragging.position - world;
                }
            }
            if (dragging != null)
            {
                if (LeftHeld) dragging.position = world + dragOffset;
                else dragging = null;
            }

            // Drop an Element by hand.
            if (RightPressed) SpawnElement(world);

            if (Pressed(KUp)) SetConveyorSpeed(conveyorSpeed + 0.5f);
            if (Pressed(KDown)) SetConveyorSpeed(conveyorSpeed - 0.5f);
            if (Pressed(KLeft)) condenser.Interval = Mathf.Max(0.1f, condenser.Interval - 0.1f);
            if (Pressed(KRight)) condenser.Interval = Mathf.Min(3f, condenser.Interval + 0.1f);
            if (Pressed(KSpace)) condenser.Running = !condenser.Running;
            if (Pressed(KC))
            {
                elementsCollide = !elementsCollide;
                ApplyElementCollisions();
            }
            if (Pressed(KR)) ResetSandbox();

            lastSaleTimer = Mathf.Max(0f, lastSaleTimer - Time.deltaTime);
        }

        // ------------------------------------------------------------------ HUD

        void OnGUI()
        {
            if (hudStyle == null)
            {
                hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, richText = true };
                hudStyle.normal.textColor = Color.white;
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
                labelStyle.normal.textColor = new Color(1f, 1f, 1f, 0.75f);
            }

            string sale = lastSaleTimer > 0f ? "   <color=#FFD54A>" + lastSale + "</color>" : "";
            string hud =
                "<b>Mana: " + Mana.ToString("0.##") + "</b>" + sale + "\n" +
                "Conveyor speed: " + conveyorSpeed.ToString("0.0") + "  (Up / Down)\n" +
                "Drop every " + condenser.Interval.ToString("0.0") + " s  (Left / Right)   Dropping: " +
                (condenser.Running ? "on" : "off") + "  (Space)\n" +
                "Element collisions: " + (elementsCollide ? "on" : "off") + "  (C)   Elements: " + elementsRoot.childCount + "\n" +
                "<color=#AAB4C8>Left-drag: move any block   Right-click: drop an Element   R: reset</color>";

            // Bottom-left corner, out of the way of the level.
            float top = Screen.height - 140f;
            GUI.Box(new Rect(10, top, 620, 130), GUIContent.none);
            GUI.Label(new Rect(20, top + 5f, 600, 125), hud, hudStyle);

            // Names floating above each labelled block.
            foreach (KeyValuePair<Transform, string> pair in labels)
            {
                Vector3 sp = cam.WorldToScreenPoint(pair.Key.position + Vector3.up * 0.85f);
                GUI.Label(new Rect(sp.x - 90f, Screen.height - sp.y - 12f, 180f, 24f), pair.Value, labelStyle);
            }
        }
    }
}
