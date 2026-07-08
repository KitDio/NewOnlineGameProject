using UnityEngine;

public class SpacePlanetDrift : MonoBehaviour
{
    [Header("--- 星体自转设置 (Rotation) ---")]
    [Tooltip("星球自转的速度。通常太空星球主要绕着一个特定轴慢速转动（比如Y轴填 0.5 或 1 就很明显了）")]
    public Vector3 rotationSpeed = new Vector3(0f, 0.5f, 0f);

    [Header("--- 星体太空漂移设置 (Drift) ---")]
    [Tooltip("是否开启太空微弱漂移")]
    public bool enableDrift = true;

    [Tooltip("由于背景星球非常遥远且巨大，漂移幅度需要设得比较大（例如 5 到 20 米），在镜头里才看得出变化")]
    public float driftAmplitude = 10f;

    [Tooltip("太空漂移的速度，一定要极其缓慢（例如 0.05 到 0.1），不然星球会像在跳舞一样在天上乱晃")]
    public float driftSpeed = 0.05f;

    [Header("--- 随机错开 ---")]
    [Tooltip("游戏开始时随机错开时间，防止天上的两个太阳完全同步对称地飘动")]
    public bool randomizeStart = true;

    private Vector3 initialPosition;
    private float randomOffset;

    void Start()
    {
        // 记录星球刚开始在太空中的初始相对位置
        initialPosition = transform.localPosition;

        if (randomizeStart)
        {
            randomOffset = Random.Range(0f, 1000f);
        }
    }

    void Update()
    {
        // 1. 星球匀速自转（乘以 Time.deltaTime 保证帧率平滑）
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

        // 2. 星球缓慢画圈/上下左右多维漂移
        if (enableDrift)
        {
            Vector3 targetPos = initialPosition;

            float timeFactor = (Time.time + randomOffset) * driftSpeed;

            // 使用 Sin 和 Cos 组合，让星球在天空中不是死板地上下动，而是呈现一个极为缓慢的“∞”字形或椭圆形轨迹漂移
            targetPos.x += Mathf.Cos(timeFactor) * driftAmplitude;
            targetPos.y += Mathf.Sin(timeFactor * 2f) * (driftAmplitude * 0.5f);
            targetPos.z += Mathf.Sin(timeFactor) * (driftAmplitude * 0.2f);

            transform.localPosition = targetPos;
        }
    }
}