using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 砖块在网格上按格子移动的核心组件。
    ///
    /// 工作流程（每帧）：
    /// 1. IDLE 状态累计 timer；到配置间隔后查询玩家位置；
    /// 2. 如果玩家在追击半径内，调用 <see cref="FindNextCell"/> 寻路,
    ///    返回下一步要去的 targetCell；
    /// 3. 锁定起点+终点 cells, 切换为 MOVING 状态；
    /// 4. 在 moveDuration 秒内通过 transform.position 插值移动；
    /// 5. 移动结束后释放起点锁, 更新占用为新位置, 重新进入 IDLE。
    ///
    /// 注意：本组件是 MonoBehaviour，必须挂在 Brick 同一 GameObject 上。
    ///     寻路依赖 BrickGridMoveConfig（可在 Inspector 拖入）。
    /// </summary>
    [DisallowMultipleComponent]
    public class BrickGridMover : BrickAbility
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------

        [Header("Configuration")]
        [Tooltip("移动配置（移动方向、间隔、过渡时间、追击半径等）. 不指定则禁用移动.")]
        public BrickGridMoveConfig config;

        // ---------------------------------------------------------------
        // 状态
        // ---------------------------------------------------------------

        public enum MoveState
        {
            Idle,
            Locked,
            Moving,

            /// <summary>砖块已贴住玩家, 停止移动与寻路, 仅监测是否被脱离.</summary>
            Attack,
        }

        public MoveState State { get; private set; } = MoveState.Idle;

        /// <summary>当前所在的"锚点"cell (砖块尺寸 1x1 时就是占用 cell; 否则按左下角).</summary>
        public Vector2Int AnchorCell { get; private set; }

        /// <summary>下一步要去的 cell. 仅在 IDLE 时查询.</summary>
        public Vector2Int TargetCell { get; private set; }

        /// <summary>正在移动的时间 (0 ~ moveDuration).</summary>
        public float MoveProgress { get; private set; }

        /// <summary>最近一次完全到达的 cell (用于确保下次寻路基于正确位置).</summary>
        public Vector2Int LastArrivedCell { get; private set; }

        // ---------------------------------------------------------------
        // 内部状态
        // ---------------------------------------------------------------

        BrickManager _brickManager;
        GridManager _gridManager;
        BrickGridLayout _layout;

        float _intervalTimer;
        float _restTimer;
        bool _hasArrived;

        // 移动用 scratch
        Vector3 _moveStartPos;
        Vector3 _moveEndPos;
        Vector2Int _fromAnchor;
        Vector2Int _toAnchor;

        // cells 缓存 (避免每帧 new)
        List<Vector2Int> _cellsScratch = new();
        List<Vector2Int> _fromCellsScratch = new();
        List<Vector2Int> _toCellsScratch = new();

        // ---------------------------------------------------------------
        // 生命周期
        // ---------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheReferences();
            // 初始化 anchor cell
            AnchorCell = GetCurrentAnchorCell();
            LastArrivedCell = AnchorCell;
            TargetCell = AnchorCell;
            State = MoveState.Idle;
            _intervalTimer = 0f;
            _restTimer = 0f;
            _hasArrived = true;
            MoveProgress = 0f;
        }

        protected override void OnDisable()
        {
            // 禁用时如果还处在移动中, 立刻释放锁
            if (State is MoveState.Locked or MoveState.Moving)
            {
                ReleaseMoveLocks();
                State = MoveState.Idle;
            }

            base.OnDisable();
        }

        void CacheReferences()
        {
            if (_brickManager == null)
                _brickManager = brickManager;

            if (_gridManager == null)
            {
                _gridManager = null;
#if UNITY_2023_1_OR_NEWER
                _gridManager = Object.FindFirstObjectByType<GridManager>(FindObjectsInactive.Include);
#else
                _gridManager = Object.FindObjectOfType<GridManager>(true);
#endif
            }

            if (_brickManager != null)
                _layout = _brickManager.brickLayout;
        }


        public override void OnFixedUpdate(float dt)
        {
            if (config == null)
                return;

            if (_brick.IsDead())
                return;

            CacheReferences();
            if (_brickManager == null || _layout == null)
                return;

            switch (State)
            {
                case MoveState.Idle:
                    UpdateIdle(dt);
                    break;
                case MoveState.Locked:
                    UpdateLocked(dt);
                    break;
                case MoveState.Moving:
                    UpdateMoving(dt);
                    break;
                case MoveState.Attack:
                    UpdateAttack(dt);
                    break;
            }
        }

        // ---------------------------------------------------------------
        // 状态机
        // ---------------------------------------------------------------

        void UpdateIdle(float dt)
        {
            // 休息期
            if (_restTimer > 0f)
            {
                _restTimer -= dt;
                return;
            }

            // 生死状态不允许移动
            // if (_brick.Conditions != null && _brick.Conditions.CurrentState != Character.Conditions.Normal)
            //     return;

            _intervalTimer += dt;

            if (_intervalTimer < config.moveInterval)
                return;

            // 间隔到 -> 准备寻路
            var player = _brick.getTargetPlayer();
            if (player == null || player.IsDead())
            {
                _intervalTimer = 0f;
                return;
            }

            // 追击半径
            if (!IsPlayerInChaseRange(player))
            {
                _intervalTimer = 0f;
                return;
            }

            // 重新同步 anchor cell (避免与占用表不一致)
            AnchorCell = GetCurrentAnchorCell();
            // 同步 _fromAnchor 给 BFS 的"自身起点 cells 例外"判断使用
            _fromAnchor = AnchorCell;

            // 寻路
            Vector2Int playerCell = WorldToAnchorCell(player.getWorldPosition());
            bool found = FindNextCell(AnchorCell, playerCell, out Vector2Int next);
            if (!found)
            {
                _intervalTimer = 0f;
                return;
            }

            // 锁定起点+终点 cells, 进入 Locked
            _fromAnchor = AnchorCell;
            _toAnchor = next;
            ComputeOccupiedCells(_fromAnchor, ref _fromCellsScratch);
            _brickManager.LockCells(_fromCellsScratch);

            ComputeOccupiedCells(_toAnchor, ref _toCellsScratch);
            _brickManager.LockCells(_toCellsScratch);

            _moveStartPos = ComputeWorldFromAnchor(_fromAnchor);
            _moveEndPos = ComputeWorldFromAnchor(_toAnchor);

            _intervalTimer = 0f;
            MoveProgress = 0f;
            State = MoveState.Locked;
            UpdateLocked(dt);
        }

        void UpdateLocked(float dt)
        {
            // 进入 Locked 状态的同一帧立即开始移动（不等待额外时间）
            // 这样 X 秒一到, Y 秒立即开始计时.
            State = MoveState.Moving;
            UpdateMoving(dt);
        }

        void UpdateMoving(float dt)
        {
            float dur = Mathf.Max(0.01f, config.moveDuration);
            MoveProgress += dt;
            float t = Mathf.Clamp01(MoveProgress / dur);
            transform.position = Vector3.Lerp(_moveStartPos, _moveEndPos, t);

            if (t >= 1f)
            {
                OnMoveComplete();
            }
        }

        void OnMoveComplete()
        {
            // 1. 释放起点 cells 的锁
            _cellsScratch.Clear();
            _cellsScratch.AddRange(_fromCellsScratch);
            _brickManager.UnlockCells(_cellsScratch);

            // 2. 更新占用表: 取消旧占用, 注册新占用
            if (_brick != null)
            {
                _brickManager.UnregisterOccupancy(_brick);
                _brickManager.RegisterOccupancy(_brick, _toAnchor.x, _toAnchor.y, _brick.size.x, _brick.size.y);
            }

            // 3. 终点 cells 还锁着 (给其它砖块作为目标时跳过), 留待下一次移动前再释放
            //    实际上为简化, 立即释放终点锁; 否则其它砖块永远走不到这些 cell.
            //    —— 已经被这块砖的占用注册挡掉, 不会冲突.
            _cellsScratch.Clear();
            _cellsScratch.AddRange(_toCellsScratch);
            _brickManager.UnlockCells(_cellsScratch);

            // 4. 更新锚点
            AnchorCell = _toAnchor;
            LastArrivedCell = _toAnchor;
            TargetCell = _toAnchor;
            transform.position = _moveEndPos;

            // 5. 进入下一个 idle
            State = MoveState.Idle;
            MoveProgress = 0f;
            _restTimer = config.restAfterMove;
            _hasArrived = true;

            // 7. 若到位后已贴住玩家, 进入 Attack 状态 (停止寻路与移动, 只监测脱离)
            var player = _brick.getTargetPlayer();
            if (player != null && !player.IsDead() && IsAdjacentToPlayer(player))
            {
                State = MoveState.Attack;
                _intervalTimer = 0f;
            }

            // 6. 清空缓存 (避免下次移动时误锁旧 cells)
            _fromCellsScratch.Clear();
            _toCellsScratch.Clear();
        }

        void ReleaseMoveLocks()
        {
            if (_brickManager == null)
                return;
            _cellsScratch.Clear();
            _cellsScratch.AddRange(_fromCellsScratch);
            _brickManager.UnlockCells(_cellsScratch);
            _cellsScratch.Clear();
            _cellsScratch.AddRange(_toCellsScratch);
            _brickManager.UnlockCells(_cellsScratch);
            _fromCellsScratch.Clear();
            _toCellsScratch.Clear();
        }

        // ---------------------------------------------------------------
        // 寻路 (引导式 BFS, 4 方向, 砖块尺寸 ≥ 1)
        //   1) 直觉首选: 朝玩家方向的邻接若可用就直接采用.
        //   2) 引导式 BFS: 邻接按"轴对齐度"排序, 引导沿玩家方向探索,
        //      命中 playerCell 后回溯到 start 的邻接 (即"下一步").
        //      maxAttempts 控制搜索步数上限, 防止玩家较远时过度消耗.
        // ---------------------------------------------------------------

        // 寻路 scratch (static 复用, 上游寻路串行执行).
        static Queue<Vector2Int> pathFindQueue = new();
        static List<Vector2Int> _dirScratch = new();

        static int CellIndex(Vector2Int c, int cols) => c.y * cols + c.x;

        static bool IsInBounds(Vector2Int c, int cols, int rows)
            => c.x >= 0 && c.x < cols && c.y >= 0 && c.y < rows;

        /// <summary>
        /// 寻路: 找到从 start 到 playerCell 的下一步目标 cell.
        /// 返回 false 表示无法找到路径.
        /// </summary>
        public bool FindNextCell(Vector2Int start, Vector2Int playerCell, out Vector2Int next)
        {
            next = start;

            if (start == playerCell)
                return false;

            int cols = _layout.getCols();
            int rows = _layout.getRows();

            // 方向按"与 player 方向对齐度"排序: 高 dot 的方向优先入队,
            // 形成"沿玩家方向探路"的引导, 而不是无差别向四方向扩展.
            var dirX = playerCell.x - start.x;
            var dirY = playerCell.y - start.y;
            int ax = Mathf.Clamp(dirX, -1, 1);
            int ay = Mathf.Clamp(dirY, -1, 1);

            _dirScratch.Clear();
            BrickGridMoveConfig.GetAllowedOffsets(config.allowedDirections, new(ax, ay), ref _dirScratch);
            if (_dirScratch.Count == 0)
                return false;
            // 1) 直觉首选: 朝玩家方向的 cell, 若可走 + 可用, 直接采用.
            if (ax != 0 || ay != 0)
            {
                Vector2Int dirToPlayer = new(ax, ay);
                var distX = Mathf.Abs(dirX);
                var distY = Mathf.Abs(dirY);
                switch (ax, ay)
                {
                    case (1, 1):
                        if (distX >= distY)
                            dirToPlayer = new(1, 0);
                        else
                            dirToPlayer = new(0, 1);
                        break;
                    case (-1, 1):
                        if (distX >= distY)
                            dirToPlayer = new(-1, 0);
                        else
                            dirToPlayer = new(0, 1);
                        break;
                    case (-1, -1):
                        if (distX >= distY)
                            dirToPlayer = new(-1, 0);
                        else
                            dirToPlayer = new(0, -1);
                        break;
                    case (1, -1):
                        if (distX >= distY)
                            dirToPlayer = new(1, 0);
                        else
                            dirToPlayer = new(0, -1);
                        break;
                }

                // 必须存在于"按对齐排序"的方向里 (即方向被允许).
                bool dirAllowed = false;
                for (int i = 0; i < _dirScratch.Count; i++)
                {
                    if (_dirScratch[i] == dirToPlayer)
                    {
                        dirAllowed = true;
                        break;
                    }
                }

                if (dirAllowed)
                {
                    var chosen = start + dirToPlayer;
                    if (IsInBounds(chosen, cols, rows) &&
                        IsMoveAllowed(start, chosen) &&
                        IsDestinationAvailable(chosen))
                    {
                        next = chosen;
                        return true;
                    }
                }
            }

            // 2) 引导式 BFS, 邻接按已排序的顺序探索. 命中 goal 后回溯到 start 的邻接.
            int total = cols * rows;
            using var a = new ArrayScope<bool>(out var visited, total);
            using var b = new ArrayScope<int>(out var prev, total);
            for (int i = 0; i < total; i++)
            {
                visited[i] = false;
                prev[i] = -1;
            }

            pathFindQueue.Clear();
            pathFindQueue.Enqueue(start);
            visited[CellIndex(start, cols)] = true;

            int maxAttempts = Mathf.Max(1, config.maxPathfindingAttempts);
            int attempts = 0;
            Vector2Int found = default;
            bool reached = false;

            while (pathFindQueue.Count > 0 && attempts < maxAttempts)
            {
                attempts++;
                var cur = pathFindQueue.Dequeue();

                if (cur == playerCell)
                {
                    found = cur;
                    reached = true;
                    break;
                }

                foreach (var off in _dirScratch)
                {
                    var nxt = cur + off;
                    if (!IsInBounds(nxt, cols, rows))
                        continue;

                    if (!IsMoveAllowed(cur, nxt))
                        continue;

                    int idx = CellIndex(nxt, cols);
                    if (visited[idx])
                        continue;

                    // 目标 cell 必须可用, start 自身不算 (visited 起步).
                    if (nxt != start && !IsDestinationAvailable(nxt))
                        continue;

                    visited[idx] = true;
                    prev[idx] = CellIndex(cur, cols);
                    pathFindQueue.Enqueue(nxt);
                }
            }

            if (!reached)
                return false;

            // 回溯到 start 的邻接 (即"下一步").
            int curIdx = CellIndex(found, cols);
            int startIdx = CellIndex(start, cols);
            while (prev[curIdx] != startIdx && prev[curIdx] != -1)
            {
                curIdx = prev[curIdx];
            }

            next = new(curIdx % cols, curIdx / cols);
            return true;
        }

        // ---------------------------------------------------------------
        // 验证
        // ---------------------------------------------------------------

        /// <summary>
        /// 该方向的移动是否被砖块允许 (allowedDirections).
        /// </summary>
        bool IsMoveAllowed(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            return delta.x switch
            {
                0 when delta.y == 1 => (config.allowedDirections & BrickGridMoveDirection.Up) != 0,
                0 when delta.y == -1 => (config.allowedDirections & BrickGridMoveDirection.Down) != 0,
                -1 when delta.y == 0 => (config.allowedDirections & BrickGridMoveDirection.Left) != 0,
                1 when delta.y == 0 => (config.allowedDirections & BrickGridMoveDirection.Right) != 0,
                _ => false
            };
        }

        /// <summary>
        /// 目标 cell 是否可用: 整个砖块 size 矩形都未占用、未锁、且在网格内.
        /// </summary>
        bool IsDestinationAvailable(Vector2Int targetAnchor)
        {
            int cols = _layout.getCols();
            int rows = _layout.getRows();
            int w = Mathf.Max(1, _brick.size.x);
            int h = Mathf.Max(1, _brick.size.y);

            for (int dy = 0; dy < h; dy++)
            {
                for (int dx = 0; dx < w; dx++)
                {
                    int cx = targetAnchor.x + dx;
                    int cy = targetAnchor.y + dy;
                    if (cx < 0 || cx >= cols || cy < 0 || cy >= rows)
                        return false;

                    var cell = new Vector2Int(cx, cy);
                    // 自身起点 cells 已被锁但允许经过 (因为自己马上要离开)
                    if (cell.x >= _fromAnchor.x && cell.x < _fromAnchor.x + w && cell.y >= _fromAnchor.y && cell.y < _fromAnchor.y + h)
                        continue;

                    if (_brickManager.IsCellOccupied(cell) || _brickManager.IsCellLocked(cell))
                        return false;
                }
            }

            return true;
        }

        // ---------------------------------------------------------------
        // 工具
        // ---------------------------------------------------------------

        Vector2Int WorldToAnchorCell(Vector2 worldPos)
        {
            if (_layout == null)
                return Vector2Int.zero;
            return new(
                _layout.getColAtPosX(worldPos.x),
                _layout.getRowAtPosY(worldPos.y)
            );
        }

        Vector2Int GetCurrentAnchorCell()
        {
            // 砖块 pivot 固定在所占网格的左下角 cell 中心,
            // 即 transform.position 就是左下角 cell 中心 → 直接转 cell 即可.
            return WorldToAnchorCell(transform.position);
        }

        Vector3 ComputeWorldFromAnchor(Vector2Int anchor)
        {
            // 砖块 pivot 在左下角 cell 中心 → 不能用 anchor cell 的几何中心,
            // 必须用 anchor cell 自己的中心坐标.
            float x = _layout.getPosXAtCol(anchor.x);
            float y = _layout.getPosYAtRow(anchor.y);
            return new(x, y, transform.position.z);
        }

        void ComputeOccupiedCells(Vector2Int anchor, ref List<Vector2Int> output)
        {
            output.Clear();
            int w = Mathf.Max(1, _brick.size.x);
            int h = Mathf.Max(1, _brick.size.y);
            for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++)
                output.Add(new(anchor.x + dx, anchor.y + dy));
        }

        void UpdateAttack(float dt)
        {
            // 仅监测: 一旦不再贴住玩家, 回到 Idle 重新寻路.
            var player = _brick.getTargetPlayer();
            if (player == null || player.IsDead())
            {
                State = MoveState.Idle;
                return;
            }

            if (!IsAdjacentToPlayer(player))
            {
                State = MoveState.Idle;
                _intervalTimer = 0f;
            }
        }

        /// <summary>砖块所占 cell 集 (基于 AnchorCell + size) 中是否存在与玩家 cell 上下左右相邻 (曼哈顿距离=1) 的 cell.</summary>
        bool IsAdjacentToPlayer(APlayer player)
        {
            Vector2Int playerCell = WorldToAnchorCell(player.getWorldPosition());
            int w = Mathf.Max(1, _brick.size.x);
            int h = Mathf.Max(1, _brick.size.y);
            int ax = AnchorCell.x;
            int ay = AnchorCell.y;

            // 砖块占据 [ax, ax+w) × [ay, ay+h), 只要这些 cell 中任一与 playerCell 共享一条边即可.
            // 等价条件: playerCell.x 在 [ax-1, ax+w] 且 playerCell.y 在 [ay-1, ay+h], 且 (Manhattan=1).
            // 简化为扫描 brick 边界 (外围那一圈), 任一 cell 与 playerCell 曼哈顿距离=1 即可.
            for (int dy = -1; dy <= h; dy++)
            {
                for (int dx = -1; dx <= w; dx++)
                {
                    // 仅取砖块外圈 (dx=-1 || dx==w || dy=-1 || dy==h)
                    bool onLeft = dx == -1;
                    bool onRight = dx == w;
                    bool onBottom = dy == -1;
                    bool onTop = dy == h;
                    if (!onLeft && !onRight && !onBottom && !onTop)
                        continue;

                    int cx = ax + dx;
                    int cy = ay + dy;
                    if (Mathf.Abs(cx - playerCell.x) + Mathf.Abs(cy - playerCell.y) == 1)
                        return true;
                }
            }

            return false;
        }

        bool IsPlayerInChaseRange(APlayer player)
        {
            if (config.chaseRadius <= 0f)
                return true;

            float dist = Vector2.Distance(transform.position, player.getWorldPosition());
            return dist <= config.chaseRadius;
        }

        // ---------------------------------------------------------------
        // 外部 API
        // ---------------------------------------------------------------

        /// <summary>强行取消当前移动 (例如被打断时).</summary>
        public void ForceStop()
        {
            if (State == MoveState.Idle)
                return;
            ReleaseMoveLocks();
            State = MoveState.Idle;
            MoveProgress = 0f;
            _intervalTimer = 0f;
            _restTimer = 0f;
        }

        /// <summary>刷新 anchor cell (例如 brick 被外部瞬间传送后).</summary>
        public void RefreshAnchor()
        {
            AnchorCell = GetCurrentAnchorCell();
            LastArrivedCell = AnchorCell;
        }

        /// <summary>查询当前是否处于移动状态 (含 Locked + Moving).</summary>
        public bool IsMoving => State != MoveState.Idle;
    }
}