using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ElementalSandbox.EditorTools
{
    // Adds  Tools > Elemental Sandbox > Open Physics Sandbox  to Unity's menu bar.
    public static class SandboxMenu
    {
        const string ScenePath = "Assets/Sandbox/PhysicsSandbox.unity";

        [MenuItem("Tools/Elemental Sandbox/Open Physics Sandbox")]
        static void OpenSandbox()
        {
            // Offer to save the scene you're leaving, so nothing is lost.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (System.IO.File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            new GameObject("PhysicsSandbox").AddComponent<PhysicsSandbox>();

            EditorSceneManager.SaveScene(scene, ScenePath);
        }
    }
}
