using System;
using System.Collections.Generic;
using GeekAsset.Models;
using GeekAsset.Services;
using Sunny.UI;

namespace GeekAsset.Views
{
    /// <summary>人员档案管理。</summary>
    public class EmployeePage : BaseListPage
    {
        private static List<KeyValuePair<string, object>> StatusItems() => new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("在职", (byte)1),
            new KeyValuePair<string, object>("离职", (byte)0),
        };

        private List<KeyValuePair<string, object>> DeptItems()
        {
            var items = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("(未分配)", null) };
            foreach (var d in DeptService.GetAll())
                items.Add(new KeyValuePair<string, object>(d.DeptName, d.DeptID));
            return items;
        }

        protected override void BuildColumns()
        {
            AddColumn("EmpNo", "工号", 90);
            AddColumn("EmpName", "姓名", 90);
            AddColumn("DeptName", "部门", 120);
            AddColumn("Mobile", "手机", 110);
            AddColumn("StatusText", "状态", 70);
        }

        protected override void ReloadData() => Bind(EmployeeService.GetAll());

        private List<EditField> Fields(SysEmployee m) => new List<EditField>
        {
            EditField.Text("no", "工号", m?.EmpNo, true),
            EditField.Text("name", "姓名", m?.EmpName, true),
            EditField.Combo("dept", "部门", DeptItems(), m?.DeptID, false),
            EditField.Text("mobile", "手机", m?.Mobile),
            EditField.Combo("status", "状态", StatusItems(), m?.Status ?? (byte)1, true),
        };

        protected override void DoAdd()
        {
            var dlg = new EditDialog("新增人员", Fields(null));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            byte status = Convert.ToByte(dlg.Values["status"]);
            EmployeeService.Insert(new SysEmployee
            {
                EmpNo = dlg.GetString("no"),
                EmpName = dlg.GetString("name"),
                DeptID = dlg.GetNullableInt("dept"),
                Mobile = dlg.GetString("mobile"),
                Status = status,
                LeaveDate = status == 0 ? (DateTime?)DateTime.Today : null
            });
            ReloadData();
        }

        protected override void DoEdit()
        {
            var cur = Selected<SysEmployee>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            var dlg = new EditDialog("编辑人员", Fields(cur));
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            byte status = Convert.ToByte(dlg.Values["status"]);
            cur.EmpNo = dlg.GetString("no");
            cur.EmpName = dlg.GetString("name");
            cur.DeptID = dlg.GetNullableInt("dept");
            cur.Mobile = dlg.GetString("mobile");
            // 由在职转离职时记录离职日期
            if (cur.Status == 1 && status == 0) cur.LeaveDate = DateTime.Today;
            if (status == 1) cur.LeaveDate = null;
            cur.Status = status;
            EmployeeService.Update(cur);
            ReloadData();
        }

        protected override void DoDelete()
        {
            var cur = Selected<SysEmployee>();
            if (cur == null) { UIMessageTip.ShowWarning("请先选择一行"); return; }
            if (!UIMessageBox.ShowAsk($"确认删除人员「{cur.EmpName}」？")) return;
            EmployeeService.SoftDelete(cur.EmpID);
            ReloadData();
        }
    }
}
