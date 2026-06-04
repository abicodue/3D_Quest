using UnityEngine;

public class InventoryCommandRouter : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory;
    [SerializeField]
    private GameObject inventoryUIPanel;
    [SerializeField]
    private InventoryGridUI inventoryGridUI;
    [SerializeField]
    private PlayerController3D playerController;
    [SerializeField]
    private MeleeHitBoxAttack meleeHitBoxAttack;

    private void Update()
    {
        if (inventory == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventoryPanel();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            inventory.UndoLastPickup();
        }
    }

    private void ToggleInventoryPanel()
    {
        if (inventoryUIPanel == null)
        {
            return;
        }

        bool willShow = !inventoryUIPanel.activeSelf;
        inventoryUIPanel.SetActive(willShow);       

        if (willShow)
        {
            RefreshInventoryGridIfPossible();

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

    private void RefreshInventoryGridIfPossible()
    {
        InventoryGridUI grid = inventoryGridUI;

        if (grid == null && inventoryUIPanel != null)
        {
            grid = inventoryUIPanel.GetComponentInChildren<InventoryGridUI>(true);
        }

        if (grid != null)
        {
            grid.RefreshDisplay();
        }
    }

}
