using UnityEngine;

public class ShopCommandRouter : MonoBehaviour
{
    [SerializeField]
    private GameObject shopUIPanel;

    [SerializeField]
    private ShopGridUI shopGridUI;

    [SerializeField]
    private PlayerController3D playerController;

    [SerializeField]
    private MeleeHitBoxAttack meleeHitBoxAttack;

    [SerializeField]
    private PlayerInteractionDetector interactionDetector;

    [SerializeField]
    private KeyCode openKey = KeyCode.E;

    [Header("Input Block Panels")]
    [SerializeField]
    private GameObject[] inputBlockPanels;

    private void Awake()
    {
        if (interactionDetector == null)
        {
            interactionDetector = GetComponent<PlayerInteractionDetector>();
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(openKey))
        {
            return;
        }

        if (shopUIPanel != null && shopUIPanel.activeSelf)
        {
            CloseShopPanel();
            return;
        }

        if (IsInputBlocked())
        {
            return;
        }

        Shop targetShop = GetCurrentShop();

        if (targetShop == null)
        {
            return;
        }

        OpenShopPanel(targetShop);
    }

    private Shop GetCurrentShop()
    {
        if (interactionDetector == null)
        {
            return null;
        }

        if (interactionDetector.CurrentShop != null)
        {
            return interactionDetector.CurrentShop;
        }

        if (interactionDetector.CurrentTarget != null)
        {
            return interactionDetector.CurrentTarget.GetComponentInParent<Shop>();
        }

        return null;
    }

    private void OpenShopPanel(Shop targetShop)
    {
        if (shopUIPanel == null || targetShop == null)
        {
            return;
        }

        if (shopGridUI != null)
        {
            shopGridUI.SetShop(targetShop);
        }

        shopUIPanel.SetActive(true);
        RefreshShopGridIfPossible();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (playerController != null)
        {
            playerController.LockCameraRotation();
        }

        if (meleeHitBoxAttack != null)
        {
            meleeHitBoxAttack.enabled = false;
        }
    }

    private void CloseShopPanel()
    {
        if (shopUIPanel == null)
        {
            return;
        }

        shopUIPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerController != null)
        {
            playerController.UnlockCameraRotation();
        }

        if (meleeHitBoxAttack != null)
        {
            meleeHitBoxAttack.enabled = true;
        }
    }

    private void RefreshShopGridIfPossible()
    {
        ShopGridUI grid = shopGridUI;

        if (grid == null && shopUIPanel != null)
        {
            grid = shopUIPanel.GetComponentInChildren<ShopGridUI>(true);
        }

        if (grid != null)
        {
            grid.RefreshDisplay();
        }
    }

    private bool IsInputBlocked()
    {
        if (inputBlockPanels == null)
        {
            return false;
        }

        for (int i = 0; i < inputBlockPanels.Length; i++)
        {
            GameObject panel = inputBlockPanels[i];

            if (panel != null && panel.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }
}