using UnityEngine;
using UnityEngine.AI;

public class Y_EnemyAI : MonoBehaviour
{
    public Transform player;
    public Transform aimPoint;

    public float detectionRange = 10f;
    public float loseRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float nextAttackTime = 0f;

    public Transform bulletSpawnPointLeft;
    public Transform bulletSpawnPointRight;

    public GameObject bulletPrefab;

    public float bulletSpeed = 10f;

    private Y_PlayerHealth playerHealth;

    private bool isChasing = false;

    private NavMeshAgent agent;
    private Animator anim;

    private Y_EnemyHealth health;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        health = GetComponent<Y_EnemyHealth>();

        if (player != null)
        {
            playerHealth = player.GetComponent<Y_PlayerHealth>();
        }
    }

    void Update()
    {
        if (health != null && health.IsDead)
        {
            agent.ResetPath();
            return;
        }

        anim.SetBool("shoot", false);

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (!isChasing)
            {
                if (distance <= detectionRange)
                {
                    isChasing = true;
                }
            }
            else
            {
                if (distance >= loseRange)
                {
                    isChasing = false;
                    agent.ResetPath();
                }
                else
                {
                    if (distance <= attackRange)
                    {
                        agent.ResetPath();

                        if (Time.time >= nextAttackTime)
                        {
                            anim.SetBool("shoot", true);

                            Vector3 target = aimPoint.position;

                            bulletSpawnPointLeft.LookAt(target);
                            bulletSpawnPointRight.LookAt(target);

                            if (bulletPrefab != null)
                            {
                                GameObject bulletL = Instantiate(
                                    bulletPrefab,
                                    bulletSpawnPointLeft.position,
                                    bulletSpawnPointLeft.rotation);

                                bulletL.GetComponent<Rigidbody>().velocity =
                                    bulletSpawnPointLeft.forward * bulletSpeed;

                                GameObject bulletR = Instantiate(
                                    bulletPrefab,
                                    bulletSpawnPointRight.position,
                                    bulletSpawnPointRight.rotation);

                                bulletR.GetComponent<Rigidbody>().velocity =
                                    bulletSpawnPointRight.forward * bulletSpeed;
                            }

                            nextAttackTime = Time.time + attackCooldown;
                        }
                    }
                    else
                    {
                        agent.SetDestination(player.position);

                        anim.SetBool("shoot", false);
                    }
                }
            }
        }

        // 根据移动速度播放动画
        anim.SetFloat("Speed", agent.velocity.magnitude > 0.1f ? 1f : 0f);
    }
}