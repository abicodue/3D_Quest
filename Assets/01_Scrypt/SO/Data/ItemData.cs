using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(
    fileName = "Item_New",
    menuName = "RPG Data/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    public Material mat;
    public Color iconTint = Color.white;

    [TextArea]
    public string description;

    [Header("경제 정보")]
    public int buyPrice;
    public int sellPrice;

    [Header("획득 여부")]
    public bool canPickup = true; // [변경] 기본값 true 권장

    [Header("사용 여부")]
    public bool canUse;

    public bool canStack = true; // [변경] 기본값 true 권장
    public int maxStackCount = 99;

    [Header("사용 효과")]
    public List<ItemEffect> effects = new List<ItemEffect>();

    public AssetReferenceGameObject worldPrefab;

    // [추가] 코드에서 itemId를 안전하게 쓰기 위한 프로퍼티
    public string Id
    {
        get
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return name;
            }

            return itemId.Trim();
        }
    }

    // [추가] 표시 이름이 비어 있으면 itemId를 대신 사용
    public string DisplayName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return Id;
            }

            return itemName;
        }
    }

    // [추가] canStack이 false면 무조건 1칸 1개
    public int MaxStack
    {
        get
        {
            if (!canStack)
            {
                return 1;
            }

            return Mathf.Max(1, maxStackCount);
        }
    }

    // [추가] 아이콘 색 알파가 0이면 UI에서 안 보이므로 white 보정
    public Color IconTint
    {
        get
        {
            return iconTint.a < 0.01f ? Color.white : iconTint;
        }
    }

    // [추가]
    public int BuyPrice => Mathf.Max(0, buyPrice);
    public int SellPrice => Mathf.Max(0, sellPrice);
}