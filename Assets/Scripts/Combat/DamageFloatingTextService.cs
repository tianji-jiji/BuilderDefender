using MoreMountains.Feedbacks;
using UnityEngine;

public enum DamageFloatingTextTargetType
{
    Enemy,
    Building
}

/// <summary>
/// 伤害飘字服务，负责统一通过 Feel 的浮动文字事件显示战斗伤害数字。
/// </summary>
public class DamageFloatingTextService : MonoBehaviour
{
    private const int DEFAULT_CHANNEL = 0;
    private const float DEFAULT_LIFETIME = 0.8f;
    private const float DEFAULT_INTENSITY = 1f;
    private const float BUILDING_DAMAGE_INTENSITY = 1.1f;
    private const float DAMAGE_TEXT_Y_OFFSET = 0.6f;
    private const float COLOR_ALPHA = 1f;

    [SerializeField] private int channel = DEFAULT_CHANNEL;
    [SerializeField] private float lifetime = DEFAULT_LIFETIME;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, DAMAGE_TEXT_Y_OFFSET, 0f);
    [SerializeField] private Gradient enemyDamageGradient = CreateDefaultGradient(new Color(1f, 0.86f, 0.25f, COLOR_ALPHA));
    [SerializeField] private Gradient buildingDamageGradient = CreateDefaultGradient(new Color(1f, 0.25f, 0.18f, COLOR_ALPHA));

    private static DamageFloatingTextService Instance;
    private static Gradient DefaultEnemyDamageGradient;
    private static Gradient DefaultBuildingDamageGradient;

    // 缓存全局伤害飘字服务实例，供战斗逻辑静态调用。
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 清理全局伤害飘字服务实例，避免场景切换后保留失效引用。
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 在敌人受击位置显示伤害飘字。
    public static void ShowEnemyDamage(Vector3 hitPosition, int damage)
    {
        ShowDamage(hitPosition, damage, DamageFloatingTextTargetType.Enemy);
    }

    // 在建筑受击位置显示伤害飘字。
    public static void ShowBuildingDamage(Vector3 hitPosition, int damage)
    {
        ShowDamage(hitPosition, damage, DamageFloatingTextTargetType.Building);
    }

    // 根据受击目标类型显示伤害飘字。
    public static void ShowDamage(Vector3 hitPosition, int damage, DamageFloatingTextTargetType targetType)
    {
        if (damage <= 0)
        {
            return;
        }

        if (Instance)
        {
            Instance.ShowDamageInternal(hitPosition, damage, targetType);
            return;
        }

        TriggerFloatingText(
            DEFAULT_CHANNEL,
            hitPosition + new Vector3(0f, DAMAGE_TEXT_Y_OFFSET, 0f),
            damage.ToString(),
            targetType == DamageFloatingTextTargetType.Building ? BUILDING_DAMAGE_INTENSITY : DEFAULT_INTENSITY,
            DEFAULT_LIFETIME,
            GetDefaultGradient(targetType));
    }

    // 使用当前服务配置触发 Feel 浮动文字事件。
    private void ShowDamageInternal(Vector3 hitPosition, int damage, DamageFloatingTextTargetType targetType)
    {
        TriggerFloatingText(
            channel,
            hitPosition + spawnOffset,
            damage.ToString(),
            targetType == DamageFloatingTextTargetType.Building ? BUILDING_DAMAGE_INTENSITY : DEFAULT_INTENSITY,
            lifetime,
            GetConfiguredGradient(targetType));
    }

    // 触发 Feel 的浮动文字生成事件。
    private static void TriggerFloatingText(int textChannel, Vector3 spawnPosition, string text, float intensity, float textLifetime, Gradient colorGradient)
    {
        MMFloatingTextSpawnEvent.Trigger(
            new MMChannelData(MMChannelModes.Int, textChannel, null),
            spawnPosition,
            text,
            Vector3.up,
            intensity,
            true,
            textLifetime,
            true,
            colorGradient,
            false);
    }

    // 获取当前服务配置的伤害颜色渐变。
    private Gradient GetConfiguredGradient(DamageFloatingTextTargetType targetType)
    {
        return targetType == DamageFloatingTextTargetType.Building
            ? buildingDamageGradient
            : enemyDamageGradient;
    }

    // 获取无场景服务时使用的默认伤害颜色渐变。
    private static Gradient GetDefaultGradient(DamageFloatingTextTargetType targetType)
    {
        if (DefaultEnemyDamageGradient == null)
        {
            DefaultEnemyDamageGradient = CreateDefaultGradient(new Color(1f, 0.86f, 0.25f, COLOR_ALPHA));
        }

        if (DefaultBuildingDamageGradient == null)
        {
            DefaultBuildingDamageGradient = CreateDefaultGradient(new Color(1f, 0.25f, 0.18f, COLOR_ALPHA));
        }

        return targetType == DamageFloatingTextTargetType.Building
            ? DefaultBuildingDamageGradient
            : DefaultEnemyDamageGradient;
    }

    // 创建从完全可见到透明淡出的默认颜色渐变。
    private static Gradient CreateDefaultGradient(Color color)
    {
        Gradient gradient = new Gradient();
        Color transparentColor = color;
        transparentColor.a = 0f;

        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 0.75f),
                new GradientColorKey(transparentColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(COLOR_ALPHA, 0f),
                new GradientAlphaKey(COLOR_ALPHA, 0.75f),
                new GradientAlphaKey(0f, 1f)
            });

        return gradient;
    }
}
