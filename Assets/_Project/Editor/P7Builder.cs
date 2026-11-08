// Pulau Monster · P7 Polish, Build & Demo (builder) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// Menjalankan langkah P7 di atas hasil P6: cahaya gua (LightZone, PlayerLight, 5 kristal, BossLight), PixelBurst,
// GrassRustle, kilau Batu Simpan, BattleFeel + VfxPool tebasan, Scn_End DEMO SELESAI, dan pengaturan build.
public static class P7Builder
{
    private const string Root = P1Builder.Root;
    public const string EndPath = Root + "/Scenes/Scn_End.unity";
    private const string SlashPath = Root + "/Art/VFX/VFX_HitSlash_4f_32x32.png";
    private static readonly Color Cream = new Color32(255, 236, 196, 255);
    private static readonly Color CrystalYellow = new Color32(255, 240, 150, 255);
    private static readonly Color BossGold = new Color32(255, 198, 26, 255);

    [MenuItem("Pulau Monster/P7 Polish, Build dan Demo")]
    public static void Build()
    {
        P6Builder.Build();
        SetupVfx();
        GameObject slashPrefab = CreateSlashPrefab();
        BuildWorld();
        BuildBattle(slashPrefab);
        BuildEnd();
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(P2Builder.BootstrapPath, true),
            new EditorBuildSettingsScene(P6Builder.TitlePath, true),
            new EditorBuildSettingsScene(P2Builder.WorldPath, true),
            new EditorBuildSettingsScene(P4Builder.BattlePath, true),
            new EditorBuildSettingsScene(EndPath, true),
        };
        EditorSceneManager.OpenScene(P2Builder.WorldPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[P7Builder] selesai");
    }

    // VFX 4 frame dipotong 32 x 32, VFX tunggal Sprite Mode Single.
    private static void SetupVfx()
    {
        P4Builder.MakeSingleSprites(Root + "/Art/VFX");
        foreach (string file in Directory.GetFiles(Root + "/Art/VFX", "*.png"))
        {
            string path = file.Replace('\\', '/');
            if (!Path.GetFileName(path).Contains("_4f_")) continue;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.SaveAndReimport();
            P1Builder.SliceGrid(path, 32, 32, new Vector2(0.5f, 0.5f));
        }
    }

    private static GameObject CreateSlashPrefab()
    {
        P1Builder.EnsureFolder(Root + "/Prefabs/FX");
        var go = new GameObject("Pfb_HitSlash");
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "Characters";
        renderer.sortingOrder = 60;
        var vfx = go.AddComponent<SpriteVfx>();
        var frames = new Sprite[4];
        for (int i = 0; i < 4; i++) frames[i] = P1Builder.FindSprite(SlashPath, "VFX_HitSlash_4f_32x32_" + i);
        vfx.EditorSet(renderer, frames);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, Root + "/Prefabs/FX/Pfb_HitSlash.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static PixelBurst CreateBurst()
    {
        var burst = new GameObject("PixelBurst").AddComponent<PixelBurst>();
        P3Builder.SetField(burst, "pixel", P4Builder.LoadSingle(Root + "/Art/UI/UI_Pixel.png"));
        return burst;
    }

    private static Light2D PointLight(Transform parent, string name, Vector3 position, float inner, float outer, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        var light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.pointLightInnerRadius = inner;
        light.pointLightOuterRadius = outer;
        light.color = color;
        light.intensity = 0f; // di luar gua mati, LightZone yang menyalakan
        return light;
    }

    private static void BuildWorld()
    {
        var scene = EditorSceneManager.OpenScene(P2Builder.WorldPath);
        GameObject player = GameObject.Find("Player");
        Light2D global = null;
        foreach (Light2D l in Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None))
            if (l.lightType == Light2D.LightType.Global) global = l;

        Light2D playerLight = PointLight(player.transform, "PlayerLight", player.transform.position + new Vector3(0f, 0.5f, 0f), 1.2f, 3.6f, Cream);
        var caveLights = new GameObject("CaveLights").transform;
        var extra = new List<Object>();
        foreach (Vector2 crystal in new[] { new Vector2(26.5f, 31.5f), new Vector2(35.5f, 31.5f), new Vector2(27.5f, 27.5f), new Vector2(34.5f, 27.5f), new Vector2(30.5f, 32.5f) })
            extra.Add(PointLight(caveLights, $"Crystal_{(int)crystal.x}_{(int)crystal.y}", crystal, 0.3f, 1.9f, CrystalYellow));
        Transform boss = GameObject.Find("Boss_Raijugar").transform;
        extra.Add(PointLight(boss, "BossLight", boss.position + new Vector3(0f, 1f, 0f), 0.3f, 1.8f, BossGold));
        P2Builder.LightAllSortingLayers();

        GameObject caveArea = GameObject.Find("Area_Gua");
        var zone = caveArea.AddComponent<LightZone>();
        P3Builder.SetField(zone, "globalLight", global);
        P3Builder.SetField(zone, "playerLight", playerLight);
        UiBuild.SetArray(zone, "extraLights", extra);

        PixelBurst burst = CreateBurst();
        var rustle = player.AddComponent<GrassRustle>();
        P3Builder.SetField(rustle, "tallGrass", GameObject.Find("TM_TallGrass").GetComponent<Tilemap>());
        P3Builder.SetField(rustle, "movement", player.GetComponent<PlayerMovement>());
        P3Builder.SetField(rustle, "burst", burst);
        P3Builder.SetField(Object.FindAnyObjectByType<SaveStone>(), "burst", burst);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildBattle(GameObject slashPrefab)
    {
        var scene = EditorSceneManager.OpenScene(P4Builder.BattlePath);
        var installer = Object.FindAnyObjectByType<BattleInstaller>();
        PixelBurst burst = CreateBurst();
        var pool = new GameObject("VfxPool").AddComponent<VfxPool>();
        P3Builder.SetField(pool, "prefab", slashPrefab.GetComponent<SpriteVfx>());
        var feel = installer.gameObject.AddComponent<BattleFeel>();
        P3Builder.SetField(feel, "cameraRig", Object.FindAnyObjectByType<Camera>().transform);
        P3Builder.SetField(feel, "playerBody", GameObject.Find("PlayerMon").GetComponent<SpriteRenderer>());
        P3Builder.SetField(feel, "enemyBody", GameObject.Find("EnemyMon").GetComponent<SpriteRenderer>());
        P3Builder.SetField(feel, "hitVfx", pool);
        P3Builder.SetField(feel, "burst", burst);
        P3Builder.SetField(installer, "feel", feel);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Scn_End: DEMO SELESAI (emas), partner akhir + level, waktu bermain, Enter / klik kembali ke judul.
    private static void BuildEnd()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var installer = new GameObject("EndInstaller").AddComponent<EndInstaller>();
        var cameraGo = new GameObject("Main Camera");
        var camera = cameraGo.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = UiTheme.Ink;
        cameraGo.transform.position = new Vector3(0f, 0f, -10f);

        GameObject canvas = UiBuild.Canvas("Canvas_End", 0);
        UiBuild.FullScreen(canvas, "Background", UiTheme.Ink);
        UiBuild.Label(canvas, "Title", "DEMO SELESAI", 20, UiTheme.Gold, 0, 20, 320, 26, TextAnchor.UpperCenter, true);
        Image partner = UiBuild.Image(canvas, "Partner", null, 128, 50, 64, 64);
        partner.preserveAspect = true;
        Text partnerText = UiBuild.Label(canvas, "PartnerText", "", 9, Cream, 0, 118, 320, 12, TextAnchor.UpperCenter);
        Text timeText = UiBuild.Label(canvas, "Playtime", "", 8, UiTheme.Text, 0, 132, 320, 10, TextAnchor.UpperCenter);
        UiBuild.Label(canvas, "Hint", "Tekan Enter untuk kembali ke judul", 7, UiTheme.Muted, 0, 150, 320, 10, TextAnchor.UpperCenter);
        UiBuild.Label(canvas, "Identity", "Neza Insyirah Nasuha · 4352511002 · TP 3A Malam", 7, UiTheme.Muted, 0, 166, 320, 10, TextAnchor.UpperCenter);
        Image catcher = UiBuild.FullScreen(canvas, "ClickCatcher", new Color(0f, 0f, 0f, 0f));
        catcher.raycastTarget = true;
        Button button = catcher.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;

        P3Builder.SetField(installer, "partnerImage", partner);
        P3Builder.SetField(installer, "partnerText", partnerText);
        P3Builder.SetField(installer, "timeText", timeText);
        P3Builder.SetField(installer, "clickCatcher", button);
        EditorSceneManager.SaveScene(scene, EndPath);
    }
}
