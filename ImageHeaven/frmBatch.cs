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

namespace ImageHeaven
{
    public partial class frmBatch : Form
    {
        protected int mode;
        MemoryStream stateLog;
        private udtBatch objBatch;
        private OdbcDataAdapter sqlAdap = null;
        private DataSet dsPath = null;
        private wfeProject objProj = null;
        private INIReader rd = null;
        private KeyValueStruct udtKeyValue;
        public string err = null;
        private int projCode;
        wfeBatch crtBatch = null;
        OdbcConnection sqlCon = null;
        byte[] tmpWrite;
        public static NovaNet.Utils.exLog.Logger exMailLog = new NovaNet.Utils.exLog.emailLogger("./errLog.log", NovaNet.Utils.exLog.LogLevel.Dev, Constants._MAIL_TO, Constants._MAIL_FROM, Constants._SMTP);
        public static NovaNet.Utils.exLog.Logger exTxtLog = new NovaNet.Utils.exLog.txtLogger("./errLog.log", NovaNet.Utils.exLog.LogLevel.Dev);

        string name = frmMain.name;

        DataLayerDefs.Mode _mode = DataLayerDefs.Mode._Edit;

        public string currentDate;
        Credentials crd = new Credentials();
        string old_path;

        public frmBatch()
        {
            InitializeComponent();
        }

        public frmBatch(wItem prmCmd, OdbcConnection prmCon, DataLayerDefs.Mode mode, Credentials prmCrd)
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            //this.Icon = 
            exMailLog.SetNextLogger(exTxtLog);
            _mode = mode;
            crtBatch = (wfeBatch)prmCmd;
            sqlCon = prmCon;
            crd = prmCrd;
            if (crtBatch.GetMode() == Constants._ADDING)
                this.Text = "B'Zer - KP (Add Batch)";
            else
                this.Text = "B'Zer - KP (Edit Batch)";
            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
        }

        private void frmBatch_Load(object sender, EventArgs e)
        {
            if (_mode == DataLayerDefs.Mode._Add)
            {
                groupBox3.Enabled = false;
                populateProject();
                button2.Enabled = false;

                currentDate = DateTime.Now.ToString("MM-yyyy");

                txtCreateDate.Text = currentDate;
                
                dateTimePicker1.Text = currentDate;
                dateTimePicker1.Format = DateTimePickerFormat.Custom;
                dateTimePicker1.CustomFormat = "MM-yyyy";
                dateTimePicker1.Value = Convert.ToDateTime(currentDate.ToString());
                dateTimePicker1.Enabled = true;
            }
        }

        private void populateProject()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select proj_key, proj_code from project_master ";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                cmbProject.DataSource = dt;
                cmbProject.DisplayMember = "proj_code";
                cmbProject.ValueMember = "proj_key";


            }
            else
            {
                MessageBox.Show("Add one project first...");
            }

        }
        public void populateDivision()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select div_code, div_name from division_master ";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                deComboBox1.DataSource = dt;
                deComboBox1.DisplayMember = "div_name";
                deComboBox1.ValueMember = "div_code";

                populatePS();
            }
            //else
            //{
            //    MessageBox.Show("Add one project first...");
            //}
        }
        public void populateCategory()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select category_code, category_name from category_master ";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                deComboBox3.DataSource = dt;
                deComboBox3.DisplayMember = "category_name";
                deComboBox3.ValueMember = "category_code";

            }
            //else
            //{
            //    MessageBox.Show("Add one project first...");
            //}
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public DataTable getProjectName(string pcode)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct proj_key,proj_code from project_master where proj_key = '" + pcode + "' ";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }
        
        private void populatePS()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select police_station_code, police_station_name from ps_master where div_code IN (select div_code from division_master where div_name = '" + deComboBox1.Text.Trim() + "')";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                deComboBox2.DataSource = dt;
                deComboBox2.DisplayMember = "police_station_name";
                deComboBox2.ValueMember = "police_station_code";
            }
            //else
            //{
            //    MessageBox.Show("Add one project first...");
            //}

        }

        private void cmbProject_Leave(object sender, EventArgs e)
        {
            if (cmbProject.Text == "" || cmbProject.Text == null)
            {
                MessageBox.Show("Please select a project name");
                cmbProject.Focus();
                cmbProject.Select();
            }
            else
            {
                //populateEstablishment();

                groupBox3.Enabled = true;

                //textBox1.Focus();
                //textBox1.Select();
                populateDivision();
                populateCategory();
                txtCreateDate.Text = currentDate;
                //txtHandoverDate.Text = handoverDate;
                //dateTimePicker1.Text = handoverDate;
                //dateTimePicker1.Format = DateTimePickerFormat.Custom;
                //dateTimePicker1.CustomFormat = "yyyy-MM-dd";
                //dateTimePicker1.Value = Convert.ToDateTime(handoverDate.ToString());
            }
        }

        private void cmbProject_MouseLeave(object sender, EventArgs e)
        {
            if (cmbProject.Text == "" || cmbProject.Text == null)
            {
                MessageBox.Show("Please select a project name");
                cmbProject.Focus();
                cmbProject.Select();
            }
            else
            {
                //populateEstablishment();

                groupBox3.Enabled = true;

                //textBox1.Focus();
                //textBox1.Select();
                populateDivision();
                populateCategory();
                txtCreateDate.Text = currentDate;
                //txtHandoverDate.Text = handoverDate;
                //dateTimePicker1.Text = handoverDate;
                //dateTimePicker1.Format = DateTimePickerFormat.Custom;
                //dateTimePicker1.CustomFormat = "yyyy-MM-dd";
                //dateTimePicker1.Value = Convert.ToDateTime(handoverDate.ToString());
            }
        }

        private void deComboBox1_Leave(object sender, EventArgs e)
        {
            if (deComboBox1.Text != "" || deComboBox1.Text != null || deComboBox1.Text != string.Empty || !string.IsNullOrEmpty(deComboBox1.Text))
            {
                populatePS();
            }
        }

        private void deComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            populatePS();
        }

        private void dateTimePicker1_Leave(object sender, EventArgs e)
        {
            if (_mode == DataLayerDefs.Mode._Add)
            {
                DateTime temp;
                string isDate = dateTimePicker1.Text;
                string currDate = DateTime.Now.ToString("MM-yyyy");


                if (DateTime.TryParse(isDate, out temp))
                {
                    //validateBol = true;
                    txtCreateDate.Text = isDate;

                    dateTimePicker1.Text = isDate;
                    dateTimePicker1.Format = DateTimePickerFormat.Custom;
                    dateTimePicker1.CustomFormat = "MM-yyyy";
                    dateTimePicker1.Value = Convert.ToDateTime(isDate.ToString());
                }
                else
                {
                    //retval = false;
                    MessageBox.Show("Please select a valid Date", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    dateTimePicker1.Select();
                    //validateBol = false;

                }
            }
            if (_mode == DataLayerDefs.Mode._Edit)
            {
                DateTime temp;
                string isDate = dateTimePicker1.Text;
                string currDate = DateTime.Now.ToString("MM-yyyy");
                if (DateTime.TryParse(isDate, out temp))
                {
                    //validateBol = true;
                    txtCreateDate.Text = isDate;

                    dateTimePicker1.Text = isDate;
                    dateTimePicker1.Format = DateTimePickerFormat.Custom;
                    dateTimePicker1.CustomFormat = "MM-yyyy";
                    dateTimePicker1.Value = Convert.ToDateTime(isDate.ToString());
                }
                else
                {
                    //retval = false;
                    MessageBox.Show("Please select a valid Date", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    dateTimePicker1.Select();
                    //validateBol = false;

                }
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if (_mode == DataLayerDefs.Mode._Add)
            {
                DateTime temp;
                string isDate = dateTimePicker1.Text;
                string currDate = DateTime.Now.ToString("MM-yyyy");
                if (DateTime.TryParse(isDate, out temp))
                {
                    //validateBol = true;
                    txtCreateDate.Text = isDate;

                    dateTimePicker1.Text = isDate;
                    dateTimePicker1.Format = DateTimePickerFormat.Custom;
                    dateTimePicker1.CustomFormat = "MM-yyyy";
                    dateTimePicker1.Value = Convert.ToDateTime(isDate.ToString());
                }
                else
                {
                    //retval = false;
                    MessageBox.Show("Please select a valid Date", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    dateTimePicker1.Select();
                    //validateBol = false;

                }
            }
            if (_mode == DataLayerDefs.Mode._Edit)
            {
                DateTime temp;
                string isDate = dateTimePicker1.Text;
                string currDate = DateTime.Now.ToString("MM-yyyy");
                if (DateTime.TryParse(isDate, out temp))
                {
                    //validateBol = true;
                    txtCreateDate.Text = isDate;

                    dateTimePicker1.Text = isDate;
                    dateTimePicker1.Format = DateTimePickerFormat.Custom;
                    dateTimePicker1.CustomFormat = "MM-yyyy";
                    dateTimePicker1.Value = Convert.ToDateTime(isDate.ToString());
                }
                else
                {
                    //retval = false;
                    MessageBox.Show("Please select a valid Date", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    dateTimePicker1.Select();
                    //validateBol = false;

                }
            }
        }
    }
}
