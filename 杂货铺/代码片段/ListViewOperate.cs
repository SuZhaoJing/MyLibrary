using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace AthenaCati.ProjectManager.Common
{
    public class ListViewOperate
    {
        private static Dictionary<string, string> dic = new Dictionary<string, string>();
        /// <summary>
        /// 获取DataTable列名
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<string> GetColumnName(DataTable dt)
        {
            List<string> ls = new List<string>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dt.Columns[i].ColumnName != "Pa_RandPhoneLen")
                {
                    ls.Add(dt.Columns[i].ColumnName);
                }
            }
            return ls;
        }

        public static string GetColumnName(int type, string name)
        {
            dic.Clear();
            dic.Add("N_Name", "国家");
            dic.Add("N_Code", "国家代号");
            dic.Add("P_Name", "省或直辖市");
            dic.Add("C_Name", "城市");
            dic.Add("C_Code", "区号");
            dic.Add("Se_Name", "地区");
            dic.Add("Pa_Name", "局名");
            dic.Add("Pa_Code", "局号");
            dic.Add("Pa_PhoneLen", "号码长度");
            return ChangeName(dic, type, name);
        }


        /// <summary>
        /// 获取运算符
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetOperator(int type, string name)
        {
            dic.Clear();
            dic.Add("=", "等于");
            dic.Add(">", "大于");
            dic.Add(">=", "大于等于");
            dic.Add("<", "小于");
            dic.Add("<=", "小于等于");
            dic.Add("<>", "不等于");
            dic.Add("like", "包含");
            dic.Add("is", "是");
            dic.Add("or", "或者");
            dic.Add("and", "并且");
            return ChangeName(dic, type, name);
        }

        /// <summary>
        /// 名称转换
        /// </summary>
        /// <param name="type">转换类型，1--获取值，2--获取键</param>
        /// <param name="name">需要转换的字符串</param>
        /// <returns>转换后的结果</returns>
        public static string ChangeName(Dictionary<string, string> dict, int type, string name)
        {

            string res = "";
            if (dict.Count > 0)
            {
                if (type == 1)
                {
                    res = dict[name];
                }
                else
                {
                    foreach (string key in dict.Keys)
                    {
                        if (dict[key].Equals(name))
                        {
                            res = key;
                        }
                    }
                }
            }
            else
            {
                res = name;
            }

            return res;
        }

        /// <summary>
        /// 获取选中列下的数据
        /// </summary>
        /// <param name="name">列名</param>
        /// <returns>返回查询到的集合</returns>
        public static List<string> GetSelectedData(DataTable dt, string name)
        {
            List<string> ls = new List<string>();
            DataView dv = new DataView(dt);
            dt = dv.ToTable(true, name);
            dt.DefaultView.Sort = dt.Columns[0].ColumnName+" ASC";

            dt = dt.DefaultView.ToTable();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ls.Add(dt.Rows[i][name].ToString());
            }
            return ls;
        }


         /// <summary>
        /// 为ListView绑定DataTable数据项
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="lv">ListView控件</param>
        public static void DataTableToListview(DataTable dt, ListView lv)
        {
            if (dt != null)
            {
                lv.View = View.Details;
                lv.GridLines = true;//显示网格线
                lv.Items.Clear();//所有的项
                lv.Columns.Clear();//标题
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    //lv.Columns.Add(dt.Columns[i].Caption);//增加标题
                    lv.Columns.Add(MyTools.GetText(MyTools.ChangeType.GetValue, 0, dt.Columns[i].Caption));//增加标题
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ListViewItem lvi = new ListViewItem(dt.Rows[i][0].ToString());
                    for (int j = 1; j < dt.Columns.Count; j++)
                    {
                        // lvi.ImageIndex = 0;
                        lvi.SubItems.Add(dt.Rows[i][j].ToString());
                    }
                    lv.Items.Add(lvi);
                }
                lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);//调整列的宽度
            }
        }

        /// <summary>
        /// ListView反向填充DataTable数据项
        /// </summary>
        /// <param name="lv">ListView控件</param>
        /// <param name="dt">DataTable</param>
        public static void ListViewToDataTable(ListView lv, DataTable dt)
        {
            dt.Clear();
            dt.Columns.Clear();
            for (int k = 0; k < lv.Columns.Count; k++)
            {
                dt.Columns.Add(lv.Columns[k].Text.Trim());//生成DataTable列头
            }
            for (int i = 0; i < lv.Items.Count; i++)
            {
                var dr = dt.NewRow();
                for (int j = 0; j < lv.Columns.Count; j++)
                {
                    dr[j] = lv.Items[i].SubItems[j].Text.Trim();
                }
                dt.Rows.Add(dr);//每行内容
            }
        }


    }
}
