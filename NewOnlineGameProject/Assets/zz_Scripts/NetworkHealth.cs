using UnityEngine;
using Photon.Pun;
using UnityEngine.Events; // 用于触发本地的 UI 或特效更新

public class NetworkHealth : MonoBehaviourPun
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events (Local Only)")]
    // 利用 UnityEvent 方便在面板上拖拽挂载受击特效、音效或 UI 更新
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 这是一个公有方法，供本地的武器碰撞检测脚本调用
    public void ApplyDamage(float damage)
    {
        // 只有房主（如果是打怪）或者本地玩家（如果是PVP）有权判定伤害
        // 对于 PVE 怪物，通常由 MasterClient 来统一扣血防作弊
        // 但为了我们现在的初步测试，直接发送 RPC 给所有人
        photonView.RPC(nameof(RpcTakeDamage), RpcTarget.All, damage);
    }

    // [PunRPC] 标签说明这个函数可以通过网络被远程调用
    [PunRPC]
    public void RpcTakeDamage(float damage)
    {
        // 扣除血量
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 受到了 {damage} 点伤害，当前血量：{currentHealth}");

        // 触发本地的受击表现（比如更新血条 UI，播放受击动画等）
        onTakeDamage?.Invoke();

        // 死亡判定
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 已经死亡！");
        onDeath?.Invoke();

        // 如果是怪物，可能需要在这里播放死亡动画然后销毁
        // 如果是玩家，可能需要触发复活或进入观战模式
        // PhotonNetwork.Destroy(gameObject); // (暂不开启，以免测试时把沙袋直接删了)
    }
}