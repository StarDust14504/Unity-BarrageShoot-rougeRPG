using BattleCore.Combat;
using UnityEngine;

namespace BattleCore.Barrages
{
    /// <summary>
    /// 管理通用弹丸的移动、寿命、碰撞、伤害和外观。
    /// 玩家和敌人通过不同的生成数据共用此组件。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Barrage : MonoBehaviour
    {
        #region 变量定义

        [Header("组件引用")]
        [Tooltip("驱动弹幕移动的二维刚体。")] [SerializeField] private Rigidbody2D barrageRigidbody;
        [Tooltip("显示弹幕外观的精灵渲染器。")] [SerializeField] private SpriteRenderer barrageRenderer;

        [Header("运行时属性")]
        [Tooltip("弹丸当前所属阵营，仅用于运行时查看。")] [SerializeField] private CombatFaction faction;
        [Tooltip("弹丸命中目标时造成的伤害。")] [SerializeField] private float damage;
        [Tooltip("弹丸被自动回收前的剩余时间。")] [SerializeField] private float remainingLifetime;
        [Tooltip("弹丸还能额外穿透的目标数量。")] [SerializeField] private int remainingPenetrations;

        private GameObject owner; // 发射者，用于忽略自身碰撞。
        private bool initialized; // 标记弹丸是否已收到生成数据。

        #endregion



        #region Unity方法

        private void Awake()
        {
            ResolveComponentReferences();
        }

        private void Update()
        {
            UpdateLifetime();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleCollision(other);
        }

        #endregion



        #region 自定义方法

        /// <summary>
        /// 使用发射器计算完成的数据初始化弹丸。
        /// </summary>
        public void Initialize(BarrageSpawnData spawnData)
        {
            ResolveComponentReferences();

            owner = spawnData.Owner;
            faction = spawnData.Faction;
            damage = Mathf.Max(0f, spawnData.Damage);
            remainingLifetime = Mathf.Max(0.01f, spawnData.Lifetime);
            remainingPenetrations = Mathf.Max(0, spawnData.PenetrationCount);
            transform.localScale = new Vector3(spawnData.Scale.x, spawnData.Scale.y, 1f);

            Vector2 normalizedDirection = spawnData.Direction.sqrMagnitude > 0f
                ? spawnData.Direction.normalized
                : Vector2.right;

            barrageRigidbody.velocity = normalizedDirection * Mathf.Max(0f, spawnData.Speed);
            ApplyVisual(spawnData.SpriteOverride, spawnData.Color);
            IgnoreOwnerCollisions();
            initialized = true;
        }

        /// <summary>
        /// 补全组件引用，支持 Prefab 自动绑定和后续对象池复用。
        /// </summary>
        private void ResolveComponentReferences()
        {
            if (barrageRigidbody == null)
            {
                barrageRigidbody = GetComponent<Rigidbody2D>();
            }

            if (barrageRenderer == null)
            {
                barrageRenderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// 应用本次发射指定的精灵与颜色；精灵为空时保留 Prefab 默认外观。
        /// </summary>
        private void ApplyVisual(Sprite spriteOverride, Color color)
        {
            if (barrageRenderer == null)
            {
                return;
            }

            if (spriteOverride != null)
            {
                barrageRenderer.sprite = spriteOverride;
            }

            barrageRenderer.color = color;
        }

        /// <summary>
        /// 忽略发射者及其子对象的碰撞体，避免弹丸生成时击中自己。
        /// </summary>
        private void IgnoreOwnerCollisions()
        {
            if (owner == null)
            {
                return;
            }

            Collider2D barrageCollider = GetComponent<Collider2D>();
            Collider2D[] ownerColliders = owner.GetComponentsInChildren<Collider2D>(true);

            foreach (Collider2D ownerCollider in ownerColliders)
            {
                if (ownerCollider != null)
                {
                    Physics2D.IgnoreCollision(barrageCollider, ownerCollider, true);
                }
            }
        }

        /// <summary>
        /// 处理阵营判断、伤害和穿透；普通场景碰撞会直接回收弹丸。
        /// </summary>
        private void HandleCollision(Collider2D other)
        {
            if (!initialized || other == null || IsOwnerCollider(other))
            {
                return;
            }

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                Despawn();
                return;
            }

            if (damageable.Faction == faction)
            {
                return;
            }

            damageable.TakeDamage(damage, owner);

            if (remainingPenetrations > 0)
            {
                remainingPenetrations--;
                return;
            }

            Despawn();
        }

        /// <summary>
        /// 判断碰撞体是否属于发射者层级，作为 IgnoreCollision 之外的安全检查。
        /// </summary>
        private bool IsOwnerCollider(Collider2D other)
        {
            return owner != null
                && (other.gameObject == owner || other.transform.IsChildOf(owner.transform));
        }

        /// <summary>
        /// 更新弹丸寿命，防止未命中的弹丸永久留在场景中。
        /// </summary>
        private void UpdateLifetime()
        {
            if (!initialized)
            {
                return;
            }

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Despawn();
            }
        }

        /// <summary>
        /// 当前版本直接销毁弹丸；后续接入对象池时只需替换此方法。
        /// </summary>
        private void Despawn()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}
