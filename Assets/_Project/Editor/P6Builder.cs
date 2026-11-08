// Pulau Monster · P6 Dialog, Save & Audio (builder) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Menjalankan langkah P6 di atas hasil P5: audio, Scn_Bootstrap lengkap (Canvas_Dialogue, toast, menu jeda, AudioService),
// Scn_Title, NPC turunan (Tetua, Penjaga, Pak Dagang, Sari, Pak Nelayan), Batu Simpan, palang Gate_Hutan, musik & suara battle.
public static class P6Builder
{
    private const string Root = P1Builder.Root;
    public const string TitlePath = Root + "/Scenes/Scn_Title.unity";

    [MenuItem("Pulau Monster/P6 Dialog, Save dan Audio")]
    public static void Build()
    {
        P5Builder.Build();
        ImportAudio();
        var stone = (TextureImporter)AssetImporter.GetAtPath(Root + "/Art/UI/Prop_SaveStone.png");
        stone.spritePivot = new Vector2(0.5f, 0f);
        var settings = new TextureImporterSettings();
        stone.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        stone.SetTextureSettings(settings);
        stone.SaveAndReimport();

        BuildBootstrap();
        BuildTitle();
        BuildWorld();
        BuildBattleAudio();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(P2Builder.BootstrapPath, true),
            new EditorBuildSettingsScene(TitlePath, true),
            new EditorBuildSettingsScene(P2Builder.WorldPath, true),
            new EditorBuildSettingsScene(P4Builder.BattlePath, true),
        };
        EditorSceneManager.OpenScene(P2Builder.WorldPath);
        AssetDatabase.SaveAssets();
        Debug.Log("[P6Builder] selesai");
    }

    public static AudioClip Clip(string name)
    {
        foreach (string folder in new[] { "/Audio/SFX/", "/Audio/BGM/" })
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + folder + name + ".wav");
            if (clip != null) return clip;
        }
        return null;
    }

    private static void ImportAudio()
    {
        foreach (string folder in new[] { "SFX", "BGM" })
        {
            P1Builder.EnsureFolder(Root + "/Audio/" + folder);
            foreach (string file in Directory.GetFiles("StarterAssetPack/Audio/" + folder, "*.wav"))
            {
                string target = Root + "/Audio/" + folder + "/" + Path.GetFileName(file);
                if (!File.Exists(target)) File.Copy(file, target);
            }
        }
        AssetDatabase.Refresh();
    }

    private static void MenuSounds(TextMenu menu)
    {
        P3Builder.SetField(menu, "moveClip", Clip("SFX_UI_Move"));
        P3Builder.SetField(menu, "confirmClip", Clip("SFX_UI_Confirm"));
        P3Builder.SetField(menu, "cancelClip", Clip("SFX_UI_Cancel"));
    }

    // Scn_Bootstrap: Composition Root + EventSystem + Audio + Fader + Canvas_Dialogue + Canvas_Toast + Canvas_Pause.
    private static void BuildBootstrap()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        var audioGo = new GameObject("Audio");
        audioGo.AddComponent<AudioListener>();
        var audio = audioGo.AddComponent<AudioService>();
        P3Builder.SetField(audio, "musicA", audioGo.AddComponent<AudioSource>());
        P3Builder.SetField(audio, "musicB", audioGo.AddComponent<AudioSource>());
        P3Builder.SetField(audio, "sfx", audioGo.AddComponent<AudioSource>());

        GameObject faderCanvas = UiBuild.Canvas("Canvas_Fader", 100);
        Image black = UiBuild.FullScreen(faderCanvas, "Black", Color.black);
        var fader = faderCanvas.AddComponent<ScreenFader>();
        P3Builder.SetField(fader, "black", black);

        GameObject dialogueCanvas = UiBuild.Canvas("Canvas_Dialogue", 20);
        var view = dialogueCanvas.AddComponent<DialogueView>();
        var runner = dialogueCanvas.AddComponent<DialogueRunner>();
        var dialogueRoot = new GameObject("DialogueRoot", typeof(RectTransform));
        dialogueRoot.transform.SetParent(dialogueCanvas.transform, false);
        UiBuild.Stretch(dialogueRoot);
        Image catcher = UiBuild.FullScreen(dialogueRoot, "ClickCatcher", new Color(0f, 0f, 0f, 0f));
        catcher.raycastTarget = true;
        Button catcherButton = catcher.gameObject.AddComponent<Button>();
        catcherButton.transition = Selectable.Transition.None;
        Image box = UiBuild.Panel(dialogueRoot, "Box", "UI_Panel_Navy", 6, 128, 308, 46);
        Text body = UiBuild.Label(box.gameObject, "Body", "", 8, UiTheme.Text, 8, 7, 292, 34, TextAnchor.UpperLeft);
        Image speakerTag = UiBuild.Panel(dialogueRoot, "SpeakerTag", "UI_Panel_Gold", 12, 117, 60, 12);
        Text speaker = UiBuild.Label(speakerTag.gameObject, "Speaker", "", 8, UiTheme.Ink, 6, 2, 100, 10, TextAnchor.UpperLeft, true);
        Object.DestroyImmediate(speaker.GetComponent<Shadow>());
        Text hint = UiBuild.Label(dialogueRoot, "ContinueHint", "▼", 8, UiTheme.Gold, 300, 162, 12, 10, TextAnchor.UpperLeft);
        Image choicesPanel = UiBuild.Panel(dialogueRoot, "Choices", "UI_Panel_Navy", 206, 84, 108, 40);
        var choices = choicesPanel.gameObject.AddComponent<TextMenu>();
        var choiceRows = new List<Object>();
        for (int i = 0; i < 3; i++)
            choiceRows.Add(UiBuild.Label(choicesPanel.gameObject, "Choice_" + i, "", 8, UiTheme.Text, 7, 5 + i * 11, 96, 10, TextAnchor.UpperLeft));
        UiBuild.SetArray(choices, "rows", choiceRows);
        UiBuild.SetInt(choices, "columns", 1);
        MenuSounds(choices);
        choicesPanel.gameObject.SetActive(false);
        P3Builder.SetField(view, "canvas", dialogueCanvas.GetComponent<Canvas>());
        P3Builder.SetField(view, "root", dialogueRoot);
        P3Builder.SetField(view, "box", box);
        P3Builder.SetField(view, "boxSprite", P4Builder.LoadSingle(Root + "/Art/UI/UI_Panel_Navy.png"));
        P3Builder.SetField(view, "narrationSprite", P4Builder.LoadSingle(Root + "/Art/UI/UI_Panel_Narr.png"));
        P3Builder.SetField(view, "body", body);
        P3Builder.SetField(view, "speakerTag", speakerTag.rectTransform);
        P3Builder.SetField(view, "speaker", speaker);
        P3Builder.SetField(view, "continueHint", hint);
        P3Builder.SetField(view, "choices", choices);
        P3Builder.SetField(view, "clickCatcher", catcherButton);
        P3Builder.SetField(runner, "view", view);
        P3Builder.SetField(runner, "blipClip", Clip("SFX_Dialog_Blip"));
        dialogueRoot.SetActive(false);

        GameObject toastCanvas = UiBuild.Canvas("Canvas_Toast", 30);
        var toast = toastCanvas.AddComponent<ToastView>();
        Image toastPanel = UiBuild.Panel(toastCanvas, "ToastPanel", "UI_Panel_Gold", 60, 8, 200, 16);
        Text toastLabel = UiBuild.Label(toastPanel.gameObject, "Label", "", 7, UiTheme.Ink, 4, 3, 192, 10, TextAnchor.UpperCenter);
        Object.DestroyImmediate(toastLabel.GetComponent<Shadow>());
        P3Builder.SetField(toast, "root", toastPanel.gameObject);
        P3Builder.SetField(toast, "label", toastLabel);
        toastPanel.gameObject.SetActive(false);

        GameObject pauseCanvas = UiBuild.Canvas("Canvas_Pause", 40);
        var pause = pauseCanvas.AddComponent<PauseMenu>();
        var pauseRoot = new GameObject("PauseRoot", typeof(RectTransform));
        pauseRoot.transform.SetParent(pauseCanvas.transform, false);
        UiBuild.Stretch(pauseRoot);
        UiBuild.FullScreen(pauseRoot, "Dim", new Color(UiTheme.Ink.r, UiTheme.Ink.g, UiTheme.Ink.b, 0.55f));
        Image pausePanel = UiBuild.Panel(pauseRoot, "Panel", "UI_Panel_Navy", 104, 28, 112, 62);
        UiBuild.Label(pausePanel.gameObject, "Title", "JEDA", 8, UiTheme.Gold, 8, 5, 96, 10, TextAnchor.UpperCenter, true);
        var pauseMenu = pausePanel.gameObject.AddComponent<TextMenu>();
        var pauseRows = new List<Object>();
        for (int i = 0; i < 3; i++)
            pauseRows.Add(UiBuild.Label(pausePanel.gameObject, "Pause_" + i, "", 8, UiTheme.Text, 8, 19 + i * 12, 100, 10, TextAnchor.UpperLeft));
        UiBuild.SetArray(pauseMenu, "rows", pauseRows);
        MenuSounds(pauseMenu);
        P3Builder.SetField(pause, "root", pauseRoot);
        P3Builder.SetField(pause, "menu", pauseMenu);
        pauseRoot.SetActive(false);

        var bootstrapSo = new SerializedObject(bootstrap);
        bootstrapSo.FindProperty("firstScene").stringValue = SceneFlow.TitleScene;
        bootstrapSo.ApplyModifiedPropertiesWithoutUndo();
        P3Builder.SetField(bootstrap, "monsterDatabase", AssetDatabase.LoadAssetAtPath<MonsterDatabase>(P4Builder.DatabasePath));
        P3Builder.SetField(bootstrap, "fader", fader);
        P3Builder.SetField(bootstrap, "dialogue", runner);
        P3Builder.SetField(bootstrap, "toast", toast);
        P3Builder.SetField(bootstrap, "audio", audio);
        P3Builder.SetField(bootstrap, "pauseMenu", pause);
        EditorSceneManager.SaveScene(scene, P2Builder.BootstrapPath);
    }

    // Scn_Title versi demo: latar BG_Title_World bergeser, overlay, 4 starter beranimasi, menu teks, identitas.
    private static void BuildTitle()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraGo = new GameObject("Main Camera");
        var camera = cameraGo.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = UiTheme.Ink;
        cameraGo.transform.position = new Vector3(0f, 0f, -10f);

        var installer = new GameObject("TitleInstaller").AddComponent<TitleInstaller>();
        GameObject canvas = UiBuild.Canvas("Canvas_Title", 0);
        Image background = UiBuild.Image(canvas, "Background", P4Builder.LoadSingle(Root + "/Art/Backgrounds/BG_Title_World.png"), 0, 0, 440, 180);
        background.rectTransform.anchorMin = background.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        background.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        background.rectTransform.anchoredPosition = Vector2.zero;
        UiBuild.FullScreen(canvas, "Overlay", new Color(UiTheme.Ink.r, UiTheme.Ink.g, UiTheme.Ink.b, 0.55f));
        UiBuild.Label(canvas, "Title", "PULAU MONSTER", 20, UiTheme.Gold, 0, 22, 320, 26, TextAnchor.UpperCenter, true);
        UiBuild.Label(canvas, "Subtitle", "Proyek Gim 2D · Seri 7 Pertemuan", 7, UiTheme.Muted, 0, 50, 320, 10, TextAnchor.UpperCenter);

        var database = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(P4Builder.DatabasePath);
        string[] starterNames = { "Pyrit", "Blubi", "Sprig", "Zapi" };
        var starterImages = new List<Object>();
        var starterData = new List<Object>();
        for (int i = 0; i < 4; i++)
        {
            MonsterData m = database.GetByName(starterNames[i]);
            Image image = UiBuild.Image(canvas, "Starter_" + i, m.IdleFrame(0), 160 + (i - 2) * 44 + 6, 64, 32, 32);
            image.preserveAspect = true;
            starterImages.Add(image);
            starterData.Add(m);
        }

        var menu = canvas.AddComponent<TextMenu>();
        var rows = new List<Object>();
        for (int i = 0; i < 5; i++)
            rows.Add(UiBuild.Label(canvas, "Title_" + i, "", 8, UiTheme.Text, 112, 102 + i * 12, 130, 11, TextAnchor.UpperLeft));
        UiBuild.SetArray(menu, "rows", rows);
        MenuSounds(menu);
        UiBuild.Label(canvas, "Identity", "Neza Insyirah Nasuha · 4352511002 · TP 3A Malam", 7, UiTheme.Muted, 0, 166, 320, 10, TextAnchor.UpperCenter);

        P3Builder.SetField(installer, "menu", menu);
        P3Builder.SetField(installer, "titleMusic", Clip("BGM_Title"));
        P3Builder.SetField(installer, "background", background.rectTransform);
        UiBuild.SetArray(installer, "starterImages", starterImages);
        UiBuild.SetArray(installer, "starterData", starterData);
        EditorSceneManager.SaveScene(scene, TitlePath);
    }

    private static void BuildWorld()
    {
        var scene = EditorSceneManager.OpenScene(P2Builder.WorldPath);
        var listener = Camera.main.GetComponent<AudioListener>();
        if (listener != null) Object.DestroyImmediate(listener);
        var database = AssetDatabase.LoadAssetAtPath<MonsterDatabase>(P4Builder.DatabasePath);

        var tetua = SwapNpc<StarterGiver>("Pfb_Npc_Elder");
        UiBuild.SetArray(tetua, "starters", new List<Object> { database.GetByName("Pyrit"), database.GetByName("Blubi"), database.GetByName("Sprig") });
        P3Builder.SetField(tetua, "getClip", Clip("SFX_LevelUp"));
        SwapNpc<GuardNpc>("Pfb_Npc_Guard");
        P3Builder.SetField(SwapNpc<MerchantNpc>("Pfb_Npc_Merchant"), "giftClip", Clip("SFX_Heal"));
        SwapNpc<SariNpc>("Pfb_Npc_Girl");
        AddFisherman();

        var gate = new GameObject("Gate_Hutan");
        gate.transform.position = new Vector2(20f, 11.5f);
        gate.AddComponent<BoxCollider2D>().size = new Vector2(2f, 1f);
        var toggle = gate.AddComponent<FlagToggle>();
        P3Builder.SetField(toggle, "hideWhenSet", gate);

        var stone = new GameObject("SaveStone");
        stone.layer = LayerMask.NameToLayer("Interactable");
        stone.transform.position = new Vector2(21.5f, 7f);
        var stoneRenderer = stone.AddComponent<SpriteRenderer>();
        stoneRenderer.sprite = P4Builder.LoadSingle(Root + "/Art/UI/Prop_SaveStone.png");
        stoneRenderer.sortingLayerName = "Characters";
        stoneRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
        var stoneCollider = stone.AddComponent<BoxCollider2D>();
        stoneCollider.size = new Vector2(0.8f, 0.5f);
        stoneCollider.offset = new Vector2(0f, 0.25f);
        P3Builder.SetField(stone.AddComponent<SaveStone>(), "saveClip", Clip("SFX_Save"));

        foreach (AreaZone zone in Object.FindObjectsByType<AreaZone>(FindObjectsSortMode.None))
            P3Builder.SetField(zone, "music", Clip(zone.name == "Area_Desa" ? "BGM_Village" : "BGM_Forest"));
        P3Builder.SetField(Object.FindAnyObjectByType<WorldInstaller>(), "villageMusic", Clip("BGM_Village"));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Ganti component Npc dasar dengan turunannya (polymorphism), nama dan sprite hadap tetap.
    private static T SwapNpc<T>(string objectName) where T : Npc
    {
        GameObject go = GameObject.Find(objectName);
        Npc old = go.GetComponent<Npc>();
        var oldSo = new SerializedObject(old);
        string npcName = oldSo.FindProperty("npcName").stringValue;
        Object body = oldSo.FindProperty("body").objectReferenceValue;
        var faces = new Dictionary<string, Object>();
        foreach (string face in new[] { "faceDown", "faceLeft", "faceRight", "faceUp" })
            faces[face] = oldSo.FindProperty(face).objectReferenceValue;
        Object.DestroyImmediate(old);
        var npc = go.AddComponent<T>();
        var so = new SerializedObject(npc);
        so.FindProperty("npcName").stringValue = npcName;
        so.FindProperty("body").objectReferenceValue = body;
        foreach (var face in faces) so.FindProperty(face.Key).objectReferenceValue = face.Value;
        so.ApplyModifiedPropertiesWithoutUndo();
        return npc;
    }

    // Tugas kelompok P6: NPC baru Pak Nelayan di tepi Danau Sunyi (sprite NPC_Merchant, warna kebiruan).
    private static void AddFisherman()
    {
        var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/NPC/Pfb_Npc_Merchant.prefab");
        var go = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        go.name = "Npc_Nelayan";
        go.transform.position = new Vector2(16.5f, 25.5f);
        go.GetComponent<SpriteRenderer>().color = new Color(0.75f, 0.88f, 1f);
        var so = new SerializedObject(go.GetComponent<Npc>());
        so.FindProperty("npcName").stringValue = "Pak Nelayan";
        so.ApplyModifiedPropertiesWithoutUndo();
        SwapNpc<FishermanNpc>("Npc_Nelayan");
    }

    private static void BuildBattleAudio()
    {
        var scene = EditorSceneManager.OpenScene(P4Builder.BattlePath);
        var installer = Object.FindAnyObjectByType<BattleInstaller>();
        var battleAudio = installer.gameObject.AddComponent<BattleAudio>();
        P3Builder.SetField(battleAudio, "hit", Clip("SFX_Hit"));
        P3Builder.SetField(battleAudio, "hitCritical", Clip("SFX_Hit_Critical"));
        P3Builder.SetField(battleAudio, "miss", Clip("SFX_Miss"));
        P3Builder.SetField(battleAudio, "levelUp", Clip("SFX_LevelUp"));
        P3Builder.SetField(battleAudio, "evolve", Clip("SFX_Evolve"));
        P3Builder.SetField(battleAudio, "victory", Clip("SFX_Victory_Jingle"));
        P3Builder.SetField(battleAudio, "faint", Clip("SFX_Faint"));
        P3Builder.SetField(battleAudio, "heal", Clip("SFX_Heal"));
        P3Builder.SetField(battleAudio, "encounter", Clip("SFX_Encounter"));
        P3Builder.SetField(installer, "battleAudio", battleAudio);
        P3Builder.SetField(installer, "battleMusic", Clip("BGM_Battle"));
        P3Builder.SetField(installer, "bossMusic", Clip("BGM_Boss"));
        MenuSounds(Object.FindAnyObjectByType<TextMenu>(FindObjectsInactive.Include));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
