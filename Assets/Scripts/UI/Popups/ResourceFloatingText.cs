using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// 资源浮动文字，负责让资源图标跟随 More Mountains 浮动文字一起淡出。
/// </summary>
public class ResourceFloatingText : MMFloatingTextMeshPro
{
    [SerializeField] private SpriteRenderer resourceIconRenderer;

    private Color _initialIconColor;

    // 初始化资源图标颜色状态。
    protected override void Initialization()
    {
        base.Initialization();

        if (resourceIconRenderer)
        {
            _initialIconColor = resourceIconRenderer.color;
        }
    }

    // 设置文字颜色并同步图标透明度。
    public override void SetColor(Color newColor)
    {
        base.SetColor(newColor);
        SetIconOpacity(newColor.a);
    }

    // 设置文字透明度并同步图标透明度。
    public override void SetOpacity(float newOpacity)
    {
        base.SetOpacity(newOpacity);
        SetIconOpacity(newOpacity);
    }

    // 设置资源图标透明度。
    private void SetIconOpacity(float alpha)
    {
        if (!resourceIconRenderer)
        {
            return;
        }

        Color iconColor = _initialIconColor;
        iconColor.a = alpha;
        resourceIconRenderer.color = iconColor;
    }
}
