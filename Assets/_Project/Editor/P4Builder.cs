// Pulau Monster · P4 Data Monster & Monster Liar (builder) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Menjalankan langkah P4 di atas hasil P3: 12 Mon_* dari MonsterDatabase.csv + MonsterDatabase, AC_Monster + 12 AOC,
// Pfb_WildMonster (8 liar + Raijugar + 1 di Danau Sunyi dari tugas kelompok), Partner, ScreenFader, Scn_Battle versi P4.
public static class P4Builder
{
    private const string Root = P1Builder.Root;
    private const string MonsterFolder = Root + "/Data/Monsters";
    private const string AnimFolder = Root + "/Animations/Monsters";
    public const string DatabasePath = MonsterFolder + "/MonsterDatabase.asset";
    public const string BattlePath = Root + "/Scenes/Scn_Battle.unity";
    public const string FontPath = Root + "/Art/Fonts/PixelifySans-Regular.ttf";
    private static readonly Vector3 BattleOrigin = new Vector3(-100f, -100f, 0f);

    private static readonly Dictionary<string, string> SkillNames = new Dictionary<string, string>
    {
        { "Pyro", "Semburan Api" }, { "Aqua", "Tembakan Air" }, { "Sprout", "Daun Tajam" }, { "Volt", "Setrum Kilat" },
    };

    [MenuItem("Pulau Monster/P4 Data Monster dan Monster Liar")]
    public static void Build()
    {
        P3Builder.Build();
        MakeSingleSprites(Root + "/Art/Backgrounds");
        MakeSingleSprites(Root + "/Art/UI");
        CreateMonsterData();
        CreateMonsterAnimators();
        GameObject wildPrefab = CreateWildPrefab();
        BuildWorld(wildPrefab);
        BuildBattleScene();
        BuildBootstrap();
        AddSandboxDebugger();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(P2Builder.BootstrapPath, true),
            new EditorBuildSettingsScene(P2Builder.WorldPath, true),
            new EditorBuildSettingsScene(BattlePath, true),
        };
        EditorSceneManager.OpenScene(P2Builder.WorldPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[P4Builder] selesai");
    }

    // Latar battle dan sprite UI: Sprite Mode Single (preset folder Art membuat Multiple tanpa potongan).
    public static void MakeSingleSprites(string folder)
    {
        foreach (string file in Directory.GetFiles(folder, "*.png"))
        {
            var importer = AssetImporter.GetAtPath(file.Replace('\\', '/')) as TextureImporter;
            if (importer == null || importer.spriteImportMode == SpriteImportMode.Single) continue;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SaveAndReimport();
        }
    }

    public static Sprite LoadSingle(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    private static void CreateMonsterData()
    {
        P1Builder.EnsureFolder(MonsterFolder);
        string[] lines = File.ReadAllLines("StarterAssetPack/Data/MonsterDatabase.csv");
        string csvTarget = Root + "/Data/MonsterDatabase.csv";
        if (!File.Exists(csvTarget)) File.Copy("StarterAssetPack/Data/MonsterDatabase.csv", csvTarget);

        var rows = new List<string[]>();
        var assets = new Dictionary<string, MonsterData>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] c = lines[i].Split(',');
            rows.Add(c);
            string path = MonsterFolder + "/Mon_" + c[1] + ".asset";
            var data = AssetDatabase.LoadAssetAtPath<MonsterData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<MonsterData>();
                AssetDatabase.CreateAsset(data, path);
            }
            assets[c[1]] = data;
        }

        var list = new List<MonsterData>();
        foreach (string[] c in rows)
        {
            string sheet = Path.GetFileNameWithoutExtension(c[11]);
            string sheetPath = Root + "/Art/Monsters/" + c[11];
            var frames = new Sprite[4];
            for (int f = 0; f < 4; f++) frames[f] = P1Builder.FindSprite(sheetPath, sheet + "_" + f);
            MonsterData evolvesTo = string.IsNullOrEmpty(c[8]) ? null : assets[c[8]];
            int evolveLevel = string.IsNullOrEmpty(c[9]) ? 0 : int.Parse(c[9]);
            MonsterData data = assets[c[1]];
            data.EditorSet(c[0], c[1], (Element)System.Enum.Parse(typeof(Element), c[2]), frames,
                int.Parse(c[4]), int.Parse(c[5]), int.Parse(c[6]), int.Parse(c[7]), SkillNames[c[2]], evolvesTo, evolveLevel);
            EditorUtility.SetDirty(data);
            list.Add(data);
        }

        var database = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(DatabasePath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<MonsterDatabase>();
            AssetDatabase.CreateAsset(database, DatabasePath);
        }
        database.EditorSet(list);
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
    }

    // Satu AC_Monster (state Idle), dua belas Animator Override Controller AOC_Mon_<Nama>.
    private static void CreateMonsterAnimators()
    {
        P1Builder.EnsureFolder(AnimFolder);
        var clips = new Dictionary<string, AnimationClip>();
        foreach (var m in P3Builder.Monsters)
        {
            string sheetPath = Root + "/Art/Monsters/" + m.sheet + ".png";
            clips[m.name] = P3Builder.SaveClip(AnimFolder + "/Anim_Mon_" + m.name + "_Idle.anim", 3f,
                P1Builder.FindSprite(sheetPath, m.sheet + "_0"), P1Builder.FindSprite(sheetPath, m.sheet + "_1"));
        }

        string controllerPath = AnimFolder + "/AC_Monster.controller";
        if (File.Exists(controllerPath)) AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorState idle = controller.layers[0].stateMachine.AddState("Idle");
        idle.motion = clips["Pyrit"];

        foreach (var m in P3Builder.Monsters)
        {
            string path = AnimFolder + "/AOC_Mon_" + m.name + ".overrideController";
            var aoc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            if (aoc == null)
            {
                aoc = new AnimatorOverrideController(controller);
                AssetDatabase.CreateAsset(aoc, path);
            }
            aoc.runtimeAnimatorController = controller;
            aoc.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new KeyValuePair<AnimationClip, AnimationClip>(clips["Pyrit"], clips[m.name]),
            });
            EditorUtility.SetDirty(aoc);
        }
        AssetDatabase.SaveAssets();
    }

    private static GameObject CreateWildPrefab()
    {
        P1Builder.EnsureFolder(Root + "/Prefabs/Monsters");
        var go = new GameObject("Pfb_WildMonster");
        go.layer = LayerMask.NameToLayer("Wild");
        var body = go.AddComponent<SpriteRenderer>();
        body.sortingLayerName = "Characters";
        body.spriteSortPoint = SpriteSortPoint.Pivot;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var hitbox = go.AddComponent<BoxCollider2D>();
        hitbox.size = new Vector2(0.7f, 0.4f);
        hitbox.offset = new Vector2(0f, 0.2f);
        go.AddComponent<Animator>();
        var alert = new GameObject("Alert");
        alert.transform.SetParent(go.transform, false);
        alert.transform.localPosition = new Vector3(0f, 1.3f, 0f);
        var alertRenderer = alert.AddComponent<SpriteRenderer>();
        alertRenderer.sprite = LoadSingle(Root + "/Art/UI/UI_Alert.png");
        alertRenderer.sortingLayerName = "Characters";
        alertRenderer.sortingOrder = 50;
        alert.SetActive(false);
        var wild = go.AddComponent<WildMonster>();
        var so = new SerializedObject(wild);
        so.FindProperty("rb").objectReferenceValue = rb;
        so.FindProperty("body").objectReferenceValue = body;
        so.FindProperty("hitbox").objectReferenceValue = hitbox;
        so.FindProperty("alert").objectReferenceValue = alert;
        so.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, Root + "/Prefabs/Monsters/Pfb_WildMonster.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void BuildWorld(GameObject wildPrefab)
    {
        var scene = EditorSceneManager.OpenScene(P2Builder.WorldPath);
        var database = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(DatabasePath);
        var parent = new GameObject("WildMonsters").transform;
        PlaceWild(wildPrefab, parent, database, "Sprig", 2, new Vector2(5.5f, 17.5f));
        PlaceWild(wildPrefab, parent, database, "Zapi", 3, new Vector2(9.5f, 17.5f));
        PlaceWild(wildPrefab, parent, database, "Blubi", 3, new Vector2(5.5f, 14.5f));
        PlaceWild(wildPrefab, parent, database, "Pyrit", 4, new Vector2(9.5f, 14.5f));
        PlaceWild(wildPrefab, parent, database, "Pyrago", 5, new Vector2(26.5f, 17.5f));
        PlaceWild(wildPrefab, parent, database, "Finnox", 6, new Vector2(31.5f, 17.5f));
        PlaceWild(wildPrefab, parent, database, "Voltcat", 6, new Vector2(26.5f, 14.5f));
        PlaceWild(wildPrefab, parent, database, "Bloomlet", 7, new Vector2(31.5f, 14.5f));
        PlaceWild(wildPrefab, parent, database, "Finnox", 5, new Vector2(17.5f, 29.5f), "Wild_Danau_Finnox");
        GameObject boss = PlaceWild(wildPrefab, parent, database, "Raijugar", 9, new Vector2(30.5f, 31.5f), "Boss_Raijugar");
        var bossSo = new SerializedObject(boss.GetComponent<WildMonster>());
        bossSo.FindProperty("isBoss").boolValue = true;
        bossSo.FindProperty("stationary").boolValue = true;
        bossSo.ApplyModifiedPropertiesWithoutUndo();
        boss.transform.Find("Alert").localPosition = new Vector3(0f, 2.2f, 0f);

        var partner = new GameObject("Partner");
        var partnerBody = partner.AddComponent<SpriteRenderer>();
        partnerBody.sortingLayerName = "Characters";
        partnerBody.spriteSortPoint = SpriteSortPoint.Pivot;
        var follower = partner.AddComponent<PartnerFollower>();
        P3Builder.SetField(follower, "body", partnerBody);

        var installer = Object.FindAnyObjectByType<WorldInstaller>();
        P3Builder.SetField(installer, "follower", follower);
        P3Builder.SetField(installer, "debugStarter", database.GetByName("Pyrit"));
        P3Builder.SetField(installer, "guard", GameObject.Find("Pfb_Npc_Guard").transform);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject PlaceWild(GameObject prefab, Transform parent, MonsterDatabase database, string name, int level,
        Vector2 position, string objectName = null)
    {
        MonsterData data = database.GetByName(name);
        var wild = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        wild.name = objectName ?? "Wild_" + name;
        wild.transform.position = position;
        wild.GetComponent<SpriteRenderer>().sprite = data.IdleFrame(0);
        wild.GetComponent<Animator>().runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(AnimFolder + "/AOC_Mon_" + name + ".overrideController");
        float scale = data.Frames[0].rect.width / 16f;
        wild.GetComponent<BoxCollider2D>().size = new Vector2(0.7f * scale, 0.4f);
        var so = new SerializedObject(wild.GetComponent<WildMonster>());
        so.FindProperty("species").objectReferenceValue = data;
        so.FindProperty("level").intValue = level;
        so.ApplyModifiedPropertiesWithoutUndo();
        return wild;
    }

    // Scn_Battle versi P4 di (-100, -100): kamera sendiri, latar BG_Battle_Grassland, musuh, partner, teks info.
    private static void BuildBattleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraGo = new GameObject("Battle Camera");
        var camera = cameraGo.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.625f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.depth = 10;
        cameraGo.transform.position = BattleOrigin + new Vector3(0f, 0f, -10f);

        var background = new GameObject("Background").AddComponent<SpriteRenderer>();
        background.sprite = LoadSingle(Root + "/Art/Backgrounds/BG_Battle_Grassland_320x180.png");
        background.sortingLayerName = "Ground";
        background.transform.position = BattleOrigin;

        SpriteRenderer enemy = MakeBattleSprite("Enemy", BattleOrigin + new Vector3(4.5f, 0.5f, 0f), 2.5f);
        SpriteRenderer partner = MakeBattleSprite("Partner", BattleOrigin + new Vector3(-4.5f, -3f, 0f), 2.5f);
        partner.flipX = true;

        var canvasGo = new GameObject("Canvas_Battle");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        var textGo = new GameObject("Info");
        textGo.transform.SetParent(canvasGo.transform, false);
        var text = textGo.AddComponent<Text>();
        text.font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        text.fontSize = 36;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.white;
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -24f);
        rect.sizeDelta = new Vector2(-48f, 120f);
        textGo.AddComponent<Shadow>();

        var installerGo = new GameObject("BattleInstaller");
        var installer = installerGo.AddComponent<BattleInstaller>();
        P3Builder.SetField(installer, "enemyBody", enemy);
        P3Builder.SetField(installer, "partnerBody", partner);
        P3Builder.SetField(installer, "info", text);
        EditorSceneManager.SaveScene(scene, BattlePath);
    }

    private static SpriteRenderer MakeBattleSprite(string name, Vector3 position, float scale)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        go.transform.localScale = new Vector3(scale, scale, 1f);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "Characters";
        return renderer;
    }

    // Scn_Bootstrap: GameBootstrap mendapat MonsterDatabase dan ScreenFader (Canvas order 100, Image hitam).
    private static void BuildBootstrap()
    {
        var scene = EditorSceneManager.OpenScene(P2Builder.BootstrapPath);
        var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
        var canvasGo = new GameObject("Canvas_Fader");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var blackGo = new GameObject("Black");
        blackGo.transform.SetParent(canvasGo.transform, false);
        var black = blackGo.AddComponent<Image>();
        black.color = new Color(0f, 0f, 0f, 0f);
        black.raycastTarget = false;
        black.rectTransform.anchorMin = Vector2.zero;
        black.rectTransform.anchorMax = Vector2.one;
        black.rectTransform.sizeDelta = Vector2.zero;
        var fader = canvasGo.AddComponent<ScreenFader>();
        P3Builder.SetField(fader, "black", black);
        P3Builder.SetField(bootstrap, "monsterDatabase", AssetDatabase.LoadAssetAtPath<MonsterDatabase>(DatabasePath));
        P3Builder.SetField(bootstrap, "fader", fader);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Praktikum 2: MonsterDebugger di scene sandbox (Species Mon_Pyrit, Level 4).
    private static void AddSandboxDebugger()
    {
        var scene = EditorSceneManager.OpenScene(P1Builder.ScenePath);
        if (Object.FindAnyObjectByType<MonsterDebugger>() == null)
        {
            var debuggerGo = new GameObject("MonsterDebugger");
            var debugger = debuggerGo.AddComponent<MonsterDebugger>();
            var so = new SerializedObject(debugger);
            so.FindProperty("species").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<MonsterDatabase>(DatabasePath).GetByName("Pyrit");
            so.FindProperty("level").intValue = 4;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
