using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopItemType { Tower, Equipment }

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    public string itemId;        // 예: "tower_rifle" (중복 금지)
    public string displayName;   // 예: "라이플 스쿼드"
    public ShopItemType itemType;
    public int price;
    public Sprite icon;
    [TextArea] public string description;
}
