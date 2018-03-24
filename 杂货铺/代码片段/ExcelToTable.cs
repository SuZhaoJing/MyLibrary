using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace AthenaCati.ProjectManager.Common
{
    public class ExcelToTable
    {
        /// <summary>
        /// 得到Excel连接字符串
        /// </summary>
        /// <param name="filepath">Excel文件路径</param>
        /// <returns>Excel连接字符串</returns>
        private static string GetConnectionString(string filePath)
        {
            string connectionString = "";
            if (System.IO.Path.GetExtension(filePath) == ".xls")
                connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=Excel 8.0;";
            else
                connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=YES\"";
            return connectionString;
        }

        /// <summary>
        /// 读取Excel文件所有的工作区表名
        /// </summary>
        /// <param name="filepath">Excel文件路径</param>
        /// <returns>表名数组string[]</returns>
        public static string[] GetTableName(string filePath)
        {
            string connectionString = GetConnectionString(filePath);
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                conn.Close();

                string[] tableNames = new string[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    tableNames[i] = dt.Rows[i]["TABLE_NAME"].ToString();
                }
                return tableNames;
            }
        }

        /// <summary>
        /// 读取Excel表数据
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <param name="tableName">Excel表名</param>
        /// <returns>Excel表数据DataTable</returns>
        public static DataTable ToDataTable(string filePath, string tableName)
        {
            string connectionString = GetConnectionString(filePath);
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                DataTable dt = new DataTable();
                string sql = "select * from [" + tableName + "]";
                OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
                conn.Open();
                da.Fill(dt);
                conn.Close();
                return dt;
            }
        }
    }
}
