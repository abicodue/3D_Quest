using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeHitBoxAttack : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MeleeHitBox meleeHitBox;

    [SerializeField] private InputAction attackAction;

    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private string attackTriggerName = "meleeAttack";

    private float lastAttackTime = -999f;
    private bool isAttacking;
    private int attackTriggerHash;

    public event Action OnAttackStarted;
    public event Action OnAttackEnded;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (meleeHitBox == null)
        {
            meleeHitBox = GetComponentInChildren<MeleeHitBox>();
        }

        attackTriggerHash = Animator.StringToHash(attackTriggerName);
    }

    private void OnEnable()
    {
        attackAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.Disable();
    }

    private void Update()
    {
        if (attackAction.WasPressedThisFrame())
        {
            //Debug.Log("attackAction pressed");
            StartAttack();
        }

        /*
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("force trigger");
            animator.SetTrigger(attackTriggerHash);
        }
        */
    }

    private void StartAttack()
    {
        //Debug.Log("StartAttack");

        if (animator == null || meleeHitBox == null)
        {
            Debug.Log("animator == null || meleeHitBox == null");
            return;
        }
        
        if (isAttacking)
        {
            Debug.Log("<color=grey>isAttacking</color> == true");
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log($"OnAttackCooldown: <color=grey>{(lastAttackTime + attackCooldown - Time.time).ToString("F2")}s</color> / {attackCooldown}s left");
            return;
        }

        OnAttackStarted?.Invoke();

        //Debug.Log("SetTrigger");
        animator.SetTrigger(attackTriggerHash);

        isAttacking = true;
        lastAttackTime = Time.time;
    }

    public void EndAttack()
    {
        //Debug.Log("EndAttack");
        isAttacking = false;
        
        if (meleeHitBox != null)
        {
            meleeHitBox.DisableHitBox();
        }

        OnAttackEnded?.Invoke();
    }
}
