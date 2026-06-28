using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace GeekAsset.Data
{
    /// <summary>
    /// 数据访问基座：统一从 App.config 读取连接串，提供连接与事务封装。
    /// 资产“主表更新 + 日志表插入”等操作请走 <see cref="InTransaction"/>。
    /// </summary>
    public static class Db
    {
        private static readonly string ConnStr =
            ConfigurationManager.ConnectionStrings["GeekAsset"].ConnectionString;

        /// <summary>打开并返回一个已开启的连接（调用方负责 using 释放）。</summary>
        public static IDbConnection Open()
        {
            var conn = new SqlConnection(ConnStr);
            conn.Open();
            return conn;
        }

        /// <summary>在单个事务内执行一组操作，异常自动回滚。</summary>
        public static void InTransaction(Action<IDbConnection, IDbTransaction> work)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        work(conn, tx);
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>带返回值的事务版本。</summary>
        public static T InTransaction<T>(Func<IDbConnection, IDbTransaction, T> work)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        var result = work(conn, tx);
                        tx.Commit();
                        return result;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
