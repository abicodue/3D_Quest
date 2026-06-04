using UnityEngine;

public class HurtBox : MonoBehaviour
{
    [SerializeField]
    private HealthPointManager healthPointManager;

    public HealthPointManager HealthPointManager => healthPointManager;

    private void Awake()
    {
        if (healthPointManager == null)
        {
            healthPointManager = GetComponentInParent<HealthPointManager>();
        }
        
    }

}
