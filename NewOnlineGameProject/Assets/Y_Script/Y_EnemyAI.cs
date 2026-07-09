using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
public class Y_EnemyAI : MonoBehaviour
{
    public Transform player;
    private Transform aimPoint;
    private Y_PlayerStatus playerHealth;

    public float detectionRange = 10f;
    public float loseRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float nextAttackTime = 0f;

    public Transform bulletSpawnPointLeft;
    public Transform bulletSpawnPointRight;

    public GameObject bulletPrefab;

    public float bulletSpeed = 10f;

    private bool isChasing = false;

    public bool canAct = true;

    private NavMeshAgent agent;
    private Animator anim;

    private Y_EnemyHealth health;

    [Header("Wander")]
    public float wanderRadius = 5f;
    public float wanderInterval = 4f;

    private float nextWanderTime;
    private Vector3 wanderTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        health = GetComponent<Y_EnemyHealth>();

    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (health != null && health.IsDead)
        {
            agent.ResetPath();
            return;
        }

        if (!canAct)
        {
            if (player == null)
            {
                FindNewTarget();
            }

            agent.ResetPath();
            agent.velocity = Vector3.zero;

            anim.SetFloat("Speed", 0f);
            anim.SetBool("shoot", false);

            return;
        }

        anim.SetBool("shoot", false);

        if (player == null)
        {
            FindNewTarget();
        }

        if (player != null)
        {
            if (playerHealth != null && playerHealth.IsDead)
            {
                player = null;
                aimPoint = null;
                playerHealth = null;

                isChasing = false;
                agent.ResetPath();

                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);

            PhotonView pv = player.GetComponent<PhotonView>();

            if (!isChasing)
            {
                Wander();

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

                    player = null;
                    aimPoint = null;
                    playerHealth = null;

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
                        Debug.Log("目的地：" + player.position);
                        agent.SetDestination(player.position);

                        anim.SetBool("shoot", false);
                    }
                }
            }
        }

        // 根据移动速度播放动画
        anim.SetFloat("Speed", agent.velocity.magnitude > 0.1f ? 1f : 0f);
    }

    void Wander()
    {
        if (Time.time < nextWanderTime)
            return;

        Vector3 randomPos = transform.position +
                            Random.insideUnitSphere * wanderRadius;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPos, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        nextWanderTime = Time.time + wanderInterval;
    }

    public void FindNewTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); 

        // 如果已经在追击中，搜索半径放宽到 loseRange；否则仅在 detectionRange 内搜索
        float currentSearchRadius = isChasing ? loseRange : detectionRange; 
        float closestDistance = currentSearchRadius;

        Transform potentialTarget = null;
        Y_PlayerStatus potentialHealth = null;
        Transform potentialAimPoint = null;

        foreach (GameObject p in players) 
        {
            Y_PlayerStatus ph = p.GetComponent<Y_PlayerStatus>(); 
            
            // 忽略已经死亡的玩家
            if (ph != null && ph.IsDead) continue;

            float distance = Vector3.Distance(transform.position, p.transform.position); 

            // 寻找距离最近的玩家
            if (distance < closestDistance) 
            {
                closestDistance = distance; 
                potentialTarget = p.transform;
                potentialHealth = ph;
                potentialAimPoint = p.transform.Find("AimPoint"); 
            }
        }

        // 应用最新找到的目标
        player = potentialTarget;
        playerHealth = potentialHealth;
        aimPoint = potentialAimPoint;

        // 如果找到了目标，标记为正在追击；如果所有玩家都跑出了 loseRange，取消追击
        isChasing = (player != null);
    }
}