using UnityEngine;

public class ZoneField : MonoBehaviour
{
    [SerializeField] private int hpChange = 0;
    [SerializeField] private float interval = 1f;
    [SerializeField] private LayerMask targetLayer;

    private float timer = 0f;

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"ZoneField Enter: {collision.gameObject.name}");

        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
        {
            return;
        }

        HealthPointManager hpManager = collision.transform.root.GetComponent<HealthPointManager>();

        if (hpManager == null)
        {
            return;
        }

        timer = 0f;       
        ApplyHpChange(collision, hpManager);        
    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log($"ZoneField Stay: {collision.gameObject.name}");

        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
        {
            return;
        }

        HealthPointManager hpManager = collision.transform.root.GetComponent<HealthPointManager>();

        if (hpManager == null)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            ApplyHpChange(collision, hpManager);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log($"ZoneField Exit: {collision.gameObject.name}");

        if (((1 << collision.gameObject.layer) & targetLayer) == 0)
        {
            return;
        }

        timer = 0f;
    }

    private void ApplyHpChange(Collision collision, HealthPointManager hpManager)
    {
        if (hpChange < 0)
        {
            Debug.Log($"<color=cyan>{gameObject.name}</color> dps <color=magenta>{collision.gameObject.name}</color> <color=white>{-hpChange}</color> Magical");
            hpManager.TakeDamage(-hpChange);
        }
        else if (hpChange > 0 && hpManager.CurrentHp != hpManager.MaxHp)
        {
            Debug.Log($"<color=cyan>{gameObject.name}</color> heals <color=magenta>{collision.gameObject.name}</color> <color=white>{hpChange}</color> Magical");
            hpManager.Heal(hpChange);
        }
    }
}

