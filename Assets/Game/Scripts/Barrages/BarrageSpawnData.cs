using BattleCore.Combat;
using UnityEngine;

namespace BattleCore.Barrages
{
    /// <summary>
    /// 保存单颗弹丸生成时的最终属性，后续 Build 增益可以在发射前修改这些数值。
    /// </summary>
    public readonly struct BarrageSpawnData
    {
        #region 变量定义

        public GameObject Owner { get; }
        public CombatFaction Faction { get; }
        public Vector2 Direction { get; }
        public float Speed { get; }
        public float Damage { get; }
        public float Lifetime { get; }
        public int PenetrationCount { get; }
        public Sprite SpriteOverride { get; }
        public Color Color { get; }
        public Vector2 Scale { get; }

        #endregion



        #region Unity方法

        // 纯数据结构不使用 Unity 生命周期函数。

        #endregion



        #region 自定义方法

        /// <summary>
        /// 创建一份不会在飞行过程中被外部修改的弹丸生成数据。
        /// </summary>
        public BarrageSpawnData(
            GameObject owner,
            CombatFaction faction,
            Vector2 direction,
            float speed,
            float damage,
            float lifetime,
            int penetrationCount,
            Sprite spriteOverride,
            Color color,
            Vector2 scale)
        {
            Owner = owner;
            Faction = faction;
            Direction = direction;
            Speed = speed;
            Damage = damage;
            Lifetime = lifetime;
            PenetrationCount = penetrationCount;
            SpriteOverride = spriteOverride;
            Color = color;
            Scale = scale;
        }

        #endregion
    }
}
