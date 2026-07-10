using UnityEngine;

public class Y_IceBullet : Bullet
{
    protected override void ApplySpecialEffect(GameObject target)
    {
        Y_PlayerStatus player = target.GetComponent<Y_PlayerStatus>();

        if (player != null)
        {
            player.RpcApplyFreeze();
        }
    }
}