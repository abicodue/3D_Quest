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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleShopPanel();
        }
    }

    private void ToggleShopPanel()
    {
        if (shopUIPanel == null)
        {
            return;
        }

        bool willShow = !shopUIPanel.activeSelf;

        if (willShow)
        {
            if (interactionDetector == null || interactionDetector.CurrentTarget == null)
            {
                return;
            }

            Shop targetShop = interactionDetector.CurrentTarget.GetComponent<Shop>();

            if (targetShop == null)
            {
                return;
            }

            if (shopGridUI != null)
            {
                shopGridUI.SetShop(targetShop);
            }
        }

        shopUIPanel.SetActive(willShow);

        if (willShow)
        {
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
        else
        {
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

}
