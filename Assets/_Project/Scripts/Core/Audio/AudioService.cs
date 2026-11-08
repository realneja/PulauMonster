// Pulau Monster · P6 Dialog, Save & Audio · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// Dua AudioSource musik (crossfade 1 detik, waktu unscaled) + satu AudioSource SFX. Tombol M = mute.
public class AudioService : MonoBehaviour
{
    [SerializeField] private AudioSource musicA;
    [SerializeField] private AudioSource musicB;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private float crossfadeSeconds = 1f;
    [SerializeField] private float musicVolume = 0.6f;

    private AudioSource active;
    private AudioClip worldMusic;
    private Coroutine fade;

    public bool Muted { get; private set; }

    void Awake()
    {
        active = musicA;
        if (musicA != null) { musicA.loop = true; musicA.playOnAwake = false; }
        if (musicB != null) { musicB.loop = true; musicB.playOnAwake = false; }
    }

    public void PlayWorldMusic(AudioClip clip)
    {
        worldMusic = clip;
        PlayMusic(clip);
    }

    public void PlayBattleMusic(AudioClip clip) => PlayMusic(clip);

    // Dipanggil SceneFlow setelah battle selesai.
    public void ResumeWorldMusic() => PlayMusic(worldMusic);

    public void PlaySfx(AudioClip clip, float pitchJitter = 0.05f)
    {
        if (clip == null || sfx == null) return;
        sfx.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        sfx.PlayOneShot(clip);
    }

    public bool ToggleMute()
    {
        Muted = !Muted;
        AudioListener.volume = Muted ? 0f : 1f;
        return Muted;
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicA == null || musicB == null) return;
        if (clip == null)
        {
            musicA.Stop();
            musicB.Stop();
            return;
        }
        if (active != null && active.clip == clip && active.isPlaying) return;
        AudioSource next = active == musicA ? musicB : musicA;
        next.clip = clip;
        next.volume = 0f;
        next.Play();
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Crossfade(active, next));
        active = next;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float fromStart = from != null ? from.volume : 0f;
        for (float t = 0f; t < crossfadeSeconds; t += Time.unscaledDeltaTime)
        {
            float k = t / crossfadeSeconds;
            if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
            to.volume = Mathf.Lerp(0f, musicVolume, k);
            yield return null;
        }
        if (from != null) { from.volume = 0f; from.Stop(); }
        to.volume = musicVolume;
        fade = null;
    }
}
