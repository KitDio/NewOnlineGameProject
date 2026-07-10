using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 10f;
    public GameObject HitSplash;
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    public BulletOwner owner;
    public int damage = 20;

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }
    protected virtual void ApplySpecialEffect(GameObject target)
    {

    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (owner == BulletOwner.Player)
        {
            Y_EnemyHealth enemy = collision.gameObject.GetComponentInParent<Y_EnemyHealth>();
            if (enemy != null)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
        else
        {
            NetworkHealth health = collision.gameObject.GetComponentInParent<NetworkHealth>();
            Y_PlayerStatus status = collision.gameObject.GetComponentInParent<Y_PlayerStatus>();

            // 【核心修复 2】：防多重触发！
            // 获取受击玩家的 PhotonView，必须确保“只有受害者自己的电脑”才能处理受击和减速逻辑。
            // 这样可以防止房间里 4 个人同时给你扣血和挂减速。
            PhotonView targetView = collision.gameObject.GetComponentInParent<PhotonView>();

            if (targetView != null && targetView.IsMine)
            {
                if (health != null)
                {
                    health.ApplyDamage(damage);
                }

                if (status != null)
                {
                    ApplySpecialEffect(status.gameObject);
                }
            }
        }

        // 【核心修复 1】：防数组越界报错！
        // 必须先判断 contacts 数组里有没有东西，再去读取 [0]，否则必定报错导致子弹无法销毁！
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal);
            Vector3 pos = contact.point;

            if (HitSplash != null)
            {
                Instantiate(HitSplash, pos, rot);
            }
        }

        // 无论有没有成功生成特效，无论打中了谁，子弹最后必须被强制销毁，杜绝无限贴身粘连！
        Destroy(gameObject);
    }
}
