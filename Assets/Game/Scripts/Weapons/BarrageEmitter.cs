using BattleCore.Combat;
using BattleCore.Barrages;
using UnityEngine;

namespace BattleCore.Weapons
{
    /// <summary>
    /// 通用弹丸发射器。玩家输入和敌人 AI 只需向它提供发射方向。
    /// </summary>
    public sealed class BarrageEmitter : MonoBehaviour
    {
        #region 变量定义

        [Header("通用设置")]
        [Tooltip("发射者所属阵营。")] [SerializeField] private CombatFaction faction = CombatFaction.Player;
        [Tooltip("每秒允许发射的弹丸数量。")] [Min(0.01f)] [SerializeField] private float shotsPerSecond = 5f;

        [Header("弹幕类型")]
        [Tooltip("决定弹幕 Prefab、基础数值和外观的共享配置。")] [SerializeField] private BarrageDefinition barrageDefinition;

        [Header("局内增益")]
        [Tooltip("发射者对弹幕基础伤害的倍率。")] [Min(0f)] [SerializeField] private float damageMultiplier = 1f;
        [Tooltip("发射者对弹幕基础速度的倍率。")] [Min(0f)] [SerializeField] private float speedMultiplier = 1f;
        [Tooltip("发射者对弹幕基础尺寸的倍率。")] [Min(0.01f)] [SerializeField] private float scaleMultiplier = 1f;

        [Header("组件引用")]
        [Tooltip("弹丸生成位置；为空时使用发射器自身位置。")] [SerializeField] private Transform firePoint;

        private float nextFireTime; // 下一次允许发射的时间。

        #endregion



        #region Unity方法

        private void Awake()
        {
            if (firePoint == null)
            {
                firePoint = transform;
            }
        }

        #endregion



        #region 自定义方法

        /// <summary>
        /// 尝试朝指定方向发射一颗弹丸，并返回本次是否成功发射。
        /// </summary>
        public bool TryFire(Vector2 direction)
        {
            if (barrageDefinition == null
                || barrageDefinition.BarragePrefab == null
                || direction.sqrMagnitude <= 0f
                || Time.time < nextFireTime)
            {
                return false;
            }

            Transform spawnPoint = firePoint != null ? firePoint : transform;
            Barrage barrage = Instantiate(
                barrageDefinition.BarragePrefab,
                spawnPoint.position,
                Quaternion.identity);

            BarrageSpawnData spawnData = barrageDefinition.CreateSpawnData(
                gameObject,
                faction,
                direction,
                damageMultiplier,
                speedMultiplier,
                scaleMultiplier);

            barrage.Initialize(spawnData);
            nextFireTime = Time.time + 1f / shotsPerSecond;
            return true;
        }

        /// <summary>
        /// 返回弹幕实际生成位置，确保瞄准向量与生成起点完全一致。
        /// </summary>
        public Vector3 GetFireOriginPosition()
        {
            return firePoint != null ? firePoint.position : transform.position;
        }

        #endregion
    }
}
