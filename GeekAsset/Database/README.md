# GeekAsset 数据库建表台账

> 企业IT固定资产管理系统 —— 数据库脚本与建表记录，便于排查问题。
> 目标实例：`localhost`（SQL Server 2019，默认实例，Windows 身份）
> 数据库名：`GeekAsset`　ORM：Dapper　字符：表/字段统一 `NVARCHAR`。

## 一、目录结构

```
Database/
├── 00_CreateDatabase.sql        建库（幂等）
├── Tables/                      每表一个脚本（幂等、含 USE GeekAsset，可单独执行）
│   ├── 01_Sys_Dept.sql
│   ├── 02_Sys_Employee.sql
│   ├── 03_Sys_User.sql
│   ├── 04_Base_AssetCategory.sql
│   ├── 05_Base_Location.sql
│   ├── 06_Base_Supplier.sql
│   ├── 07_Base_DepreciationRule.sql
│   ├── 08_Asset_Info.sql
│   ├── 09_Asset_Flow_Log.sql
│   ├── 10_Asset_Hardware_Log.sql
│   ├── 11_Asset_Depreciation_Log.sql
│   ├── 12_Audit_Task.sql
│   ├── 13_Audit_Detail.sql
│   ├── 14_Consumables_Info.sql
│   └── 15_Consumables_Log.sql
├── 99_SeedData.sql             初始数据（默认 admin / 顶层组织 / 分类根）
└── README.md                   本台账
```

## 二、执行顺序（务必按编号，存在外键依赖）

脚本均幂等（`IF OBJECT_ID ... IS NULL`），可重复执行不报错。
**01→15** 顺序不可乱：主数据表先建，资产表 08 依赖 02/04/05/06，日志/明细表依赖 08。

### 方式 A：一键执行（推荐，sqlcmd）
```bash
# 在 Database 目录下执行
sqlcmd -S localhost -E -i 00_CreateDatabase.sql
for f in Tables/0*.sql Tables/1*.sql; do sqlcmd -S localhost -E -i "$f"; done
sqlcmd -S localhost -E -i 99_SeedData.sql
```

### 方式 B：逐个执行
在 SSMS 中按 01→15 顺序依次打开运行，最后运行 `99_SeedData.sql`。

## 三、表清单与职责

| # | 表名 | 模块 | 职责 | 关键约束 |
|---|------|------|------|---------|
| 01 | `Sys_Dept` | 基础设置 | 组织架构（公司-部门-小组）自引用树 | FK 自引用 ParentID |
| 02 | `Sys_Employee` | 基础设置 | 人员档案；Status 离职联动未交接预警 | EmpNo 唯一 |
| 03 | `Sys_User` | 权限 | 登录用户 RBAC | Role∈{Admin,Operator,Finance} |
| 04 | `Base_AssetCategory` | 基础设置 | 资产分类树（硬件/软件） | FK 自引用 |
| 05 | `Base_Location` | 基础设置 | 存放位置库 | — |
| 06 | `Base_Supplier` | 基础设置 | 供应商/维保商 | — |
| 07 | `Base_DepreciationRule` | 折旧 | 按分类配置年限+残值率 | CategoryID 唯一 |
| 08 | `Asset_Info` | 生命周期 | 资产黄金卡片（主表） | AssetNo 唯一；含财务/维保/IT/防调包字段 |
| 09 | `Asset_Flow_Log` | 生命周期 | **流转日志**：领用/退还/借用/归还/调拨 | ActionType CHECK |
| 10 | `Asset_Hardware_Log` | 维修维保 | **硬件/维保变更日志**：换件/升级/维修 + Old/New_Spec + 费用 | ChangeType CHECK |
| 11 | `Asset_Depreciation_Log` | 折旧 | 月度折旧流水 | **(AssetID,Year,Month) 唯一→幂等防重** |
| 12 | `Audit_Task` | 盘点 | 盘点任务（按部门/位置/分类） | ScopeType CHECK |
| 13 | `Audit_Detail` | 盘点 | 盘点明细与差异（盘盈/盘亏/信息不符） | FK Task/Asset |
| 14 | `Consumables_Info` | 耗材 | 耗材主表+微仓库存 | Inventory≥0 CHECK |
| 15 | `Consumables_Log` | 耗材 | 出入库流水 | Qty>0；ActionType∈{1,2} |

## 四、关键设计决策（与项目计划书对应）

- **日志双表分离**（计划书 3.1）：流转(`Asset_Flow_Log`) 与 硬件变更(`Asset_Hardware_Log`) 拆开，避免大表字段冗余。
- **防调包**（计划书 3.2）：`Asset_Info.Original_Spec`(只读) vs `Current_Spec`(可变)；任何配置变更须经维修/升级窗体并写 `Asset_Hardware_Log`。
- **折旧三铁律**（计划书 2.5）：当月增加次月计提（业务层控制）；残值封顶（`Asset_Info.IsDepreciated` + 计提时净值不破残值）；幂等防重（11 表联合唯一索引 + `IF NOT EXISTS` 校验）。
- **RBAC**（计划书 三点五.A）：`Sys_User.Role` 硬编码三角色；Operator 禁物理删除——业务层一律走 `IsDeleted` 作废标记。
- **耗材防超扣**（计划书 3.3）：领用须在事务内 `UPDATE ... SET Inventory=Inventory-@Qty WHERE Inventory>=@Qty`，按受影响行数判定成功。

## 五、状态字段取值速查

- `Asset_Info.Status`：1=闲置 2=在用 3=报修 4=借出 5=报废
- `Sys_Employee.Status`：1=在职 0=离职
- `Base_AssetCategory.AssetType`：1=硬件 2=虚拟/软件
- `Base_Supplier.SupplierType`：1=供应商 2=维保商 3=两者
- `Asset_Info.IPType`：1=固定IP 2=DHCP
- `Asset_Hardware_Log.RepairMethod`：1=内部更换 2=原厂/第三方外修
- `Audit_Task.ScopeType`：1=部门 2=位置 3=分类
- `Audit_Detail.ResultType`：1=正常 2=盘盈 3=盘亏 4=信息不符
- `Consumables_Log.ActionType`：1=入库 2=领用

## 六、执行记录（Change Log）

| 日期 | 操作 | 结果 | 备注 |
|------|------|------|------|
| 2026-06-28 | 初始建库 + 01~15 建表 + Seed | ✅ 成功，15 张表全部创建，admin/总公司/分类根已初始化 | 默认实例 localhost；旧版 sqlcmd 读 UTF-8 会乱码，改用 .NET SqlClient 执行 |

> 默认管理员：`admin / Admin@123`，首次登录后请立即修改。
