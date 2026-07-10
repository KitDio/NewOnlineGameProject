using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviourPun
{
    private Animator animator;


    [Header("Weapon Reference")]
    public WeaponHitBox currentWeapon;

    [Header("Combat States")]
    public bool isAttacking = false; //[cite: 7]
    public bool isHit = false; //[cite: 7]

    void Start()
    {
        animator = GetComponent<Animator>(); //[cite: 7]
    }

    void Update()
    {
        // 网络隔离：只能控制自己的角色
        if (!photonView.IsMine) return; //[cite: 7]

        // 【规则 2】如果正在受击状态中，直接 return，禁止任何攻击输入
        if (isHit) return; //[cite: 7]

        // 监听鼠标左键点击 (直接调用新输入系统的底层 API，无需修改 Controls.cs)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) //[cite: 7]
        {
            // 如果当前不在攻击状态，则允许发起攻击
            if (!isAttacking) //[cite: 7]
            {
                PerformAttack(); //[cite: 7]
            }
        }
    }

    private void PerformAttack()
    {
        // 锁上攻击状态，防止连续狂点鼠标导致动画抽搐
        isAttacking = true; //[cite: 7]

        if (currentWeapon != null) currentWeapon.isDamageEnabled = true;

        // 核心修改：不再只呼叫本地的 SetTrigger，而是通知房间里的所有人播放攻击动画
        photonView.RPC(nameof(RpcPlayAttack), RpcTarget.All);
    }

    // 新增：用于全网同步攻击动画的 RPC
    [PunRPC]
    public void RpcPlayAttack()
    {
        animator.SetTrigger("Attack");

        GetComponent<PlayerAudio>().PlayAttack();
    }

    // 这是用来给外界（比如 NetworkHealth 脚本）调用的受击接口
    public void TakeHit()
    {
        if (!photonView.IsMine) return; //[cite: 7]

        // 核心修改：通知房间里的所有人播放受击动画
        photonView.RPC(nameof(RpcPlayHit), RpcTarget.All);

       
    }

    // 新增：用于全网同步受击动画的 RPC
    [PunRPC]
    public void RpcPlayHit()
    {
        // 【规则 1】攻击时被命中，会被打断。所以强制重置攻击状态
        isAttacking = false; //[cite: 7]

        // 锁上受击状态
        isHit = true; //[cite: 7]
        animator.SetTrigger("Hit"); //[cite: 7]
        GetComponent<PlayerAudio>().PlayHit();
    }

    // ==========================================
    // 以下两个方法是提供给 动画事件 (Animation Event) 调用的
    // ==========================================
    public void ResetAttackState()
    {
        isAttacking = false; //[cite: 7]

        if (currentWeapon != null) currentWeapon.isDamageEnabled = false;//防止过去蹭一下也扣血
    }

    public void ResetHitState()
    {
        isHit = false; //[cite: 7]
    }
}