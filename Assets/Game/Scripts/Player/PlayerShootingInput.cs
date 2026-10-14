using BattleCore.Weapons;
using UnityEngine;

namespace BattleCore.Player
{
    /// <summary>
    /// 读取玩家鼠标输入并向通用发射器提供方向，不负责创建或管理弹丸。
    /// </summary>
    [RequireComponent(typeof(BarrageEmitter))]
    public sealed class PlayerShootingInput : MonoBehaviour
    {
        #region 变量定义

        [Header("通用设置")]
        [Tooltip("关闭后停止响应射击输入。")] [SerializeField] private bool shootingEnabled = true;

        [Header("输入系统")]
        [Tooltip("按住鼠标左键时是否允许连续射击。")] [SerializeField] private bool continuousFire = true;

        [Header("组件引用")]
        [Tooltip("把屏幕鼠标位置转换到游戏世界的摄像机。")] [SerializeField] private Camera aimingCamera;
        [Tooltip("接收瞄准方向并生成弹幕的通用发射器。")] [SerializeField] private BarrageEmitter barrageEmitter;

        #endregion



        #region Unity方法

        private void Awake()
        {
            ResolveComponentReferences();
        }

        private void Update()
        {
            HandleShootingInput();
        }

        #endregion



        #region 自定义方法

        /// <summary>
        /// 补全摄像机和发射器引用，减少场景中的手动绑定工作。
        /// </summary>
        private void ResolveComponentReferences()
        {
            if (aimingCamera == null)
            {
                aimingCamera = Camera.main;
            }

            if (barrageEmitter == null)
            {
                barrageEmitter = GetComponent<BarrageEmitter>();
            }
        }

        /// <summary>
        /// 根据当前输入模式检测鼠标左键，并向鼠标所在世界位置射击。
        /// </summary>
        private void HandleShootingInput()
        {
            if (!shootingEnabled || aimingCamera == null || barrageEmitter == null)
            {
                return;
            }

            bool fireRequested = continuousFire
                ? Input.GetMouseButton(0)
                : Input.GetMouseButtonDown(0);

            if (!fireRequested)
            {
                return;
            }

            Vector3 fireOrigin = barrageEmitter.GetFireOriginPosition();
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Mathf.Abs(aimingCamera.transform.position.z - fireOrigin.z);

            Vector3 mouseWorldPosition = aimingCamera.ScreenToWorldPoint(mouseScreenPosition);
            Vector2 fireDirection = (Vector2)(mouseWorldPosition - fireOrigin);
            barrageEmitter.TryFire(fireDirection);
        }

        /// <summary>
        /// 控制玩家是否允许射击，供暂停、升级选择和死亡流程调用。
        /// </summary>
        public void SetShootingEnabled(bool isEnabled)
        {
            shootingEnabled = isEnabled;
        }

        #endregion
    }
}
