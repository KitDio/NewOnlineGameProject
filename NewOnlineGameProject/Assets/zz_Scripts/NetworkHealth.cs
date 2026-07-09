using UnityEngine;
using Photon.Pun;
using UnityEngine.Events; // 用于触发本地的 UI 或特效更新
using System.Collections;

public class NetworkHealth : MonoBehaviourPun
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Respawn Settings")]
    public float respawnDelay = 10f; // 复活等待时间
    public string spawnPointTag = "Respawn"; // 我们一会给复活点打个标签方便动态查找

    [Header("Events (Local Only)")]
    // 利用 UnityEvent 方便在面板上拖拽挂载受击特效、音效或 UI 更新
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    private Animator animator;
    private CharacterController characterController;
    private Transform[] allSpawnPoints; // 缓存场景里的所有复活点

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }


    // 这是一个公有方法，供本地的武器碰撞检测脚本调用
    public void ApplyDamage(float damage)
    {
        if (currentHealth <= 0) return;
        photonView.RPC(nameof(RpcTakeDamage), RpcTarget.All, damage); //[cite: 11]
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
        onDeath?.Invoke(); //[cite: 11]

        // 告诉全网：我死了，播放死亡动画，关闭碰撞体
        photonView.RPC(nameof(RpcSetDeadState), RpcTarget.All, true);

        // 【核心】只有本地玩家负责跑复活倒计时和传送逻辑！
        if (photonView.IsMine)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    [PunRPC]
    public void RpcSetDeadState(bool isDead)
    {
        // 触发你配置好的 Animator 逻辑
        if (animator != null)
        {
            animator.SetBool("IsDead", isDead);
        }

        // 死亡时禁用角色控制器和物理碰撞，防止阻挡别人或掉下虚空
        if (characterController != null)
        {
            characterController.enabled = !isDead;
        }

        // 如果你有 PlayerCombat，也可以在这里禁用它，防止死人开枪
        // GetComponent<PlayerCombat>().enabled = !isDead;
    }

    private IEnumerator RespawnRoutine()
    {
        // 1. 呼叫 HUD 显示倒计时
        if (HUDManager.Instance != null)
        {


            HUDManager.Instance.StartRespawnCountdown(respawnDelay);
        }

        // 2. 等待设定的复活时间
        yield return new WaitForSeconds(respawnDelay);

        // 3. 动态寻找场景里的复活点并随机四选一
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);
        if (spawnObjects.Length > 0)
        {
            Transform randomSpawnPoint = spawnObjects[Random.Range(0, spawnObjects.Length)].transform;

            // 【传送大坑】必须确保 CharacterController 处于 disabled 状态才能修改 position
            transform.position = randomSpawnPoint.position;
            transform.rotation = randomSpawnPoint.rotation;
        }

        // 4. 恢复满血，通知全网我复活了 (关闭死亡动画，重新开启物理组件)
        photonView.RPC(nameof(RpcRevive), RpcTarget.All);
    }

    [PunRPC]
    public void RpcRevive()
    {
        currentHealth = maxHealth;
        RpcSetDeadState(false); // 调用上面的 RPC 恢复站立动画和控制器
    }
}