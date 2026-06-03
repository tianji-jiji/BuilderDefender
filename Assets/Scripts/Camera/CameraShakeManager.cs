using Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    private static readonly Vector3 DefaultImpulseVelocity = Vector3.down;

    [Header("敌人死亡震动")]
    [Tooltip("敌人死亡时触发的轻微屏幕震动预设。")]
    [SerializeField] private CinemachineImpulseSource enemyDeadImpulseSource;

    [Header("普通建筑摧毁震动")]
    [Tooltip("普通建筑被敌人摧毁时触发的较大屏幕震动预设。")]
    [SerializeField] private CinemachineImpulseSource buildingDestroyedImpulseSource;

    [Header("防御塔摧毁震动")]
    [Tooltip("防御塔被敌人摧毁时触发的最大屏幕震动预设。")]
    [SerializeField] private CinemachineImpulseSource defenseTowerDestroyedImpulseSource;

    // 订阅敌人死亡和建筑摧毁事件。
    private void OnEnable()
    {
        Enemy.OnEnemyDead += HandleEnemyDead;
        Building.OnBuildingDestroyed += HandleBuildingDestroyed;
    }

    // 取消订阅敌人死亡和建筑摧毁事件。
    private void OnDisable()
    {
        Enemy.OnEnemyDead -= HandleEnemyDead;
        Building.OnBuildingDestroyed -= HandleBuildingDestroyed;
    }

    // 处理敌人死亡时的轻微屏幕震动。
    private void HandleEnemyDead()
    {
        GenerateShake(enemyDeadImpulseSource, transform.position);
    }

    // 根据建筑类型处理普通建筑或防御塔被摧毁时的屏幕震动。
    private void HandleBuildingDestroyed(BuildingSo buildingSo, Vector3 position)
    {
        bool isDefenseTower = buildingSo && buildingSo.buildingType == BuildingSo.BuildingType.Defense;
        CinemachineImpulseSource impulseSource = isDefenseTower
            ? defenseTowerDestroyedImpulseSource
            : buildingDestroyedImpulseSource;

        GenerateShake(impulseSource, position);
    }

    // 触发指定震动源的 Cinemachine Impulse。
    private void GenerateShake(CinemachineImpulseSource impulseSource, Vector3 position)
    {
        if (!impulseSource)
        {
            return;
        }

        impulseSource.GenerateImpulseAt(position, DefaultImpulseVelocity);
    }
}
