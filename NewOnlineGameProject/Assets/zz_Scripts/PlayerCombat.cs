using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem; // 引入新输入系统用于读取鼠标

public class PlayerCombat : MonoBehaviourPun
{
    private Animator animator;

    [Header("Combat States")]
    public bool isAttacking = false;
    public bool isHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 网络隔离：只能控制自己的角色
        if (!photonView.IsMine) return;

        // 【规则 2】如果正在受击状态中，直接 return，禁止任何攻击输入
        if (isHit) return;

        // 监听鼠标左键点击 (直接调用新输入系统的底层 API，无需修改 Controls.cs)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 如果当前不在攻击状态，则允许发起攻击
            if (!isAttacking)
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        // 锁上攻击状态，防止连续狂点鼠标导致动画抽搐
        isAttacking = true;
        animator.SetTrigger("Attack");

        // （后续你可以在这里或者在动画事件里，加上挥刀的物理碰撞检测代码）
    }

    // 这是用来给外界（比如 NetworkHealth 脚本）调用的受击接口
    public void TakeHit()
    {
        if (!photonView.IsMine) return;

        // 【规则 1】攻击时被命中，会被打断。所以强制重置攻击状态
        isAttacking = false;

        // 锁上受击状态
        isHit = true;
        animator.SetTrigger("Hit");
    }

    // ==========================================
    // 以下两个方法是提供给 动画事件 (Animation Event) 调用的
    // ==========================================
    public void ResetAttackState()
    {
        isAttacking = false;
    }

    public void ResetHitState()
    {
        isHit = false;
    }
}