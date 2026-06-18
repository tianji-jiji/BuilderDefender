using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// 伤害飘字事件入口，负责向 More Mountains 浮动文字生成器发送伤害数字。
/// </summary>
public static class DamageFloatingTextEvent
{
    private const int DEFAULT_CHANNEL = 0;
    private const float DEFAULT_INTENSITY = 1f;

    // 显示一次伤害飘字。
    public static void ShowDamage(Vector3 position, int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        MMFloatingTextSpawnEvent.Trigger(
            new MMChannelData(MMChannelModes.Int, DEFAULT_CHANNEL, null),
            position,
            damage.ToString(),
            Vector3.up,
            DEFAULT_INTENSITY);
    }
}
