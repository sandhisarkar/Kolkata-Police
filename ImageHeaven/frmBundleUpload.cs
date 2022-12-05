using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using NovaNet.Utils;
using NovaNet.wfe;
using System.Data;
using System.Data.Odbc;
using System.Collections;
using LItems;
//using AForge.Imaging;
//using AForge;
//using AForge.Imaging.Filters;
using TwainLib;
using Inlite.ClearImageNet;
//using System.Drawing.Bitmap;
//using System.Drawing.Graphics;
//using Graphics.DrawImage;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ImageHeaven
{
    public partial class frmBundleUpload : Form
    {
        Credentials crd = new Credentials();

        NovaNet.Utils.dbCon dbcon;
        OdbcConnection sqlCon = null;
        private OdbcDataAdapter sqlAdap = null;
        eSTATES[] state;
        DataSet dsdeed = null;
        public static string projKey;
        public static string bundleKey;
        public static NovaNet.Utils.exLog.Logger exMailLog = new NovaNet.Utils.exLog.emailLogger("./errLog.log", NovaNet.Utils.exLog.LogLevel.Dev, Constants._MAIL_TO, Constants._MAIL_FROM, Constants._SMTP);
        public static NovaNet.Utils.exLog.Logger exTxtLog = new NovaNet.Utils.exLog.txtLogger("./errLog.log", NovaNet.Utils.exLog.LogLevel.Dev);

        iTextSharp.text.Image i2;
        System.Drawing.Image image3;
        int j;
        Paragraph para;
        Paragraph para1;
        Paragraph para2;
        int flag = 0;
        int flag1 = 0;
        int flag2 = 0;

        public static string category = string.Empty;

        public frmBundleUpload()
        {
            InitializeComponent();
        }

        public frmBundleUpload(OdbcConnection prmCon, Credentials prmCrd)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            sqlCon = prmCon;
            crd = prmCrd;
            this.Text = "B'Zer - KP - Batch Upload";

            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
        }

        private void frmBundleUpload_Load(object sender, EventArgs e)
        {
            populateProject();
        }
        private void populateProject()
        {

            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select proj_key, proj_code from project_master  ";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                cmbProject.DataSource = dt;
                cmbProject.DisplayMember = "proj_code";
                cmbProject.ValueMember = "proj_key";

                populateBundle();
            }
            else
            {
                cmbProject.DataSource = null;
                // cmbProject.Text = "";
                MessageBox.Show("Add one project first...");
                this.Close();
            }


        }
        private void populateBundle()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select a.bundle_key, a.bundle_code from bundle_master a, project_master b where a.proj_code = b.proj_key and a.proj_code = '" + cmbProject.SelectedValue.ToString() + "' and a.status = '0'";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                cmbBundle.DataSource = dt;
                cmbBundle.DisplayMember = "bundle_code";
                cmbBundle.ValueMember = "bundle_key";
            }
            else
            {

                cmbBundle.Text = string.Empty;
                cmbBundle.DataSource = null;
                cmbBundle.DisplayMember = "";
                cmbBundle.ValueMember = "";
                cmbProject.Select();

            }
        }

        private void cmdsearch_Click(object sender, EventArgs e)
        {
            grdCsv.DataSource = null;
            grdCsv.DataSource = ReadDatabase().Tables[0];
            if (grdCsv.Rows.Count > 0)
            {
                //FormatDataGridView();
                cmdExport.Enabled = true;
                for (int i = 0; i < grdCsv.Rows.Count; i++)
                {
                    lstImage.Items.Add(ReadDatabase().Tables[0].Rows[i][1]);
                }
            }
            else
            {
                cmdExport.Enabled = false;
            }
        }

        private DataSet ReadDatabase()
        {
            DataSet ds = new DataSet();
            dsdeed = new DataSet();
            try
            {

                string sql = "select * from metadata_entry where proj_code = '" + cmbProject.SelectedValue.ToString() + "' and bundle_key = '" + cmbBundle.SelectedValue.ToString() + "' ";


                OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
                odap.Fill(ds);
                odap.Fill(dsdeed);

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());
                statusStrip1.Text = ex.Message.ToString();
            }
            return ds;
        }

        private void cmbProject_Leave(object sender, EventArgs e)
        {
            populateBundle();
        }

        public bool updateBundle()
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateBundle();

                ret = true;
            }
            return ret;
        }

        public bool _UpdateBundle()
        {
            bool retVal = false;
            string sql = string.Empty;
            string sqlStr = null;

            OdbcCommand sqlCmd = new OdbcCommand();


            sqlStr = "UPDATE bundle_master SET status = '1' WHERE proj_code = '" + projKey + "' AND bundle_key = '" + bundleKey + "'";
            System.Diagnostics.Debug.Print(sqlStr);
            OdbcCommand cmd = new OdbcCommand(sqlStr, sqlCon);


            if (cmd.ExecuteNonQuery() > 0)
            {
                retVal = true;
            }


            return retVal;
        }

        public bool updateCaseFile()
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateCaseFile();

                ret = true;
            }
            return ret;
        }
        public bool _UpdateCaseFile()
        {
            string sqlStr = null;

            OdbcCommand sqlCmd = new OdbcCommand();

            bool retVal = false;
            string sql = string.Empty;


            sqlStr = "UPDATE metadata_entry SET status = '1' WHERE proj_code = '" + projKey + "' AND bundle_key = '" + bundleKey + "'";
            System.Diagnostics.Debug.Print(sqlStr);
            OdbcCommand cmd = new OdbcCommand(sqlStr, sqlCon);
            if (cmd.ExecuteNonQuery() > 0)
            {
                retVal = true;
            }


            return retVal;
        }

        private void cmdExport_Click(object sender, EventArgs e)
        {
            DialogResult dlg;
            if ((cmbProject.Text == "" || cmbProject.Text == null) && (cmbBundle.Text == "" || cmbBundle.Text == null))
            {
                MessageBox.Show("Please select proper Project and Batch...");
                cmbProject.Focus();
                cmbProject.Select();
            }
            else
            {

                projKey = cmbProject.SelectedValue.ToString();

                bundleKey = cmbBundle.SelectedValue.ToString();
                if (category == "General Diary")
                {
                    string month_year = _GetBundleDetails(projKey, bundleKey).Rows[0][9].ToString();
                    int month = Convert.ToInt32(month_year.Substring(0, 2));
                    int year = Convert.ToInt32(month_year.Substring(3, 4));
                    int noOfDays = DateTime.DaysInMonth(year, month);

                    if(ReadDatabase().Tables[0].Rows.Count != noOfDays)
                    {
                        statusStrip1.Items.Clear();
                        statusStrip1.Items.Add("Status: Uploading Cannot be Completed");
                        MessageBox.Show(this, "Number of files dosen't match with number of days for the month", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                //this.Hide();
                statusStrip1.Items.Add("Status: Wait While Uploading the Database......");
                bool updatebundle = updateBundle();
                bool updatecasefile = updateCaseFile();

                if (updatebundle == true && updatecasefile == true)
                {

                    statusStrip1.Items.Clear();
                    statusStrip1.Items.Add("Status: Batch Sucessfully Uploaded");
                    MessageBox.Show("Batch Sucessfully Uploaded");
                    populateBundle();
                    grdCsv.DataSource = null;
                    cmdExport.Enabled = false;
                    return;

                }
                else
                {
                    statusStrip1.Items.Clear();
                    statusStrip1.Items.Add("Status: Uploading Cannot be Completed");
                    MessageBox.Show(this, "Uploading Cannot be Completed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public DataTable _GetBundleDetails(string proj, string bundle)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct proj_code, bundle_Key, bundle_name as 'Batch Name', bundle_code as 'Batch Code',category,ps_name,ps_code,div_name,div_code,month_year from bundle_master where proj_code = '" + proj + "' and bundle_key = '" + bundle + "' ";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }
        private void cmbBundle_Leave(object sender, EventArgs e)
        {
            category = _GetBundleDetails(cmbProject.SelectedValue.ToString(), cmbBundle.SelectedValue.ToString()).Rows[0][4].ToString();
        }
    }
}
