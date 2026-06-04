using TMPro;
using UnityEngine;

public class CrosshairInfoUI : MonoBehaviour
{
    [SerializeField]
    private PlayerInteractionDetector interactionDetector;

    [SerializeField]
    private CanvasGroup infoCanvasGroup;

    [SerializeField]
    private TMP_Text promptText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private string promptFormat = "[E] {0}";

    [SerializeField]
    private bool showItemDescription = true;

    [Header("Hide When Active")]
    [SerializeField]
    private GameObject[] hideWhenActivePanels;

    private void Awake()
    {
        if (interactionDetector == null)
        {
            interactionDetector = FindFirstObjectByType<PlayerInteractionDetector>();
        }

        if (infoCanvasGroup == null)
        {
            infoCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (infoCanvasGroup == null)
        {
            infoCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        Hide();
    }

    private void Update()
    {
        if (IsAnyHidePanelActive())
        {
            Hide();
            return;
        }

        if (interactionDetector == null)
        {
            Hide();
            return;
        }

        if (TryBuildInfo(out string prompt, out string description))
        {
            Show(prompt, description);
        }
        else
        {
            Hide();
        }
    }

    private bool TryBuildInfo(out string prompt, out string description)
    {
        prompt = string.Empty;
        description = string.Empty;

        WorldItem worldItem = interactionDetector.CurrentWorldItem;

        if (worldItem != null)
        {
            string itemName = worldItem.DisplayName;

            if (string.IsNullOrWhiteSpace(itemName))
            {
                itemName = GetCleanName(worldItem.gameObject);
            }

            prompt = worldItem.CanPickup
                ? FormatPrompt(itemName)
                : $"{itemName}";

            description = showItemDescription ? worldItem.Description : string.Empty;

            return true;
        }

        GameObject target = interactionDetector.CurrentTarget;

        if (target == null)
        {
            return false;
        }

        MerchantInteractable merchant = target.GetComponentInParent<MerchantInteractable>();

        if (merchant != null)
        {
            prompt = FormatPrompt(GetCleanName(merchant.gameObject));
            description = string.Empty;
            return true;
        }

        Shop shop = interactionDetector.CurrentShop;

        if (shop != null)
        {
            prompt = FormatPrompt(GetCleanName(shop.gameObject));
            description = string.Empty;
            return true;
        }

        prompt = FormatPrompt(GetCleanName(target));
        description = string.Empty;

        return true;
    }

    private void Show(string prompt, string description)
    {
        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.alpha = 1f;
            infoCanvasGroup.interactable = false;
            infoCanvasGroup.blocksRaycasts = false;
        }

        if (promptText != null)
        {
            promptText.text = prompt;
        }

        if (descriptionText != null)
        {
            bool hasDescription = !string.IsNullOrWhiteSpace(description);

            descriptionText.gameObject.SetActive(hasDescription);
            descriptionText.text = hasDescription ? description : string.Empty;
        }
    }

    private void Hide()
    {
        if (infoCanvasGroup != null)
        {
            infoCanvasGroup.alpha = 0f;
            infoCanvasGroup.interactable = false;
            infoCanvasGroup.blocksRaycasts = false;
        }

        if (promptText != null)
        {
            promptText.text = string.Empty;
        }

        if (descriptionText != null)
        {
            descriptionText.text = string.Empty;
            descriptionText.gameObject.SetActive(false);
        }
    }

    private bool IsAnyHidePanelActive()
    {
        if (hideWhenActivePanels == null)
        {
            return false;
        }

        for (int i = 0; i < hideWhenActivePanels.Length; i++)
        {
            GameObject panel = hideWhenActivePanels[i];

            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private string FormatPrompt(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            targetName = "Target";
        }

        if (string.IsNullOrWhiteSpace(promptFormat))
        {
            return $"[E] {targetName}";
        }

        return string.Format(promptFormat, targetName);
    }

    private string GetCleanName(GameObject target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        string result = target.name;
        result = result.Replace("(Clone)", string.Empty);
        result = result.Trim();

        return result;
    }
}