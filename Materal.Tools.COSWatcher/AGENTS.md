# Materal.Tools.COSWatcher
监听本地目录并将新文件上传至腾讯云 COS 的控制台工具
# 文档目录

```text
docs\
└─ plans\
   ├─ 001-总体计划\
   │  ├─ design.md
   │  └─ impl.md
   ├─ 002-阶段一XXXXXX\
   │  ├─ design.md
   │  └─ impl.md
   └─ 003-阶段二XXXXX\
      ├─ design.md
      ├─ impl.md
      ├─ client\
      │  ├─ testPlan.md
      │  ├─ design.md
      │  ├─ impl.md
      │  ├─ pages\
      │  │  └─ XXX\
      │  │     ├─ design.md
      │  │     └─ impl.md
      │  └─ components\
      │     └─ XXX\
      │        ├─ design.md
      │        └─ impl.md
      └─ server\
         ├─ design.md
         ├─ impl.md
         └─ XXX\
            ├─ design.md
            └─ impl.md
```

- 计划文档需要按文件夹拆分，文件夹命名格式为 `001-XXXXXXXX`、`002-XXXXXXXXX`。
- 每个计划文件夹必须包含 `design.md` 和 `impl.md` 文件。
- 如果是具体的实现阶段计划，还需要补充 `Client/testPlan.md`、`Client/design.md`、`Client/impl.md`、`Server/design.md`、`Server/impl.md`。
- 如果服务端设计较为复杂，需要额外拆分设计文档，例如复杂工厂或供应者设计应补充 `Server/XXX/design.md` 和 `Server/XXX/impl.md`。
- 客户端需要拆分页面和组件设计，页面补充 `Client/Pages/XXX/design.md` 和 `Client/Pages/XXX/impl.md`，组件补充 `Client/Components/XXX/design.md` 和 `Client/Components/XXX/impl.md`。

# 文档维护策略

- `docs/plans/` 中的文档是**当前或已批准计划的最终状态**，不是工作日志、变更记录或历史档案；不要为保留旧版本、记录本次修改过程或描述一次小改动而新增 Markdown 文件或计划目录。
- 用户要求“修改文档”“补充文档”“同步文档”时，必须先检索 `docs/plans/`，按业务主题定位已有计划目录和已有 `design.md`、`impl.md`（以及对应 Client/Server、页面、组件文档），直接更新最贴合的现有文件。
- 目录名中的“阶段”仅表示计划的业务范围与实施顺序，**不表示**每次需求、缺陷修复、代码提交或文档更新都要创建一个新阶段；对某个阶段范围内的增量，继续维护该阶段的既有文档。
- 只有在需求确实引入了与现有计划不重叠的独立业务范围，且用户明确要求新建计划/新阶段时，才能创建新的 `NNN-名称` 计划目录。若归属不明确，先询问用户；不得以新建文档代替判断。
- 更新时清理已失效的描述；保留仍然适用的约束、验收标准和未来计划，并明确其状态。除非用户明确要求，不创建 changelog、会议纪要、迁移记录、执行日志、临时设计等旁路文档。
- 完成文档修改后，说明更新了哪些既有文件；如新建了文件或目录，必须说明其与现有文档无法合并的原因及用户的明确授权。

# 注意事项

- **不要主动提交代码**：完成代码修改后不要自动执行 `git commit`，需要提交时询问用户确认
- **提交说明必须使用中文**：任何 `git commit` 的提交信息都必须使用中文，不要使用英文提交描述
- **提交标题格式建议**：`类型: 中文描述`，类型使用英文前缀，描述使用中文
  - 常用类型：`feat`（新功能）、`fix`（修复）、`refactor`（重构）、`docs`（文档）
  - 示例：`feat: 添加数据导入功能`、`fix: 修复保存失败问题`、`refactor: 优化服务处理流程`、`docs: 补充接口使用说明`
- **禁止自己创建分支**：创建分支的动作需要获得用户的确定