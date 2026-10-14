using BattleCore.Combat;
using UnityEngine;

namespace BattleCore.Barrages
{
    /// <summary>
    /// 定义一种可被任意阵营使用的弹幕类型及其基础属性。
    /// 发射者只提供阵营、方向和局内增益，不决定弹幕本身的类型。
    /// </summary>
    [CreateAssetMenu(menuName = "Barrage Rogue/Barrages/Barrage Definition", fileName = "NewBarrageDefinition")]
    public sealed class BarrageDefinition : ScriptableObject
    {
        #region 变量定义

        [Header("基础信息")]
        [Tooltip("弹幕类型的稳定标识，用于存档和配置查找。")] [SerializeField] private string barrageId = "new_barrage";
        [Tooltip("该类型实例化时使用的通用弹幕 Prefab。")] [SerializeField] private Barrage barragePrefab;

        [Header("基础属性")]
        [Tooltip("弹幕命中目标时造成的基础伤害。")] [Min(0f)] [SerializeField] private float baseDamage = 10f;
        [Tooltip("弹幕每秒移动的基础世界单位距离。")] [Min(0f)] [SerializeField] private float baseSpeed = 12f;
        [Tooltip("弹幕未命中时的最大存在秒数。")] [Min(0.01f)] [SerializeField] private float baseLifetime = 3f;
        [Tooltip("弹幕命中首个目标后还能额外穿透的目标数。")] [Min(0)] [SerializeField] private int basePenetrationCount;

        [Header("外观属性")]
        [Tooltip("为空时使用 Prefab 自带精灵。")] [SerializeField] private Sprite barrageSprite;
        [Tooltip("弹幕生成时应用的基础颜色。")] [SerializeField] private Color barrageColor = Color.white;
        [Tooltip("弹幕生成时应用的基础局部缩放。")] [SerializeField] private Vector2 barrageScale = new Vector2(0.2f, 0.2f);

        #endregion



        #region Unity方法

        private void OnValidate()
        {
            baseDamage = Mathf.Max(0f, baseDamage);
            baseSpeed = Mathf.Max(0f, baseSpeed);
            baseLifetime = Mathf.Max(0.01f, baseLifetime);
            basePenetrationCount = Mathf.Max(0, basePenetrationCount);
            barrageScale.x = Mathf.Max(0.01f, barrageScale.x);
            barrageScale.y = Mathf.Max(0.01f, barrageScale.y);
        }

        #endregion



        #region 自定义方法

        public string BarrageId => barrageId;
        public Barrage BarragePrefab => barragePrefab;

        /// <summary>
        /// 将弹幕类型的基础值与发射者当前增益合并为单颗弹幕的运行时数据。
        /// </summary>
        public BarrageSpawnData CreateSpawnData(
            GameObject owner,
            CombatFaction faction,
            Vector2 direction,
            float damageMultiplier,
            float speedMultiplier,
            float scaleMultiplier)
        {
            Vector2 finalScale = barrageScale * Mathf.Max(0.01f, scaleMultiplier);

            return new BarrageSpawnData(
                owner,
                faction,
                direction,
                baseSpeed * Mathf.Max(0f, speedMultiplier),
                baseDamage * Mathf.Max(0f, damageMultiplier),
                baseLifetime,
                basePenetrationCount,
                barrageSprite,
                barrageColor,
                finalScale);
        }

        #endregion
    }
}
