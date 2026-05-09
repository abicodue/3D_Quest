using UnityEngine;

public class AnimationEventManager : MonoBehaviour
{
    [SerializeField] private MeleeHitBox meleeHitBox;
    [SerializeField] private MeleeHitBoxAttack meleeHitBoxAttack;

    public void EnableHitBox()
    {
        if (meleeHitBox != null)
        {
            meleeHitBox.EnableHitBox();
        }
    }

    public void DisableHitBox()
    {
        if (meleeHitBox != null)
        {
            meleeHitBox.DisableHitBox();
        }
    }

    public void EndAttack()
    {
        if (meleeHitBoxAttack != null)
        {
            meleeHitBoxAttack.EndAttack();
        }
    }
}