using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Y_ChestEnemy : MonoBehaviour
{
    public float wakeDistance = 5f;

    private Animator anim;
    private Y_EnemyAI enemyAI;

    private bool isOpening = false;

    public GameObject healthBar;

    private AudioSource audioSource;
    public AudioClip openClip;
    void Start()
    {
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<Y_EnemyAI>();

        anim.SetBool("close", true);

        enemyAI.canAct = false;

        healthBar.SetActive(false);
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        enemyAI.FindNewTarget();

        if (enemyAI.player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            enemyAI.player.position);

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
        audioSource.PlayOneShot(openClip);
        yield return new WaitForSeconds(1.6f);

        enemyAI.canAct = true;

        healthBar.SetActive(true);
    }

    IEnumerator CloseChest()
    {
        audioSource.PlayOneShot(openClip);
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        agent.ResetPath();
        agent.velocity = Vector3.zero;

        enemyAI.canAct = false;

        anim.SetBool("close", true);

        healthBar.SetActive(false);

        isOpening = false;

        yield return null;
    }
}