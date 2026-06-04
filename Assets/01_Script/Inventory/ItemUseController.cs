using UnityEngine;

public class ItemUseController : MonoBehaviour
{
    [SerializeField]
    private HealthPointManager hpManager;

    [SerializeField]
    private StaminaManager staminaManager;

    [SerializeField]
    private ItemCatalogManager catalog;

    private void Awake()
    {
        if (catalog == null)
        {
            catalog = FindFirstObjectByType<ItemCatalogManager>();
        }
    }

    public bool UseItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        /*
        [삭제] 기존 하드코딩 사용 방식

        if (itemId == "hp_potion")
        {
            hpManager.Heal(10);
            return true;
        }

        if (itemId == "stamina_potion")
        {
            staminaManager.RecoverStamina(5);
            return true;
        }
        */

        if (catalog == null || !catalog.TryGetItemData(itemId, out ItemData itemData))
        {
            Debug.Log($"[ItemUse] 등록되지 않은 아이템입니다: {itemId}");
            return false;
        }

        if (!itemData.canUse)
        {
            Debug.Log($"[ItemUse] {itemData.DisplayName}은/는 사용할 수 없는 아이템입니다.");
            return false;
        }

        if (itemData.effects == null || itemData.effects.Count == 0)
        {
            Debug.Log($"[ItemUse] {itemData.DisplayName}에 사용 효과가 없습니다.");
            return false;
        }

        bool appliedAnyEffect = false;

        for (int i = 0; i < itemData.effects.Count; i++)
        {
            ItemEffect effect = itemData.effects[i];

            if (effect == null)
            {
                continue;
            }

            if (effect.value <= 0)
            {
                continue;
            }

            switch (effect.effectType)
            {
                case ItemEffectType.HealHp:
                    if (hpManager != null)
                    {
                        hpManager.Heal(effect.value);
                        appliedAnyEffect = true;
                    }
                    break;

                case ItemEffectType.RecoverStamina:
                    if (staminaManager != null)
                    {
                        staminaManager.RecoverStamina(effect.value);
                        appliedAnyEffect = true;
                    }
                    break;
            }
        }

        if (appliedAnyEffect)
        {
            Debug.Log($"[ItemUse] {itemData.DisplayName}을/를 사용했습니다.");
            return true;
        }

        Debug.Log($"[ItemUse] {itemData.DisplayName} 사용 효과 적용 실패");
        return false;
    }
}