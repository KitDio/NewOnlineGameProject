using UnityEngine;

public class SpaceShipHover : MonoBehaviour
{
    [Header("--- 位置上下起伏 (Translation) ---")]
    [Tooltip("飞船上下浮动的最大距离（米）。背景飞船建议设在 1 到 5 米之间，看它离相机的远近")]
    public float positionAmplitude = 2f;
    [Tooltip("上下浮动的速度（频率）")]
    public float positionSpeed = 0.4f;

    [Header("--- 船身姿态倾斜 (Rotation) ---")]
    [Tooltip("是否开启飞船角度的微弱晃动（模拟低头/抬头、左右侧倾）")]
    public bool enableRotationHover = true;
    [Tooltip("飞船前后/左右晃动的最大角度（度）。通常 1 到 3 度就会有很明显的钢铁巨兽感")]
    public float rotationAmplitude = 1.5f;
    [Tooltip("角度晃纳的速度（频率），建议和位置速度错开，看起来更自然")]
    public float rotationSpeed = 0.3f;

    [Header("--- 节奏随机 ---")]
    [Tooltip("游戏开始时随机错开时间，防止多艘飞船晃动的节奏完全一模一样")]
    public bool randomizeStart = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float timeOffset;

    void Start()
    {
        // 记录飞船刚开始摆放在场景里的初始位置和角度
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        if (randomizeStart)
        {
            timeOffset = Random.Range(0f, 500f);
        }
    }

    void Update()
    {
        float timeFactor = Time.time + timeOffset;

        // 1. 核心位置悬浮逻辑（使用 Sin 正弦波）
        Vector3 targetPos = initialPosition;
        // 让 X 轴也带有一点点极微弱的左右平移，组合成圆润的运动轨迹
        targetPos.x += Mathf.Cos(timeFactor * positionSpeed * 0.7f) * (positionAmplitude * 0.3f);
        targetPos.y += Mathf.Sin(timeFactor * positionSpeed) * positionAmplitude;
        transform.localPosition = targetPos;

        // 2. 核心姿态角度晃动逻辑
        if (enableRotationHover)
        {
            // 计算 X 轴（抬头/低头）和 Z 轴（左右翻滚侧倾）的微弱旋转偏移
            float pitch = Mathf.Sin(timeFactor * rotationSpeed) * rotationAmplitude; // 抬头低头
            float roll = Mathf.Cos(transform.position.y + timeFactor * rotationSpeed * 0.8f) * (rotationAmplitude * 0.6f); // 左右侧倾
            float yaw = Mathf.Sin(timeFactor * rotationSpeed * 0.5f) * (rotationAmplitude * 0.2f); // 左右微弱摆头

            // 基于飞船的初始旋转，叠加这层微弱的太空姿态起伏
            transform.localRotation = initialRotation * Quaternion.Euler(pitch, yaw, roll);
        }
    }
}