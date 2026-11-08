// Pulau Monster · P5 Battle & UI (builder) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Menjalankan langkah P5 di atas hasil P4: tema UI (panel Border 4, font Hinted Raster), EventSystem di Scn_Bootstrap,
// Canvas_Battle 320 x 180 piksel demo (x 4) + BattleSystem/BattleView/MonsterPanel, dan HUD dunia (PartnerHud).
public static class P5Builder
{
    private const string Root = P1Builder.Root;
    private const string Ui = Root + "/Art/UI/";
    private static readonly Vector3 BattleOrigin = new Vector3(-100f, -100f, 0f);

    [MenuItem("Pulau Monster/P5 Battle dan UI")]
    public static void Build()
    {
        P4Builder.Build();
        SetupUiAssets();
        AddEventSystem();
        BuildBattleScene();
        BuildWorldHud();
        EditorSceneManager.OpenScene(P2Builder.WorldPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[P5Builder] selesai");
    }

    // Praktikum 1: panel UI Sliced dengan Border 4, font Pixelify Sans Hinted Raster + Dynamic.
    private static void SetupUiAssets()
    {
        foreach (string file in Directory.GetFiles(Ui.TrimEnd('/'), "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file.Replace('\\', '/'));
            string name = Path.GetFileNameWithoutExtension(file);
            Vector4 border = name.StartsWith("UI_Panel") ? new Vector4(4, 4, 4, 4) : Vector4.zero;
            if (importer.spriteBorder == border) continue;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }
        foreach (string font in new[] { "PixelifySans-Regular.ttf", "PixelifySans-Bold.ttf" })
        {
            var importer = AssetImporter.GetAtPath(Root + "/Art/Fonts/" + font) as TrueTypeFontImporter;
            if (importer == null) continue;
            importer.fontRenderingMode = FontRenderingMode.HintedRaster;
            importer.fontTextureCase = FontTextureCase.Dynamic;
            importer.SaveAndReimport();
        }
    }

    // Satu EventSystem saja, di Scn_Bootstrap (klik dan hover menu).
    private static void AddEventSystem()
    {
        var scene = EditorSceneManager.OpenScene(P2Builder.BootstrapPath);
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void BuildBattleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var rootGo = new GameObject("BattleInstaller");
        var installer = rootGo.AddComponent<BattleInstaller>();
        var battle = rootGo.AddComponent<BattleSystem>();

        var cameraGo = new GameObject("Battle Camera");
        var camera = cameraGo.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.625f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = UiTheme.Ink;
        camera.depth = 10;
        cameraGo.transform.position = BattleOrigin + new Vector3(0f, 0f, -10f);

        var background = new GameObject("Background").AddComponent<SpriteRenderer>();
        background.sprite = P4Builder.LoadSingle(Root + "/Art/Backgrounds/BG_Battle_Forest_320x180.png");
        background.sortingLayerName = "Ground";
        background.transform.position = BattleOrigin;
        SpriteRenderer enemyBody = MonsterBody("EnemyMon", BattleOrigin + new Vector3(4.5f, -0.125f, 0f));
        SpriteRenderer playerBody = MonsterBody("PlayerMon", BattleOrigin + new Vector3(-4.5f, -2.375f, 0f));

        GameObject canvas = UiBuild.Canvas("Canvas_Battle", 10);
        var view = canvas.AddComponent<BattleView>();
        Image enemyPanel = UiBuild.Panel(canvas, "EnemyPanel", "UI_Panel_Navy", 8, 8, 128, 30);
        MonsterPanel enemy = FillMonsterPanel(enemyPanel, enemyBody, false, false);
        Image playerPanel = UiBuild.Panel(canvas, "PlayerPanel", "UI_Panel_Navy", 186, 92, 128, 36);
        MonsterPanel player = FillMonsterPanel(playerPanel, playerBody, true, true);
        Text star = UiBuild.Label(playerPanel.gameObject, "Star", "★", 8, UiTheme.Gold, 72, 4, 14, 10, TextAnchor.UpperCenter);
        P3Builder.SetField(player, "starText", star);
        UiBuild.Panel(canvas, "MessagePanel", "UI_Panel_Navy", 0, 132, 196, 48);
        Image menuPanel = UiBuild.Panel(canvas, "MenuPanel", "UI_Panel_Navy", 196, 132, 124, 48);
        Text message = UiBuild.Label(canvas, "Message", "", 8, UiTheme.Text, 8, 139, 182, 26, TextAnchor.UpperLeft);
        Text hint = UiBuild.Label(canvas, "SkillHint", "", 7, UiTheme.Muted, 8, 166, 182, 10, TextAnchor.UpperLeft);
        var menu = menuPanel.gameObject.AddComponent<TextMenu>();
        var rows = new List<Object>();
        for (int i = 0; i < 5; i++)
        {
            bool left = i % 2 == 0;
            Text row = UiBuild.Label(menuPanel.gameObject, "Btn_" + i, "", 8, UiTheme.Text, left ? 6 : 57, 4 + (i / 2) * 15, left ? 50 : 66, 14, TextAnchor.MiddleLeft);
            row.horizontalOverflow = HorizontalWrapMode.Overflow;
            rows.Add(row);
        }
        UiBuild.SetArray(menu, "rows", rows);
        UiBuild.SetInt(menu, "columns", 2);

        var rings = new List<Object>();
        for (int i = 0; i < 3; i++)
        {
            Image ring = UiBuild.Image(canvas, "Ring_" + i, P4Builder.LoadSingle(Ui + "UI_Ring.png"), 56, 72, 64, 64);
            ring.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            ring.rectTransform.anchoredPosition = new Vector2(88 * UiBuild.Px, -104 * UiBuild.Px);
            ring.color = UiTheme.Gold;
            ring.enabled = false;
            rings.Add(ring);
        }
        Text damage = UiBuild.Label(canvas, "DamageTemplate", "0", 10, Color.white, 0, 0, 40, 14, TextAnchor.MiddleCenter);
        damage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        damage.raycastTarget = false;
        Image flash = UiBuild.FullScreen(canvas, "Flash", new Color(1f, 1f, 1f, 0f));
        Image wipeTop = UiBuild.FullScreen(canvas, "WipeTop", UiTheme.Ink);
        Wipe(wipeTop.rectTransform, true);
        Image wipeBottom = UiBuild.FullScreen(canvas, "WipeBottom", UiTheme.Ink);
        Wipe(wipeBottom.rectTransform, false);

        P3Builder.SetField(view, "messageText", message);
        P3Builder.SetField(view, "hintText", hint);
        P3Builder.SetField(view, "menu", menu);
        P3Builder.SetField(view, "enemyPanel", enemy);
        P3Builder.SetField(view, "playerPanel", player);
        P3Builder.SetField(view, "flash", flash);
        P3Builder.SetField(view, "wipeTop", wipeTop.rectTransform);
        P3Builder.SetField(view, "wipeBottom", wipeBottom.rectTransform);
        P3Builder.SetField(view, "damageTemplate", damage);
        P3Builder.SetField(view, "battleCamera", camera);
        UiBuild.SetArray(view, "rings", rings);

        P3Builder.SetField(installer, "battle", battle);
        P3Builder.SetField(installer, "view", view);
        P3Builder.SetField(installer, "background", background);
        P3Builder.SetField(installer, "forestBackground", P4Builder.LoadSingle(Root + "/Art/Backgrounds/BG_Battle_Forest_320x180.png"));
        P3Builder.SetField(installer, "caveBackground", P4Builder.LoadSingle(Root + "/Art/Backgrounds/BG_Battle_Cave_320x180.png"));
        EditorSceneManager.SaveScene(scene, P4Builder.BattlePath);
    }

    private static void Wipe(RectTransform rect, bool top)
    {
        rect.anchorMin = new Vector2(0f, top ? 1f : 0f);
        rect.anchorMax = new Vector2(1f, top ? 1f : 0f);
        rect.pivot = new Vector2(0.5f, top ? 1f : 0f);
        rect.sizeDelta = new Vector2(0f, 360f);
        rect.anchoredPosition = Vector2.zero;
        rect.gameObject.SetActive(false);
    }

    private static SpriteRenderer MonsterBody(string name, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = "Characters";
        return renderer;
    }

    private static MonsterPanel FillMonsterPanel(Image panel, SpriteRenderer body, bool partner, bool faceRight)
    {
        GameObject go = panel.gameObject;
        var monsterPanel = go.AddComponent<MonsterPanel>();
        P3Builder.SetField(monsterPanel, "nameText", UiBuild.Label(go, "Name", "", 8, UiTheme.Text, 5, 4, 86, 10, TextAnchor.UpperLeft, true));
        P3Builder.SetField(monsterPanel, "levelText", UiBuild.Label(go, "Level", "", 8, UiTheme.Text, 88, 4, 36, 10, TextAnchor.UpperRight));
        P3Builder.SetField(monsterPanel, "body", body);
        UiBuild.SetBool(monsterPanel, "faceRight", faceRight);
        if (!partner)
        {
            P3Builder.SetField(monsterPanel, "elementText", UiBuild.Label(go, "Element", "", 7, UiTheme.Muted, 5, 13, 80, 9, TextAnchor.UpperLeft));
            UiBuild.Bar(go, "HpBar", 5, 23, 118, 5, HpColors.High, out Image hpFill);
            P3Builder.SetField(monsterPanel, "hpFill", hpFill);
        }
        else
        {
            UiBuild.Bar(go, "HpBar", 5, 15, 118, 5, HpColors.High, out Image hpFill);
            P3Builder.SetField(monsterPanel, "hpFill", hpFill);
            P3Builder.SetField(monsterPanel, "hpText", UiBuild.Label(go, "HpText", "", 7, UiTheme.Text, 60, 21, 63, 9, TextAnchor.UpperRight));
            UiBuild.Bar(go, "ExpBar", 5, 31, 118, 3, UiTheme.Exp, out Image expFill);
            P3Builder.SetField(monsterPanel, "expFill", expFill);
        }
        return monsterPanel;
    }

    // HUD dunia versi demo: panel partner (4, 4, 112 x 34) dan panel area (226, 4, 90 x 24) + Ramuan xN.
    private static void BuildWorldHud()
    {
        var scene = EditorSceneManager.OpenScene(P2Builder.WorldPath);
        GameObject canvas = UiBuild.Canvas("Canvas_Hud", 5);
        var hud = canvas.AddComponent<PartnerHud>();
        var root = new GameObject("HudRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        UiBuild.Stretch(root);
        Image panel = UiBuild.Panel(root, "PartnerPanel", "UI_Panel_Navy", 4, 4, 112, 34);
        Image portrait = UiBuild.Image(panel.gameObject, "Portrait", null, 4, 5, 24, 24);
        portrait.preserveAspect = true;
        Text name = UiBuild.Label(panel.gameObject, "Name", "", 7, UiTheme.Text, 32, 3, 78, 9, TextAnchor.UpperLeft, true);
        UiBuild.Bar(panel.gameObject, "HpBar", 32, 14, 74, 6, HpColors.High, out Image hpFill);
        Text hpText = UiBuild.Label(panel.gameObject, "HpText", "", 6, UiTheme.Text, 32, 20, 74, 8, TextAnchor.UpperRight);
        UiBuild.Bar(panel.gameObject, "ExpBar", 32, 28, 74, 3, UiTheme.Exp, out Image expFill);
        Image areaPanel = UiBuild.Panel(root, "AreaPanel", "UI_Panel_Navy", 226, 4, 90, 24);
        Text area = UiBuild.Label(areaPanel.gameObject, "Area", "Desa Awal", 8, UiTheme.Gold, 2, 2, 86, 10, TextAnchor.UpperCenter, true);
        Text potions = UiBuild.Label(areaPanel.gameObject, "Potions", "", 6, UiTheme.Muted, 2, 13, 86, 9, TextAnchor.UpperCenter);
        P3Builder.SetField(hud, "root", root);
        P3Builder.SetField(hud, "partnerPanel", panel.gameObject);
        P3Builder.SetField(hud, "portrait", portrait);
        P3Builder.SetField(hud, "nameText", name);
        P3Builder.SetField(hud, "hpFill", hpFill);
        P3Builder.SetField(hud, "hpText", hpText);
        P3Builder.SetField(hud, "expFill", expFill);
        P3Builder.SetField(hud, "potionsText", potions);
        P3Builder.SetField(hud, "areaText", area);
        P3Builder.SetField(Object.FindAnyObjectByType<WorldInstaller>(), "hud", hud);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}

// Pembuat uGUI dari koordinat desain 320 x 180 (x 4): anchor & pivot kiri-atas, panel rect (x-1, y-1, w+2, h+2).
public static class UiBuild
{
    public const float Px = UiTheme.PixelScale;

    public static GameObject Canvas(string name, int order)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<UnityEngine.Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        canvas.pixelPerfect = true;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    public static RectTransform Rect(GameObject go, float x, float y, float w, float h)
    {
        var rect = go.GetComponent<RectTransform>();
        if (rect == null) rect = go.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x * Px, -y * Px);
        rect.sizeDelta = new Vector2(w * Px, h * Px);
        return rect;
    }

    public static void Stretch(GameObject go)
    {
        var rect = go.GetComponent<RectTransform>();
        if (rect == null) rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static GameObject Child(GameObject parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    public static Image Image(GameObject parent, string name, Sprite sprite, float x, float y, float w, float h)
    {
        GameObject go = Child(parent, name);
        Rect(go, x, y, w, h);
        var image = go.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    public static Image Panel(GameObject parent, string name, string spriteName, float x, float y, float w, float h)
    {
        Image image = Image(parent, name, P4Builder.LoadSingle(P1Builder.Root + "/Art/UI/" + spriteName + ".png"), x - 1, y - 1, w + 2, h + 2);
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = UiTheme.SlicedMultiplier;
        return image;
    }

    public static Image FullScreen(GameObject parent, string name, Color color)
    {
        GameObject go = Child(parent, name);
        Stretch(go);
        var image = go.AddComponent<UnityEngine.UI.Image>();
        image.sprite = P4Builder.LoadSingle(P1Builder.Root + "/Art/UI/UI_Pixel.png");
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text Label(GameObject parent, string name, string text, int demoPx, Color color,
        float x, float y, float w, float h, TextAnchor anchor, bool bold = false)
    {
        GameObject go = Child(parent, name);
        Rect(go, x, y, w, h);
        var label = go.AddComponent<Text>();
        label.font = AssetDatabase.LoadAssetAtPath<Font>(P1Builder.Root + "/Art/Fonts/PixelifySans-" + (bold ? "Bold" : "Regular") + ".ttf");
        label.fontSize = Mathf.RoundToInt(demoPx * Px);
        label.text = text;
        label.color = color;
        label.alignment = anchor;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = UiTheme.Ink;
        shadow.effectDistance = new Vector2(2f, -2f);
        return label;
    }

    // Bar = latar #0B0918 + anak Fill (Image Filled, Horizontal, inset 1 px).
    public static Image Bar(GameObject parent, string name, float x, float y, float w, float h, Color fillColor, out Image fill)
    {
        Image back = Image(parent, name, P4Builder.LoadSingle(P1Builder.Root + "/Art/UI/UI_Pixel.png"), x, y, w, h);
        back.color = UiTheme.BarBack;
        fill = Image(back.gameObject, "Fill", P4Builder.LoadSingle(P1Builder.Root + "/Art/UI/UI_Pixel.png"), 1, 1, w - 2, h - 2);
        fill.type = UnityEngine.UI.Image.Type.Filled;
        fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        fill.fillAmount = 1f;
        fill.color = fillColor;
        return back;
    }

    public static void SetArray(Object target, string field, List<Object> values)
    {
        var serialized = new SerializedObject(target);
        SerializedProperty array = serialized.FindProperty(field);
        array.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetInt(Object target, string field, int value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void SetBool(Object target, string field, bool value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
