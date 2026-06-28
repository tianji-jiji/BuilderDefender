/// <summary>
/// 防御塔击杀上下文，保存造成击杀的来源防御塔。
/// </summary>
public class DefenseTowerEnemyKilledContext
{
    public DefenseTowerCombatSystem SourceDefenseTowerCombatSystem { get; }

    public DefenseTowerEnemyKilledContext(DefenseTowerCombatSystem sourceDefenseTowerCombatSystem)
    {
        SourceDefenseTowerCombatSystem = sourceDefenseTowerCombatSystem;
    }
}
