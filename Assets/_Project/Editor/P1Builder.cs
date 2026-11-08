// Pulau Monster · P1 Setup Proyek & Gerak Karakter (builder) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Menjalankan langkah P1 lewat menu Pulau Monster: Project Settings (Praktikum 2), struktur folder,
// preset pixel art + sprite terpotong (Praktikum 3), scene sandbox Scn_Neza + Player + Wall (Praktikum 3 dan 6)
// dan tugas individu (4 tembok batas, 3 rintangan, 1 NPC diam).
public static class P1Builder
{
    public const string Root = "Assets/_Project";
    public const string ScenePath = Root + "/Scenes/Sandbox/Scn_Neza.unity";
    private const string PackFolder = "StarterAssetPack";
    private const string PresetPath = Root + "/Settings/Presets/Pre_PixelSprite.preset";
    private const string SquarePath = "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Square.png";

    private static readonly string[] ArtFolders = { "Characters", "Monsters", "Tiles", "UI", "VFX", "Backgrounds", "Fonts" };
    private static readonly string[] CharacterSheets =
    {
        "Player_Walk_16x16", "NPC_Elder_Walk_16x16", "NPC_Girl_Walk_16x16", "NPC_Guard_Walk_16x16", "NPC_Merchant_Walk_16x16",
    };

    private static readonly Color WallColor = new Color(0.55f, 0.37f, 0.22f);
    private static readonly Color GroundColor = new Color(0.35f, 0.62f, 0.33f);

    [MenuItem("Pulau Monster/P1 Setup Proyek dan Sandbox")]
    public static void Build()
    {
        SetupProjectSettings();
        CreateFolders();
        ImportArt();
        BuildSandboxScene();
        Debug.Log("[P1Builder] selesai: " + ScenePath);
    }

    private static void SetupProjectSettings()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        VersionControlSettings.mode = "Visible Meta Files";
        PlayerSettings.companyName = "Politeknik Negeri Batam";
        PlayerSettings.productName = "Pulau Monster";
        PlayerSettings.runInBackground = true;
    }

    private static void CreateFolders()
    {
        foreach (string art in ArtFolders) EnsureFolder(Root + "/Art/" + art);
        EnsureFolder(Root + "/Audio");
        EnsureFolder(Root + "/Data");
        EnsureFolder(Root + "/Prefabs");
        EnsureFolder(Root + "/Scenes/Sandbox");
        EnsureFolder(Root + "/Scripts/Player");
        EnsureFolder(Root + "/Settings/Presets");
    }

    private static void ImportArt()
    {
        string playerPath = Root + "/Art/Characters/Player_Walk_16x16.png";
        CopyFromPack("Art/Characters/Player_Walk_16x16.png", playerPath);
        AssetDatabase.ImportAsset(playerPath, ImportAssetOptions.ForceUpdate);

        var playerImporter = (TextureImporter)AssetImporter.GetAtPath(playerPath);
        if (playerImporter.spriteImportMode != SpriteImportMode.Multiple || playerImporter.spritePixelsPerUnit != 16f)
        {
            ApplyPixelSettings(playerImporter);
            playerImporter.SaveAndReimport();
        }

        if (AssetDatabase.LoadAssetAtPath<Preset>(PresetPath) == null)
        {
            var preset = new Preset(playerImporter);
            AssetDatabase.CreateAsset(preset, PresetPath);
            Preset.SetDefaultPresetsForType(new PresetType(playerImporter),
                new[] { new DefaultPreset("glob:\"Assets/_Project/Art/**\"", preset) });
        }

        foreach (string art in ArtFolders)
        {
            string source = Path.Combine(PackFolder, "Art", art);
            if (!Directory.Exists(source)) continue;
            foreach (string file in Directory.GetFiles(source))
                CopyFromPack(Path.Combine("Art", art, Path.GetFileName(file)), Root + "/Art/" + art + "/" + Path.GetFileName(file));
        }
        AssetDatabase.Refresh();

        foreach (string sheet in CharacterSheets)
            SliceGrid(Root + "/Art/Characters/" + sheet + ".png", 16, 16, new Vector2(0.5f, 0f));
    }

    private static void ApplyPixelSettings(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
    }

    // Potong sheet karakter 3 kolom x 4 baris: baris Down, Left, Right, Up; kolom Diam, Langkah A, Langkah B.
    public static void SliceGrid(string path, int cellW, int cellH, Vector2 pivot)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (importer == null || texture == null) return;

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        int cols = texture.width / cellW;
        int rows = texture.height / cellH;
        string baseName = Path.GetFileNameWithoutExtension(path);
        SpriteRect[] old = provider.GetSpriteRects();
        var rects = new List<SpriteRect>();
        var pairs = new List<SpriteNameFileIdPair>();
        for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            string name = baseName + "_" + (row * cols + col);
            GUID id = GUID.Generate();
            foreach (SpriteRect o in old) if (o.name == name) id = o.spriteID;
            rects.Add(new SpriteRect
            {
                name = name,
                rect = new Rect(col * cellW, texture.height - (row + 1) * cellH, cellW, cellH),
                alignment = SpriteAlignment.Custom,
                pivot = pivot,
                spriteID = id,
            });
            pairs.Add(new SpriteNameFileIdPair(name, id));
        }
        provider.SetSpriteRects(rects.ToArray());
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(pairs);
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static void BuildSandboxScene()
    {
        if (File.Exists(ScenePath)) AssetDatabase.DeleteAsset(ScenePath);
        var template = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>("Assets/Settings/Lit2DSceneTemplate.scenetemplate");
        if (template != null) SceneTemplateService.Instantiate(template, false, ScenePath);
        else EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = GroundColor;
        }

        var square = AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
        var walls = new GameObject("Walls").transform;
        CreateBlock("Wall", square, new Vector2(2.5f, 0f), Vector2.one, walls);
        CreateBlock("Wall_Top", square, new Vector2(0f, 4.5f), new Vector2(17f, 1f), walls);
        CreateBlock("Wall_Bottom", square, new Vector2(0f, -4.5f), new Vector2(17f, 1f), walls);
        CreateBlock("Wall_Left", square, new Vector2(-8f, 0f), new Vector2(1f, 8f), walls);
        CreateBlock("Wall_Right", square, new Vector2(8f, 0f), new Vector2(1f, 8f), walls);
        CreateBlock("Obstacle_1", square, new Vector2(-4.5f, 2f), new Vector2(2f, 1f), walls);
        CreateBlock("Obstacle_2", square, new Vector2(-3f, -2.5f), new Vector2(1f, 2f), walls);
        CreateBlock("Obstacle_3", square, new Vector2(5f, -2f), new Vector2(2f, 1f), walls);

        GameObject player = CreateCharacter("Player", "Player_Walk_16x16_0", Vector2.zero);
        var rb = player.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var movement = player.AddComponent<PlayerMovement>();
        var serialized = new SerializedObject(movement);
        serialized.FindProperty("rb").objectReferenceValue = rb;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        CreateCharacter("NPC_Elder", "NPC_Elder_Walk_16x16_0", new Vector2(5f, 2f));

        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene, ScenePath);
        Selection.activeGameObject = player;
    }

    private static void CreateBlock(string name, Sprite sprite, Vector2 position, Vector2 size, Transform parent)
    {
        var block = new GameObject(name);
        block.transform.SetParent(parent);
        block.transform.position = position;
        block.transform.localScale = new Vector3(size.x, size.y, 1f);
        var renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = WallColor;
        block.AddComponent<BoxCollider2D>();
    }

    private static GameObject CreateCharacter(string name, string spriteName, Vector2 position)
    {
        var character = new GameObject(name);
        character.transform.position = position;
        var renderer = character.AddComponent<SpriteRenderer>();
        renderer.sprite = FindSprite(Root + "/Art/Characters/" + spriteName.Substring(0, spriteName.LastIndexOf('_')) + ".png", spriteName);
        renderer.sortingOrder = 10;
        var collider = character.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.6f, 0.4f);
        collider.offset = new Vector2(0f, 0.2f);
        return character;
    }

    public static Sprite FindSprite(string sheetPath, string spriteName)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
            if (asset is Sprite sprite && sprite.name == spriteName) return sprite;
        return null;
    }

    private static void CopyFromPack(string relative, string target)
    {
        string source = Path.Combine(PackFolder, relative);
        if (!File.Exists(source) || File.Exists(target)) return;
        File.Copy(source, target);
    }

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
