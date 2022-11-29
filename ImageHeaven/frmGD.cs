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
    public partial class frmGD : Form
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

        public frmGD()
        {
            InitializeComponent();
        }

        public frmGD(string proj, string bundle, OdbcConnection pCon, Credentials pcrd, DataLayerDefs.Mode mode, eSTATES[] prmState)
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

        public frmGD(string proj, string bundle, OdbcConnection pCon, Credentials pcrd, DataLayerDefs.Mode mode, string file, string stage)
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

        public frmGD(string proj, string bundle, OdbcConnection pCon, Credentials pcrd, DataLayerDefs.Mode mode, string file)
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

        private void frmGD_Load(object sender, EventArgs e)
        {
            string month_year = _GetBundleDetails().Rows[0][8].ToString();

            if (_mode == Mode._Add)
            {
                deTextBox10.Text = string.Empty;

                deTextBox11.Text = month_year.Substring(0, 2).ToString();
                deTextBox12.Text = month_year.Substring(3, 4).ToString();
                deTextBox11.Enabled = false;
                deTextBox12.Enabled = false;

                deTextBox14.Text = string.Empty;
                deTextBox14.Enabled = false;
                deTextBox15.Text = month_year.Substring(0, 2).ToString();
                deTextBox16.Text = month_year.Substring(3, 4).ToString();
                deTextBox15.Enabled = false;
                deTextBox16.Enabled = false;

                deTextBox9.Text = string.Empty;
                deTextBox13.Text = string.Empty;

                deTextBox10.Focus();
                deTextBox10.Select();
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

                MessageBox.Show("You cannot leave GD Serial number start field blank...", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show(this, "Please input valid numeric GD Serial number start...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    deTextBox9.Focus();
                    return retval;
                }
            }
            if (deTextBox13.Text == "" || deTextBox13.Text == null || String.IsNullOrEmpty(deTextBox13.Text) || String.IsNullOrWhiteSpace(deTextBox13.Text))
            {
                retval = false;

                MessageBox.Show("You cannot leave GD Serial number end field blank...", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                deTextBox13.Focus();
                return retval;
            }
            else
            {
                bool res = System.Text.RegularExpressions.Regex.IsMatch(deTextBox13.Text, "[^0-9]");
                if (res != true)
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please input valid numeric GD Serial number start...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    deTextBox13.Focus();
                    return retval;
                }
            }

            if (Convert.ToInt32(deTextBox9.Text) < Convert.ToInt32(deTextBox13.Text))
            {
                retval = true;
            }
            else
            {
                retval = false;

                MessageBox.Show("GD Serial number start cannot be less than GD Serial number end...", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                deTextBox13.Focus();
                return retval;
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
                    MessageBox.Show(this, "Please select a valid date", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    deTextBox10.Select();
                    return retval;

                }
            }
            else
            {
                retval = true;
            }

            if (deTextBox14.Text != "" || deTextBox15.Text != "" || deTextBox16.Text != "")
            {
                if (deTextBox16.Text != "")
                {

                    bool res = System.Text.RegularExpressions.Regex.IsMatch(deTextBox16.Text, "[^0-9]");
                    if (res != true && Convert.ToInt32(deTextBox16.Text) <= curIntYear && deTextBox16.Text.Length == 4 && deTextBox16.Text.Substring(0, 1) != "0")
                    {
                        //retval = true;
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                        MessageBox.Show(this, "Please input Valid Year...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        deTextBox16.Focus();
                        return retval;
                    }
                }

                if (deTextBox15.Text != "")
                {

                    bool res1 = System.Text.RegularExpressions.Regex.IsMatch(deTextBox15.Text, "[^0-9]");

                    if (res1 != true && deTextBox15.Text.Length == 2 && Convert.ToInt32(deTextBox15.Text) <= 12 && Convert.ToInt32(deTextBox15.Text) != 0 && deTextBox15.Text != "00")
                    {
                        //retval = true;
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                        MessageBox.Show(this, "Please input Valid Month...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        deTextBox15.Focus();
                        return retval;
                    }
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please input Valid Month...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    deTextBox15.Focus();
                    return retval;
                }

                if (deTextBox14.Text != "")
                {

                    bool res2 = System.Text.RegularExpressions.Regex.IsMatch(deTextBox14.Text, "[^0-9]");
                    if (res2 != true && deTextBox14.Text.Length == 2 && Convert.ToInt32(deTextBox14.Text) <= 31 && Convert.ToInt32(deTextBox14.Text) != 0 && deTextBox14.Text != "00")
                    {
                        //retval = true;
                        retval = true;
                    }
                    else
                    {
                        retval = false;
                        MessageBox.Show(this, "Please input Valid Date...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        deTextBox14.Focus();
                        return retval;
                    }
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please input Valid Date...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    deTextBox14.Focus();
                    return retval;
                }

                DateTime temp;
                string isDate = deTextBox16.Text + "-" + deTextBox15.Text + "-" + deTextBox14.Text;
                if (DateTime.TryParse(isDate, out temp) && DateTime.Parse(isDate) <= DateTime.Parse(currDate))
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please select a valid date", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    deTextBox14.Select();
                    return retval;

                }
            }
            else
            {
                retval = true;
            }



            DateTime temp1;
            string isSDate = deTextBox12.Text + deTextBox11.Text + deTextBox10.Text;
            string isEDate = deTextBox16.Text + deTextBox15.Text + deTextBox14.Text;

            if ((isSDate != "") && (isEDate != ""))
            {
                isSDate = deTextBox12.Text + "-" + deTextBox11.Text + "-" + deTextBox10.Text;
                isEDate = deTextBox16.Text + "-" + deTextBox15.Text + "-" + deTextBox14.Text;
                if (DateTime.TryParse(isSDate, out temp1) && DateTime.TryParse(isEDate, out temp1) && DateTime.Parse(isSDate) < DateTime.Parse(isEDate))
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                    MessageBox.Show(this, "Please select a valid start date or end date", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

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


                MessageBox.Show("This GD file number already exists for this batch", "B'Zer - Confirmation !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private bool insertIntoMeta(int itemno, string psname, string pscode, string divname, string divcode, string filename, string startD, string endD, string startSerial, string endSerial, OdbcTransaction trans)
        {
            bool commitBol = true;

            string sqlStr = string.Empty;

            OdbcCommand sqlCmd = new OdbcCommand();



            if (frmGD.state[0] == eSTATES.METADATA_ENTRY)
            {
                sqlStr = @"insert into metadata_entry(proj_code,bundle_key,item_no,ps_name,ps_code,div_name,div_code,filename,gd_startdate,gd_enddate,gd_start_serial,gd_end_serial,created_by,created_dttm,status) values('" +
                        projKey + "','" + bundleKey + "','" + itemno +
                        "','" + psname + "','" + pscode + "','" + divname + "','" + divcode + "','" + filename + "','" + startD + "','" + endD + "','" + startSerial + "','" + endSerial + "','" + crd.created_by + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0)";
                //sqlCmd.Connection = sqlCon;
            }
            else
            {
                sqlStr = @"insert into metadata_entry(proj_code,bundle_key,item_no,ps_name,ps_code,div_name,div_code,filename,gd_startdate,gd_enddate,gd_start_serial,gd_end_serial,created_by,created_dttm,status) values('" +
                        projKey + "','" + bundleKey + "','" + itemno +
                        "','" + psname + "','" + pscode + "','" + divname + "','" + divcode + "','" + filename + "','" + startD + "','" + endD + "','" + startSerial + "','" + endSerial + "','" + crd.created_by + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',5)";
                //sqlCmd.Connection = sqlCon;
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
                        string sDate = string.Empty;
                        string eDate = string.Empty;

                        string divname = deTextBox3.Text.Trim();
                        string psname = deTextBox4.Text.Trim();

                        string divcode = deTextBox6.Text.Trim();
                        string pscode = deTextBox7.Text.Trim();

                        string category = deTextBox5.Text.Trim();

                        if (deTextBox12.Text != "" && deTextBox11.Text != "" && deTextBox10.Text != "")
                        {
                            sDate = deTextBox12.Text + "-" + deTextBox11.Text + "-" + deTextBox10.Text;
                        }
                        else
                        {
                            sDate = "";
                        }

                        if (deTextBox16.Text != "" && deTextBox15.Text != "" && deTextBox14.Text != "")
                        {
                            eDate = deTextBox16.Text + "-" + deTextBox15.Text + "-" + deTextBox14.Text;
                        }
                        else
                        {
                            eDate = "";
                        }

                        string filenumber = "GD" + deTextBox10.Text + deTextBox11.Text + deTextBox12.Text;
                        filename = filenumber;

                        if (checkFileNotExists(projKey, bundleKey, filename) == true)
                        {

                            int item = Convert.ToInt32(itemCount(projKey, bundleKey).Rows[0][0].ToString()) + 1;


                            string gdSTSerial = deTextBox9.Text.Trim();
                            string gdEdSerial = deTextBox13.Text.Trim();


                            bool insertCase = insertIntoMeta(item, psname, pscode, divname, divcode, filename, sDate, eDate, gdSTSerial, gdEdSerial, sqlTrans);
                            if (insertCase == true)
                            {
                                if (sqlTrans == null)
                                {
                                    sqlTrans = sqlCon.BeginTransaction();
                                }
                                sqlTrans.Commit();
                                sqlTrans = null;
                                MessageBox.Show(this, "Record Saved Successfully...", "B'Zer ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                if (frmGD.state[0] == eSTATES.METADATA_ENTRY)
                                {
                                    frmGD_Load(sender, e);
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
                    DateTime dt = new DateTime(Convert.ToInt32(deTextBox12.Text), Convert.ToInt32(deTextBox11.Text), Convert.ToInt32(deTextBox10.Text));
                    string nextDate = dt.AddDays(1).ToString("ddMMyyyy");
                    deTextBox14.Text = nextDate.Substring(0, 2);

                    string month_year = _GetBundleDetails().Rows[0][8].ToString();
                    //if (month_year.Substring(0, 2).ToString() == nextDate.Substring(2, 2) &&
                    //    month_year.Substring(3, 4).ToString() == nextDate.Substring(4, 4))
                    //{
                    deTextBox15.Text = nextDate.Substring(0, 2).ToString();
                    deTextBox15.Text = nextDate.Substring(2, 2).ToString();
                    deTextBox16.Text = nextDate.Substring(4, 4).ToString();
                    deTextBox14.Enabled = false;
                    deTextBox15.Enabled = false;
                    deTextBox16.Enabled = false;
                    //}
                    //else
                    //{
                    //    MessageBox.Show(this, "Please select valid GD start date...", "B'Zer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //    deTextBox15.Text = string.Empty;
                    //    deTextBox15.Text = string.Empty;
                    //    deTextBox16.Text = string.Empty;
                    //    deTextBox10.Focus();
                    //    deTextBox10.Select();
                    //    return;
                    //}
                }
            }
            if (_mode == Mode._Add)
            {
                string filenumber = "GD" + deTextBox10.Text + deTextBox11.Text + deTextBox12.Text;
                checkFileNotExists(projKey, bundleKey, filenumber.Trim());
            }
        }

        private void deTextBox14_Leave(object sender, EventArgs e)
        {
            if (deTextBox14.Text == "" || deTextBox14.Text == null || String.IsNullOrEmpty(deTextBox14.Text) || String.IsNullOrWhiteSpace(deTextBox14.Text))
            { }
            else
            {
                if (deTextBox14.Text.Length < 2)
                {
                    deTextBox14.Text = deTextBox14.Text.PadLeft(2, '0');
                }
            }

        }

        private void frmGD_KeyUp(object sender, KeyEventArgs e)
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

        private void deTextBox14_KeyPress(object sender, KeyPressEventArgs e)
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

        private void deTextBox13_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Regex.IsMatch(e.KeyChar.ToString(), @"^[0-9\s\b]*$")))
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }
    }
}
