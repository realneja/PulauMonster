// Pulau Monster · P3 Animasi, Kamera & Interaksi (dipakai P6) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using UnityEngine;

// Dialog sederhana untuk NPC tanpa cabang (Sari: tips elemen).
[CreateAssetMenu(fileName = "Dlg_", menuName = "Pulau Monster/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string speaker = "Warga";
    [SerializeField, TextArea(2, 4)] private string[] lines;

    public string Speaker => speaker;
    public IReadOnlyList<string> Lines => lines;

#if UNITY_EDITOR
    public void EditorSet(string speakerName, string[] text)
    {
        speaker = speakerName;
        lines = text;
    }
#endif
}
