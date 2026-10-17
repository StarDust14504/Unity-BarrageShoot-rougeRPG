using System;
using UnityEngine;

namespace BattleCore.Combat
{
    /// <summary>
    /// 为玩家、敌人和后续可破坏物提供统一的生命值实现。
    /// 弹幕只通过 IDamageable 接口传递伤害，不需要知道目标究竟是哪一种角色。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CombatHealth : MonoBehaviour, IDamageable
    {
        #region 变量定义

        [Header("阵营设置")]
        [Tooltip("当前对象所属阵营，弹幕使用它判断是否为友方目标。")] [SerializeField] private CombatFaction faction = CombatFaction.Neutral;

        [Header("生命值设置")]
        [Tooltip("每次进入战斗时的最大生命值。")] [Min(1f)] [SerializeField] private float maxHealth = 100f;

        [Header("死亡设置")]
        [Tooltip("开启后，生命值归零时直接销毁所在的 GameObject。")] [SerializeField] private bool destroyOnDeath = true;

        [Header("运行时状态")]
        [Tooltip("当前剩余生命值，进入 Play Mode 时会自动回复至上限。")] [SerializeField] private float currentHealth;
        [Tooltip("标记对象是否已经死亡，用于防止重复结算伤害和死亡。")] [SerializeField] private bool isDead;

        public CombatFaction Faction => faction; // IDamageable 通过该属性读取目标阵营。
        public float CurrentHealth => currentHealth; // 为后续血条 UI 提供只读数据。
        public float MaxHealth => maxHealth; // 对外只读，避免其他系统绕过生命流程直接修改。
        public bool IsDead => isDead; // 供 AI、UI 和游戏流程判断角色是否失效。

        public event Action<float, float> HealthChanged; // 参数依次为当前生命和最大生命。
        public event Action<GameObject> Died; // 参数为造成最后一次伤害的来源。

        #endregion



        #region Unity方法

        private void Awake()
        {
            // Demo 阶段每次生成角色都以满血开始。
            // 后续接入存档或对象池时，可由外部在合适时机调用 ResetHealth。
            ResetHealth();
        }

        private void OnValidate()
        {
            // OnValidate 在 Inspector 数值改动时调用，用于保证配置不会出现无效生命上限。
            maxHealth = Mathf.Max(1f, maxHealth);

            if (Application.isPlaying)
            {
                currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            }
        }

        #endregion



        #region 自定义方法

        /// <summary>
        /// 接收一次伤害请求。Barrage 会在调用前完成阵营判断，因此本方法只处理生命值变化。
        /// damageSource 会继续传入死亡事件，后续可用于击杀归属、经验奖励和伤害统计。
        /// </summary>
        public void TakeDamage(float damage, GameObject damageSource)
        {
            float validDamage = Mathf.Max(0f, damage);
            if (isDead || validDamage <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - validDamage);
            HealthChanged?.Invoke(currentHealth, maxHealth);

            // 暂时通过 Console 观察战斗数据；制作血条后可删除或改为可选调试日志。
            Debug.Log($"{name} 受到 {validDamage:0.##} 点伤害，剩余生命 {currentHealth:0.##}/{maxHealth:0.##}。", this);

            if (currentHealth <= 0f)
            {
                Die(damageSource);
            }
        }

        /// <summary>
        /// 恢复指定生命值，但不会超过最大生命值；死亡对象不会被普通治疗直接复活。
        /// </summary>
        public void Heal(float amount)
        {
            float validAmount = Mathf.Max(0f, amount);
            if (isDead || validAmount <= 0f || currentHealth >= maxHealth)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + validAmount);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 将对象恢复到初始满血状态，后续对象池重新启用敌人时可以直接复用。
        /// </summary>
        public void ResetHealth()
        {
            isDead = false;
            currentHealth = maxHealth;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 统一执行一次死亡结算，并保证 Died 事件在对象销毁前发出。
        /// </summary>
        private void Die(GameObject damageSource)
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            currentHealth = 0f;
            Died?.Invoke(damageSource);

            Debug.Log($"{name} 已死亡。", this);

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }

        #endregion
    }
}
