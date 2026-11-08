// Pulau Monster · P2 Arsitektur & World Tilemap (builder) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

// Menjalankan langkah P2: Cfg_Player, sorting layer, Custom Axis, 56 tile + Pal_Overworld, RT_Path & RT_Water,
// Scn_Bootstrap + Scn_World (peta 40 x 36 berlapis) dan tugas kelompok Pfb_Area_Neza.
public static class P2Builder
{
    private const string Root = P1Builder.Root;
    private const string TileFolder = Root + "/Art/Tiles/Palette";
    private const string SheetPath = Root + "/Art/Tiles/Tiles_Overworld_16x16.png";
    public const string BootstrapPath = Root + "/Scenes/Scn_Bootstrap.unity";
    public const string WorldPath = Root + "/Scenes/Scn_World.unity";
    public const string ConfigPath = Root + "/Data/Cfg_Player.asset";

    private static readonly string[] TileNames =
    {
        "Path_TL", "Path_T", "Path_TR", "Path_iTL", "Path_iTR", "Water_TL", "Water_T", "Water_TR", "Water_iTL", "Water_iTR", "Grass_A", "Grass_B",
        "Path_L", "Path_C", "Path_R", "Path_iBL", "Path_iBR", "Water_L", "Water_C", "Water_R", "Water_iBL", "Water_iBR", "Grass_Flower", "Sand",
        "Path_BL", "Path_B", "Path_BR", "TallGrass_A", "TallGrass_B", "Water_BL", "Water_B", "Water_BR", "Flowers", "SmallRock", "Sign", "SaveStone",
        "Tree_Round", "Tree_Pine", "Bush", "BigRock", "Stump", "Fence_H", "Fence_V", "Cave_Floor_A", "Cave_Floor_B", "Cave_Wall", "Cave_Entrance", "Crystal",
        "Roof_L", "Roof_M", "Roof_R", "Wall_L", "Wall_Window", "Wall_Door", "Wall_R", "Wall_M",
    };

    private static readonly HashSet<string> BlockingTiles = new HashSet<string>
    {
        "Tree_Round", "Tree_Pine", "Bush", "BigRock", "Stump", "Fence_H", "Fence_V", "Cave_Wall",
        "Roof_L", "Roof_M", "Roof_R", "Wall_L", "Wall_Window", "Wall_Door", "Wall_R", "Wall_M", "TallGrass_A", "TallGrass_B", "SaveStone",
    };

    private static Dictionary<string, Tile> tiles;
    private static RuleTile rtPath;
    private static RuleTile rtWater;

    [MenuItem("Pulau Monster/P2 Arsitektur dan World Tilemap")]
    public static void Build()
    {
        P1Builder.EnsureFolder(Root + "/Data");
        P1Builder.EnsureFolder(TileFolder);
        P1Builder.EnsureFolder(Root + "/Prefabs/Areas");
        PlayerConfig config = CreateConfig();
        SetupSortingLayers("Ground", "Decor", "Characters");
        SetupCustomAxis();
        CreateTiles();
        CreatePalette();
        BuildBootstrapScene();
        BuildWorldScene(config);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootstrapPath, true),
            new EditorBuildSettingsScene(WorldPath, true),
        };
        UpdateSandbox(config);
        EditorSceneManager.OpenScene(WorldPath);
        Debug.Log("[P2Builder] selesai: " + WorldPath);
    }

    private static PlayerConfig CreateConfig()
    {
        var config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(ConfigPath);
        if (config != null) return config;
        config = ScriptableObject.CreateInstance<PlayerConfig>();
        AssetDatabase.CreateAsset(config, ConfigPath);
        return config;
    }

    public static void SetupSortingLayers(params string[] names)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
        foreach (string name in names)
        {
            bool exists = false;
            for (int i = 0; i < layers.arraySize; i++)
                if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name) exists = true;
            if (exists) continue;
            layers.InsertArrayElementAtIndex(layers.arraySize);
            SerializedProperty layer = layers.GetArrayElementAtIndex(layers.arraySize - 1);
            layer.FindPropertyRelative("name").stringValue = name;
            layer.FindPropertyRelative("uniqueID").intValue = (name.GetHashCode() & 0x7fffffff) | 1;
        }
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetupCustomAxis()
    {
        var renderer = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Settings/Renderer2D.asset");
        if (renderer == null) return;
        var serialized = new SerializedObject(renderer);
        serialized.FindProperty("m_TransparencySortMode").intValue = (int)TransparencySortMode.CustomAxis;
        serialized.FindProperty("m_TransparencySortAxis").vector3Value = new Vector3(0f, 1f, 0f);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(renderer);
    }

    private static void CreateTiles()
    {
        if (P1Builder.FindSprite(SheetPath, "Tiles_Overworld_16x16_55") == null)
            P1Builder.SliceGrid(SheetPath, 16, 16, new Vector2(0.5f, 0.5f));

        tiles = new Dictionary<string, Tile>();
        for (int i = 0; i < TileNames.Length; i++)
        {
            string path = TileFolder + "/" + TileNames[i] + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = P1Builder.FindSprite(SheetPath, "Tiles_Overworld_16x16_" + i);
            tile.colliderType = BlockingTiles.Contains(TileNames[i]) ? Tile.ColliderType.Grid : Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            tiles[TileNames[i]] = tile;
        }

        rtPath = CreateRuleTile("RT_Path", 0, Tile.ColliderType.None);
        rtWater = CreateRuleTile("RT_Water", 5, Tile.ColliderType.Grid);
        AssetDatabase.SaveAssets();
    }

    // Aturan dari tabel modul (atas, bawah, kiri, kanan) + tantangan 4 sudut dalam (cek diagonal).
    private static RuleTile CreateRuleTile(string name, int firstIndex, Tile.ColliderType collider)
    {
        string path = Root + "/Art/Tiles/" + name + ".asset";
        var ruleTile = AssetDatabase.LoadAssetAtPath<RuleTile>(path);
        if (ruleTile == null)
        {
            ruleTile = ScriptableObject.CreateInstance<RuleTile>();
            AssetDatabase.CreateAsset(ruleTile, path);
        }
        Sprite S(int offset) => P1Builder.FindSprite(SheetPath, "Tiles_Overworld_16x16_" + (firstIndex + offset));
        const int Y = RuleTile.TilingRuleOutput.Neighbor.This;
        const int N = RuleTile.TilingRuleOutput.Neighbor.NotThis;
        const int A = 0;

        ruleTile.m_DefaultSprite = S(13);
        ruleTile.m_DefaultColliderType = collider;
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>
        {
            Rule(S(3), collider, Y, Y, Y, Y, N, A, A, A),
            Rule(S(4), collider, Y, Y, Y, Y, A, N, A, A),
            Rule(S(15), collider, Y, Y, Y, Y, A, A, N, A),
            Rule(S(16), collider, Y, Y, Y, Y, A, A, A, N),
            Rule(S(0), collider, N, Y, N, Y),
            Rule(S(1), collider, N, Y, Y, Y),
            Rule(S(2), collider, N, Y, Y, N),
            Rule(S(12), collider, Y, Y, N, Y),
            Rule(S(14), collider, Y, Y, Y, N),
            Rule(S(24), collider, Y, N, N, Y),
            Rule(S(25), collider, Y, N, Y, Y),
            Rule(S(26), collider, Y, N, Y, N),
        };
        EditorUtility.SetDirty(ruleTile);
        return ruleTile;
    }

    private static RuleTile.TilingRule Rule(Sprite sprite, Tile.ColliderType collider, int up, int down, int left, int right,
        int upLeft = 0, int upRight = 0, int downLeft = 0, int downRight = 0)
    {
        var rule = new RuleTile.TilingRule { m_Sprites = new[] { sprite }, m_ColliderType = collider };
        rule.m_NeighborPositions = new List<Vector3Int>
        {
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 1, 0), new Vector3Int(1, 1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(1, -1, 0),
        };
        rule.m_Neighbors = new List<int> { up, down, left, right, upLeft, upRight, downLeft, downRight };
        return rule;
    }

    private static void CreatePalette()
    {
        string palettePath = Root + "/Art/Tiles/Pal_Overworld.prefab";
        if (!File.Exists(palettePath))
            GridPaletteUtility.CreateNewPalette(Root + "/Art/Tiles", "Pal_Overworld", GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Automatic, Vector3.one, GridLayout.CellSwizzle.XYZ);

        GameObject palette = PrefabUtility.LoadPrefabContents(palettePath);
        Tilemap tilemap = palette.GetComponentInChildren<Tilemap>();
        tilemap.ClearAllTiles();
        for (int i = 0; i < TileNames.Length; i++)
            tilemap.SetTile(new Vector3Int(i % 12, -(i / 12), 0), tiles[TileNames[i]]);
        tilemap.SetTile(new Vector3Int(13, 0, 0), rtPath);
        tilemap.SetTile(new Vector3Int(13, -1, 0), rtWater);
        PrefabUtility.SaveAsPrefabAsset(palette, palettePath);
        PrefabUtility.UnloadPrefabContents(palette);
    }

    private static void BuildBootstrapScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        EditorSceneManager.SaveScene(scene, BootstrapPath);
    }

    private static void BuildWorldScene(PlayerConfig config)
    {
        if (File.Exists(WorldPath)) AssetDatabase.DeleteAsset(WorldPath);
        var template = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>("Assets/Settings/Lit2DSceneTemplate.scenetemplate");
        SceneTemplateService.Instantiate(template, false, WorldPath);

        Camera camera = Camera.main;
        camera.transform.position = new Vector3(19.5f, 6.5f, -10f);
        var pixelPerfect = camera.gameObject.AddComponent<PixelPerfectCamera>();
        pixelPerfect.assetsPPU = 16;
        pixelPerfect.refResolutionX = 320;
        pixelPerfect.refResolutionY = 180;

        LightAllSortingLayers();

        var grid = new GameObject("Grid").AddComponent<Grid>();
        Tilemap ground = CreateTilemap(grid.transform, "TM_Ground", "Ground", 0);
        Tilemap decor = CreateTilemap(grid.transform, "TM_Decor", "Decor", 0);
        Tilemap blocking = CreateTilemap(grid.transform, "TM_Blocking", "Characters", 0);
        Tilemap tallGrass = CreateTilemap(grid.transform, "TM_TallGrass", "Decor", 1);
        blocking.GetComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;
        PaintWorld(ground, decor, blocking, tallGrass, new RectInt(0, 0, 40, 36), Vector3Int.zero);

        var blockingRb = blocking.gameObject.AddComponent<Rigidbody2D>();
        blockingRb.bodyType = RigidbodyType2D.Static;
        var blockingCollider = blocking.gameObject.AddComponent<TilemapCollider2D>();
        blockingCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        var composite = blocking.gameObject.AddComponent<CompositeCollider2D>();
        blockingCollider.ProcessTilemapChanges();
        composite.GenerateGeometry();
        var tallGrassCollider = tallGrass.gameObject.AddComponent<TilemapCollider2D>();
        tallGrassCollider.isTrigger = true;
        tallGrassCollider.ProcessTilemapChanges();
        Debug.Log("[P2Builder] composite paths: " + composite.pathCount);

        config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(ConfigPath);
        GameObject player = CreatePlayer(config, new Vector2(16f, 5f));
        var installer = new GameObject("WorldInstaller").AddComponent<WorldInstaller>();
        var serialized = new SerializedObject(installer);
        serialized.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerMovement>();
        serialized.ApplyModifiedPropertiesWithoutUndo();

        CreateAreaPrefab();
        EditorSceneManager.MarkSceneDirty(player.scene);
        EditorSceneManager.SaveScene(player.scene, WorldPath);
    }

    // Global Light 2D bawaan template hanya menerangi layer Default: tambahkan semua sorting layer.
    public static void LightAllSortingLayers()
    {
        foreach (Light2D light in Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            var serialized = new SerializedObject(light);
            SerializedProperty layers = serialized.FindProperty("m_ApplyToSortingLayers");
            layers.arraySize = SortingLayer.layers.Length;
            for (int i = 0; i < SortingLayer.layers.Length; i++)
                layers.GetArrayElementAtIndex(i).intValue = SortingLayer.layers[i].id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    public static Tilemap CreateTilemap(Transform parent, string name, string sortingLayer, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingLayerName = sortingLayer;
        renderer.sortingOrder = order;
        return tilemap;
    }

    private static GameObject CreatePlayer(PlayerConfig config, Vector2 position)
    {
        var player = new GameObject("Player");
        player.transform.position = position;
        var renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = P1Builder.FindSprite(Root + "/Art/Characters/Player_Walk_16x16.png", "Player_Walk_16x16_0");
        renderer.sortingLayerName = "Characters";
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.6f, 0.4f);
        collider.offset = new Vector2(0f, 0.2f);
        var movement = player.AddComponent<PlayerMovement>();
        var serialized = new SerializedObject(movement);
        serialized.FindProperty("config").objectReferenceValue = config;
        serialized.FindProperty("rb").objectReferenceValue = rb;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return player;
    }

    // Peta 40 x 36: Desa Awal (selatan), pagar y = 11 dengan celah x 19 sampai 20, Hutan Rimbun (tengah),
    // Danau Sunyi (barat laut), Gua Petir (timur laut). Hanya sel di dalam area yang dilukis, digeser offset.
    private static void PaintWorld(Tilemap ground, Tilemap decor, Tilemap blocking, Tilemap tallGrass, RectInt area, Vector3Int offset)
    {
        void Set(Tilemap map, int x, int y, TileBase tile)
        {
            if (area.Contains(new Vector2Int(x, y))) map.SetTile(new Vector3Int(x, y, 0) + offset, tile);
        }
        void Fill(Tilemap map, int x0, int y0, int x1, int y1, TileBase tile)
        {
            for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++) Set(map, x, y, tile);
        }
        void Points(Tilemap map, string tile, params int[] xy)
        {
            for (int i = 0; i + 1 < xy.Length; i += 2) Set(map, xy[i], xy[i + 1], tiles[tile]);
        }

        for (int x = 0; x < 40; x++)
        for (int y = 0; y < 36; y++)
        {
            int hash = (x * 7 + y * 13) % 23;
            Set(ground, x, y, hash == 0 ? tiles["Grass_Flower"] : hash % 5 == 0 ? tiles["Grass_B"] : tiles["Grass_A"]);
        }

        Fill(ground, 2, 3, 37, 4, rtPath);
        Fill(ground, 19, 5, 20, 21, rtPath);
        Fill(ground, 6, 22, 33, 23, rtPath);
        Fill(ground, 7, 24, 8, 25, rtPath);

        for (int x = 23; x <= 38; x++)
        for (int y = 24; y <= 34; y++)
        {
            bool edge = x == 23 || x == 38 || y == 24 || y == 34;
            bool entrance = y == 24 && (x == 29 || x == 30);
            Set(ground, x, y, (x + y) % 3 == 0 ? tiles["Cave_Floor_B"] : tiles["Cave_Floor_A"]);
            if (edge && !entrance) Set(blocking, x, y, tiles["Cave_Wall"]);
            if (entrance) Set(decor, x, y, tiles["Cave_Entrance"]);
        }
        Points(decor, "Crystal", 26, 31, 35, 31, 27, 27, 34, 27, 30, 32);

        Fill(blocking, 2, 26, 14, 33, rtWater);
        Fill(blocking, 30, 6, 34, 9, rtWater);

        for (int x = 0; x < 40; x++)
        {
            Set(blocking, x, 0, tiles[x % 2 == 0 ? "Tree_Pine" : "Tree_Round"]);
            Set(blocking, x, 35, tiles[x % 2 == 0 ? "Tree_Pine" : "Tree_Round"]);
        }
        for (int y = 1; y < 35; y++)
        {
            Set(blocking, 0, y, tiles[y % 2 == 0 ? "Tree_Round" : "Tree_Pine"]);
            Set(blocking, 39, y, tiles[y % 2 == 0 ? "Tree_Pine" : "Tree_Round"]);
        }

        for (int x = 1; x <= 38; x++)
            if (x != 19 && x != 20) Set(blocking, x, 11, tiles["Fence_H"]);
        Fill(blocking, 3, 1, 12, 1, tiles["Fence_H"]);

        void House(int x, int y)
        {
            Points(blocking, "Roof_L", x, y + 1);
            Points(blocking, "Roof_M", x + 1, y + 1, x + 2, y + 1);
            Points(blocking, "Roof_R", x + 3, y + 1);
            Points(blocking, "Wall_L", x, y);
            Points(blocking, "Wall_Window", x + 1, y);
            Points(blocking, "Wall_Door", x + 2, y);
            Points(blocking, "Wall_R", x + 3, y);
        }
        House(3, 8);
        House(9, 8);
        House(24, 8);

        Points(decor, "Sign", 17, 6);
        Points(decor, "Flowers", 5, 6, 14, 9, 28, 6, 22, 9, 36, 5, 18, 29, 11, 13, 33, 20);
        Points(decor, "SmallRock", 13, 6, 36, 2, 17, 25, 22, 17, 2, 13);
        Points(blocking, "Bush", 15, 8, 37, 9, 11, 16, 33, 16, 24, 13, 17, 13, 21, 31);
        Points(blocking, "Stump", 8, 6, 19, 31, 35, 21);
        Points(blocking, "BigRock", 5, 20, 31, 21, 16, 33);

        Points(blocking, "Tree_Round", 3, 13, 13, 14, 16, 16, 12, 20, 23, 19, 34, 13, 36, 18, 22, 13, 2, 20, 16, 30, 21, 26);
        Points(blocking, "Tree_Pine", 8, 21, 15, 20, 28, 20, 37, 15, 1, 17, 26, 12, 18, 27, 20, 33, 15, 25, 12, 18);

        for (int x = 4; x <= 10; x++)
        for (int y = 14; y <= 18; y++) Set(tallGrass, x, y, tiles[(x + y) % 2 == 0 ? "TallGrass_A" : "TallGrass_B"]);
        for (int x = 25; x <= 32; x++)
        for (int y = 14; y <= 18; y++) Set(tallGrass, x, y, tiles[(x + y) % 2 == 0 ? "TallGrass_A" : "TallGrass_B"]);
    }

    // Tugas kelompok: satu area per orang sebagai prefab (Grid + 4 Tilemap). Area Neza = Desa Awal (y 0 sampai 11).
    private static void CreateAreaPrefab()
    {
        var grid = new GameObject("Pfb_Area_Neza").AddComponent<Grid>();
        Tilemap ground = CreateTilemap(grid.transform, "TM_Ground", "Ground", 0);
        Tilemap decor = CreateTilemap(grid.transform, "TM_Decor", "Decor", 0);
        Tilemap blocking = CreateTilemap(grid.transform, "TM_Blocking", "Characters", 0);
        Tilemap tallGrass = CreateTilemap(grid.transform, "TM_TallGrass", "Decor", 1);
        blocking.GetComponent<TilemapRenderer>().mode = TilemapRenderer.Mode.Individual;
        PaintWorld(ground, decor, blocking, tallGrass, new RectInt(0, 0, 40, 12), Vector3Int.zero);
        PrefabUtility.SaveAsPrefabAsset(grid.gameObject, Root + "/Prefabs/Areas/Pfb_Area_Neza.prefab");
        Object.DestroyImmediate(grid.gameObject);
    }

    private static void UpdateSandbox(PlayerConfig config)
    {
        if (!File.Exists(P1Builder.ScenePath)) return;
        var scene = EditorSceneManager.OpenScene(P1Builder.ScenePath);
        config = AssetDatabase.LoadAssetAtPath<PlayerConfig>(ConfigPath);
        var movement = Object.FindAnyObjectByType<PlayerMovement>();
        if (movement != null)
        {
            var serialized = new SerializedObject(movement);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
        }
    }
}
