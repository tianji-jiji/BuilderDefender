using UnityEngine;

public class BuildingUpgradeVisual : MonoBehaviour
{
    private const int STAR_LEVEL_TWO = 2;
    private const int STAR_LEVEL_THREE = 3;

    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Material starOneMaterial;
    [SerializeField] private Material starTwoMaterial;
    [SerializeField] private Material starThreeMaterial;

    // 缓存一星默认材质，避免未配置时升星后无法恢复。
    private void Awake()
    {
        if (!starOneMaterial && targetRenderer)
        {
            starOneMaterial = targetRenderer.sharedMaterial;
        }
    }

    // 根据建筑星级切换对应的升星发光材质。
    public void ApplyStarLevel(int starLevel)
    {
        if (!targetRenderer)
        {
            return;
        }

        targetRenderer.sharedMaterial = GetMaterialForStarLevel(starLevel);
    }

    // 选择当前星级应该使用的材质。
    private Material GetMaterialForStarLevel(int starLevel)
    {
        if (starLevel >= STAR_LEVEL_THREE && starThreeMaterial)
        {
            return starThreeMaterial;
        }

        if (starLevel >= STAR_LEVEL_TWO && starTwoMaterial)
        {
            return starTwoMaterial;
        }

        return starOneMaterial ? starOneMaterial : targetRenderer.sharedMaterial;
    }
}
