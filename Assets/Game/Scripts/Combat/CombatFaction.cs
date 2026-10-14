namespace BattleCore.Combat
{
    /// <summary>
    /// 标记伤害来源所属阵营，用于阻止友方弹丸造成伤害。
    /// </summary>
    public enum CombatFaction
    {
        Neutral = 0,
        Player = 1,
        Enemy = 2
    }
}
