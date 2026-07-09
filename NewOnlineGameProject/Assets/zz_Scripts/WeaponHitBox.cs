using Photon.Pun;
using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    public float weaponDamage = 20f;

    // 伤害判定开关，由玩家的攻击状态统一控制
    public bool isDamageEnabled = false;

    private void OnTriggerEnter(Collider other)
    {
        PhotonView playerPV = GetComponentInParent<PhotonView>();

        if (!playerPV.IsMine)
            return;

        // 如果当前没有开启伤害判定（比如只是拿在手里没挥刀），直接忽略
        if (!isDamageEnabled) return;

        Debug.Log($"剑碰到了物体：{other.name}");

        // 如果碰到了标签为 Enemy 的物体
        if (other.CompareTag("Enemy"))
        {
            // 尝试获取对方身上的 NetworkHealth 脚本
            Y_EnemyHealth enemyHealth = other.GetComponent<Y_EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.ApplyDamage((int)weaponDamage);

                isDamageEnabled = false;
            }
        }
    }
}