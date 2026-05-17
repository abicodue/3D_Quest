using UnityEngine;

public class PlayerInteractionDetector : MonoBehaviour
{
    [SerializeField]
    private Camera m_cam;
    [SerializeField]
    private LayerMask m_interactableMask;
    [SerializeField]
    private float m_distance = 5.0f;

    private GameObject currentTarget;
    private GameObject previousTarget;
    public GameObject CurrentTarget => currentTarget;

    private void Awake()
    {
        if (m_cam == null) m_cam = Camera.main;

    }

    private void Update()
    {
        currentTarget = RaycastDetect();
        DebugTarget();
    }

    private GameObject RaycastDetect()
    {
        Vector2 _screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray _ray = m_cam.ScreenPointToRay(_screenCenter);

        if (Physics.Raycast(_ray, out RaycastHit hit, m_distance, m_interactableMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(_ray.origin, _ray.direction * hit.distance, Color.green);

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                return null;
            }

            return hit.collider.gameObject;
        }

        Debug.DrawRay(_ray.origin, _ray.direction * m_distance, Color.purple);

        return null;
    }

    private void DebugTarget()
    {
        if (currentTarget == previousTarget)
        {
            return;
        }

        if (currentTarget != null)
        {
            Debug.Log($"[E] {currentTarget.name}");
        }
        else
        {
            Debug.Log("nothing to interact");
        }

        previousTarget = currentTarget;

    }

}
