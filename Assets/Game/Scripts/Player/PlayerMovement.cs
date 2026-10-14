using UnityEngine;

namespace BattleCore.Player
{
    /// <summary>
    /// 负责读取玩家的二维移动输入，并通过 Rigidbody2D 完成稳定的物理移动。
    /// 攻击、生命和动画逻辑会由其他组件负责，避免玩家控制脚本承担过多职责。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        #region 变量定义

        [Header("通用设置")]
        [Tooltip("控制玩家是否可以移动。停止时用于暂停、升级选择或死亡状态。")] [SerializeField] private bool movementEnabled = true;

        [Header("移动系统")]
        [Tooltip("玩家每秒移动的世界单位距离。")] [Min(0f)] [SerializeField] private float moveSpeed = 5f;

        [Header("组件引用")]
        [Tooltip("玩家二维刚体；为空时自动获取。")] [SerializeField] private Rigidbody2D playerRigidbody;

        private Vector2 moveInput; // 缓存输入，供物理帧使用。

        #endregion



        #region Unity方法

        private void Awake()
        {
            // 显式使用 Unity 的空引用判断，以正确处理丢失或已销毁的序列化引用。
            if (playerRigidbody == null)
            {
                playerRigidbody = GetComponent<Rigidbody2D>();
            }
        }

        private void Update()
        {
            ReadMoveInput();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void OnDisable()
        {
            StopImmediately();
        }

        #endregion



        #region 自定义方法

        /// <summary>
        /// 读取水平与垂直输入，并限制向量长度，避免斜向移动比单轴移动更快。
        /// </summary>
        private void ReadMoveInput()
        {
            if (!movementEnabled)
            {
                moveInput = Vector2.zero;
                return;
            }

            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            moveInput = Vector2.ClampMagnitude(
                new Vector2(horizontalInput, verticalInput),
                1f);
        }

        /// <summary>
        /// 在物理时间步中移动刚体，使移动速度不受渲染帧率影响。
        /// </summary>
        private void MovePlayer()
        {
            if (playerRigidbody == null)
            {
                return;
            }

            Vector2 nextPosition = playerRigidbody.position
                + moveInput * moveSpeed * Time.fixedDeltaTime;

            playerRigidbody.MovePosition(nextPosition);
        }

        /// <summary>
        /// 立即清除输入和刚体速度，防止组件关闭后保留上一帧的运动状态。
        /// </summary>
        private void StopImmediately()
        {
            moveInput = Vector2.zero;

            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero;
            }
        }

        /// <summary>
        /// 控制玩家是否允许移动，供后续暂停、升级选择和死亡流程调用。
        /// </summary>
        /// <param name="isEnabled">是否允许玩家移动。</param>
        public void SetMovementEnabled(bool isEnabled)
        {
            movementEnabled = isEnabled;

            if (!movementEnabled)
            {
                StopImmediately();
            }
        }

        #endregion
    }
}
