using UnityEngine;

public class Y_IceBullet : Bullet
{
    protected override void ApplySpecialEffect(GameObject target)
    {
        Y_PlayerHealth player = target.GetComponent<Y_PlayerHealth>();

        if (player != null)
        {
            player.ApplyFreeze();
        }
    }
}