using UnityEngine;

public class ZoneItem : MonoBehaviour
{
    [SerializeField] private int hpChange = 0;
    [SerializeField] private LayerMask targetLayer;

    private void OnTriggerEnter(Collider collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
        {
            return;
        }

        HealthPointManager hpManager = collision.transform.root.GetComponent<HealthPointManager>();

        if (hpManager == null)
        {
            return;
        }

        if (ApplyHpChange(collision, hpManager))
        {
            Destroy(gameObject);
        }        
    }

    private bool ApplyHpChange(Collider collision, HealthPointManager hpManager)
    {
        if (hpChange < 0)
        {
            Debug.Log($"<color=cyan>{gameObject.name}</color> dps <color=magenta>{collision.gameObject.name}</color> <color=white>{-hpChange}</color> Magical");
            hpManager.TakeDamage(-hpChange);
            return true;
        }
        else if (hpChange > 0 && hpManager.CurrentHp != hpManager.MaxHp)
        {
            Debug.Log($"<color=cyan>{gameObject.name}</color> heals <color=magenta>{collision.gameObject.name}</color> <color=white>{hpChange}</color> Magical");
            hpManager.Heal(hpChange);
            return true;
        }

        return false;
    }
}
