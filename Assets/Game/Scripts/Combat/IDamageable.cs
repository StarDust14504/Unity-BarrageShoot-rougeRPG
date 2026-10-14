using UnityEngine;

namespace BattleCore.Combat
{
    /// <summary>
    /// 所有能够受到伤害的对象都实现此接口，使弹丸不依赖具体角色类型。
    /// </summary>
    public interface IDamageable
    {
        CombatFaction Faction { get; }

        void TakeDamage(float damage, GameObject damageSource);
    }
}
