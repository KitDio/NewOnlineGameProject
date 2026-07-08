using UnityEngine;

public class Y_PlayerAttack : MonoBehaviour
{
    public Camera playerCamera;
    public float attackDistance = 20f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(
    playerCamera.transform.position,
    playerCamera.transform.forward,
    out hit,
    attackDistance))
            {
                Debug.Log("Hit: " + hit.collider.name);

                /*Y_EnemyHealth enemy = hit.collider.GetComponent<Y_EnemyHealth>();

                if (enemy != null)
                {
                    enemy.TakeDamage(20);
                }*/
            }
        }
    }
}