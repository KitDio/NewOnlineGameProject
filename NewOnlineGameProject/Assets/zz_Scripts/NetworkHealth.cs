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
    public float respawnDelay = 10f;
    public string spawnPointTag = "Respawn";

    [Header("Events (Local Only)")]
    public UnityEvent onTakeDamage;
    public UnityEvent onDeath;

    private Animator animator;
    private CharacterController characterController;
    private Transform[] allSpawnPoints;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    // ================= 扣血逻辑 =================

    public void ApplyDamage(float damage)
    {
        if (currentHealth <= 0) return;
        photonView.RPC(nameof(RpcTakeDamage), RpcTarget.All, damage);
    }

    [PunRPC]
    public void RpcTakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 受到了 {damage} 点伤害，当前血量：{currentHealth}");

        // 【核心修改】只在本地客户端触发受击表现（屏幕血条、红框闪烁）
        if (photonView.IsMine)
        {
            onTakeDamage?.Invoke();
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    // ================= 回血逻辑 (新增) =================

    public void ApplyHeal(float healAmount)
    {
        if (currentHealth <= 0 || currentHealth >= maxHealth) return;
        photonView.RPC(nameof(RpcHeal), RpcTarget.All, healAmount);
    }

    [PunRPC]
    public void RpcHeal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log($"{gameObject.name} 恢复了 {healAmount} 点生命值，当前血量：{currentHealth}");

    }

    // ================= 死亡与复活 =================

    private void Die()
    {
        // 死亡 UI 也通常只给本地玩家看
        if (photonView.IsMine)
        {
            onDeath?.Invoke();
        }

        photonView.RPC(nameof(RpcSetDeadState), RpcTarget.All, true);

        if (photonView.IsMine)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    [PunRPC]
    public void RpcSetDeadState(bool isDead)
    {
        if (animator != null) animator.SetBool("IsDead", isDead);
        if (characterController != null) characterController.enabled = !isDead;
    }

    private IEnumerator RespawnRoutine()
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.StartRespawnCountdown(respawnDelay);
        }

        yield return new WaitForSeconds(respawnDelay);

        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);
        if (spawnObjects.Length > 0)
        {
            Transform randomSpawnPoint = spawnObjects[Random.Range(0, spawnObjects.Length)].transform;
            transform.position = randomSpawnPoint.position;
            transform.rotation = randomSpawnPoint.rotation;
        }

        photonView.RPC(nameof(RpcRevive), RpcTarget.All);
    }

    [PunRPC]
    public void RpcRevive()
    {
        currentHealth = maxHealth;
        RpcSetDeadState(false);

    }
}