using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private HashSet<string> ownedItemIds = new HashSet<string>();
    private const string SaveKey = "OwnedItems";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    public bool Owns(string itemId) => ownedItemIds.Contains(itemId);

    public void AddItem(string itemId)
    {
        ownedItemIds.Add(itemId);
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetString(SaveKey, string.Join(",", ownedItemIds));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        string data = PlayerPrefs.GetString(SaveKey, "");
        if (data == "") return;
        foreach (var id in data.Split(',')) ownedItemIds.Add(id);
    }
}