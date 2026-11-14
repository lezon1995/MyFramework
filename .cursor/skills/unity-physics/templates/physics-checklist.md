# 物理排查清单

> 对象：`<场景 / 预制体路径>`　问题现象：`<穿透 / 抖动 / 打滑 / 不触发 / 检测不到>`

## 1. 基础属性核对

| 对象 | Rigidbody | Mass | Drag | Use Gravity | Is Kinematic | Interpolate | Collision Detection | Constraints |
|------|-----------|------|------|--------------|--------------|-------------|---------------------|-------------|
| | 有/无 | | | | | | | |

| 对象 | Collider 类型 | isTrigger | PhysicMaterial | Layer |
|------|--------------|-----------|----------------|-------|
| | | | | |

## 2. 常见根因排查

- [ ] 是否同时用 `transform.position` 和 Rigidbody 驱动同一物体（冲突）
- [ ] 高速物体是否设置 `Continuous Dynamic`
- [ ] `Time.fixedDeltaTime` 是否过小导致单步位移过大（穿模）
- [ ] 缩放（scale）是否异常导致 Collider 变形
- [ ] 手动修改 transform 后是否调用 `Physics.SyncTransforms()`
- [ ] 层碰撞矩阵是否禁用了需要的组合
- [ ] Trigger 是否双方至少一方有 Rigidbody
- [ ] 是否混用 `Physics`（3D）与 `Physics2D`（2D）
- [ ] Rigidbody 质量是否与场景其它物体差异过大
- [ ] `Solver Iterations` 是否过低导致堆叠抖动
- [ ] 摩擦力是否为 0 或 `Bounce Threshold` 过高导致接触被忽略

## 3. 检测类问题

- [ ] LayerMask 是否包含目标层
- [ ] `QueryTriggerInteraction` 是否匹配（Ignore / Collide / UseGlobal）
- [ ] 射线起点是否在 Collider 内部（起点在内部会被忽略）
- [ ] 射线方向是否正确、距离是否足够
- [ ] 是否用了 `RaycastAll` 产生 GC（改 `RaycastNonAlloc`）
- [ ] 目标 Collider 是否在 `Physics.queriesHitBackfaces` 关闭时被忽略

## 4. 车辆专项（WheelCollider）

- [ ] Rigidbody mass 是否合理（1400~1600）
- [ ] `centerOfMass` 是否下移防翻车
- [ ] `suspensionDistance` / `suspensionSpring` 是否合理
- [ ] 转向角 `steerAngle` 是否过激
- [ ] 刹车是否用 `brakeTorque` 而非负 `motorTorque`
- [ ] 视觉轮子是否同步（`GetWorldPose`）
- [ ] 是否需要 `maxSuspensionForce` 限制防止弹飞

## 5. 角色专项（CharacterController）

- [ ] `isGrounded` 是否可靠（建议额外射线）
- [ ] `slopeLimit` / `stepOffset` 是否合适
- [ ] `Move` 返回值是否被用来计算真实速度
- [ ] 重力是否手动累加

## 6. 结论

| # | 根因 | 涉及对象 | 修复方案 | 验证结果 |
|---|------|---------|---------|---------|
| 1 | | | | |
