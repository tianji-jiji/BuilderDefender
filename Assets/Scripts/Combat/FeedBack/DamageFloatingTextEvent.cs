using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// 伤害飘字显示样式。
/// </summary>
public enum DamageFloatingTextStyle
{
    Normal,
    Poison,
    Burn,
    Explosion
}

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
        ShowDamage(position, damage, DamageFloatingTextStyle.Normal);
    }

    // 按指定样式显示一次伤害飘字。
    public static void ShowDamage(Vector3 position, int damage, DamageFloatingTextStyle style)
    {
        if (damage <= 0)
        {
            return;
        }

        bool forceColor = style != DamageFloatingTextStyle.Normal;
        Color styleColor = GetStyleColor(style);
        MMFloatingTextSpawnEvent.Trigger(
            new MMChannelData(MMChannelModes.Int, DEFAULT_CHANNEL, null),
            position,
            FormatDamageText(damage, style, styleColor),
            Vector3.up,
            DEFAULT_INTENSITY,
            false,
            1f,
            forceColor,
            forceColor ? CreateStyleGradient(styleColor) : null);
    }

    // 获取指定伤害样式对应的颜色。
    public static Color GetStyleColor(DamageFloatingTextStyle style)
    {
        switch (style)
        {
            case DamageFloatingTextStyle.Poison:
                return new Color(0.45f, 1f, 0.45f, 1f);
            case DamageFloatingTextStyle.Burn:
                return new Color(1f, 0.45f, 0.45f, 1f);
            case DamageFloatingTextStyle.Explosion:
                return new Color(0.55f, 0f, 0f, 1f);
            default:
                return Color.white;
        }
    }

    // 构建用于 Feel 浮动文字强制颜色的渐变。
    private static Gradient CreateStyleGradient(Color color)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(color.a, 0f),
                new GradientAlphaKey(0f, 1f)
            });
        return gradient;
    }

    // 构建伤害飘字文本，非普通伤害使用 TMP 富文本颜色作为保底显示。
    private static string FormatDamageText(int damage, DamageFloatingTextStyle style, Color color)
    {
        if (style == DamageFloatingTextStyle.Normal)
        {
            return damage.ToString();
        }

        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{colorHex}>{damage}</color>";
    }
}
