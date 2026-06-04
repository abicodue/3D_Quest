using UnityEngine;

public class WorldSpaceUIManager : MonoBehaviour
{
    [SerializeField]
    private Transform cameraPivot;

    private void Awake()
    {
        if (cameraPivot == null)
        {
            Debug.LogWarning("cameraPivot is missing");
        }
    }

    private void LateUpdate()
    {
        transform.forward = cameraPivot.forward;
    }
}
