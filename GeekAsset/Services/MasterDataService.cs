using System.Collections.Generic;
using System.Linq;
using Dapper;
using GeekAsset.Data;
using GeekAsset.Models;

namespace GeekAsset.Services
{
    /// <summary>组织架构服务。删除一律软删除（IsDeleted=1），符合“禁止物理删除”。</summary>
    public static class DeptService
    {
        public static List<SysDept> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<SysDept>(
                    "SELECT DeptID, ParentID, DeptName, DeptLevel, SortOrder FROM Sys_Dept WHERE IsDeleted=0 ORDER BY SortOrder, DeptID").ToList();
        }

        public static int Insert(SysDept d)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Sys_Dept(ParentID, DeptName, DeptLevel, SortOrder)
                      VALUES(@ParentID,@DeptName,@DeptLevel,@SortOrder); SELECT CAST(SCOPE_IDENTITY() AS INT);", d);
        }

        public static void Update(SysDept d)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Sys_Dept SET ParentID=@ParentID, DeptName=@DeptName, DeptLevel=@DeptLevel, SortOrder=@SortOrder WHERE DeptID=@DeptID", d);
        }

        public static bool HasChildren(int deptId)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>("SELECT COUNT(1) FROM Sys_Dept WHERE ParentID=@id AND IsDeleted=0", new { id = deptId }) > 0;
        }

        public static void SoftDelete(int deptId)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Sys_Dept SET IsDeleted=1 WHERE DeptID=@id", new { id = deptId });
        }
    }

    public static class EmployeeService
    {
        public static List<SysEmployee> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<SysEmployee>(
                    @"SELECT e.EmpID, e.EmpNo, e.EmpName, e.DeptID, e.Mobile, e.Status, e.LeaveDate, d.DeptName
                      FROM Sys_Employee e LEFT JOIN Sys_Dept d ON e.DeptID=d.DeptID
                      WHERE e.IsDeleted=0 ORDER BY e.EmpID").ToList();
        }

        public static int Insert(SysEmployee e)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Sys_Employee(EmpNo, EmpName, DeptID, Mobile, Status, LeaveDate)
                      VALUES(@EmpNo,@EmpName,@DeptID,@Mobile,@Status,@LeaveDate); SELECT CAST(SCOPE_IDENTITY() AS INT);", e);
        }

        public static void Update(SysEmployee e)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Sys_Employee SET EmpNo=@EmpNo, EmpName=@EmpName, DeptID=@DeptID, Mobile=@Mobile, Status=@Status, LeaveDate=@LeaveDate WHERE EmpID=@EmpID", e);
        }

        public static void SoftDelete(int empId)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Sys_Employee SET IsDeleted=1 WHERE EmpID=@id", new { id = empId });
        }
    }

    public static class CategoryService
    {
        public static List<BaseAssetCategory> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<BaseAssetCategory>(
                    "SELECT CategoryID, ParentID, CategoryName, AssetType, SortOrder FROM Base_AssetCategory WHERE IsDeleted=0 ORDER BY SortOrder, CategoryID").ToList();
        }

        public static int Insert(BaseAssetCategory m)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Base_AssetCategory(ParentID, CategoryName, AssetType, SortOrder)
                      VALUES(@ParentID,@CategoryName,@AssetType,@SortOrder); SELECT CAST(SCOPE_IDENTITY() AS INT);", m);
        }

        public static void Update(BaseAssetCategory m)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Base_AssetCategory SET ParentID=@ParentID, CategoryName=@CategoryName, AssetType=@AssetType, SortOrder=@SortOrder WHERE CategoryID=@CategoryID", m);
        }

        public static bool HasChildren(int categoryId)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>("SELECT COUNT(1) FROM Base_AssetCategory WHERE ParentID=@id AND IsDeleted=0", new { id = categoryId }) > 0;
        }

        public static void SoftDelete(int categoryId)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Base_AssetCategory SET IsDeleted=1 WHERE CategoryID=@id", new { id = categoryId });
        }
    }

    public static class LocationService
    {
        public static List<BaseLocation> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<BaseLocation>("SELECT LocationID, LocationName, Remark FROM Base_Location WHERE IsDeleted=0 ORDER BY LocationID").ToList();
        }

        public static int Insert(BaseLocation m)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    "INSERT INTO Base_Location(LocationName, Remark) VALUES(@LocationName,@Remark); SELECT CAST(SCOPE_IDENTITY() AS INT);", m);
        }

        public static void Update(BaseLocation m)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Base_Location SET LocationName=@LocationName, Remark=@Remark WHERE LocationID=@LocationID", m);
        }

        public static void SoftDelete(int id)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Base_Location SET IsDeleted=1 WHERE LocationID=@id", new { id });
        }
    }

    public static class SupplierService
    {
        public static List<BaseSupplier> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<BaseSupplier>(
                    "SELECT SupplierID, SupplierName, SupplierType, ContactPerson, Phone, PaymentTerm, Remark FROM Base_Supplier WHERE IsDeleted=0 ORDER BY SupplierID").ToList();
        }

        public static int Insert(BaseSupplier m)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Base_Supplier(SupplierName, SupplierType, ContactPerson, Phone, PaymentTerm, Remark)
                      VALUES(@SupplierName,@SupplierType,@ContactPerson,@Phone,@PaymentTerm,@Remark); SELECT CAST(SCOPE_IDENTITY() AS INT);", m);
        }

        public static void Update(BaseSupplier m)
        {
            using (var c = Db.Open())
                c.Execute(@"UPDATE Base_Supplier SET SupplierName=@SupplierName, SupplierType=@SupplierType, ContactPerson=@ContactPerson,
                            Phone=@Phone, PaymentTerm=@PaymentTerm, Remark=@Remark WHERE SupplierID=@SupplierID", m);
        }

        public static void SoftDelete(int id)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Base_Supplier SET IsDeleted=1 WHERE SupplierID=@id", new { id });
        }
    }

    public static class DeprecRuleService
    {
        public static List<BaseDepreciationRule> GetAll()
        {
            using (var c = Db.Open())
                return c.Query<BaseDepreciationRule>(
                    @"SELECT r.RuleID, r.CategoryID, r.UsefulLifeYears, r.ResidualRate, cat.CategoryName
                      FROM Base_DepreciationRule r LEFT JOIN Base_AssetCategory cat ON r.CategoryID=cat.CategoryID
                      ORDER BY r.RuleID").ToList();
        }

        public static bool ExistsForCategory(int categoryId, int excludeRuleId)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM Base_DepreciationRule WHERE CategoryID=@cid AND RuleID<>@rid",
                    new { cid = categoryId, rid = excludeRuleId }) > 0;
        }

        public static int Insert(BaseDepreciationRule m)
        {
            using (var c = Db.Open())
                return c.ExecuteScalar<int>(
                    @"INSERT INTO Base_DepreciationRule(CategoryID, UsefulLifeYears, ResidualRate)
                      VALUES(@CategoryID,@UsefulLifeYears,@ResidualRate); SELECT CAST(SCOPE_IDENTITY() AS INT);", m);
        }

        public static void Update(BaseDepreciationRule m)
        {
            using (var c = Db.Open())
                c.Execute("UPDATE Base_DepreciationRule SET CategoryID=@CategoryID, UsefulLifeYears=@UsefulLifeYears, ResidualRate=@ResidualRate WHERE RuleID=@RuleID", m);
        }

        // 折旧规则为物理删除（无 IsDeleted 列，且规则可随时重配）
        public static void Delete(int ruleId)
        {
            using (var c = Db.Open())
                c.Execute("DELETE FROM Base_DepreciationRule WHERE RuleID=@id", new { id = ruleId });
        }
    }
}
