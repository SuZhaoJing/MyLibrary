using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;
using AthenaCati.Utils;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace AthenaCati.ProjectManager.Common
{
    public class MyTools
    {
        public static DataTable MyDt;//临时存储的DataTable，右键导出也有用到

        #region 比较两表，获取不同的列
        /// <summary>
        /// 比较两表，获取不同的列
        /// </summary>
        /// <param name="tableName">要比较的表</param>
        /// <param name="baseTableName">原始表（基础表）</param>
        /// <returns></returns>
        public static DataTable GetNewColumn(string tableName, string baseTableName)
        {
            List<string> ls = new List<string>();
            DataTable dt1 = GetData.GetTableColumns(baseTableName);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    ls.Add(dt1.Rows[i][0].ToString());
                }
            }

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new[] { 
                           new DataColumn("clName",typeof(string)),
                            new DataColumn("clValue",typeof(string))
              });

            DataTable dt2 = GetData.GetTableColumns(tableName);
            if (dt2 != null && dt2.Rows.Count > 0)
            {
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    string clName = dt2.Rows[i][0].ToString();
                    if (!ls.Contains(clName))
                    {
                        ls.Add(clName);
                    }
                    else
                    {
                        ls.Remove(clName);
                    }
                }

                string[] notCols = { "Pa_ID", "Pa_PhoneLen", "Pa_RandPhoneLen", "aId", "Se_ID", "Pa_Name", "Pa_Code" };
                foreach (string col in notCols)
                {
                    ls.Remove(col);
                }
                ls.Add("citycode");
                ls.Add("pa_phone");
                ls.Add("extPhone");


                foreach (string str in ls)
                {
                    DataRow row = dt.NewRow();
                    row[0] = GetText(ChangeType.GetKey, 0, str);
                    row[1] = "";
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

        /// <summary>
        /// 比较两表，获取不同的列
        /// </summary>
        /// <param name="tableName">要比较的表</param>
        /// <param name="baseTableName">原始表（基础表）</param>
        /// <returns></returns>
        public static List<string> GetNewColumnToList(string tableName, string baseTableName)
        {
            List<string> ls = new List<string>();
            DataTable dt1 = GetData.GetTableColumns(baseTableName);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    ls.Add(dt1.Rows[i][0].ToString());
                }
            }


            DataTable dt2 = GetData.GetTableColumns(tableName);
            if (dt2 != null && dt2.Rows.Count > 0)
            {
                for (int i = 0; i < dt2.Rows.Count; i++)
                {
                    string clName = dt2.Rows[i][0].ToString();
                    if (!ls.Contains(clName))
                    {
                        ls.Add(clName);
                    }
                    else
                    {
                        ls.Remove(clName);
                    }
                }

                string[] notCols = { "Pa_ID", "Pa_PhoneLen", "Pa_RandPhoneLen", "aId", "Se_ID", "Pa_Name", "Pa_Code" };
                foreach (string col in notCols)
                {
                    ls.Remove(col);
                }
                ls.Add("citycode");
                ls.Add("pa_phone");
                ls.Add("extPhone");
            }
            return ls;
        }
        #endregion

        #region 白名单样本导入

        public static string WlId;//白名单编号
        public static string TableName;//白名单表名
        //public static string FileName;//写入的文件名(*.sql)
        public static DataTable Table;//待导入的表
        /// <summary>
        /// 白名单样本导入
        /// </summary>
        public static string WriteSql()
        {
            if (string.IsNullOrEmpty(WlId))
            {
                WlId = StaticInfo.WlId;
            }

            string fileName = Application.StartupPath + "\\" + StaticInfo.PrCode + ".sql";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            //-------拼接Sql语句，写入文件-------//
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode))
                {
                    try
                    {
                        //sw.WriteLine("Use AthenaCati");
                        //sw.WriteLine("GO");
                        DataTable dtCols = GetData.GetTableColumns(TableName);
                        if (dtCols.Rows.Count < 1) //判断是否是空表
                        {
                            /*
                             * 没有此表或是空表
                             * 先删除此表，再创建，然后导入数据
                             */
                            sw.WriteLine("if exists (select * from sysobjects where id = OBJECT_ID('[" + TableName +
                                         "]') and OBJECTPROPERTY(id, 'IsUserTable') = 1) DROP TABLE [" + TableName +
                                         "]CREATE TABLE [" + TableName +
                                         "] ([scode] [int]  IDENTITY (1, 1)  NOT NULL,[wsId] [int]  NOT NULL DEFAULT (0),[isDel] [int]  NOT NULL DEFAULT (0),[wDes] [nvarchar]  (512) NULL,[createDate] [datetime]  NOT NULL DEFAULT (getdate()),[endDate] [datetime]  NULL,[useCount] [int]  NOT NULL DEFAULT (0)) ALTER TABLE [" +
                                         TableName + "] WITH NOCHECK ADD  CONSTRAINT [PK_" + TableName +
                                         "] PRIMARY KEY  NONCLUSTERED ( [scode] );");

                            sw.WriteLine("GO");
                        }

                        //添加列
                        string colsName = "";
                        if (dtCols.Columns.Count > 0)
                            dtCols.PrimaryKey = new[] { dtCols.Columns[0] }; //设置首列为表的主键
                        for (int m = 0; m < Table.Columns.Count; m++)
                        {
                            if (!dtCols.Rows.Contains(Table.Columns[m].ColumnName))
                            {
                                sw.WriteLine("alter table [" + TableName + "] add [" + Table.Columns[m].ColumnName +
                                             "] varchar(150);");
                            }

                            if (m < Table.Columns.Count - 1)
                            {
                                colsName += "[" + Table.Columns[m].ColumnName + "],";

                            }
                            else
                            {

                                colsName += "[" + Table.Columns[m].ColumnName + "]";

                            }
                        }
                        if (colsName.Length > 0)
                        {
                            colsName = "," + colsName;
                            sw.WriteLine("GO");
                        }

                        //拼接sql语句
                        for (int k = 0; k < Table.Rows.Count; k++)
                        {
                            string insertSql = "";
                            for (int j = 0; j < Table.Columns.Count; j++)
                            {
                                if (j < Table.Columns.Count - 1)
                                {
                                    insertSql += "'" + Table.Rows[k][j] + "',";
                                }
                                else
                                {
                                    insertSql += "'" + Table.Rows[k][j] + "'";
                                }
                            }//,pa_phone,citycode

                            string sql = "insert into [" + TableName + "](wsId,isDel,createDate,useCount" + colsName +
                                         ") values('" + WlId + "',0,'" + DateTime.Now + "',0," + insertSql + ");";

                            sw.WriteLine(sql);
                        }
                    }
                    catch (Exception ex)
                    {
                        // throw new Exception(ex.Message);
                        MessageBoxE.Show(ex.Message);
                    }
                    finally
                    {
                        sw.Close();
                        fs.Close();
                    }
                }
            }
            return "over";
        }
        #endregion

        #region 语言转换
        /// <summary>
        /// 转换方式
        /// </summary>
        public enum ChangeType
        {
            //获取键  //获取值
            GetKey, GetValue
        }

        /// <summary>
        /// 获取项目语言下的释义
        /// </summary>
        /// <param name="ct">转换方式，获取键 / 获取值</param>
        /// <param name="type">0-数据库字段，1-弹窗提示信息</param>
        /// <param name="str">待转义的字符串</param>
        /// <returns></returns>
        public static string GetText(ChangeType ct, int type, string str)
        {
            string text = "";
            switch (ct)
            {
                case ChangeType.GetKey://获取键
                    text = ChangeName(StaticInfo.DicLanguage, ChangeType.GetValue, str);
                    break;
                case ChangeType.GetValue://获取值
                    string path = StaticInfo.LangPath;
                    IniHelper ini = new IniHelper(path);
                    text = ini.IniReadValue(type == 0 ? "数据库字段" : "提示信息", str);
                    break;
            }
            return string.IsNullOrEmpty(text) ? str : text;
        }

        /// <summary>
        /// 名称转换
        /// </summary>
        /// <param name="dict">存储转换的字典</param>
        /// <param name="ct">转换类型，1--获取值，2--获取键</param>
        /// <param name="name">需要转换的字符串</param>
        /// <returns>转换后的结果</returns>
        public static string ChangeName(Dictionary<string, string> dict, ChangeType ct, string name)
        {

            string res = "";
            if (dict.Count > 0)
            {
                if (ct == ChangeType.GetKey)
                {
                    res = dict.ContainsKey(name) ? dict[name] : name;
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
        #endregion

        #region 文本框只能输入数字
        ///文本框只能输入数字
        public static void WriteNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 0x20) e.KeyChar = (char)0;  //禁止空格键
            if ((e.KeyChar == 0x2D) && (((TextBox)sender).Text.Length == 0)) return; //处理负数
            if (e.KeyChar > 0x20)
            {
                try
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    double.Parse(((TextBox)sender).Text + e.KeyChar);
                }
                catch
                {
                    e.KeyChar = (char)0;   //处理非法字符
                }
            }
        }
        #endregion

        #region 数据导出

        /// <summary>
        /// 将DataTable对象转换成XML字符串
        /// </summary>
        /// <param name="dt">DataTable对象</param>
        /// <returns>XML字符串</returns>
        public static string DataToXml(DataTable dt)
        {
            StringBuilder strXml = new StringBuilder();
            strXml.AppendLine("<Data>");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                strXml.AppendLine("<rows>");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    strXml.AppendLine("<" + dt.Columns[j].ColumnName + ">" + dt.Rows[i][j] + "</" + dt.Columns[j].ColumnName + ">");
                }
                strXml.AppendLine("</rows>");
            }
            strXml.AppendLine("</Data>");
            return strXml.ToString();
        }


        /// <summary>
        /// 导出到html
        /// </summary>
        /// <param name="exportFileName">导出路径</param>
        /// <param name="isPrint">是否打印</param>
        /// <param name="tbl">需要导出的表DataTable</param>
        /// <returns>html代码</returns>
        public static string GetHtmlString(string exportFileName, bool isPrint, DataTable tbl)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<HTML><HEAD>");
            sb.Append("<title>" + exportFileName + "</title>");
            sb.Append("<META HTTP-EQUIV='content-type' CONTENT='text/html; charset=utf-8'> ");
            sb.Append("<style type=text/css>");
            sb.Append("table {margin: 0 auto;}");
            sb.Append(".gridtable td{ text-overflow:ellipsis;white-space: nowrap;padding:2px}");
            sb.Append("table.gridtable {font-family: verdana,arial,sans-serif;font-size:11px;color:#333333;border-width: 1px;border-color: #666666;border-collapse: collapse;}");
            sb.Append("table.gridtable th {border-width: 1px;padding: 8px;border-style: solid;border-color: #666666;background-color: #dedede;}");
            sb.Append("table.gridtable td {border-width: 1px;padding: 8px;border-style: solid;border-color: #666666;background-color: #ffffff;}");
            sb.Append("</style>");
            sb.Append("</HEAD>");
            sb.Append(!isPrint ? "<BODY  >" : "<BODY   onload = 'window.print()'>");
            sb.Append("<table cellSpacing='0' cellPadding='0' width ='90%' border='1'class='gridtable'>");
            sb.Append("<tr valign='middle' style='hight:10px'>");
            // sb.Append("<td><b>" + CommonUI.Translate("RowSequences") + "</b></td>");
            sb.Append("<td style='background:#D6D6D6'><b><span>ID</span></b></td>");
            foreach (DataColumn column in tbl.Columns)
            {
                sb.Append("<td style='background:#D6D6D6' ><b><span>" + GetText(ChangeType.GetValue, 0, column.ColumnName) + "</span></b></td>");
            }
            sb.Append("</tr>");
            int iColsCount = tbl.Columns.Count;
            int rowsCount = tbl.Rows.Count - 1;
            for (int j = 0; j <= rowsCount; j++)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + (j + 1) + "</td>");
                for (int k = 0; k <= iColsCount - 1; k++)
                {
                    sb.Append("<td");
                    sb.Append(">");
                    object obj = tbl.Rows[j][k];
                    if (obj == DBNull.Value)
                    {
                        // 如果是NULL则在HTML里面使用一个空格替换之
                        obj = "&nbsp;";
                    }
                    if (obj.ToString() == "")
                    {
                        obj = "&nbsp;";
                    }
                    string strCellContent = obj.ToString().Trim();
                    sb.Append("<span>" + strCellContent + "</span>");
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</TABLE></BODY></HTML>");
            return sb.ToString();
        }

        #endregion

        #region 自动生成项目编号
        /// <summary>
        /// 自动生成编号
        /// </summary>
        public static string AutoBuild()
        {
            //Random r = new Random(Guid.NewGuid().GetHashCode());
            return DateTime.Now.ToString("yyyyMMddHHmmssfff"); //+ r.Next(999).ToString("000");
        }
        #endregion

        #region 将DataTable的列名转换为中文，返回新的DataTable
        public static DataTable ConvertDataTable(DataTable dt)
        {
            DataTable dtNew = dt.Copy();
            for (int i = 0; i < dtNew.Columns.Count; i++)
            {
                if (dtNew.Columns[i].ColumnName == "Ss_ID")
                {
                    dtNew.Columns[i].ColumnName = "样本状态";
                }
                else if (dtNew.Columns[i].ColumnName == "SC_ID")
                {
                    dtNew.Columns[i].ColumnName = "晒号状态";
                }
                else if (dtNew.Columns[i].ColumnName == "scode")
                {
                    dtNew.Columns[i].ColumnName = "样本 编号";
                }
                else if (dtNew.Columns[i].ColumnName == "pId")
                {
                    dtNew.Columns[i].ColumnName = "省ID";
                }
                else if (dtNew.Columns[i].ColumnName == "cId")
                {
                    dtNew.Columns[i].ColumnName = "市ID";
                }
                else if (dtNew.Columns[i].ColumnName == "seId")
                {
                    dtNew.Columns[i].ColumnName = "县ID";
                }
                else if (dtNew.Columns[i].ColumnName == "Country")
                {
                    dtNew.Columns[i].ColumnName = "国家";
                }
                else if (dtNew.Columns[i].ColumnName == "U_name")
                {
                    dtNew.Columns[i].ColumnName = "姓 名";
                }
                else if (dtNew.Columns[i].ColumnName.ToLower() == "extphone")
                {
                    //dtNew.Columns[i].ColumnName = "分机号码";
                }
                string headerText = GetText(ChangeType.GetValue, 0, dtNew.Columns[i].ColumnName);
                if (!dtNew.Columns.Contains(headerText))
                {
                    dtNew.Columns[i].ColumnName = headerText;
                }
            }
            return dtNew;
        }
        #endregion

        #region 语言切换
        /// <summary>
        /// 语言切换
        /// </summary>
        public static void ChangeLanguage(Form f)
        {
            if (!StaticInfo.ChangeLanguage) return;

            //设置窗体标题
            f.Text = GetLanguage(f.Text);

            //遍历Form上的所有控件
            foreach (Control control in f.Controls)
            {
                if (control is Panel || control is GroupBox)
                {
                    foreach (Control con in control.Controls)
                    {
                        SetLanguage(con);
                    }
                }
                else
                {
                    SetLanguage(control);
                }
            }
        }

        private static void SetLanguage(Control con)
        {
            //设置按钮
            Button btn = con as Button;
            if (btn != null)
            {
                btn.Text = GetLanguage(btn.Text);
            }

            //设置文本标签
            Label lb = con as Label;
            if (lb != null)
            {
                lb.Text = GetLanguage(lb.Text);
            }

            //设置复选框
            CheckBox cb = con as CheckBox;
            if (cb != null)
            {
                cb.Text = GetLanguage(cb.Text);
            }

            //设置菜单栏
            MenuStrip ms = con as MenuStrip;
            if (ms != null)
            {
                foreach (ToolStripMenuItem item in ms.Items)
                {
                    if (StaticInfo.Language.ToUpper() == "CN")
                    {
                        item.Text = Strings.StrConv(item.Text, VbStrConv.TraditionalChinese, 1);
                        for (int i = 0; i < item.DropDownItems.Count; i++)
                        {
                            item.DropDownItems[i].Text = GetLanguage(item.DropDownItems[i].Text);
                        }
                    }
                    else
                    {
                        item.Text = Strings.StrConv(item.Text, VbStrConv.SimplifiedChinese, 1);
                        for (int i = 0; i < item.DropDownItems.Count; i++)
                        {
                            item.DropDownItems[i].Text = GetLanguage(item.DropDownItems[i].Text);
                        }
                    }
                }
            }

            //设置工具栏
            ToolStrip ts = con as ToolStrip;
            if (ts != null)
            {
                for (int i = 0; i < ts.Items.Count; i++)
                {
                    ts.Items[i].Text = GetLanguage(ts.Items[i].Text);
                }
            }

            //设置数据表格
            DataGridView dgv = con as DataGridView;
            if (dgv != null)
            {
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    dgv.Columns[i].HeaderText = GetLanguage(dgv.Columns[i].HeaderText);
                }
            }

            //设置选项卡
            TabControl tc = con as TabControl;
            if (tc != null)
            {
                tc.Text = GetLanguage(tc.Text);

                for (int i = 0; i < tc.TabPages.Count; i++)
                {
                    tc.TabPages[i].Text = GetLanguage(tc.TabPages[i].Text);

                    foreach (Control c in tc.TabPages[i].Controls)
                    {
                        SetLanguage(c);
                    }
                }
            }

            //设置ListView
            ListView lv = con as ListView;
            if (lv != null)
            {
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    lv.Columns[i].Text = GetLanguage(lv.Columns[i].Text);
                }
            }

            //设置分组框
            GroupBox gb = con as GroupBox;
            if (gb != null)
            {
                foreach (Control control in gb.Controls)
                {
                    SetLanguage(control);
                }
            }
        }

        //获取当前语言下的文本
        public static string GetLanguage(string text)
        {
            return Strings.StrConv(text,
                              StaticInfo.Language.ToUpper() == "CN"
                                  ? VbStrConv.SimplifiedChinese
                                  : VbStrConv.TraditionalChinese, 1);
        }

        #endregion

        #region 设置WebBrowser的IE版本
        /// <summary>
        /// 定义IE版本的枚举
        /// </summary>
        public enum IeVersion
        {
            强制ie11,//11001 (0x2EDF) Windows Internet Explorer 11. 强制IE11显示，忽略!DOCTYPE指令 
            强制ie10,//10001 (0x2711) Internet Explorer 10。网页以IE 10的标准模式展现，页面!DOCTYPE无效 
            标准ie10,//10000 (0x02710) Internet Explorer 10。在IE 10标准模式中按照网页上!DOCTYPE指令来显示网页。Internet Explorer 10 默认值。
            强制ie9,//9999 (0x270F) Windows Internet Explorer 9. 强制IE9显示，忽略!DOCTYPE指令 
            标准ie9,//9000 (0x2328) Internet Explorer 9. Internet Explorer 9默认值，在IE9标准模式中按照网页上!DOCTYPE指令来显示网页。
            强制ie8,//8888 (0x22B8) Internet Explorer 8，强制IE8标准模式显示，忽略!DOCTYPE指令 
            标准ie8,//8000 (0x1F40) Internet Explorer 8默认设置，在IE8标准模式中按照网页上!DOCTYPE指令展示网页
            标准ie7//7000 (0x1B58) 使用WebBrowser Control控件的应用程序所使用的默认值，在IE7标准模式中按照网页上!DOCTYPE指令来展示网页
        }

        /// <summary>
        /// 设置WebBrowser的默认版本
        /// </summary>
        /// <param name="ver">IE版本</param>
        public static void SetIeVersion(IeVersion ver)
        {
            string productName = AppDomain.CurrentDomain.SetupInformation.ApplicationName;//获取程序名称

            object version;
            switch (ver)
            {
                case IeVersion.标准ie7:
                    version = 0x1B58;
                    break;
                case IeVersion.标准ie8:
                    version = 0x1F40;
                    break;
                case IeVersion.强制ie8:
                    version = 0x22B8;
                    break;
                case IeVersion.标准ie9:
                    version = 0x2328;
                    break;
                case IeVersion.强制ie9:
                    version = 0x270F;
                    break;
                case IeVersion.标准ie10:
                    version = 0x02710;
                    break;
                case IeVersion.强制ie10:
                    version = 0x2711;
                    break;
                case IeVersion.强制ie11:
                    version = 0x2EDF;
                    break;
                default:
                    version = 0x1F40;
                    break;
            }

            RegistryKey key = Registry.CurrentUser;
            RegistryKey software =
                key.CreateSubKey(
                    @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION\" + productName);
            if (software != null)
            {
                software.Close();
                software.Dispose();
            }
            RegistryKey wwui =
                key.OpenSubKey(
                    @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true);
            //该项必须已存在
            if (wwui != null) wwui.SetValue(productName, version, RegistryValueKind.DWord);
        }
        #endregion

        #region 下载文件
        /// <summary>        
        /// 下载文件        
        /// </summary>        
        /// <param name="url">下载文件地址</param>
        /// <param name="filePath">下载后的存放地址</param>    
        public static bool DownloadFile(string url, string filePath)
        {
            try
            {
                HttpWebRequest myrq = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse myrp = (HttpWebResponse)myrq.GetResponse();
                Stream st = myrp.GetResponseStream();
                Stream so = new FileStream(filePath, FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                if (st != null)
                {
                    int osize = st.Read(@by, 0, @by.Length);
                    while (osize > 0)
                    {
                        totalDownloadedByte = osize + totalDownloadedByte;
                        Application.DoEvents();
                        so.Write(@by, 0, osize);
                        osize = st.Read(@by, 0, @by.Length);
                    }
                }
                so.Dispose();
                if (st != null)
                {
                    st.Dispose();
                    so.Close();
                    st.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }
        #endregion

        #region 根据路径创建目录
        /// <summary>
        /// 根据路径创建目录
        /// </summary>
        /// <param name="basePath">用户选择的目录</param>
        /// <param name="url">下载地址</param>
        /// <returns>创建后的路径</returns>
        public static string CreatePath(string basePath, string url)
        {
            try
            {
                string tempPath = url.Replace("http://", "").Replace("/", "\\");//截取路径
                int index = tempPath.IndexOf('\\');
                string path = basePath + tempPath.Substring(index, tempPath.Length - index);//拼接物理路径（含有文件名）
                string filePath = Path.GetDirectoryName(path);//获取文件目录
                string fileName = Path.GetFileName(path);//获取文件名
                if (filePath != null && Directory.Exists(filePath))
                {
                    return Path.Combine(filePath, fileName);
                }
                if (filePath != null)
                {
                    Directory.CreateDirectory(filePath);//创建目录
                    if (Directory.Exists(filePath))
                    {
                        return Path.Combine(filePath, fileName);//合并路径
                    }
                }
                return "error";
            }
            catch (Exception ex)
            {
                return "error\r\n" + ex;
            }
        }

        #endregion
    }
}
