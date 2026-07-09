using UnityEngine;
using UnityEngine.UI;

public class Y_EnemyHealthBar : MonoBehaviour
{
    public Y_EnemyHealth enemyHealth;
    public Slider slider;

    void Start()
    {
        slider.maxValue = enemyHealth.maxHealth;
        slider.value = enemyHealth.currentHealth;
    }

    void Update()
    {
        Debug.Log(
    $"HealthBar:{gameObject.name}  Enemy:{enemyHealth.gameObject.name}  HP:{enemyHealth.currentHealth}"
);

        slider.value = enemyHealth.currentHealth;
    }
}