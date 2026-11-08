// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Perpindahan scene: fade, muat additive di atas Scn_Bootstrap, lalu panggil Install pada installer scene itu.
public class SceneFlow
{
    public const string BootstrapScene = "Scn_Bootstrap";
    public const string TitleScene = "Scn_Title";
    public const string WorldScene = "Scn_World";
    public const string BattleScene = "Scn_Battle";
    public const string EndScene = "Scn_End";

    private readonly MonoBehaviour host;
    private readonly ScreenFader fader;
    private readonly GameServices services;

    public string Current { get; private set; }
    public bool IsBusy { get; private set; }
    public bool InBattle { get; private set; }
    public bool HoldFade { get; set; }
    public bool InGame => Current == WorldScene;

    public SceneFlow(MonoBehaviour host, ScreenFader fader, GameServices services)
    {
        this.host = host;
        this.fader = fader;
        this.services = services;
    }

    public void GoTo(string scene)
    {
        if (IsBusy) return;
        host.StartCoroutine(GoToRoutine(scene));
    }

    public void EnterBattle()
    {
        if (IsBusy || InBattle) return;
        host.StartCoroutine(EnterBattleRoutine());
    }

    public void EndBattle(BattleResult result)
    {
        if (!InBattle) return;
        host.StartCoroutine(EndBattleRoutine(result));
    }

    // Dipanggil WorldInstaller setelah narasi pembuka selesai (HoldFade menahan fade-in).
    public IEnumerator ReleaseFade()
    {
        HoldFade = false;
        yield return fader.FadeIn();
    }

    private IEnumerator GoToRoutine(string scene)
    {
        IsBusy = true;
        services.Input.Lock();
        yield return fader.FadeOut();
        if (InBattle)
        {
            yield return SceneManager.UnloadSceneAsync(BattleScene);
            InBattle = false;
        }
        if (Current != null && SceneManager.GetSceneByName(Current).isLoaded)
            yield return SceneManager.UnloadSceneAsync(Current);
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        Scene loaded = SceneManager.GetSceneByName(scene);
        SceneManager.SetActiveScene(loaded);
        Current = scene;
        Install(loaded);
        services.Input.Unlock();
        if (!HoldFade) yield return fader.FadeIn();
        IsBusy = false;
    }

    private IEnumerator EnterBattleRoutine()
    {
        IsBusy = true;
        InBattle = true;
        services.Input.Lock();
        yield return fader.FadeOut();
        yield return SceneManager.LoadSceneAsync(BattleScene, LoadSceneMode.Additive);
        Scene battle = SceneManager.GetSceneByName(BattleScene);
        SceneManager.SetActiveScene(battle);
        Install(battle);
        yield return fader.FadeIn();
        IsBusy = false;
    }

    private IEnumerator EndBattleRoutine(BattleResult result)
    {
        IsBusy = true;
        yield return fader.FadeOut();
        yield return SceneManager.UnloadSceneAsync(BattleScene);
        InBattle = false;
        Scene world = SceneManager.GetSceneByName(Current);
        if (world.isLoaded) SceneManager.SetActiveScene(world);
        services.Audio?.ResumeWorldMusic();
        services.Battle.Finish(result);
        services.Input.Unlock();
        yield return fader.FadeIn();
        IsBusy = false;
    }

    private void Install(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            SceneInstaller installer = root.GetComponent<SceneInstaller>();
            if (installer != null)
            {
                installer.Install(services);
                return;
            }
        }
        Debug.LogWarning($"SceneFlow: scene {scene.name} tidak punya SceneInstaller.");
    }
}
