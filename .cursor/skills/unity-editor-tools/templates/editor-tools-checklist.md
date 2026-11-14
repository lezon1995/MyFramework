# Editor 工具开发清单

> 工具名称：`<工具名>`　文件路径：`<path/Editor/...cs>`　程序集：`Editor_Game`

## 1. 位置与程序集

- [ ] 文件位于 Editor 目录 或 所在 asmdef 设置了 `"includePlatforms": ["Editor"]`
- [ ] 确认不会被打进运行时包（Build Report 中无该程序集）
- [ ] asmdef 依赖已声明（引用了 `Game` / `HotFix` 需在 references 中列出）
- [ ] 命名符合约定：`XxxEditor` / `XxxWindow` / `XxxTools`

## 2. Inspector 规范

- [ ] 使用 `SerializedProperty` 而非直接改字段
- [ ] 正确调用 `serializedObject.Update()` / `ApplyModifiedProperties()`
- [ ] 需要多选时加 `[CanEditMultipleObjects]`
- [ ] `RequiresConstantRepaint()` 仅在确有实时值时返回 true
- [ ] `OnInspectorGUI` 内无 `AssetDatabase` 查询 / 文件 IO / 重计算

## 3. 窗口规范

- [ ] 窗口状态用 `[SerializeField]` 字段保存
- [ ] 设置了 `minSize`
- [ ] 菜单路径统一 `Tools/<项目>/<模块>/<操作>`
- [ ] 长耗时操作有进度条，且 `finally` 中 `ClearProgressBar()`

## 4. 修改安全性

- [ ] 修改前 `Undo.RegisterCompleteObjectUndo`
- [ ] 批量导入用 `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` 成对
- [ ] 结束前 `EditorUtility.SetDirty()` + `AssetDatabase.SaveAssets()`
- [ ] 有二次确认（`EditorUtility.DisplayDialog`）或 `Validate` 菜单函数
- [ ] 支持 Undo（避免不可撤销的破坏性操作）

## 5. 批量操作

- [ ] 用 `AssetDatabase.FindAssets("t:Type", new[] { "路径" })` 限定搜索范围
- [ ] 跳过加载失败的资源（`LoadAssetAtPath` 返回 null）
- [ ] 处理进程：`try/finally` 保证进度条清理
- [ ] 结束时输出处理数量日志

## 6. 构建 / CI

- [ ] `-executeMethod` 指向完整命名空间路径
- [ ] 使用 `Application.isBatchMode` 判断后调用 `EditorApplication.Exit(code)`
- [ ] 输出路径含时间戳，避免覆盖
- [ ] 构建失败时输出错误数与警告数
- [ ] 校验逻辑用 `IPreprocessBuildWithReport` 实现，抛 `BuildFailedException`

## 7. 性能与健壮性

- [ ] 无每帧执行的 `AssetDatabase.FindAssets`
- [ ] 无无限循环 / 递归深度不可控
- [ ] 目录操作前检查 `AssetDatabase.IsValidFolder`
- [ ] 异常有 try/catch，失败时给用户明确提示

## 8. 结论

- 通过项：`N / M`
- 需修改：

| # | 位置 | 问题 | 严重级别 | 建议 |
|---|------|------|---------|------|
| 1 | | | 严重/一般/建议 | |
