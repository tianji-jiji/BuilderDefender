using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ArrowEffectTests
{
    // 清理测试场景中创建的对象。
    [TearDown]
    public void TearDown()
    {
        foreach (GameObject gameObject in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (gameObject.name.StartsWith("ArrowEffectTest_"))
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }

    // 验证箭矢直接伤害仍然使用护甲修正后的结果。
    [Test]
    public void ApplyDamage_UsesArmorAdjustedDamage()
    {
        Enemy enemy = CreateEnemy("ArrowEffectTest_Enemy", Vector3.zero, 100, 100);

        int actualDamage = ArrowDamageApplier.ApplyDamage(enemy, 20, 0f, null);

        Assert.AreEqual(10, actualDamage);
        Assert.AreEqual(90, GetHealthSystem(enemy).CurrentHealth);
    }

    // 验证爆裂箭只伤害范围内的非直接命中目标。
    [Test]
    public void ExplosiveArrowEffect_DamagesNearbyEnemiesExceptDirectHit()
    {
        Enemy directHitEnemy = CreateEnemy("ArrowEffectTest_Direct", Vector3.zero, 100, 0);
        Enemy splashEnemy = CreateEnemy("ArrowEffectTest_Splash", new Vector3(1f, 0f, 0f), 100, 0);
        Enemy farEnemy = CreateEnemy("ArrowEffectTest_Far", new Vector3(5f, 0f, 0f), 100, 0);
        ExplosiveArrowEffectSo effect = ScriptableObject.CreateInstance<ExplosiveArrowEffectSo>();
        ArrowHitContext context = new(
            directHitEnemy,
            Vector3.zero,
            20,
            0f,
            null,
            2f,
            0.5f,
            new Collider2D[32]);

        effect.Apply(context);

        Assert.AreEqual(100, GetHealthSystem(directHitEnemy).CurrentHealth);
        Assert.AreEqual(90, GetHealthSystem(splashEnemy).CurrentHealth);
        Assert.AreEqual(100, GetHealthSystem(farEnemy).CurrentHealth);

        Object.DestroyImmediate(effect);
    }

    // 创建测试敌人并初始化生命、护甲和碰撞体。
    private Enemy CreateEnemy(string objectName, Vector3 position, int maxHealth, int armor)
    {
        GameObject enemyObject = new GameObject(objectName);
        enemyObject.transform.position = position;
        HealthSystem healthSystem = enemyObject.AddComponent<HealthSystem>();
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        SetField(enemy, "healthSystem", healthSystem);
        enemy.Init(null, new EnemyRuntimeStats(maxHealth, armor, 1, 1f, 1f));
        Physics2D.SyncTransforms();

        return enemy;
    }

    // 获取敌人的生命系统组件。
    private HealthSystem GetHealthSystem(Enemy enemy)
    {
        return enemy.GetComponent<HealthSystem>();
    }

    // 设置私有序列化字段，模拟 Inspector 引用。
    private void SetField(object target, string fieldName, object value)
    {
        FieldInfo fieldInfo = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(fieldInfo, $"Missing field {fieldName}");
        fieldInfo.SetValue(target, value);
    }
}
