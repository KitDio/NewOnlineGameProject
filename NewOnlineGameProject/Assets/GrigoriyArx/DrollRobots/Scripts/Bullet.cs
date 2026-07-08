using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (owner == BulletOwner.Player)
        {
            Y_EnemyHealth enemy = collision.gameObject.GetComponentInParent<Y_EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(20);
            }
        }
        else
        {
            Y_PlayerHealth player = collision.gameObject.GetComponentInParent<Y_PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(20);
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
