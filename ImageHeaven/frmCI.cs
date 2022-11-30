using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NovaNet.wfe;
using NovaNet.Utils;
using System.Data.Odbc;
using System.Net;
using LItems;
using System.IO;
using System.Collections;
using nControls;
using DataLayerDefs;
using System.Text.RegularExpressions;

namespace ImageHeaven
{
    public partial class frmCI : Form
    {
        public string name = frmMain.name;
        //OdbcConnection sqlCon = null;
        NovaNet.Utils.GetProfile pData;
        NovaNet.Utils.ChangePassword pCPwd;
        NovaNet.Utils.Profile p;
        public static NovaNet.Utils.IntrRBAC rbc;
        //public Credentials crd;
        static wItem wi;
        public static string projKey;
        public static string bundleKey;
        public static string batchNumber;
        public static string batchCode;
        public static string creationDate;
        public static string department;
        public static string subCategory;

        public static string caseStatus = null;
        public static string caseNature = null;
        public static string caseType = null;
        public static string caseYear = null;
        public static string casefile = null;
        public static bool isWith = false;

        public static string filename = null;
        public static string old_filename = null;

        public Credentials crd = new Credentials();
        private OdbcConnection sqlCon;
        OdbcTransaction txn;

        public static string currStage = string.Empty;

        public static DataLayerDefs.Mode _mode = DataLayerDefs.Mode._Edit;

        public static eSTATES[] state;
        //public delegate void OnAccept(DeedDetails retDeed);
        //OnAccept m_OnAccept;
        ////The method to be invoked when the user aborts all operations
        //public delegate void OnAbort();
        OdbcDataAdapter sqlAdap;

        public static NovaNet.Utils.exLog.Logger exMailLog = new NovaNet.Utils.exLog.emailLogger("./errLog.log", NovaNet.Utils.exLog.LogLevel.Dev, Constants._MAIL_TO, Constants._MAIL_FROM, Constants._SMTP);
        public static NovaNet.Utils.exLog.Logger exTxtLog = new NovaNet.Utils.exLog.txtLogger("./errLog.log", NovaNet.Utils.exLog.LogLevel.Dev);

        string iniPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6) + "\\" + "IhConfiguration.ini";
        INIFile ini = new INIFile();

        public frmCI()
        {
            InitializeComponent();
        }
        
        public frmCI(string proj, string bundle, OdbcConnection pCon, Credentials pcrd, DataLayerDefs.Mode mode, eSTATES[] prmState)
        {
            InitializeComponent();

            projKey = proj;

            bundleKey = bundle;

            sqlCon = pCon;

            //txn = pTxn;

            crd = pcrd;

            _mode = mode;

            state = prmState;

            init();
        }

        public frmCI(string proj, string bundle, OdbcConnection pCon, Credentials pcrd, DataLayerDefs.Mode mode, string file, string stage)
        {
            InitializeComponent();

            projKey = proj;

            bundleKey = bundle;

            sqlCon = pCon;

            //txn = pTxn;

            crd = pcrd;

            _mode = mode;

            old_filename = file;

            currStage = stage;

            init();
        }

        public frmCI(string proj, string bundle, OdbcConnection pCon, Credentials pcrd, DataLayerDefs.Mode mode, string file)
        {
            InitializeComponent();

            projKey = proj;

            bundleKey = bundle;

            sqlCon = pCon;

            //txn = pTxn;

            crd = pcrd;

            _mode = mode;

            old_filename = file;

            init();
        }
        public void init()
        {
            deTextBox1.Text = _GetBundleDetails().Rows[0][0].ToString();
            deTextBox2.Text = _GetBundleDetails().Rows[0][1].ToString();
            //divname,divcode
            deTextBox3.Text = _GetBundleDetails().Rows[0][6].ToString();
            deTextBox6.Text = _GetBundleDetails().Rows[0][7].ToString();
            //psname,ps,code
            deTextBox4.Text = _GetBundleDetails().Rows[0][4].ToString();
            deTextBox7.Text = _GetBundleDetails().Rows[0][5].ToString();
            //category
            deTextBox5.Text = _GetBundleDetails().Rows[0][3].ToString();
            string month_year = _GetBundleDetails().Rows[0][8].ToString();
        }
        public DataTable _GetBundleDetails()
        {
            DataTable dt = new DataTable();
            string sql = "select distinct Bundle_name ,bundle_code,date_format(created_dttm,'%Y-%m-%d'),category,ps_name,ps_code,div_name,div_code,month_year from bundle_master where proj_code = '" + projKey + "' and bundle_key = '" + bundleKey + "'";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }
        public DataTable _GetFileCaseDetailsIndividual(string proj, string bundle, string fileName)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct proj_code, bundle_Key,item_no,filename,ps_name,ps_code,div_name,div_code," +
                         "date_format(CI_date, '%Y-%m-%d'),CI_case_no " +
                         "from metadata_entry where proj_code = '" + proj + "' and bundle_key = '" + bundle + "' and filename = '" + fileName + "' ";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon, txn);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }
        private void frmCI_Load(object sender, EventArgs e)
        {
            string month_year = _GetBundleDetails().Rows[0][8].ToString();
            if (_mode == Mode._Add)
            {
                deTextBox10.Text = string.Empty;
                deTextBox11.Text = string.Empty;
                deTextBox12.Text = month_year.Substring(3, 4).ToString();
                deTextBox12.Enabled = false;


                deTextBox9.Text = string.Empty;

                deTextBox9.Focus();
                deTextBox9.Select();
            }
            if (_mode == Mode._Edit)
            {
                string CIDate = _GetFileCaseDetailsIndividual(projKey, bundleKey, old_filename).Rows[0][8].ToString();
                string CIcaseno = _GetFileCaseDetailsIndividual(projKey, bundleKey, old_filename).Rows[0][9].ToString();
                deTextBox12.Enabled = false;

                if (CIDate != "")
                {
                    deTextBox10.Text = CIDate.Substring(8, 2);
                    deTextBox11.Text = CIDate.Substring(5, 2);
                    deTextBox12.Text = CIDate.Substring(0, 4);

                }

                deTextBox9.Text = CIcaseno;

                deTextBox9.Focus();
                deTextBox9.Select();
            }
        }

        private void deButton2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to Exit ? ", "B'Zer - Confirmation !", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
            {

                return;
            }
            else
            {
                this.Close();
            }
        }
        public bool validate()
        {
            bool retval = false;

            string currDate = DateTime.Now.ToString("yyyy-MM-dd");
            string curYear = DateTime.Now.ToString("yyyy");
            int curIntYear = Convert.ToInt32(curYear);


            if (deTextBox9.Text == "" || deTextBox9.Text == null || String.IsNullOrEmpty(deTextBox9.Text) || String.IsNullOrWhiteSpace(deTextBox9.Text))
            {
                retval = false;

                MessageBox.Show("You cannot leave CI Case Number field blank...", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                deTextBox9.Focus();
                return retval;
            }
            else
            {
                bool res = System.Text.RegularExpressions.Regex.IsMatch(deTextBox9.Text, "[^0-9]");
                if (res != true)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please input valid numeric CI Case Number...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    deTextBox9.Focus();
                    return retval;
                }
            }

            if (deTextBox10.Text != "" || deTextBox11.Text != "" || deTextBox12.Text != "")
            {
                if (deTextBox12.Text != "")
                {

                    bool res = System.Text.RegularExpressions.Regex.IsMatch(deTextBox12.Text, "[^0-9]");
                    if (res != true && Convert.ToInt32(deTextBox12.Text) <= curIntYear && deTextBox12.Text.Length == 4 && deTextBox12.Text.Substring(0, 1) != "0")
                    {
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                        MessageBox.Show(this, "Please input Valid Year...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        deTextBox12.Focus();
                        return retval;
                    }
                }

                if (deTextBox11.Text != "")
                {

                    bool res1 = System.Text.RegularExpressions.Regex.IsMatch(deTextBox11.Text, "[^0-9]");

                    if (res1 != true && deTextBox11.Text.Length == 2 && Convert.ToInt32(deTextBox11.Text) <= 12 && Convert.ToInt32(deTextBox11.Text) != 0 && deTextBox11.Text != "00")
                    {
                        retval = true;

                    }
                    else
                    {
                        retval = false;
                        MessageBox.Show(this, "Please input Valid Month...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        deTextBox11.Focus();
                        return retval;
                    }
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please input Valid Month...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    deTextBox11.Focus();
                    return retval;
                }
                if (deTextBox10.Text != "")
                {

                    bool res2 = System.Text.RegularExpressions.Regex.IsMatch(deTextBox10.Text, "[^0-9]");
                    if (res2 != true && deTextBox10.Text.Length == 2 && Convert.ToInt32(deTextBox10.Text) <= 31 && Convert.ToInt32(deTextBox10.Text) != 0 && deTextBox10.Text != "00")
                    {
                        retval = true;

                    }
                    else
                    {
                        retval = false;
                        MessageBox.Show(this, "Please input Valid Date...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        deTextBox10.Focus();
                        return retval;
                    }
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please input Valid Date...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    deTextBox10.Focus();
                    return retval;
                }

                DateTime temp;
                string isDate = deTextBox12.Text + "-" + deTextBox11.Text + "-" + deTextBox10.Text;
                if (DateTime.TryParse(isDate, out temp) && DateTime.Parse(isDate) < DateTime.Parse(currDate))
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please select a valid date, date should not be beyond current date", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    deTextBox10.Select();
                    return retval;

                }
            }
            else
            {
                retval = true;
            }


            DateTime temp1;
            string isSDate = deTextBox12.Text + deTextBox11.Text + deTextBox10.Text;

            if ((isSDate != ""))
            {
                isSDate = deTextBox12.Text + "-" + deTextBox11.Text + "-" + deTextBox10.Text;
                if (DateTime.TryParse(isSDate, out temp1) && DateTime.Parse(isSDate) < DateTime.Parse(currDate))
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please select a valid CI date", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    deTextBox10.Select();
                    return retval;

                }
            }
            else
            {
                retval = true;
            }


            return retval;
        }
        private bool checkFileNotExists(string proj, string bundle, string file)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            bool retval = false;

            string sql = "select filename from metadata_entry where filename = '" + file + "' and proj_code = '" + proj + "' and bundle_key = '" + bundle + "'  ";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {


                MessageBox.Show("This CI file number already exists for this batch", "B'Zer - Confirmation !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                retval = false;
                deTextBox10.Focus();
                return retval;
            }
            else
            {
                retval = true;
            }

            return retval;
        }
        private DataTable itemCount(string proj, string bundle)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();



            string sql = "select Count(*) from metadata_entry where  proj_code = '" + proj + "' and bundle_key = '" + bundle + "'  ";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);



            return dt;
        }
        private bool insertIntoMeta(int itemno, string psname, string pscode, string divname, string divcode, string filename, string CIcaseno, string CIdate, OdbcTransaction trans)
        {
            bool commitBol = true;

            string sqlStr = string.Empty;

            OdbcCommand sqlCmd = new OdbcCommand();



            if (frmCI.state[0] == eSTATES.METADATA_ENTRY)
            {
                sqlStr = @"insert into metadata_entry(proj_code,bundle_key,item_no,ps_name,ps_code,div_name,div_code,filename,CI_date,CI_case_no,created_by,created_dttm,status) values('" +
                        projKey + "','" + bundleKey + "','" + itemno +
                        "','" + psname + "','" + pscode + "','" + divname + "','" + divcode + "','" + filename + "','" + CIdate + "','" + CIcaseno + "','" + crd.created_by + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0)";
            }
            else
            {
                sqlStr = @"insert into metadata_entry(proj_code,bundle_key,item_no,ps_name,ps_code,div_name,div_code,filename,CI_date,CI_case_no,created_by,created_dttm,status) values('" +
                        projKey + "','" + bundleKey + "','" + itemno +
                        "','" + psname + "','" + pscode + "','" + divname + "','" + divcode + "','" + filename + "','" + CIdate + "','" + CIcaseno + "','" + crd.created_by + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',5)";
            }
            sqlCmd.Connection = sqlCon;
            sqlCmd.Transaction = trans;
            sqlCmd.CommandText = sqlStr;
            int i = sqlCmd.ExecuteNonQuery();
            if (i > 0)
            {
                commitBol = true;
            }
            else
            {
                commitBol = false;
            }

            return commitBol;
        }

        private string GetPolicyPath()
        {
            wfeBatch wBatch = new wfeBatch(sqlCon);
            string batchPath = GetPath(Convert.ToInt32(projKey), Convert.ToInt32(bundleKey));
            return batchPath;
        }
        public string GetPath(int prmProjKey, int prmBatchKey)
        {
            string sqlStr = null;
            DataSet projDs = new DataSet();
            string Path;

            try
            {
                sqlStr = @"select bundle_path from bundle_master where proj_code=" + prmProjKey + " and bundle_key=" + prmBatchKey;
                sqlAdap = new OdbcDataAdapter(sqlStr, sqlCon);
                sqlAdap.Fill(projDs);
            }
            catch (Exception ex)
            {
                sqlAdap.Dispose();

            }
            if (projDs.Tables[0].Rows.Count > 0)
            {
                Path = projDs.Tables[0].Rows[0]["bundle_path"].ToString();
            }
            else
                Path = string.Empty;

            return Path;
        }

        public bool updateMetaEdit(string divN, string divC, string psN, string psC, string CIdate, string CIcaseno)
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateMetaEdit(projKey, bundleKey, old_filename, filename, divN, divC, psN, psC, CIdate, CIcaseno);

                ret = true;
            }
            return ret;
        }
        public bool updateImageEdit()
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateImageEdit(projKey, bundleKey, old_filename, filename);

                ret = true;
            }
            return ret;
        }
        public bool updateTransLogEdit()
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateTransLogEdit(projKey, bundleKey, old_filename, filename);

                ret = true;
            }
            return ret;
        }
        public bool updateQaEdit()
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateQaEdit(projKey, bundleKey, old_filename, filename);

                ret = true;
            }
            return ret;
        }
        public bool updateCustExcEdit()
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateCustExcEdit(projKey, bundleKey, old_filename, filename);

                ret = true;
            }
            return ret;
        }

        public bool _UpdateTransLogEdit(string projKey, string bundleKey, string oldFileName, string fileName)
        {
            string sqlStr = null;

            OdbcCommand sqlCmd = new OdbcCommand();

            bool retVal = false;
            string sql = string.Empty;
            string remarks = string.Empty;


            sqlStr = "UPDATE transaction_log SET policy_number= '" + fileName + "' WHERE proj_key = '" + projKey + "' AND batch_key = '" + bundleKey + "' and policy_number = '" + oldFileName + "' ";

            System.Diagnostics.Debug.Print(sqlStr);
            OdbcCommand cmd = new OdbcCommand(sqlStr, sqlCon);
            //cmd.Connection = sqlCon;
            //cmd.CommandText = sqlStr;
            if (cmd.ExecuteNonQuery() >= 0)
            {
                retVal = true;
                //txn.Commit();
            }

            return retVal;
        }
        public bool _UpdateCustExcEdit(string projKey, string bundleKey, string oldFileName, string fileName)
        {
            string sqlStr = null;

            OdbcCommand sqlCmd = new OdbcCommand();

            bool retVal = false;
            string sql = string.Empty;
            string remarks = string.Empty;


            sqlStr = "UPDATE custom_exception SET policy_number= '" + fileName + "'," +
                "image_name = REPLACE(image_name,'" + oldFileName + "_" + "','" + fileName + "_" + "')," +
                "modified_by ='" + crd.created_by + "',modified_dttm = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE proj_key = '" + projKey + "' AND batch_key = '" + bundleKey + "' and policy_number = '" + oldFileName + "' ";

            System.Diagnostics.Debug.Print(sqlStr);
            OdbcCommand cmd = new OdbcCommand(sqlStr, sqlCon);
            //cmd.Connection = sqlCon;
            //cmd.CommandText = sqlStr;
            if (cmd.ExecuteNonQuery() >= 0)
            {
                retVal = true;
                //txn.Commit();
            }

            return retVal;
        }
        public bool _UpdateQaEdit(string projKey, string bundleKey, string oldFileName, string fileName)
        {
            string sqlStr = null;

            OdbcCommand sqlCmd = new OdbcCommand();

            bool retVal = false;
            string sql = string.Empty;
            string remarks = string.Empty;


            sqlStr = "UPDATE lic_qa_log SET policy_number= '" + fileName + "' WHERE proj_key = '" + projKey + "' AND batch_key = '" + bundleKey + "' and policy_number = '" + oldFileName + "' ";

            System.Diagnostics.Debug.Print(sqlStr);
            OdbcCommand cmd = new OdbcCommand(sqlStr, sqlCon);
            //cmd.Connection = sqlCon;
            //cmd.CommandText = sqlStr;
            if (cmd.ExecuteNonQuery() >= 0)
            {
                retVal = true;
                //txn.Commit();
            }

            return retVal;
        }
        public bool _UpdateMetaEdit(string projKey, string bundleKey, string oldFileName, string fileName,
            string divN, string divC, string psN, string psC, string ciDate, string cicaseno)
        {
            string sqlStr = null;

            OdbcCommand sqlCmd = new OdbcCommand();

            bool retVal = false;
            string sql = string.Empty;
            string remarks = string.Empty;


            sqlStr = "UPDATE metadata_entry SET filename = '" + fileName + "',ps_name  = '" + psN + "',ps_code = '" + psC + "'," +
                "div_name = '" + divN + "',div_code = '" + divC + "',CI_date= '" + ciDate + "',CI_case_no='" + cicaseno + "'," +
                "modified_by ='" + crd.created_by + "',modified_dttm = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'  WHERE proj_code = '" + projKey + "' AND bundle_key = '" + bundleKey + "' and filename = '" + oldFileName + "' ";

            System.Diagnostics.Debug.Print(sqlStr);
            OdbcCommand cmd = new OdbcCommand(sqlStr, sqlCon);
            //cmd.Connection = sqlCon;
            //cmd.CommandText = sqlStr;
            if (cmd.ExecuteNonQuery() >= 0)
            {
                retVal = true;
                //txn.Commit();
            }

            return retVal;
        }
        public bool _UpdateImageEdit(string projKey, string bundleKey, string oldFileName, string fileName)
        {
            string sqlStr = null;

            OdbcCommand sqlCmd = new OdbcCommand();

            bool retVal = false;
            string sql = string.Empty;
            string remarks = string.Empty;


            sqlStr = "UPDATE image_master SET policy_number= '" + fileName + "',page_index_name = REPLACE(page_index_name,'" + oldFileName + "_" + "','" + fileName + "_" + "'),page_name = REPLACE(page_name,'" + oldFileName + "_" + "','" + fileName + "_" + "'),modified_by ='" + crd.created_by + "',modified_dttm = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'  WHERE proj_key = '" + projKey + "' AND batch_key = '" + bundleKey + "' and policy_number = '" + oldFileName + "' ";

            System.Diagnostics.Debug.Print(sqlStr);
            OdbcCommand cmd = new OdbcCommand(sqlStr, sqlCon);
            //cmd.Connection = sqlCon;
            //cmd.CommandText = sqlStr;
            if (cmd.ExecuteNonQuery() >= 0)
            {
                retVal = true;
                //txn.Commit();
            }

            return retVal;
        }
        private bool checkFileNotExistsEdit(string file, string projK, string bundleK)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            bool retval = false;

            string sql = "select filename,proj_code,bundle_key from metadata_entry where filename = '" + file + "' and proj_code = '" + projK + "' and bundle_key = '" + bundleK + "' ";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                DataTable dt1 = new DataTable();

                string sql1 = "select bundle_code from bundle_master where proj_code = '" + dt.Rows[0][1].ToString() + "' and bundle_key = '" + dt.Rows[0][2].ToString() + "'  ";

                OdbcDataAdapter odap1 = new OdbcDataAdapter(sql1, sqlCon);
                odap1.Fill(dt1);

                MessageBox.Show("This file number already exists for batch - " + dt1.Rows[0][0].ToString(), "B'Zer - Confirmation !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                retval = false;
                deTextBox10.Focus();
                return retval;
            }
            else
            {
                retval = true;
            }

            return retval;
        }

        private void deButtonSave_Click(object sender, EventArgs e)
        {
            OdbcTransaction sqlTrans = null;
            if (sqlCon.State == ConnectionState.Closed || sqlCon.State == ConnectionState.Broken)
            {
                sqlCon.Open();
            }
            if (_mode == Mode._Add)
            {
                DialogResult result = MessageBox.Show("Do you want to save changes ? ", "B'Zer - Confirmation !", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    if (validate() == true)
                    {
                        string CIDate = string.Empty;

                        string divname = deTextBox3.Text.Trim();
                        string psname = deTextBox4.Text.Trim();

                        string divcode = deTextBox6.Text.Trim();
                        string pscode = deTextBox7.Text.Trim();

                        string category = deTextBox5.Text.Trim();

                        string CIcaseNumber = deTextBox9.Text.Trim();

                        if (deTextBox12.Text != "" && deTextBox11.Text != "" && deTextBox10.Text != "")
                        {
                            CIDate = deTextBox12.Text + "-" + deTextBox11.Text + "-" + deTextBox10.Text;
                        }
                        else
                        {
                            CIDate = "";
                        }

                        string filenumber = "CI" + CIDate + deTextBox12.Text;
                        filename = filenumber;

                        if (checkFileNotExists(projKey, bundleKey, filename) == true)
                        {

                            int item = Convert.ToInt32(itemCount(projKey, bundleKey).Rows[0][0].ToString()) + 1;

                            bool insertCase = insertIntoMeta(item, psname, pscode, divname, divcode, filename, CIcaseNumber, CIDate, sqlTrans);
                            if (insertCase == true)
                            {
                                if (sqlTrans == null)
                                {
                                    sqlTrans = sqlCon.BeginTransaction();
                                }
                                sqlTrans.Commit();
                                sqlTrans = null;
                                MessageBox.Show(this, "Record Saved Successfully...", "B'Zer ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                if (frmCI.state[0] == eSTATES.METADATA_ENTRY)
                                {
                                    frmCI_Load(sender, e);
                                }
                                else
                                { this.Close(); }
                            }
                            else
                            {

                                MessageBox.Show(this, "Ooops!!! There is an Error - Record not Saved...", "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //this.Hide();

                                return;

                            }
                        }
                        else
                        {

                            return;
                        }
                    }
                }
                else
                {

                    return;
                }

            }
            if (_mode == Mode._Edit)
            {
                if (currStage == "Entry")
                {
                    if (validate() == true)
                    {
                        string CIDate = string.Empty;

                        string divname = deTextBox3.Text.Trim();
                        string psname = deTextBox4.Text.Trim();

                        string divcode = deTextBox6.Text.Trim();
                        string pscode = deTextBox7.Text.Trim();

                        string category = deTextBox5.Text.Trim();

                        if (deTextBox12.Text != "" && deTextBox11.Text != "" && deTextBox10.Text != "")
                        {
                            CIDate = deTextBox12.Text + "-" + deTextBox11.Text + "-" + deTextBox10.Text;
                        }
                        else
                        {
                            CIDate = "";
                        }

                        string CIcaseno = deTextBox9.Text.Trim();

                        string filenumber = "CI" + CIcaseno.Trim() + deTextBox12.Text;
                        filename = filenumber;

                        if (filenumber.Trim() != Files.filename)
                        {
                            if (checkFileNotExistsEdit(filenumber.Trim(), projKey, bundleKey) == true)
                            {
                                casefile = filenumber.Trim();

                                filename = casefile;


                                bool updateMeta = updateMetaEdit(divname, divcode, psname, pscode, CIDate, CIcaseno);
                                bool updateimageMaster = updateImageEdit();
                                bool updatetransLog = updateTransLogEdit();
                                bool updatecusExc = updateCustExcEdit();
                                bool updateQa = updateQaEdit();


                                if (updateMeta == true && updateimageMaster == true && updatetransLog == true && updatecusExc == true && updateQa == true)
                                {
                                    //if (txn == null || txn.Connection == null)
                                    //{
                                    //    txn = sqlCon.BeginTransaction();
                                    //}
                                    //txn.Commit();
                                    //txn = null;

                                    string pathTemp = GetPolicyPath();

                                    string pathFinal = pathTemp + "\\" + old_filename;
                                    string pathDest = pathTemp + "\\" + filename;
                                    //Directory Rename
                                    if (old_filename != filename)
                                    {
                                        if (Directory.Exists(pathFinal))
                                        {

                                            Directory.Move(pathFinal, pathDest);

                                        }
                                    }

                                    //Scan folder check 
                                    string pathScan = pathTemp + "\\" + filename + "\\Scan";
                                    //Qc folder check 
                                    string pathQc = pathTemp + "\\" + filename + "\\QC";
                                    //Deleted
                                    string pathDeleted = pathScan + "\\" + ihConstants._DELETE_FOLDER;

                                    // Files Rename scan
                                    if (Directory.Exists(pathScan))
                                    {
                                        DirectoryInfo DirInfo = new DirectoryInfo(pathScan);
                                        FileInfo[] names = DirInfo.GetFiles();
                                        foreach (FileInfo f in names)
                                        {
                                            if (f.Name.Contains(old_filename + "_"))
                                            {
                                                string str1 = f.Name;

                                                string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                                                File.Move(pathScan + "\\" + str1, pathScan + "\\" + str2);
                                            }

                                        }
                                    }

                                    //// Files Rename Qc
                                    if (Directory.Exists(pathQc))
                                    {
                                        DirectoryInfo DirInfo = new DirectoryInfo(pathQc);
                                        FileInfo[] names = DirInfo.GetFiles();
                                        foreach (FileInfo f in names)
                                        {
                                            if (f.Name.Contains(old_filename + "_"))
                                            {
                                                string str1 = f.Name;

                                                string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                                                File.Move(pathQc + "\\" + str1, pathQc + "\\" + str2);
                                            }

                                        }
                                    }

                                    //// Files Rename deleted
                                    if (Directory.Exists(pathDeleted))
                                    {
                                        DirectoryInfo DirInfo = new DirectoryInfo(pathDeleted);
                                        FileInfo[] names = DirInfo.GetFiles();
                                        foreach (FileInfo f in names)
                                        {
                                            if (f.Name.Contains(old_filename + "_"))
                                            {
                                                string str1 = f.Name;

                                                string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                                                File.Move(pathDeleted + "\\" + str1, pathDeleted + "\\" + str2);
                                            }

                                        }
                                    }

                                    MessageBox.Show(this, "Record Saved Successfully...", "B'Zer", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    this.Close();

                                }
                                else
                                {

                                    MessageBox.Show(this, "Ooops!!! There is an Error - Record not Saved...", "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    return;

                                }
                            }
                        }
                        else
                        {

                            casefile = filenumber.Trim();

                            filename = casefile;

                            bool updateMeta = updateMetaEdit(divname, divcode, psname, pscode, CIDate, CIcaseno);
                            bool updateimageMaster = updateImageEdit();
                            bool updatetransLog = updateTransLogEdit();
                            bool updatecusExc = updateCustExcEdit();
                            bool updateQa = updateQaEdit();


                            if (updateMeta == true && updateimageMaster == true && updatetransLog == true && updatecusExc == true && updateQa == true)
                            {
                                //if (txn == null || txn.Connection == null)
                                //{
                                //    txn = sqlCon.BeginTransaction();
                                //}
                                //txn.Commit();
                                //txn = null;

                                string pathTemp = GetPolicyPath();

                                string pathFinal = pathTemp + "\\" + old_filename;
                                string pathDest = pathTemp + "\\" + filename;

                                //Directory Rename
                                if (old_filename != filename)
                                {
                                    if (Directory.Exists(pathFinal))
                                    {

                                        Directory.Move(pathFinal, pathDest);

                                    }
                                }


                                //Scan folder check 
                                string pathScan = pathTemp + "\\" + filename + "\\Scan";
                                //Qc folder check 
                                string pathQc = pathTemp + "\\" + filename + "\\QC";
                                //Deleted
                                string pathDeleted = pathScan + "\\" + ihConstants._DELETE_FOLDER;

                                // Files Rename scan
                                if (Directory.Exists(pathScan))
                                {
                                    DirectoryInfo DirInfo = new DirectoryInfo(pathScan);
                                    FileInfo[] names = DirInfo.GetFiles();
                                    foreach (FileInfo f in names)
                                    {
                                        if (f.Name.Contains(old_filename + "_"))
                                        {
                                            string str1 = f.Name;

                                            string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                                            File.Move(pathScan + "\\" + str1, pathScan + "\\" + str2);
                                        }

                                    }
                                }

                                //// Files Rename Qc
                                if (Directory.Exists(pathQc))
                                {
                                    DirectoryInfo DirInfo = new DirectoryInfo(pathQc);
                                    FileInfo[] names = DirInfo.GetFiles();
                                    foreach (FileInfo f in names)
                                    {
                                        if (f.Name.Contains(old_filename + "_"))
                                        {
                                            string str1 = f.Name;

                                            string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                                            File.Move(pathQc + "\\" + str1, pathQc + "\\" + str2);
                                        }

                                    }
                                }

                                //// Files Rename deleted
                                if (Directory.Exists(pathDeleted))
                                {
                                    DirectoryInfo DirInfo = new DirectoryInfo(pathDeleted);
                                    FileInfo[] names = DirInfo.GetFiles();
                                    foreach (FileInfo f in names)
                                    {
                                        if (f.Name.Contains(old_filename + "_"))
                                        {
                                            string str1 = f.Name;

                                            string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                                            File.Move(pathDeleted + "\\" + str1, pathDeleted + "\\" + str2);
                                        }

                                    }
                                }

                                MessageBox.Show(this, "Record Saved Successfully...", "B'Zer", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                this.Close();

                            }
                            else
                            {

                                MessageBox.Show(this, "Ooops!!! There is an Error - Record not Saved...", "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;

                            }
                        }
                    }
                }
                //if (currStage == "FQC")
                //{
                //    if (validate() == true)
                //    {
                //        string CIDate = string.Empty;

                //        string divname = deTextBox3.Text.Trim();
                //        string psname = deTextBox4.Text.Trim();

                //        string divcode = deTextBox6.Text.Trim();
                //        string pscode = deTextBox7.Text.Trim();

                //        string category = deTextBox5.Text.Trim();

                //        if (deTextBox12.Text != "" && deTextBox11.Text != "" && deTextBox10.Text != "")
                //        {
                //            CIDate = deTextBox12.Text + "-" + deTextBox11.Text + "-" + deTextBox10.Text;
                //        }
                //        else
                //        {
                //            CIDate = "";
                //        }

                //        string CIcaseno = deTextBox9.Text.Trim();

                //        string filenumber = "CI" + CIcaseno.Trim() + deTextBox12.Text;
                //        filename = filenumber;

                //        if (filenumber.Trim() != aeFQC.filename)
                //        {
                //            if (checkFileNotExistsEdit(filenumber.Trim(), projKey, bundleKey) == true)
                //            {
                //                casefile = filenumber.Trim();

                //                filename = casefile;


                //                bool updateMeta = updateMetaEdit(divname, divcode, psname, pscode, CIDate, CIcaseno);
                //                bool updateimageMaster = updateImageEdit();
                //                bool updatetransLog = updateTransLogEdit();
                //                bool updatecusExc = updateCustExcEdit();
                //                bool updateQa = updateQaEdit();


                //                if (updateMeta == true && updateimageMaster == true && updatetransLog == true && updatecusExc == true && updateQa == true)
                //                {
                //                    //if (txn == null || txn.Connection == null)
                //                    //{
                //                    //    txn = sqlCon.BeginTransaction();
                //                    //}
                //                    //txn.Commit();
                //                    //txn = null;

                //                    string pathTemp = GetPolicyPath();

                //                    string pathFinal = pathTemp + "\\" + old_filename;
                //                    string pathDest = pathTemp + "\\" + filename;
                //                    //Directory Rename
                //                    if (old_filename != filename)
                //                    {
                //                        if (Directory.Exists(pathFinal))
                //                        {

                //                            Directory.Move(pathFinal, pathDest);

                //                        }
                //                    }

                //                    //Scan folder check 
                //                    string pathScan = pathTemp + "\\" + filename + "\\Scan";
                //                    //Qc folder check 
                //                    string pathQc = pathTemp + "\\" + filename + "\\QC";
                //                    //Deleted
                //                    string pathDeleted = pathScan + "\\" + ihConstants._DELETE_FOLDER;

                //                    // Files Rename scan
                //                    if (Directory.Exists(pathScan))
                //                    {
                //                        DirectoryInfo DirInfo = new DirectoryInfo(pathScan);
                //                        FileInfo[] names = DirInfo.GetFiles();
                //                        foreach (FileInfo f in names)
                //                        {
                //                            if (f.Name.Contains(old_filename + "_"))
                //                            {
                //                                string str1 = f.Name;

                //                                string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                //                                File.Move(pathScan + "\\" + str1, pathScan + "\\" + str2);
                //                            }

                //                        }
                //                    }

                //                    //// Files Rename Qc
                //                    if (Directory.Exists(pathQc))
                //                    {
                //                        DirectoryInfo DirInfo = new DirectoryInfo(pathQc);
                //                        FileInfo[] names = DirInfo.GetFiles();
                //                        foreach (FileInfo f in names)
                //                        {
                //                            if (f.Name.Contains(old_filename + "_"))
                //                            {
                //                                string str1 = f.Name;

                //                                string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                //                                File.Move(pathQc + "\\" + str1, pathQc + "\\" + str2);
                //                            }

                //                        }
                //                    }

                //                    //// Files Rename deleted
                //                    if (Directory.Exists(pathDeleted))
                //                    {
                //                        DirectoryInfo DirInfo = new DirectoryInfo(pathDeleted);
                //                        FileInfo[] names = DirInfo.GetFiles();
                //                        foreach (FileInfo f in names)
                //                        {
                //                            if (f.Name.Contains(old_filename + "_"))
                //                            {
                //                                string str1 = f.Name;

                //                                string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                //                                File.Move(pathDeleted + "\\" + str1, pathDeleted + "\\" + str2);
                //                            }

                //                        }
                //                    }

                //                    MessageBox.Show(this, "Record Saved Successfully...", "B'Zer", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //                    this.Close();

                //                }
                //                else
                //                {

                //                    MessageBox.Show(this, "Ooops!!! There is an Error - Record not Saved...", "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //                    return;

                //                }
                //            }
                //        }
                //        else
                //        {
                //            casefile = filenumber.Trim();

                //            filename = casefile;

                //            bool updateMeta = updateMetaEdit(divname, divcode, psname, pscode, CIDate, CIcaseno);
                //            bool updateimageMaster = updateImageEdit();
                //            bool updatetransLog = updateTransLogEdit();
                //            bool updatecusExc = updateCustExcEdit();
                //            bool updateQa = updateQaEdit();


                //            if (updateMeta == true && updateimageMaster == true && updatetransLog == true && updatecusExc == true && updateQa == true)
                //            {
                //                //if (txn == null || txn.Connection == null)
                //                //{
                //                //    txn = sqlCon.BeginTransaction();
                //                //}
                //                //txn.Commit();
                //                //txn = null;

                //                string pathTemp = GetPolicyPath();

                //                string pathFinal = pathTemp + "\\" + old_filename;
                //                string pathDest = pathTemp + "\\" + filename;

                //                //Directory Rename
                //                if (old_filename != filename)
                //                {
                //                    if (Directory.Exists(pathFinal))
                //                    {

                //                        Directory.Move(pathFinal, pathDest);

                //                    }
                //                }


                //                //Scan folder check 
                //                string pathScan = pathTemp + "\\" + filename + "\\Scan";
                //                //Qc folder check 
                //                string pathQc = pathTemp + "\\" + filename + "\\QC";
                //                //Deleted
                //                string pathDeleted = pathScan + "\\" + ihConstants._DELETE_FOLDER;

                //                // Files Rename scan
                //                if (Directory.Exists(pathScan))
                //                {
                //                    DirectoryInfo DirInfo = new DirectoryInfo(pathScan);
                //                    FileInfo[] names = DirInfo.GetFiles();
                //                    foreach (FileInfo f in names)
                //                    {
                //                        if (f.Name.Contains(old_filename + "_"))
                //                        {
                //                            string str1 = f.Name;

                //                            string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                //                            File.Move(pathScan + "\\" + str1, pathScan + "\\" + str2);
                //                        }

                //                    }
                //                }

                //                //// Files Rename Qc
                //                if (Directory.Exists(pathQc))
                //                {
                //                    DirectoryInfo DirInfo = new DirectoryInfo(pathQc);
                //                    FileInfo[] names = DirInfo.GetFiles();
                //                    foreach (FileInfo f in names)
                //                    {
                //                        if (f.Name.Contains(old_filename + "_"))
                //                        {
                //                            string str1 = f.Name;

                //                            string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                //                            File.Move(pathQc + "\\" + str1, pathQc + "\\" + str2);
                //                        }

                //                    }
                //                }

                //                //// Files Rename deleted
                //                if (Directory.Exists(pathDeleted))
                //                {
                //                    DirectoryInfo DirInfo = new DirectoryInfo(pathDeleted);
                //                    FileInfo[] names = DirInfo.GetFiles();
                //                    foreach (FileInfo f in names)
                //                    {
                //                        if (f.Name.Contains(old_filename + "_"))
                //                        {
                //                            string str1 = f.Name;

                //                            string str2 = f.Name.Replace(old_filename + "_", filename + "_");

                //                            File.Move(pathDeleted + "\\" + str1, pathDeleted + "\\" + str2);
                //                        }

                //                    }
                //                }

                //                MessageBox.Show(this, "Record Saved Successfully...", "Record Management", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //                this.Close();

                //            }
                //            else
                //            {

                //                MessageBox.Show(this, "Ooops!!! There is an Error - Record not Saved...", "B'Zer", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //                return;

                //            }
                //        }
                //    }
                //}
            }
        }

        private void deTextBox10_Leave(object sender, EventArgs e)
        {
            if (deTextBox10.Text == "" || deTextBox10.Text == null || String.IsNullOrEmpty(deTextBox10.Text) || String.IsNullOrWhiteSpace(deTextBox10.Text))
            { return; }
            else
            {
                if (deTextBox10.Text.Length <= 2)
                {
                    deTextBox10.Text = deTextBox10.Text.PadLeft(2, '0');
                }
            }
        }

        private void deTextBox9_Leave(object sender, EventArgs e)
        {
            if (deTextBox9.Text == "" || deTextBox9.Text == null || String.IsNullOrEmpty(deTextBox9.Text) || String.IsNullOrWhiteSpace(deTextBox9.Text))
            { return; }
            else
            {
                if (_mode == Mode._Add)
                {
                    string filenumber = "CI" + deTextBox9.Text.Trim() + deTextBox12.Text;
                    checkFileNotExists(projKey, bundleKey, filenumber.Trim());
                }
            }
        }

        private void frmCI_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult result = MessageBox.Show("Do you want to Exit ? ", "B'Zer - Confirmation !", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    this.Close();

                }
                else
                {
                    return;
                }
            }
        }

        private void deTextBox10_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Regex.IsMatch(e.KeyChar.ToString(), @"^[0-9\s\b]*$")))
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void deTextBox9_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Regex.IsMatch(e.KeyChar.ToString(), @"^[0-9\s\b]*$")))
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void deTextBox11_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Regex.IsMatch(e.KeyChar.ToString(), @"^[0-9\s\b]*$")))
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void deTextBox11_Leave(object sender, EventArgs e)
        {
            if (deTextBox11.Text == "" || deTextBox11.Text == null || String.IsNullOrEmpty(deTextBox11.Text) || String.IsNullOrWhiteSpace(deTextBox11.Text))
            { return; }
            else
            {
                if (deTextBox11.Text.Length <= 2)
                {
                    deTextBox11.Text = deTextBox11.Text.PadLeft(2, '0');
                }
            }
            if (_mode == Mode._Add)
            {
                string filenumber = "CI" + deTextBox9.Text.Trim() + deTextBox12.Text;
                checkFileNotExists(projKey, bundleKey, filenumber.Trim());
            }
        }
    }
}
