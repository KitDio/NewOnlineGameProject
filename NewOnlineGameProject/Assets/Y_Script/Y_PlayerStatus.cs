using Photon.Pun;
using Synty.AnimationBaseLocomotion.Samples;
using System.Collections;
using UnityEngine;


public class Y_PlayerStatus : MonoBehaviour
{

    public int maxHealth = 100;
    private int currentHealth;
    public bool IsDead { get; private set; } = false;

    public Y_StatusOverlay statusOverlay;
    private bool isBurning = false;
    private bool isFrozen = false;

    private SamplePlayerAnimationController playerController;

    void Start()
    {
        currentHealth = maxHealth;

        playerController = GetComponent<SamplePlayerAnimationController>();
    }

    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        IsDead = true;

        Debug.Log("Player Dead");
    }
    public void ApplyBurn()
    {
        if (isBurning)
            return;

        isBurning = true;

        Debug.Log("Player Burning");

        if (statusOverlay != null)
        {
            statusOverlay.ShowBurn(true);
        }

        StartCoroutine(BurnCoroutine());
    }

    [PunRPC]
    public void RpcApplyFreeze()
    {
        if (isFrozen) return;
        isFrozen = true;

        Debug.Log("Player Frozen");
        if (statusOverlay != null)
        {
            statusOverlay.ShowFreeze(true);
        }

        // 【核心修改】呼叫咱们的背包移速大管家，传入 0.4f (保留40%移速) 和持续时间 3f
        PlayerWeightController weightController = GetComponent<PlayerWeightController>();
        if (weightController != null)
        {
            weightController.ApplySpeedDebuff(0.6f, 3f);
        }

        // UI 表现的协程依然可以保留
        StartCoroutine(FreezeUI_Coroutine());
    }

    // UI 显示的协程 (只负责在3秒后关掉屏幕上的冰冻特效)
    IEnumerator FreezeUI_Coroutine()
    {
        yield return new WaitForSeconds(3f);

        if (statusOverlay != null)
        {
            statusOverlay.ShowFreeze(false);
        }
        isFrozen = false;
    }

    IEnumerator BurnCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);

            TakeDamage(2);

            Debug.Log("Burn Damage");
        }

        isBurning = false;

        if (statusOverlay != null)
        {
            statusOverlay.ShowBurn(false);
        }

        Debug.Log("Burn End");
    }

}