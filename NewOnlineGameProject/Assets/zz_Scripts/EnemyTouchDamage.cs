using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    [Header("伤害数值")]
    public float touchDamage = 10f;

    // 简单的冷却时间，防止玩家一直站在怪物身上被一秒扣血 60 次
    public float damageCooldown = 1f;
    private float lastDamageTime = 0f;

    // 如果怪物用的是 Trigger 碰撞盒，就用 OnTriggerStay；如果是普通碰撞盒，就用 OnCollisionStay
    private void OnCollisionStay(Collision collision)
    {
        // 检查冷却时间
        if (Time.time - lastDamageTime < damageCooldown) return;

        // 判断碰到的是不是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            // 尝试获取玩家身上的网络血量脚本
            NetworkHealth playerHealth = collision.gameObject.GetComponent<NetworkHealth>();

            if (playerHealth != null)
            {
                playerHealth.ApplyDamage(touchDamage);
                lastDamageTime = Time.time; // 重置冷却
            }
        }
    }
}