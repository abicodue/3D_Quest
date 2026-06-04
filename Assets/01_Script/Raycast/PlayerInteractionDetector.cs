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

    // [추가]
    private WorldItem currentWorldItem;

    // [추가]
    private Shop currentShop;

    public GameObject CurrentTarget => currentTarget;
    public WorldItem CurrentWorldItem => currentWorldItem;
    public Shop CurrentShop => currentShop;

    private void Awake()
    {
        if (m_cam == null)
        {
            m_cam = Camera.main;
        }
    }

    private void Update()
    {
        currentTarget = RaycastDetect();

        // [추가] 현재 타겟에서 상호작용 컴포넌트 캐싱
        CacheCurrentComponents();

        DebugTarget();
    }

    private GameObject RaycastDetect()
    {
        if (m_cam == null)
        {
            return null;
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = m_cam.ScreenPointToRay(screenCenter);

        /*
        [삭제] Trigger Collider를 무시하던 방식

        if (Physics.Raycast(_ray, out RaycastHit hit, m_distance, m_interactableMask, QueryTriggerInteraction.Ignore))
        */

        // [변경] Trigger Collider도 RayCast로 감지
        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                m_distance,
                m_interactableMask,
                QueryTriggerInteraction.Collide))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                return null;
            }

            return hit.collider.gameObject;
        }

        Debug.DrawRay(ray.origin, ray.direction * m_distance, Color.magenta);

        return null;
    }

    // [추가]
    private void CacheCurrentComponents()
    {
        currentWorldItem = null;
        currentShop = null;

        if (currentTarget == null)
        {
            return;
        }

        currentWorldItem = currentTarget.GetComponentInParent<WorldItem>();
        currentShop = currentTarget.GetComponentInParent<Shop>();
    }

    private void DebugTarget()
    {
        if (currentTarget == previousTarget)
        {
            return;
        }

        if (currentWorldItem != null)
        {
            if (currentWorldItem.CanPickup)
            {
                Debug.Log($"[E] 획득하기: {currentWorldItem.DisplayName} / {currentWorldItem.Description}");
            }
            else
            {
                Debug.Log($"획득 불가: {currentWorldItem.DisplayName}");
            }
        }
        else if (currentShop != null)
        {
            Debug.Log($"[E] 상점 열기: {currentShop.name}");
        }
        else if (currentTarget != null)
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