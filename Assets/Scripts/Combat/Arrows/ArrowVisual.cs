using UnityEngine;

/// <summary>
/// 箭矢视觉组件，负责材质、渲染器、拖尾和视觉显示状态。
/// </summary>
public class ArrowVisual : MonoBehaviour, IPoolable
{
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private TrailRenderer[] trailRenderers;
    [SerializeField] private Material defaultVisualMaterial;

    // 缓存默认材质，避免对象池复用后保留临时材质。
    private void Awake()
    {
        CacheDefaultVisualMaterial();
    }

    // 重置箭矢从对象池取出后的视觉状态。
    public void OnSpawned()
    {
        ResetForSpawn();
    }

    // 清理箭矢回收到对象池前的视觉状态。
    public void OnDespawned()
    {
        ResetForDespawn();
    }

    // 重置生成后的可见性、材质和拖尾状态。
    public void ResetForSpawn()
    {
        SetVisualsActive(true);
        RestoreDefaultMaterial();
        SetTrailsActive(false);
        ClearTrails();
    }

    // 重置回收前的可见性、材质和拖尾状态。
    public void ResetForDespawn()
    {
        SetVisualsActive(false);
        RestoreDefaultMaterial();
        SetTrailsActive(false);
        ClearTrails();
    }

    // 应用本次发射使用的视觉材质和拖尾开关。
    public void ApplyVisualEffect(Material visualMaterial, bool enableTrail)
    {
        ApplyVisualMaterial(visualMaterial ? visualMaterial : defaultVisualMaterial);
        SetTrailsActive(enableTrail);
        ClearTrails();
    }

    // 设置视觉根节点和精灵渲染器的显示状态。
    private void SetVisualsActive(bool active)
    {
        if (visualRoot)
        {
            visualRoot.SetActive(active);
        }

        if (spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer)
            {
                spriteRenderer.enabled = active;
            }
        }
    }

    // 缓存箭矢默认材质。
    private void CacheDefaultVisualMaterial()
    {
        if (defaultVisualMaterial || spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer && spriteRenderer.sharedMaterial)
            {
                defaultVisualMaterial = spriteRenderer.sharedMaterial;
                return;
            }
        }
    }

    // 恢复箭矢默认材质。
    private void RestoreDefaultMaterial()
    {
        ApplyVisualMaterial(defaultVisualMaterial);
    }

    // 将指定材质应用到所有箭矢精灵渲染器。
    private void ApplyVisualMaterial(Material visualMaterial)
    {
        if (!visualMaterial || spriteRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer)
            {
                spriteRenderer.sharedMaterial = visualMaterial;
            }
        }
    }

    // 开关箭矢拖尾渲染器。
    private void SetTrailsActive(bool active)
    {
        if (trailRenderers == null)
        {
            return;
        }

        foreach (TrailRenderer trailRenderer in trailRenderers)
        {
            if (!trailRenderer)
            {
                continue;
            }

            trailRenderer.emitting = active;
            trailRenderer.enabled = active;
        }
    }

    // 清理箭矢可能残留的拖尾。
    private void ClearTrails()
    {
        if (trailRenderers == null)
        {
            return;
        }

        foreach (TrailRenderer trailRenderer in trailRenderers)
        {
            if (trailRenderer)
            {
                trailRenderer.Clear();
            }
        }
    }
}
