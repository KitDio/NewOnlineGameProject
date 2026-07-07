using UnityEngine;
using UnityEngine.AI;

public class Y_EnemyAI : MonoBehaviour
{
    public Transform player;

    public float detectionRange = 10f;
    public float loseRange = 15f;
    public float attackRange = 2f;

    private bool isChasing = false;

    private NavMeshAgent agent;
    private Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
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
                    }
                    else
                    {
                        agent.SetDestination(player.position);
                    }
                }
            }
        }

        // 根据移动速度播放动画
        anim.SetFloat("Speed", agent.velocity.magnitude > 0.1f ? 1f : 0f);
    }
}