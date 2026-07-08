using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Y_ChestEnemy : MonoBehaviour
{
    public float wakeDistance = 5f;
    public Transform player;

    private Animator anim;
    private Y_EnemyAI enemyAI;

    private bool isOpening = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<Y_EnemyAI>();

        anim.SetBool("close", true);

        enemyAI.canAct = false;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // 还没有变身
        if (!enemyAI.canAct)
        {
            if (distance <= wakeDistance && !isOpening)
            {
                isOpening = true;

                anim.SetBool("close", false);

                StartCoroutine(OpenChest());
            }
        }
        // 已经变身
        else
        {
            if (distance > enemyAI.loseRange)
            {
                StartCoroutine(CloseChest());
            }
        }
    }

    IEnumerator OpenChest()
    {
        yield return new WaitForSeconds(1.6f);

        enemyAI.canAct = true;
    }

    IEnumerator CloseChest()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        agent.ResetPath();
        agent.velocity = Vector3.zero;

        enemyAI.canAct = false;

        anim.SetBool("close", true);

        isOpening = false;

        yield return null;
    }
}