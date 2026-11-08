// Pulau Monster · P3 Animasi, Kamera & Interaksi (versi P6: Talk virtual) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections;
using UnityEngine;

// NPC dasar: menoleh ke pemain lalu menjalankan Talk(). Turunan menulis ulang Talk() (polymorphism).
public class Npc : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Warga";
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite faceDown, faceLeft, faceRight, faceUp;
    [SerializeField] private DialogueData dialogue;

    protected string NpcName => npcName;
    protected GameServices Services { get; private set; }
    protected DialogueRunner Dialogue { get; private set; }
    protected StoryFlags Flags { get; private set; }
    protected PlayerParty Party { get; private set; }
    protected ToastView Toast { get; private set; }
    protected AudioService Audio { get; private set; }

    public void Construct(GameServices services)
    {
        Services = services;
        Dialogue = services.Dialogue;
        Flags = services.Flags;
        Party = services.Party;
        Toast = services.Toast;
        Audio = services.Audio;
        OnConstructed();
    }

    public void Interact(PlayerInteractor who)
    {
        if (Dialogue == null || Dialogue.IsRunning) return;
        Face(who.transform.position - transform.position);
        Dialogue.Run(Talk());
    }

    // Versi dasar: DialogueData biasa (dipakai Sari).
    protected virtual IEnumerator Talk()
    {
        yield return Dialogue.PlayData(dialogue, npcName);
    }

    protected virtual void OnConstructed() { }

    protected void Face(Vector2 d)
    {
        if (body == null) return;
        Sprite face = Mathf.Abs(d.x) > Mathf.Abs(d.y)
            ? (d.x > 0 ? faceRight : faceLeft)
            : (d.y > 0 ? faceUp : faceDown);
        if (face != null) body.sprite = face;
    }
}
