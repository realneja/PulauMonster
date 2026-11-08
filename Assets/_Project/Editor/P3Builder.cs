// Pulau Monster · P3 Animasi, Kamera & Interaksi (builder) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menjalankan langkah P3 di atas hasil P2: 8 clip + AC_Player (Blend Tree), Animator di Player, layer,
// CM_Follow + Confiner 2D, PlayerInteractor, Pfb_Npc + 4 varian, 4 AreaZone, dan tugas individu
// (AOC_Npc_Elder, clip idle 4 monster tahap 1, animasi jalan 1,5 kali saat lari).
public static class P3Builder
{
    private const string Root = P1Builder.Root;
    private const string CharArt = Root + "/Art/Characters/";
    private const string MonArt = Root + "/Art/Monsters/";
    private const string AnimFolder = Root + "/Animations";
    public const string PlayerController = AnimFolder + "/Player/AC_Player.controller";

    private static readonly string[] Directions = { "Down", "Left", "Right", "Up" };
    private static readonly Vector2[] DirectionVectors = { new Vector2(0, -1), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1) };

    public static readonly (string sheet, string name, int frame)[] Monsters =
    {
        ("Aqua_1_Blubi_16x16", "Blubi", 16), ("Aqua_2_Finnox_24x24", "Finnox", 24), ("Aqua_3_Tidalord_32x32", "Tidalord", 32),
        ("Pyro_1_Pyrit_16x16", "Pyrit", 16), ("Pyro_2_Pyrago_24x24", "Pyrago", 24), ("Pyro_3_Infernox_32x32", "Infernox", 32),
        ("Sprout_1_Sprig_16x16", "Sprig", 16), ("Sprout_2_Bloomlet_24x24", "Bloomlet", 24), ("Sprout_3_Florazor_32x32", "Florazor", 32),
        ("Volt_1_Zapi_16x16", "Zapi", 16), ("Volt_2_Voltcat_24x24", "Voltcat", 24), ("Volt_3_Raijugar_32x32", "Raijugar", 32),
    };

    [MenuItem("Pulau Monster/P3b Animasi, Kamera dan Interaksi")]
    public static void Build()
    {
        P2Builder.Build();
        SetupLayers();
        foreach (var monster in Monsters)
            P1Builder.SliceGrid(MonArt + monster.sheet + ".png", monster.frame, monster.frame, new Vector2(0.5f, 0f));

        Dictionary<string, AnimationClip> playerClips = CreateCharacterClips("Player", "Player_Walk_16x16");
        AnimatorController controller = CreatePlayerController(playerClips);
        Dictionary<string, AnimationClip> elderClips = CreateCharacterClips("Npc_Elder", "NPC_Elder_Walk_16x16");
        CreateOverride(controller, playerClips, elderClips, AnimFolder + "/Npc/AOC_Npc_Elder.overrideController");
        foreach (string monster in new[] { "Blubi", "Pyrit", "Sprig", "Zapi" }) CreateMonsterIdleClip(monster);

        var scene = EditorSceneManager.OpenScene(P2Builder.WorldPath);
        GameObject player = GameObject.Find("Player");
        SetupPlayer(player, AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerController));
        SetupCamera(player.transform);
        GameObject npcBase = CreateNpcPrefab();
        PlaceNpc(npcBase, "Elder", "Tetua Rahmat", new Vector2(25.5f, 7f));
        PlaceNpc(npcBase, "Girl", "Sari", new Vector2(7f, 7f));
        PlaceNpc(npcBase, "Guard", "Penjaga Batas", new Vector2(20f, 11.4f));
        PlaceNpc(npcBase, "Merchant", "Pak Dagang", new Vector2(13f, 4f));
        CreateAreas();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[P3Builder] selesai: " + P2Builder.WorldPath);
    }

    private static void SetupLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        string[] names = { "Player", "Interactable", "Wild", "CameraBounds" };
        for (int i = 0; i < names.Length; i++)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(6 + i);
            if (string.IsNullOrEmpty(layer.stringValue)) layer.stringValue = names[i];
        }
        tagManager.ApplyModifiedPropertiesWithoutUndo();

        int bounds = LayerMask.NameToLayer("CameraBounds");
        for (int i = 0; i < 32; i++) Physics2D.IgnoreLayerCollision(bounds, i, true);
    }

    private static Dictionary<string, AnimationClip> CreateCharacterClips(string prefix, string sheet)
    {
        P1Builder.EnsureFolder(AnimFolder + "/" + (prefix == "Player" ? "Player" : "Npc"));
        string folder = AnimFolder + "/" + (prefix == "Player" ? "Player" : "Npc");
        string sheetPath = CharArt + sheet + ".png";
        var clips = new Dictionary<string, AnimationClip>();
        for (int d = 0; d < 4; d++)
        {
            int baseIndex = d * 3;
            Sprite Frame(int offset) => P1Builder.FindSprite(sheetPath, sheet + "_" + (baseIndex + offset));
            clips["Idle_" + Directions[d]] = SaveClip(folder + "/Anim_" + prefix + "_Idle_" + Directions[d] + ".anim", 8f, Frame(0));
            clips["Walk_" + Directions[d]] = SaveClip(folder + "/Anim_" + prefix + "_Walk_" + Directions[d] + ".anim", 8f,
                Frame(0), Frame(1), Frame(0), Frame(2));
        }
        return clips;
    }

    private static void CreateMonsterIdleClip(string name)
    {
        P1Builder.EnsureFolder(AnimFolder + "/Monsters");
        foreach (var monster in Monsters)
        {
            if (monster.name != name) continue;
            string sheetPath = MonArt + monster.sheet + ".png";
            SaveClip(AnimFolder + "/Monsters/Anim_Mon_" + name + "_Idle.anim", 3f,
                P1Builder.FindSprite(sheetPath, monster.sheet + "_0"), P1Builder.FindSprite(sheetPath, monster.sheet + "_1"));
        }
    }

    // Clip sprite dengan Samples tertentu dan Loop Time aktif. Kunci terakhir mengulang frame terakhir agar durasinya pas.
    public static AnimationClip SaveClip(string path, float samples, params Sprite[] frames)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        clip.frameRate = samples;
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / samples, value = frames[i] };
        keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / samples, value = frames[frames.Length - 1] };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    // AC_Player: Idle dan Walk, masing-masing Blend Tree 2D Simple Directional; transisi IsMoving tanpa exit time.
    private static AnimatorController CreatePlayerController(Dictionary<string, AnimationClip> clips)
    {
        if (File.Exists(PlayerController)) AssetDatabase.DeleteAsset(PlayerController);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(PlayerController);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter(new AnimatorControllerParameter { name = "Speed", type = AnimatorControllerParameterType.Float, defaultFloat = 1f });

        AnimatorState idle = controller.CreateBlendTreeInController("Idle", out BlendTree idleTree, 0);
        AnimatorState walk = controller.CreateBlendTreeInController("Walk", out BlendTree walkTree, 0);
        foreach (var (tree, prefix) in new[] { (idleTree, "Idle_"), (walkTree, "Walk_") })
        {
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveY";
            for (int d = 0; d < 4; d++) tree.AddChild(clips[prefix + Directions[d]], DirectionVectors[d]);
        }
        walk.speedParameterActive = true;
        walk.speedParameter = "Speed";
        controller.layers[0].stateMachine.defaultState = idle;

        AnimatorStateTransition toWalk = idle.AddTransition(walk);
        toWalk.hasExitTime = false;
        toWalk.duration = 0f;
        toWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");
        AnimatorStateTransition toIdle = walk.AddTransition(idle);
        toIdle.hasExitTime = false;
        toIdle.duration = 0f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void CreateOverride(AnimatorController controller, Dictionary<string, AnimationClip> source,
        Dictionary<string, AnimationClip> replacement, string path)
    {
        var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
        if (overrideController == null)
        {
            overrideController = new AnimatorOverrideController(controller);
            AssetDatabase.CreateAsset(overrideController, path);
        }
        overrideController.runtimeAnimatorController = controller;
        var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var entry in source) pairs.Add(new KeyValuePair<AnimationClip, AnimationClip>(entry.Value, replacement[entry.Key]));
        overrideController.ApplyOverrides(pairs);
        EditorUtility.SetDirty(overrideController);
    }

    private static void SetupPlayer(GameObject player, AnimatorController controller)
    {
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        var movement = player.GetComponent<PlayerMovement>();
        var animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        var playerAnimator = player.AddComponent<PlayerAnimator>();
        SetField(playerAnimator, "movement", movement);
        var interactor = player.AddComponent<PlayerInteractor>();
        SetField(interactor, "movement", movement);
        var serialized = new SerializedObject(interactor);
        serialized.FindProperty("interactableMask").intValue = LayerMask.GetMask("Interactable");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        SetField(Object.FindAnyObjectByType<WorldInstaller>(), "interactor", interactor);
    }

    // Main Camera + Cinemachine Brain, CM_Follow (Position Composer, Pixel Perfect, Confiner 2D ke CameraBounds).
    private static void SetupCamera(Transform player)
    {
        Camera main = Camera.main;
        if (main.GetComponent<CinemachineBrain>() == null) main.gameObject.AddComponent<CinemachineBrain>();

        var bounds = new GameObject("CameraBounds");
        bounds.layer = LayerMask.NameToLayer("CameraBounds");
        var polygon = bounds.AddComponent<PolygonCollider2D>();
        polygon.isTrigger = true;
        polygon.points = new[] { new Vector2(0, 0), new Vector2(40, 0), new Vector2(40, 36), new Vector2(0, 36) };

        var follow = new GameObject("CM_Follow");
        follow.transform.position = player.position + new Vector3(0, 0, -10f);
        var cmCamera = follow.AddComponent<CinemachineCamera>();
        cmCamera.Target.TrackingTarget = player;
        LensSettings lens = cmCamera.Lens;
        lens.OrthographicSize = 5.625f;
        cmCamera.Lens = lens;
        var composer = follow.AddComponent<CinemachinePositionComposer>();
        composer.CameraDistance = 10f;
        composer.Damping = new Vector3(0.5f, 0.5f, 0f);
        follow.AddComponent<CinemachinePixelPerfect>();
        var confiner = follow.AddComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = polygon;
        confiner.InvalidateBoundingShapeCache();
    }

    // Pfb_Npc: layer Interactable, Sorting Layer Characters, Box Collider 2D 0,8 x 0,5 di kaki.
    private static GameObject CreateNpcPrefab()
    {
        P1Builder.EnsureFolder(Root + "/Prefabs/NPC");
        var npc = new GameObject("Pfb_Npc");
        npc.layer = LayerMask.NameToLayer("Interactable");
        var renderer = npc.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "Characters";
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        renderer.sprite = P1Builder.FindSprite(CharArt + "NPC_Elder_Walk_16x16.png", "NPC_Elder_Walk_16x16_0");
        var collider = npc.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.5f);
        collider.offset = new Vector2(0f, 0.25f);
        var component = npc.AddComponent<Npc>();
        SetField(component, "body", renderer);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(npc, Root + "/Prefabs/NPC/Pfb_Npc.prefab");
        Object.DestroyImmediate(npc);
        return prefab;
    }

    // Varian prefab dari Pfb_Npc: nama + sprite hadap _0/_3/_6/_9, lalu diletakkan di peta.
    private static void PlaceNpc(GameObject basePrefab, string kind, string npcName, Vector2 position)
    {
        string sheet = "NPC_" + kind + "_Walk_16x16";
        string sheetPath = CharArt + sheet + ".png";
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        var npc = instance.GetComponent<Npc>();
        var serialized = new SerializedObject(npc);
        serialized.FindProperty("npcName").stringValue = npcName;
        serialized.FindProperty("faceDown").objectReferenceValue = P1Builder.FindSprite(sheetPath, sheet + "_0");
        serialized.FindProperty("faceLeft").objectReferenceValue = P1Builder.FindSprite(sheetPath, sheet + "_3");
        serialized.FindProperty("faceRight").objectReferenceValue = P1Builder.FindSprite(sheetPath, sheet + "_6");
        serialized.FindProperty("faceUp").objectReferenceValue = P1Builder.FindSprite(sheetPath, sheet + "_9");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        instance.GetComponent<SpriteRenderer>().sprite = P1Builder.FindSprite(sheetPath, sheet + "_0");
        GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, Root + "/Prefabs/NPC/Pfb_Npc_" + kind + ".prefab");
        Object.DestroyImmediate(instance);

        var placed = (GameObject)PrefabUtility.InstantiatePrefab(variant);
        placed.name = "Pfb_Npc_" + kind;
        placed.transform.position = position;
    }

    private static void CreateAreas()
    {
        var parent = new GameObject("Areas").transform;
        CreateArea(parent, "Area_Desa", "Desa Awal", new Vector2(20f, 6f), new Vector2(40f, 12f));
        CreateArea(parent, "Area_Hutan", "Hutan Rimbun", new Vector2(20f, 18f), new Vector2(40f, 12f));
        CreateArea(parent, "Area_Danau", "Danau Sunyi", new Vector2(11.5f, 30f), new Vector2(23f, 12f));
        CreateArea(parent, "Area_Gua", "Gua Petir", new Vector2(31.5f, 30f), new Vector2(17f, 12f));
    }

    private static void CreateArea(Transform parent, string objectName, string areaName, Vector2 center, Vector2 size)
    {
        var area = new GameObject(objectName);
        area.transform.SetParent(parent);
        area.transform.position = center;
        var box = area.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = size;
        var zone = area.AddComponent<AreaZone>();
        var serialized = new SerializedObject(zone);
        serialized.FindProperty("areaName").stringValue = areaName;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetField(Object target, string field, Object value)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null) return;
        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
