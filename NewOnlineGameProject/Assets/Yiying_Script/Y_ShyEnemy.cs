using UnityEngine;

public class Y_ShyEnemy : MonoBehaviour
{
    public Transform player;
    private Y_EnemyAI enemyAI;

    [Range(-1f, 1f)]
    public float viewThreshold = 0.7f;

    private Animator anim;

    void Start()
    {
        enemyAI = GetComponent<Y_EnemyAI>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 toEnemy = (transform.position - player.position).normalized;

        float dot = Vector3.Dot(player.forward, toEnemy);

        if (dot > viewThreshold)
        {
            enemyAI.canAct = false;
        }
        else
        {
            enemyAI.canAct = true;
        }
    }
}