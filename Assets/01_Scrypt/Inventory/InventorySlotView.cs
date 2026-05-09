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

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        SetSelectedVisual();
    }

    private void SetSelectedVisual()
    {
        SetIconAlpha(isSelected ? 0.3f : 1f);
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

        ItemCatalogEntry entry = default;
        bool hasCatalogEntry = catalogReader != null && catalogReader.TryGetEntry(slot.itemId, out entry);

        if (iconImage != null)
        {
            Sprite iconSprite = hasCatalogEntry ? entry.icon : null;
            iconImage.enabled = iconSprite != null;
            iconImage.sprite = iconSprite;

            if (hasCatalogEntry)
            {
                Color tint = entry.iconTint.a < 0.01f ? Color.white : entry.iconTint;
                iconImage.color = tint;
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
}
