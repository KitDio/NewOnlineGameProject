using UnityEngine;

public class Y_FireBullet : Bullet
{
    protected override void ApplySpecialEffect(GameObject target)
    {
        Y_PlayerHealth player = target.GetComponent<Y_PlayerHealth>();

        if (player != null)
        {
            player.ApplyBurn();
        }
    }
}