using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TMP_Text amountText;

    [SerializeField]
    private GameObject emptyVisual;

    private bool isSelected;

    public event Action<InventorySlotView, PointerEventData.InputButton> OnClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("좌클릭");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("우클릭");
        }

        OnClicked?.Invoke(this, eventData.button);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        SetSelectedVisual();
    }

    public void Bind(InventorySlotData slot, IItemCatalogReader catalogReader)
    {
        if (slot.IsEmpty)
        {
            SetEmptyVisual();
            return;
        }

        if (emptyVisual != null)
        {
            emptyVisual.SetActive(false);
        }

        /*
        [삭제] ItemCatalogEntry 체제 제거

        ItemCatalogEntry entry = default;
        bool hasCatalogEntry = catalogReader != null && catalogReader.TryGetEntry(slot.itemId, out entry);
        */

        // [변경] ItemData 직접 조회
        ItemData itemData = null;
        bool hasItemData = catalogReader != null && catalogReader.TryGetItemData(slot.itemId, out itemData);

        if (iconImage != null)
        {
            Sprite iconSprite = hasItemData ? itemData.icon : null;

            iconImage.enabled = iconSprite != null;
            iconImage.sprite = iconSprite;

            if (hasItemData)
            {
                iconImage.color = itemData.IconTint;
            }
            else
            {
                iconImage.color = Color.white;
            }

            SetSelectedVisual();
        }

        if (amountText != null)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = slot.amount > 1 ? slot.amount.ToString() : string.Empty;
        }
    }

    private void SetEmptyVisual()
    {
        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            iconImage.color = Color.white;
        }

        if (amountText != null)
        {
            amountText.gameObject.SetActive(false);
            amountText.text = string.Empty;
        }

        if (emptyVisual != null)
        {
            emptyVisual.SetActive(true);
        }
    }

    private void SetIconAlpha(float alpha)
    {
        if (iconImage == null)
        {
            return;
        }

        Color color = iconImage.color;
        color.a = alpha;
        iconImage.color = color;
    }

    private void SetSelectedVisual()
    {
        SetIconAlpha(isSelected ? 0.3f : 1f);
    }
}