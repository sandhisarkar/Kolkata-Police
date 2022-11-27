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

        }

        private void deButton1_Click(object sender, EventArgs e)
        {
            if (_mode == DataLayerDefs.Mode._Add)
            {
                DateTime temp;
                //string isDate = dateTimePicker1.Text;
                string currDate = DateTime.Now.ToString("MM-yyyy");

                if ((txtCreateDate.Text != "" || txtCreateDate.Text != null))
                {

                    if ((deComboBox1.Text != null || deComboBox1.Text != "") && (deComboBox2.Text != null || deComboBox2.Text != ""))
                    {
                        //P_CR_5
                        string category = deComboBox3.SelectedValue.ToString();
                        string divCode = deComboBox1.SelectedValue.ToString();
                        string psCode = deComboBox2.SelectedValue.ToString();
                        string bundleCount = string.Empty;
                        string checking = string.Empty;

                        if (category != null || category != "")
                        {
                            string nextDate = DateTime.Now.AddMonths(1).ToString("MM-yyyy");

                            if (category == "GD")
                            {
                                string isDate = dateTimePicker1.Text;
                                txtCreateDate.Text = isDate;
                                if (DateTime.TryParse(isDate, out temp) && DateTime.TryParse(nextDate, out temp) && DateTime.Parse(isDate) <= DateTime.Parse(currDate))
                                {
                                    checking = isDate.Substring(0, 2) + isDate.Substring(3, 4);
                                    bundleCount = category + "_" + divCode + "_" + psCode;
                                    bundleCount = bundleCount + "_" + checking;


                                    string bundleCode = bundleCount;

                                    textBox3.Text = bundleCode;
                                    textBox4.Text = bundleCode;

                                    button2.Enabled = true;
                                }
                                else
                                {
                                    //retval = false;
                                    MessageBox.Show("Please select a valid Date, creation date is beyond current date", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    dateTimePicker1.Select();
                                    //validateBol = false;


                                    textBox3.Text = string.Empty;
                                    textBox4.Text = string.Empty;

                                    button2.Enabled = false;
                                }
                            }
                            else
                            {
                                string isDate = dateTimePicker1.Text;
                                txtCreateDate.Text = isDate;
                                if (DateTime.TryParse(isDate, out temp) && DateTime.TryParse(nextDate, out temp) && DateTime.Parse(isDate) <= DateTime.Parse(currDate))
                                {
                                    checking = isDate.Substring(3, 4) + "_" + getBundleCount(deComboBox2.Text, psCode, deComboBox1.Text, divCode, deComboBox3.Text);
                                    bundleCount = category + "_" + divCode + "_" + psCode;
                                    bundleCount = bundleCount + "_" + checking;


                                    string bundleCode = bundleCount;

                                    textBox3.Text = bundleCode;
                                    textBox4.Text = bundleCode;

                                    button2.Enabled = true;
                                }
                                else
                                {
                                    //retval = false;
                                    MessageBox.Show("Please select a valid Date, creation date is beyond current date", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                    dateTimePicker1.Select();
                                    //validateBol = false;


                                    textBox3.Text = string.Empty;
                                    textBox4.Text = string.Empty;

                                    button2.Enabled = false;
                                }
                            }


                        }
                        else
                        {
                            textBox3.Text = string.Empty;
                            textBox4.Text = string.Empty;

                            button2.Enabled = false;
                        }


                    }

                }

            }
        }
        private string getBundleCount(string pN, string pC, string dC, string dN, string cat)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select Count(*) from bundle_master where ps_name = '" + pN + "' and ps_code = '" + pC + "' and div_name = '" + dN + "' " +
                "and div_code = '" + dC + "' and category = '" + cat + "'";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);

            int count = Convert.ToInt32(dt.Rows[0][0].ToString());

            string getCount = Convert.ToString(count + 1);

            return getCount;
        }

        private void ClearAllField()
        {

            txtCreateDate.Text = string.Empty;

            textBox3.Text = string.Empty;
            textBox4.Text = string.Empty;
            button2.Enabled = false;
            cmbProject.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (sqlCon.State == ConnectionState.Closed || sqlCon.State == ConnectionState.Broken)
            {
                sqlCon.Open();
            }
            if (_mode == DataLayerDefs.Mode._Add)
            {
                if (textBox3.Text == null || textBox3.Text == "")
                {
                    MessageBox.Show("Please generate a Batch Code...");
                    deButton1.Focus();
                }
                else
                {


                    NovaNet.Utils.dbCon dbcon = new NovaNet.Utils.dbCon();
                    udtBatch objBatch = new udtBatch();
                    try
                    {
                        statusStrip1.Items.Clear();
                        crtBatch = new wfeBatch(sqlCon);

                        objBatch.proj_code = Convert.ToInt32(cmbProject.SelectedValue);
                        objBatch.batch_code = textBox3.Text;
                        objBatch.batch_name = textBox4.Text;
                        objBatch.Created_By = crd.created_by;
                        objBatch.Created_DTTM = dbcon.GetCurrenctDTTM(1, sqlCon);


                        string divCode = deComboBox1.SelectedValue.ToString();
                        string psCode = deComboBox2.SelectedValue.ToString();

                        if (TransferValuesBatch(objBatch, divCode, psCode) == true)
                        {
                            MessageBox.Show("Batch SucessFully Created");
                            statusStrip1.Items.Add("Status: Data SucessFully Saved");
                            statusStrip1.ForeColor = System.Drawing.Color.Black;
                            ClearAllField();

                        }
                        else
                        {
                            MessageBox.Show(this, "Data Cannot be Saved", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            statusStrip1.Items.Add("Status: Data Cannot be Saved");
                            statusStrip1.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                    catch (KeyCheckException ex)
                    {
                        MessageBox.Show(ex.Message, "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        stateLog = new MemoryStream();
                        tmpWrite = new System.Text.ASCIIEncoding().GetBytes("Batch Key-" + objBatch.batch_key + "\n" + "project Key-" + objBatch.proj_code + "\n");
                        stateLog.Write(tmpWrite, 0, tmpWrite.Length);
                        //exMailLog.Log(ex, this);
                    }
                    catch (DbCommitException dbex)
                    {
                        MessageBox.Show(dbex.Message, "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        stateLog = new MemoryStream();
                        tmpWrite = new System.Text.ASCIIEncoding().GetBytes("Error while Commit" + "Batch Key-" + objBatch.batch_key + "\n" + "project Key-" + objBatch.proj_code + "\n");
                        stateLog.Write(tmpWrite, 0, tmpWrite.Length);
                        // exMailLog.Log(dbex, this);
                    }
                    catch (CreateFolderException folex)
                    {
                        MessageBox.Show(folex.Message, "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        stateLog = new MemoryStream();
                        tmpWrite = new System.Text.ASCIIEncoding().GetBytes("Error while Create Folder" + "Batch Key-" + objBatch.batch_key + "\n" + "project Key-" + objBatch.proj_code + "\n");
                        stateLog.Write(tmpWrite, 0, tmpWrite.Length);
                        // exMailLog.Log(folex, this);
                    }
                    catch (DBConnectionException conex)
                    {
                        MessageBox.Show(conex.Message, "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        stateLog = new MemoryStream();
                        tmpWrite = new System.Text.ASCIIEncoding().GetBytes("Error while Connection error" + "Batch Key-" + objBatch.batch_key + "\n" + "project Key-" + objBatch.proj_code + "\n");
                        stateLog.Write(tmpWrite, 0, tmpWrite.Length);
                        //exMailLog.Log(conex, this);
                    }
                    catch (INIFileException iniex)
                    {
                        MessageBox.Show(iniex.Message, "B'Zer - KP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        stateLog = new MemoryStream();
                        tmpWrite = new System.Text.ASCIIEncoding().GetBytes("Error while INI read error" + "Batch Key-" + objBatch.batch_key + "\n" + "project Key-" + objBatch.proj_code + "\n");
                        stateLog.Write(tmpWrite, 0, tmpWrite.Length);
                        //exMailLog.Log(iniex, this);
                    }




                }
            }
        }

        public bool KeyCheck(string prmValue)
        {
            string sqlStr = null;
            OdbcCommand cmd = null;
            bool existsBol = true;

            sqlStr = "select bundle_code from bundle_master where bundle_code='" + prmValue.ToUpper() + "'";
            cmd = new OdbcCommand(sqlStr, sqlCon);
            existsBol = cmd.ExecuteReader().HasRows;

            return existsBol;
        }
        private bool Validate(udtBatch cmd)
        {
            bool validateBol = true;
            //errList = new Hashtable();
            if (cmd.batch_code == string.Empty || KeyCheck(cmd.batch_code) == true)
            {
                validateBol = false;
                //errList.Add("Code", Constants.NOT_VALID);
            }

            if (cmd.batch_name == string.Empty)
            {
                validateBol = false;
                //errList.Add("Name", Constants.NOT_VALID);
            }

            if (cmd.Created_By == string.Empty && mode == Constants._ADDING)
            {
                validateBol = false;
                //errList.Add("Created_By", Constants.NOT_VALID);
            }

            if (cmd.Created_DTTM == string.Empty && mode == Constants._ADDING)
            {
                validateBol = false;
                // errList.Add("Created_DTTM", Constants.NOT_VALID);
            }

            ///Required at the time of editing
            if (cmd.Modified_By == string.Empty && mode == Constants._EDITING)
            {
                validateBol = false;
                //errList.Add("Modified_By", Constants.NOT_VALID);
            }

            if (cmd.Modified_DTTM == string.Empty && mode == Constants._EDITING)
            {
                validateBol = false;
                //errList.Add("Modified_DTTM", Constants.NOT_VALID);
            }

            //if (cmd.batch_code.Substring(0, 1).ToUpper() != deComboBox1.Text.Substring(0, 1).ToUpper())
            //{
            //    validateBol = false;
            //}

            return validateBol;
        }

        public bool Commit_Bundle(string pC, string dC)
        {
            string sqlStr = null;
            OdbcTransaction sqlTrans = null;
            bool commitBol = true;
            OdbcCommand sqlCmd = new OdbcCommand();
            string scanbatchPath = null;

            //errList = new Hashtable();
            objProj = new wfeProject(sqlCon);

            dsPath = objProj.GetPath(objBatch.proj_code);

            if (dsPath.Tables[0].Rows.Count > 0)
            {
                scanbatchPath = dsPath.Tables[0].Rows[0]["project_Path"] + "\\" + objBatch.batch_code;
            }

            string isDate = dateTimePicker1.Text;
            txtCreateDate.Text = isDate;

            sqlStr = @"insert into bundle_master(proj_code,bundle_code,category,bundle_name,ps_name,ps_code,div_name,div_code,month_year,created_by" +
                ",Created_DTTM,bundle_path) values(" +
                objBatch.proj_code + ",'" + objBatch.batch_code.ToUpper() + "','" + deComboBox3.Text.Trim() + "','" + objBatch.batch_name + "'," +
                "'" + deComboBox2.Text.Trim() + "','" + pC + "','" + deComboBox1.Text.Trim() + "','" + dC + "','" + txtCreateDate.Text + "'" +
                "'" + objBatch.Created_By + "','" + objBatch.Created_DTTM + "','" +
                scanbatchPath.Replace("\\", "\\\\") + "')";
            try
            {
                if (KeyCheck(objBatch.batch_code) == false)
                {
                    sqlTrans = sqlCon.BeginTransaction();
                    sqlCmd.Connection = sqlCon;
                    sqlCmd.Transaction = sqlTrans;
                    sqlCmd.CommandText = sqlStr;
                    sqlCmd.ExecuteNonQuery();

                    if (mode == Constants._ADDING)
                    {
                        if (FileorFolder.CreateFolder(scanbatchPath) == true)
                        {
                            commitBol = true;
                            sqlTrans.Commit();
                        }
                        else
                        {
                            commitBol = false;
                            sqlTrans.Rollback();
                            rd = new INIReader(Constants.EXCEPTION_INI_FILE_PATH);
                            udtKeyValue.Key = Constants.BATCH_FOLDER_CREATE_ERROR.ToString();
                            udtKeyValue.Section = Constants.BATCH_EXCEPTION_SECTION;
                            string ErrMsg = rd.Read(udtKeyValue);
                            throw new CreateFolderException(ErrMsg);
                        }
                    }
                    else
                    {
                        commitBol = true;
                        sqlTrans.Commit();
                    }
                }
                else
                    commitBol = false;
            }
            catch (Exception ex)
            {
                //errList.Add(Constants.DBERRORTYPE, ex.Message);
                commitBol = false;
                sqlTrans.Rollback();
                sqlCmd.Dispose();
                stateLog = new MemoryStream();
                tmpWrite = new System.Text.ASCIIEncoding().GetBytes(sqlStr + "\n");
                stateLog.Write(tmpWrite, 0, tmpWrite.Length);
                exMailLog.Log(ex);
            }
            return commitBol;
        }

        public bool TransferValuesBatch(udtCmd cmd, string dC, string pC)
        {

            objBatch = (udtBatch)(cmd);
            if (KeyCheck(objBatch.batch_code) == false)
            {
                if (Validate(objBatch) == true)
                {

                    if (Commit_Bundle(pC, dC) == true)
                    {
                        return true;
                    }
                    else
                    {
                        rd = new INIReader(Constants.EXCEPTION_INI_FILE_PATH);
                        udtKeyValue.Key = Constants.SAVE_ERROR.ToString();
                        udtKeyValue.Section = Constants.COMMON_EXCEPTION_SECTION;
                        string ErrMsg = rd.Read(udtKeyValue);
                        throw new DbCommitException(ErrMsg);
                    }
                }
                else
                {
                    //throw new ValidationException(Constants.ValidationException) ;
                    return false;
                }
            }
            else
            {
                rd = new INIReader(Constants.EXCEPTION_INI_FILE_PATH);
                udtKeyValue.Key = Constants.DUPLICATE_KEY_CHECK.ToString();
                udtKeyValue.Section = Constants.COMMON_EXCEPTION_SECTION;
                string ErrMsg = rd.Read(udtKeyValue);
                throw new KeyCheckException(ErrMsg);
            }
        }
    }
}
