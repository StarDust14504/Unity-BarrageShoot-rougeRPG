using BattleCore.Weapons;
using UnityEngine;

namespace BattleCore.Enemies
{
    /// <summary>
    /// 基础敌人的瞄准与开火控制器。
    /// 本组件只决定“何时朝哪个方向开火”，弹幕类型、属性和冷却仍由 BarrageEmitter 统一管理。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BarrageEmitter))]
    public sealed class EnemyShootingController : MonoBehaviour
    {
        #region 变量定义

        [Header("射击设置")]
        [Tooltip("关闭后敌人停止发出弹幕，可用于暂停、登场或死亡流程。")] [SerializeField] private bool shootingEnabled = true;
        [Tooltip("进入 Play Mode 后等待多少秒才尝试首次射击。")] [Min(0f)] [SerializeField] private float firstShotDelay = 1f;

        [Header("目标设置")]
        [Tooltip("当前射击目标；为空时会根据 Player Tag 自动查找。")] [SerializeField] private Transform target;
        [Tooltip("目标丢失后的重新查找间隔，避免每帧扫描场景。")] [Min(0.1f)] [SerializeField] private float targetSearchInterval = 0.5f;

        [Header("组件引用")]
        [Tooltip("负责实际生成弹幕、合并数值并限制射速的通用发射器。")] [SerializeField] private BarrageEmitter barrageEmitter;

        private float shootingAllowedTime; // 首次允许射击的时间点。
        private float nextTargetSearchTime; // 下一次允许查找玩家的时间点。

        #endregion



        #region Unity方法

        private void Awake()
        {
            ResolveComponentReferences();
        }

        private void Start()
        {
            shootingAllowedTime = Time.time + firstShotDelay;
            ResolveTarget();
        }

        private void Update()
        {
            UpdateShooting();
        }

        private void OnValidate()
        {
            firstShotDelay = Mathf.Max(0f, firstShotDelay);
            targetSearchInterval = Mathf.Max(0.1f, targetSearchInterval);
        }

        #endregion



        #region 自定义方法

        /// <summary>
        /// 补全发射器引用。RequireComponent 保证组件存在，此处负责减少 Inspector 手动绑定工作。
        /// </summary>
        private void ResolveComponentReferences()
        {
            if (barrageEmitter == null)
            {
                barrageEmitter = GetComponent<BarrageEmitter>();
            }
        }

        /// <summary>
        /// 在没有有效目标时查找带有 Player Tag 的对象，并缓存其 Transform。
        /// 已缓存的 Unity 对象被销毁后会表现为 null，因此可以自然进入重新查找流程。
        /// </summary>
        private void ResolveTarget()
        {
            if (target != null || Time.time < nextTargetSearchTime)
            {
                return;
            }

            nextTargetSearchTime = Time.time + targetSearchInterval;
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                target = player.transform;
            }
        }

        /// <summary>
        /// 计算敌人指向玩家的世界方向，并把发射请求交给 BarrageEmitter。
        /// TryFire 会自行处理射速冷却，所以本控制器不再维护第二套射击计时器。
        /// </summary>
        private void UpdateShooting()
        {
            if (!shootingEnabled || barrageEmitter == null || Time.time < shootingAllowedTime)
            {
                return;
            }

            if (target == null)
            {
                ResolveTarget();
            }

            if (target == null)
            {
                return;
            }

            Vector3 fireOrigin = barrageEmitter.GetFireOriginPosition();
            Vector2 fireDirection = (Vector2)(target.position - fireOrigin);

            if (fireDirection.sqrMagnitude > 0f)
            {
                barrageEmitter.TryFire(fireDirection);
            }
        }

        /// <summary>
        /// 允许刷怪或仇恨系统直接指定目标，不必再次通过 Tag 搜索。
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// 控制敌人是否允许射击，供暂停、生成动画和死亡流程调用。
        /// </summary>
        public void SetShootingEnabled(bool isEnabled)
        {
            shootingEnabled = isEnabled;
        }

        #endregion
    }
}
