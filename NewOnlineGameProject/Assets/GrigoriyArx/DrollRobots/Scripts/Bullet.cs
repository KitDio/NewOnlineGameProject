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
            Y_PlayerHealth player = collision.gameObject.GetComponentInParent<Y_PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage);

                ApplySpecialEffect(player.gameObject);
            }
        }

        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal); // turn to Normal
        Vector3 pos = contact.point;

        if (HitSplash != null)
        {
            var hitVFX = Instantiate(HitSplash, pos, rot);
        }

        Destroy(gameObject);
        //Debug.Log("Collision happened");
    }
}
