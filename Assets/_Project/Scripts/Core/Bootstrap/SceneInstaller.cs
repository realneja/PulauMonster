// Pulau Monster · P4 Data Monster & Monster Liar · Neza Insyirah Nasuha · 4352511002 · TP 3A Malam
using System.Collections.Generic;
using UnityEngine;

// Setiap scene punya satu installer di root; SceneFlow memanggil Install setelah scene dimuat.
public abstract class SceneInstaller : MonoBehaviour
{
    public abstract void Install(GameServices services);

    // Mencari component di scene milik installer ini saja (termasuk objek nonaktif).
    protected List<T> FindInScene<T>()
    {
        var found = new List<T>();
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            found.AddRange(root.GetComponentsInChildren<T>(true));
        return found;
    }
}
