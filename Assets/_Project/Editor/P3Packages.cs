// Pulau Monster · P3 Animasi, Kamera & Interaksi (paket) · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

// Praktikum 4 langkah 1: Package Manager, Cinemachine 6.6.0 (sesuai Unity 6000.6).
public static class P3Packages
{
    [MenuItem("Pulau Monster/P3a Pasang Cinemachine")]
    public static void InstallCinemachine()
    {
        Client.Add("com.unity.cinemachine@6.6.0");
        Debug.Log("[P3Packages] Cinemachine 6.6.0 diminta");
    }
}
