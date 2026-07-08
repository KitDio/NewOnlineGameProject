using UnityEngine;

public class WeaponHitBox : MonoBehaviour
{
    public float weaponDamage = 20f;

    // 伤害判定开关，由玩家的攻击状态统一控制
    public bool isDamageEnabled = false;

    private void OnTriggerEnter(Collider other)
    {
        // 如果当前没有开启伤害判定（比如只是拿在手里没挥刀），直接忽略
        if (!isDamageEnabled) return;

        Debug.Log($"剑碰到了物体：{other.name}");

        // 如果碰到了标签为 Enemy 的物体
        if (other.CompareTag("Enemy"))
        {
            // 尝试获取对方身上的 NetworkHealth 脚本
            NetworkHealth enemyHealth = other.GetComponent<NetworkHealth>();
            if (enemyHealth != null)
            {
                // 调用公有方法扣血
                enemyHealth.ApplyDamage(weaponDamage);

                // 核心手感优化：砍中一次后立刻关闭判定，防止同一个挥刀动作里每一帧都触发一次扣血
                isDamageEnabled = false;
            }
        }
    }
}