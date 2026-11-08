// Pulau Monster · P2 Arsitektur & World Tilemap · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEditor;
using UnityEditor.SceneManagement;

// Scene apa pun yang terbuka, Play selalu mulai dari Scn_Bootstrap.
[InitializeOnLoad]
public static class PlayFromBootstrap
{
    const string BootstrapPath = "Assets/_Project/Scenes/Scn_Bootstrap.unity";

    static PlayFromBootstrap()
    {
        EditorApplication.delayCall += Apply;
    }

    static void Apply()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapPath);
    }
}
