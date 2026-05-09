using System.Collections.Generic;
using UnityEngine;

public class MeleeHitBox : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayer;    
    private HashSet<HealthPointManager> hitTargets = new HashSet<HealthPointManager>();
    private GameObject owner;
    private Collider myCollider;
    private bool isActive = false;

    private void Awake()
    {
        owner = transform.root.gameObject;
        myCollider = GetComponent<Collider>();

        if (myCollider == null)
        {
            Debug.LogWarning("[MeleeHitBox] Collider is missing.");
            return;
        }

        //Debug.Log($"[MeleeHitBox] Collider found on {gameObject.name}: {myCollider.GetType().Name}");

        myCollider.isTrigger = true;
        myCollider.enabled = false;
    }

    public void EnableHitBox()
    {
        //Debug.Log("EnableHitBox");
        isActive = true;
        hitTargets.Clear();

        if (myCollider != null)
        {
            myCollider.enabled = true;
        }
    }

    public void DisableHitBox()
    {
        //Debug.Log("DisableHitBox");
        isActive = false;

        if (myCollider != null)
        {
            myCollider.enabled = false;
        }

        hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        ApplyHitBox(other); 
    }

    private void OnTriggerStay(Collider other)
    {
        ApplyHitBox(other);
    }

    private void ApplyHitBox(Collider other)
    {
        if (!isActive)
        {
            return;
        }

        if (owner != null && other.transform.root == owner.transform.root)
        {
            return;
        }

        if (((1 << other.gameObject.layer) & targetLayer) == 0)
        {
            return;
        }

        HurtBox hurtBox = other.GetComponent<HurtBox>();

        if (hurtBox == null)
        {
            return;
        }

        HealthPointManager hp = hurtBox.HealthPointManager;

        if (hp == null)
        {
            return;
        }

        if (hitTargets.Contains(hp))
        {
            return;
        }

        hitTargets.Add(hp);
        Debug.Log($"<color=magenta>{transform.root.name}</color> hit <color=cyan>{other.name}</color> <color=white>{damage}</color> Physical");
        hp.TakeDamage(damage);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.red : Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            return;
        }        

        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
    }
}
