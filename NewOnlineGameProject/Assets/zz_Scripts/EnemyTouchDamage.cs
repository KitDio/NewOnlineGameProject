using UnityEngine;

public class EnemyTouchDamage : MonoBehaviour
{
    [Header("伤害数值")]
    public float touchDamage = 10f;

    // 简单的冷却时间，防止玩家一直站在怪物身上被一秒扣血 60 次
    public float damageCooldown = 1f;
    private float lastDamageTime = 0f;

    // 【核心修改】将 OnCollisionStay 改为 OnTriggerStay，参数从 Collision 变成了 Collider
    private void OnTriggerStay(Collider other)
    {
        // 探针 1：看有没有发生触发
        Debug.Log($"怪物触发器碰到了：{other.gameObject.name}，它的 Tag 是：{other.gameObject.tag}");

        // 检查冷却时间
        if (Time.time - lastDamageTime < damageCooldown) return;

        // 判断碰到的是不是玩家
        if (other.gameObject.CompareTag("Player"))
        {
            // 【核心修改】使用 GetComponentInParent 向上寻找血量脚本
            NetworkHealth playerHealth = other.gameObject.GetComponentInParent<NetworkHealth>();

            // 探针 2：看有没有成功抓取到血量脚本
            if (playerHealth == null)
            {
                Debug.LogError("碰到了玩家，但是没在父层级找到 NetworkHealth 脚本！");
            }
            else
            {
                playerHealth.ApplyDamage(touchDamage);
                lastDamageTime = Time.time; // 重置冷却
            }
        }
    }
}