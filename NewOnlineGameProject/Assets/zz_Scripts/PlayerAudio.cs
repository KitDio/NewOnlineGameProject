using UnityEngine;



[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    // 原来的音源，负责攻击、受击、跳跃等“一次性”声音
    private AudioSource actionAudioSource;

    // 【新增】专门负责移动声音的音源
    private AudioSource movementAudioSource;

    [Header("Audio Clips")]
    public AudioClip walkLoopClip; // 【修改】放入你那段完整的走路循环音频
    public AudioClip runLoopClip;  // 【修改】放入你那段完整的跑步循环音频

    public AudioClip jumpClip;
    public AudioClip attackClip;
    public AudioClip hitClip;

    // 在头部的变量声明区加上：
    public AudioClip deathPromptClip; // 死亡提示音效


    void Start()
    {
        actionAudioSource = GetComponent<AudioSource>();

        // 动态给玩家再加一个喇叭，专门用来播脚步声循环
        movementAudioSource = gameObject.AddComponent<AudioSource>();
        movementAudioSource.spatialBlend = 1f; // 确保它是 3D 音效
        movementAudioSource.loop = true;       // 【核心】开启循环播放
    }

    // 【新增】由外部移动脚本统一调用的状态接口
    // gaitState 对应你动画控制器的步态: 0=Idle, 1=Walk, 2=Run, 3=Sprint
    public void UpdateMovementSound(int gaitState)
    {
        // 如果玩家站着不动，或者跳在空中
        if (gaitState == 0)
        {
            movementAudioSource.Stop();
            return;
        }

        // 判断当前该播走路还是跑步 (假设 Sprint 和 Run 用同一种高频脚步声)
        AudioClip targetClip = (gaitState == 1) ? walkLoopClip : runLoopClip;
        float targetVolume = (gaitState == 1) ? 0.4f : 0.7f; // 跑步声稍微大一点

        // 如果现在正在播的声音不是目标声音，或者根本没在播放，就切换并播放
        if (movementAudioSource.clip != targetClip || !movementAudioSource.isPlaying)
        {
            movementAudioSource.clip = targetClip;
            movementAudioSource.volume = targetVolume;
            movementAudioSource.Play();
        }
    }

    // 下面这些一次性动作的声音保持不变，依然使用 actionAudioSource
    public void PlayJump()
    {
        if (jumpClip != null) actionAudioSource.PlayOneShot(jumpClip, 0.8f);
    }

    public void PlayAttack()
    {
        if (attackClip != null) actionAudioSource.PlayOneShot(attackClip, 1.0f);
    }

    public void PlayHit()
    {
        if (hitClip != null) actionAudioSource.PlayOneShot(hitClip, 1.0f);
    }


    // 在脚本的下方加上播放方法：
    public void PlayDeathPrompt()
    {
        // 死亡提示音通常比较重要，音量可以稍微大一点 (比如 1.0f)
        if (deathPromptClip != null)
        {
            actionAudioSource.PlayOneShot(deathPromptClip, 1.0f);
        }
    }
}