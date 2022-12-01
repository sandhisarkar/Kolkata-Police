using System;
using System.Drawing;
using System.Windows.Forms;
using NovaNet.Utils;
using NovaNet.wfe;
using LItems;
using System.Data;
using System.Data.Odbc;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace ImageHeaven
{
    public partial class frmDataImport : Form
    {
        MemoryStream stateLog;
        byte[] tmpWrite;
        NovaNet.Utils.dbCon dbcon;
        int pos = 0;
        int posAdd = 0;
        int resize = 10;
        OdbcConnection sqlCon = null;
        eSTATES[] state;
        wfeProject tmpProj = null;
        DataSet ds = null;
        private double ZOOMFACTOR = 1.10;   // = 25% smaller or larger
        private int MINMAX = 5;
        Point mouseDown = new Point();
        private Size ImageSize = new Size(100, 200);
        Credentials crd = new Credentials();
        //OdbcTransaction sqlTrans = null;
        private Dictionary<string, ListViewItem> ListViewItems = new Dictionary<string, ListViewItem>();
        private Dictionary<string, ListViewItem> ListViewItems1 = new Dictionary<string, ListViewItem>();

        public static string category;

        public frmDataImport()
        {
            InitializeComponent();
        }

        public frmDataImport(OdbcConnection prmCon, Credentials prmCrd)
        {
            dbcon = new NovaNet.Utils.dbCon();
            sqlCon = prmCon;
            crd = prmCrd;
            InitializeComponent();
            tabControl1.TabPages.Remove(tabPage3);

        }
        private void KeyEvent(object sender, KeyEventArgs e) //Keyup Event 
        {
            if (tabControl1.SelectedIndex == 0 && lstPolicy.SelectedItems.Count > 0 && e.KeyCode == Keys.Add)
            {
                cmdAdd_Click(this, e);
            }
            if (tabControl1.SelectedIndex == 0 && (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown))
            {
                tabControl1.SelectedIndex = 0;
            }
            if (tabControl1.SelectedIndex == 1 && (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown))
            {
                tabControl1.SelectedIndex = 0;
            }
            if (tabControl1.SelectedIndex == 0 && lstSelImg.Items.Count > 0 && lstSelImg.SelectedItems.Count > 0 && e.KeyCode == Keys.Subtract)
            {
                cmdRemove_Click(this, e);
            }
            if (tabControl1.SelectedIndex == 0 && e.KeyCode == Keys.F5)
            {
                cmdImport_Click(this, e);
            }
            if (tabControl1.SelectedIndex == 1 && lstCheckDeed.SelectedItems.Count > 0 && e.KeyCode == Keys.Add)
            {
                cmdadd1_Click(this, e);
            }
            if (tabControl1.SelectedIndex == 1 && lstSelectedImg.Items.Count > 0 && lstSelectedImg.SelectedItems.Count > 0 && e.KeyCode == Keys.Subtract)
            {
                cmdremove1_Click(this, e);
            }
            if (tabControl1.SelectedIndex == 1 && lstCheckDeed.SelectedItems.Count > 0 && e.KeyCode == Keys.F5)
            {
                CmdFinalSave_Click(this, e);
            }
        }
        private void frmDataImport_Load(object sender, EventArgs e)
        {
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);
            PopulateProjectCombo();
            cmdBrowse.Enabled = false;
        }

        private void PopulateProjectCombo()
        {
            DataSet ds = new DataSet();


            DataTable dt = new DataTable();

            string sql = "select proj_key,proj_code from project_master order by proj_code";

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
                cmbProject.Text = "";
                MessageBox.Show("Add one project first...");

            }
        }

        private void cmbProject_Leave(object sender, EventArgs e)
        {
            PopulateBatchCombo();
        }

        private void PopulateBatchCombo()
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            string sql = "select DISTINCT a.bundle_key, a.bundle_code from bundle_master a, project_master b,metadata_entry c where a.proj_code = c.proj_code and a.bundle_key =c.bundle_key and a.proj_code = b.proj_key and a.status = '1' and a.proj_code = '" + cmbProject.SelectedValue.ToString() + "' order by a.bundle_key";

            OdbcDataAdapter odap = new OdbcDataAdapter(sql, sqlCon);
            odap.Fill(dt);


            if (dt.Rows.Count > 0)
            {
                cmbBatch.DataSource = dt;
                cmbBatch.DisplayMember = "bundle_code";
                cmbBatch.ValueMember = "bundle_key";
                cmdBrowse.Enabled = true;
                txtPath.Enabled = true;
            }
            else
            {
                //cmbBatch.Text = "";
                //MessageBox.Show("No Batch found for this project...","Add Batch");
                cmbBatch.Text = string.Empty;
                cmbBatch.DataSource = null;
                cmbBatch.DisplayMember = "";
                cmbBatch.ValueMember = "";
                cmbProject.Select();
                cmdBrowse.Enabled = false;
                txtPath.Enabled = false;
            }
        }

        private void cmbBatch_Leave(object sender, EventArgs e)
        {
            PopulateBox();
        }

        public DataTable _GetBundleStatus(string proj, string bundle)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct status,category from bundle_master where proj_code = '" + proj + "' and bundle_key = '" + bundle + "' ";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }
        private void GetIndexDetails(string deed_no, string projKey, string batchKey)
        {
            wQuery pQuery = new ihwQuery(sqlCon);
            DataSet dsVol = new DataSet();
            DataTable dt = new DataTable();
            string IMGName = deed_no;
            //IMGName = IMGName.Split(new char[] { '[', ']' })[1];
            projKey = cmbProject.SelectedValue.ToString();
            batchKey = cmbBatch.SelectedValue.ToString();
            dsVol = GetIndex(IMGName, projKey, batchKey);

            string filename = dsVol.Tables[0].Rows[0][3].ToString();

            string psN = dsVol.Tables[0].Rows[0][4].ToString();
            string psC = dsVol.Tables[0].Rows[0][5].ToString();
            string divN = dsVol.Tables[0].Rows[0][6].ToString();
            string divC = dsVol.Tables[0].Rows[0][7].ToString();
            string gdSDate = dsVol.Tables[0].Rows[0][8].ToString();
            string gdEDate = dsVol.Tables[0].Rows[0][9].ToString();
            string gdSSerial = dsVol.Tables[0].Rows[0][10].ToString();
            string gdESerial = dsVol.Tables[0].Rows[0][11].ToString();

            string GDSerialDate = dsVol.Tables[0].Rows[0][12].ToString();
            string FIRCaseno = dsVol.Tables[0].Rows[0][13].ToString();

            string CICaseno = dsVol.Tables[0].Rows[0][14].ToString();
            string CIDate = dsVol.Tables[0].Rows[0][15].ToString();

            string CRCaseno = dsVol.Tables[0].Rows[0][16].ToString();
            string CRDate = dsVol.Tables[0].Rows[0][17].ToString();

            string MRDate = dsVol.Tables[0].Rows[0][18].ToString();
            string MRSerial = dsVol.Tables[0].Rows[0][19].ToString();
            string MRCase = dsVol.Tables[0].Rows[0][20].ToString();


            if (dsVol.Tables[0].Rows.Count >= 1)
            {

                if (category.ToString() == "General Diary")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;
                    textBox9.Text = gdSDate;
                    textBox1.Text = gdEDate;
                    textBox7.Text = gdSSerial;
                    textBox2.Text = gdESerial;

                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    textBox63.Text = gdSDate;
                    textBox61.Text = gdEDate;
                    textBox10.Text = gdSSerial;
                    textBox11.Text = gdESerial;
                }
                else if (category.ToString() == "First Information Report")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;
                    label21.Text = "Start Serial Date";
                    textBox9.Text = GDSerialDate;
                    label6.Text = "FIR Case No";
                    textBox1.Text = FIRCaseno;

                    label19.Visible = false;
                    label7.Visible = false;
                    textBox7.Visible = false;
                    textBox2.Visible = false;


                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "Start Serial Date";
                    textBox63.Text = GDSerialDate;
                    label65.Text = "FIR Case No";
                    textBox61.Text = FIRCaseno;

                    label8.Visible = false;
                    label9.Visible = false;
                    textBox10.Visible = false;
                    textBox11.Visible = false;
                }
                else if (category.ToString() == "Crime Index")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;

                    label21.Text = "CI Date";
                    textBox9.Text = CIDate;
                    label6.Text = "CI Case No";
                    textBox1.Text = CICaseno;

                    label19.Visible = false;
                    label7.Visible = false;
                    textBox7.Visible = false;
                    textBox2.Visible = false;


                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "CI Date";
                    textBox63.Text = GDSerialDate;
                    label65.Text = "CI Case No";
                    textBox61.Text = FIRCaseno;

                    label8.Visible = false;
                    label9.Visible = false;
                    textBox10.Visible = false;
                    textBox11.Visible = false;
                }
                else if (category.ToString() == "Malkhana Register")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;

                    label21.Text = "MR Date";
                    textBox9.Text = MRDate;
                    label6.Text = "MR Case No";
                    textBox1.Text = MRCase;

                    label19.Text = "MR Serial No";
                    textBox7.Text = MRSerial;

                    label7.Visible = false;
                    textBox2.Visible = false;



                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "MR Date";
                    textBox63.Text = MRDate;
                    label65.Text = "MR Case No";
                    textBox61.Text = MRCase;

                    label8.Text = "MR Serial No";
                    textBox10.Text = MRSerial;
                    
                    label9.Visible = false;
                    textBox11.Visible = false;
                }
                else if (category.ToString() == "Case Records")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;

                    label21.Text = "CR Date";
                    textBox9.Text = CRDate;
                    label6.Text = "CR Case No";
                    textBox1.Text = CRCaseno;

                    label19.Visible = false;
                    label7.Visible = false;
                    textBox7.Visible = false;
                    textBox2.Visible = false;


                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "CR Date";
                    textBox63.Text = CRDate;
                    label65.Text = "CR Case No";
                    textBox61.Text = CRCaseno;

                    label8.Visible = false;
                    label9.Visible = false;
                    textBox10.Visible = false;
                    textBox11.Visible = false;
                }

            }
            if (dsVol.Tables[0].Rows.Count >= 2)
            {
                if (category.ToString() == "General Diary")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;
                    textBox9.Text = gdSDate;
                    textBox1.Text = gdEDate;
                    textBox7.Text = gdSSerial;
                    textBox2.Text = gdESerial;

                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    textBox63.Text = gdSDate;
                    textBox61.Text = gdEDate;
                    textBox10.Text = gdSSerial;
                    textBox11.Text = gdESerial;
                }
                else if (category.ToString() == "First Information Report")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;
                    label21.Text = "Start Serial Date";
                    textBox9.Text = GDSerialDate;
                    label6.Text = "FIR Case No";
                    textBox1.Text = FIRCaseno;

                    label19.Visible = false;
                    label7.Visible = false;
                    textBox7.Visible = false;
                    textBox2.Visible = false;


                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "Start Serial Date";
                    textBox63.Text = GDSerialDate;
                    label65.Text = "FIR Case No";
                    textBox61.Text = FIRCaseno;

                    label8.Visible = false;
                    label9.Visible = false;
                    textBox10.Visible = false;
                    textBox11.Visible = false;
                }
                else if (category.ToString() == "Crime Index")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;

                    label21.Text = "CI Date";
                    textBox9.Text = CIDate;
                    label6.Text = "CI Case No";
                    textBox1.Text = CICaseno;

                    label19.Visible = false;
                    label7.Visible = false;
                    textBox7.Visible = false;
                    textBox2.Visible = false;


                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "CI Date";
                    textBox63.Text = GDSerialDate;
                    label65.Text = "CI Case No";
                    textBox61.Text = FIRCaseno;

                    label8.Visible = false;
                    label9.Visible = false;
                    textBox10.Visible = false;
                    textBox11.Visible = false;
                }
                else if (category.ToString() == "Malkhana Register")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;

                    label21.Text = "MR Date";
                    textBox9.Text = MRDate;
                    label6.Text = "MR Case No";
                    textBox1.Text = MRCase;

                    label19.Text = "MR Serial No";
                    textBox7.Text = MRSerial;

                    label7.Visible = false;
                    textBox2.Visible = false;



                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "MR Date";
                    textBox63.Text = MRDate;
                    label65.Text = "MR Case No";
                    textBox61.Text = MRCase;

                    label8.Text = "MR Serial No";
                    textBox10.Text = MRSerial;

                    label9.Visible = false;
                    textBox11.Visible = false;
                }
                else if (category.ToString() == "Case Records")
                {
                    textBox8.Text = filename;
                    textBox4.Text = divN;
                    textBox5.Text = psN;
                    textBox3.Text = divC;
                    textBox6.Text = psC;

                    label21.Text = "CR Date";
                    textBox9.Text = CRDate;
                    label6.Text = "CR Case No";
                    textBox1.Text = CRCaseno;

                    label19.Visible = false;
                    label7.Visible = false;
                    textBox7.Visible = false;
                    textBox2.Visible = false;


                    textBox62.Text = filename;
                    textBox52.Text = divN;
                    textBox59.Text = psN;
                    textBox50.Text = divC;
                    textBox60.Text = psC;
                    label76.Text = "CR Date";
                    textBox63.Text = CRDate;
                    label65.Text = "CR Case No";
                    textBox61.Text = CRCaseno;

                    label8.Visible = false;
                    label9.Visible = false;
                    textBox10.Visible = false;
                    textBox11.Visible = false;
                }
            }
            if (dsVol.Tables[0].Rows.Count > 1)
            {
                
            }
        }

        public DataSet GetIndex(string deed_no, string projKey, string batchKey)
        {
            string sqlStr = null;
            DataSet dsVol = new DataSet();
            OdbcDataAdapter sqlAdap = null;

            sqlStr = "select distinct proj_code, bundle_Key,item_no,filename,ps_name,ps_code,div_name,div_code,date_format(GD_startdate,'%Y-%m-%d'),date_format(GD_enddate,'%Y-%m-%d'),GD_start_serial,GD_end_serial,GD_serial_date,FIR_caseno,CI_case_no,date_format(CI_date,'%Y-%m-%d'),CR_case_no,date_format(CR_date,'%Y-%m-%d')," +
                "date_format(MR_date,'%Y-%m-%d'),MR_serial_no,MR_case_no from metadata_entry where proj_code = '" + projKey + "' and bundle_key = '" + batchKey + "' and filename = '" + deed_no + "' ";

            try
            {
                sqlAdap = new OdbcDataAdapter(sqlStr, sqlCon);
                sqlAdap.Fill(dsVol);
            }
            catch (Exception ex)
            {
                string e = ex.Message;
                sqlAdap.Dispose();
            }
            return dsVol;
        }
        private void PopulateBox()
        {
            string batchKey = null;
            DataSet ds = new DataSet();


            dbcon = new NovaNet.Utils.dbCon();

            int policyCount;

            if (cmbBatch.SelectedValue != null)
            {
                batchKey = cmbBatch.SelectedValue.ToString();
                //CtrlPolicy pPolicy = new CtrlPolicy(Convert.ToInt32(cmbProject.SelectedValue.ToString()), Convert.ToInt32(cmbBatch.SelectedValue.ToString()), 1, "0");
                CtrlPolicy pPolicy = new CtrlPolicy(Convert.ToInt32(cmbProject.SelectedValue.ToString()), Convert.ToInt32(cmbBatch.SelectedValue.ToString()), "1", "0");
                wfePolicy wPolicy = new wfePolicy(sqlCon, pPolicy);

                //eSTATES[] state = new eSTATES[1];
                //state[0] = eSTATES.POLICY_CREATED;
                ds = wPolicy.GetPolicyListImport(cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString());

                category = _GetBundleStatus(cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString()).Rows[0][1].ToString();

                if (ds.Tables.Count > 0)
                {
                    cmdBrowse.Enabled = true;
                    lstPolicy.Items.Clear();
                    lstCheckDeed.Items.Clear();
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        cmbFile.DataSource = ds.Tables[0];
                        cmbFile.DisplayMember = ds.Tables[0].Columns[0].ToString();
                        cmbFile.ValueMember = ds.Tables[0].Columns[0].ToString();
                    }
                    //lstPolicy.Columns.Add("policy");
                    for (int z = 0; z < ds.Tables[0].Rows.Count; z++)
                    {
                        lstPolicy.Items.Add(ds.Tables[0].Rows[z][0].ToString());
                        lstCheckDeed.Items.Add(ds.Tables[0].Rows[z][0].ToString());
                        lstAddlPages.Items.Add(ds.Tables[0].Rows[z][0].ToString());
                    }
                    if (lstPolicy.Items.Count > 0)
                    {
                        lstPolicy.Items[0].Selected = true;
                        lstCheckDeed.Items[0].Selected = true;
                        GetIndexDetails(lstPolicy.Items[0].Text, cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString());
                        //GetDeedVolume(lstPolicy.Items[0].Text);
                    }
                    else
                    {

                        lblinfo.Text = "No More File Name Found...";
                        //check
                        groupBox15.Visible = false;
                        textBox8.Text = "";
                        textBox4.Text = "";
                        textBox5.Text = "";
                        textBox3.Text = "";
                        textBox6.Text = "";
                        textBox9.Text = "";
                        textBox7.Text = "";
                        //check final
                        groupBox23.Visible = false;
                        textBox62.Text = "";
                        textBox52.Text = "";
                        textBox59.Text = "";
                        textBox50.Text = "";
                        textBox60.Text = "";
                        textBox63.Text = "";
                        textBox61.Text = "";

                    }
                }
            }
        }
        public void ResizeImg(string filename, Image img, int size)
        {
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, size);
            ImageCodecInfo encoderInfo = GetEncoderInfo(ImageFormat.Jpeg);
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;
            img.Save(txtPath.Text + "\\" + filename, encoderInfo, encoderParams);
            //img.Save(txtPath.Text,ImageCodecInfo.;
        }
        public ImageCodecInfo GetEncoderInfo(ImageFormat imageFormat)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == imageFormat.Guid)
                    return codec;
            }

            // Find the correct image codec 
            //for (int i = 0; i < codecs.Length; i++)
            //    if (codecs[i].MimeType == mimeType)
            //        return codecs[i];

            return null;
        }
        private void cmdBrowse_Click(object sender, EventArgs e)
        {
            try
            {

                lblinfo.Text = "";
                List<string> fileNames = new List<string>();
                List<string> tempPath = new System.Collections.Generic.List<string>(1000);


                fbdPath.ShowDialog();
                txtPath.Text = fbdPath.SelectedPath;
                DirectoryInfo selectedPath = new DirectoryInfo(txtPath.Text);

                int len = cmbBatch.Text.Length;
                int len1 = cmbBatch.Text.IndexOf('_') + 1;

                // string zyx1 = cmbBatch.Text.Substring(cmbBatch.Text.IndexOf('_') + 1, cmbBatch.Text.Length - cmbBatch.Text.IndexOf('_') + 1);

                if (Path.GetFileName(txtPath.Text.ToLower()) == cmbBatch.Text.ToLower())
                {
                    cmdImport.Enabled = true;
                    CmdFinalSave.Enabled = true;
                }
                else
                {

                    cmdImport.Enabled = false;
                    CmdFinalSave.Enabled = false;
                    MessageBox.Show(this, "Please select proper image folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                if (Directory.Exists(txtPath.Text + "\\Backup"))
                {
                    Directory.Delete(txtPath.Text + "\\Backup", true);
                    Directory.CreateDirectory(txtPath.Text + "\\Backup");
                    txtPath.Text = txtPath.Text + "\\Backup";
                    DirectoryInfo selectedPath1 = new DirectoryInfo(txtPath.Text);
                    foreach (FileInfo file in selectedPath.GetFiles())
                    {
                        if (file.Extension == ".TIF" || file.Extension == ".tif")
                        {
                            file.CopyTo(txtPath.Text + "\\" + file.Name);
                        }
                        else if (file.Extension.ToLower() == ".jpg" || file.Extension.ToLower() == "jpeg")
                        {
                            Image img = Image.FromFile(file.FullName);

                            string filename = Path.GetFileNameWithoutExtension(file.FullName) + ".TIF";
                            try
                            {
                                ResizeImg(filename, img, resize);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(this, "Error!! Filename - " + file.FullName, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }

                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(txtPath.Text + "\\Backup");
                    txtPath.Text = txtPath.Text + "\\Backup";
                    DirectoryInfo selectedPath1 = new DirectoryInfo(txtPath.Text);
                    foreach (FileInfo file in selectedPath.GetFiles())
                    {
                        if (file.Extension == ".TIF" || file.Extension == ".tif")
                        {
                            file.CopyTo(txtPath.Text + "\\" + file.Name);
                        }
                        else if (file.Extension.ToLower() == ".jpg" || file.Extension.ToLower() == ".jpeg")
                        {
                            Image img = Image.FromFile(file.FullName);

                            string filename = Path.GetFileNameWithoutExtension(file.FullName) + ".TIF";
                            try
                            {
                                ResizeImg(filename, img, resize);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(this, "Error!! Filename - " + file.FullName, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }

                        }
                    }
                }

                if (selectedPath.GetFiles().Length > 0)
                {

                    string BackupPath = selectedPath + "\\backup";
                    foreach (string file in Directory.GetFiles(BackupPath))
                    {
                        if (Path.GetExtension(file).ToLower() == ".tif")
                        {
                            fileNames.Add(file);
                            tempPath.Add(file);
                        }
                    }
                }

                //lvwItem.SubItems.Add(CLAIMS.ToString());
                //lvwItem.SubItems.Add("0");
                ListViewItems.Clear();
                ListViewItems1.Clear();
                lstImage.Items.Clear();
                //lstTotalImage.Items.Clear();
                lstImage.BeginUpdate();
                //lstTotalImage.BeginUpdate();

                foreach (string fileName in fileNames)
                {

                    ListViewItem lvi = lstImage.Items.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
                    //ListViewItem lvi1 = lstTotalImage.Items.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
                    lvi.Tag = fileName;
                    //lvi1.Tag = fileName;
                    ListViewItems.Add(fileName, lvi);
                    lstTotalImage.Rows.Add();
                    lstTotalImage.Rows[pos].Cells[0].Value = System.IO.Path.GetFileNameWithoutExtension(fileName);

                    pos = pos + 1;
                    //ListViewItems1.Add(fileName, lvi1);
                }
                //foreach (string fileName in fileNames)
                //{
                //    ListViewItem lvi1 = lstTotalImage.Items.Add(System.IO.Path.GetFileNameWithoutExtension(fileName));
                //    lvi1.Tag = fileName;
                //    ListViewItems.Add(fileName, lvi1);
                //}
                lstImage.EndUpdate();
                // lstTotalImage.EndUpdate();
                if (lstPolicy.Items.Count > 0)
                {
                    lstPolicy.Items[0].Selected = true;
                }
                groupBox2.Enabled = false;

                if (lstImage.Items.Count > 0)
                {

                    lstImage.Items[0].Focused = true;
                    lstImage.Items[0].Selected = true;
                    picMain.Height = 647;
                    picMain.Width = 625;
                    picMain.Refresh();
                    picMain.ImageLocation = null;
                    string imgPath = txtPath.Text + "\\" + lstImage.Items[0].Text + ".TIF";
                    picMain.ImageLocation = imgPath;


                    Image newImage = Image.FromFile(imgPath);
                    picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                    picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                    picMain.Refresh();
                    newImage.Dispose();
                    // GC.Collect();
                    picMain.MouseWheel += new MouseEventHandler(picMain_MouseWheel);
                    //picMain.MouseHover += new EventHandler(picMain_MouseHover);
                    lstImage.Select();
                }
                else
                {
                    picMain.ImageLocation = null;
                    lstImage.Select();
                }
                //lstImage.Select();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void cmdImport_Click(object sender, EventArgs e)
        {
            OdbcTransaction sqlTrans = null;

            if (cmdImport.Enabled == true)
            {
                try
                {
                    if (lstPolicy.Items.Count > 0)
                    {
                        if (lstSelImg.Items.Count == 0)
                        {
                            DialogResult dr = MessageBox.Show(this, "No Images selected for this Filename... Are you sure to continue?", "Selected no Image", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                            if (dr == DialogResult.Yes)
                            {
                                if (lstPolicy.Items.Count > 0)
                                {
                                    lstPolicy.Items[0].Selected = true;

                                    if (lstPolicy.SelectedItems.Count > 0)
                                    {
                                        for (int i = 0; i < lstSelImg.Items.Count; i++)
                                        {
                                            //lstSelImg.Items[i].Text
                                            for (int j = 0; j < lstTotalImage.Rows.Count; j++)
                                            {
                                                if (lstTotalImage.Rows[j].Cells[0].Value != null)
                                                {
                                                    if (lstSelImg.Items[i].Text == lstTotalImage.Rows[j].Cells[0].Value.ToString())
                                                    {
                                                        lstTotalImage.Rows[j].Cells[1].Value = lstPolicy.Items[0].Text;
                                                    }
                                                }
                                            }

                                        }

                                    }
                                    lstSelImg.Items.Clear();
                                    lstPolicy.SelectedItems[0].Remove();
                                    if (lstPolicy.Items.Count > 0)
                                    {
                                        lstPolicy.Items[0].Selected = true;

                                    }
                                }
                            }

                            else
                            {
                                return;
                            }
                        }
                        ////
                        else
                        {
                            if (lstPolicy.Items.Count > 0)
                            {
                                lstPolicy.Items[0].Selected = true;

                                if (lstPolicy.SelectedItems.Count > 0)
                                {
                                    for (int i = 0; i < lstSelImg.Items.Count; i++)
                                    {
                                        for (int j = 0; j < lstTotalImage.Rows.Count; j++)
                                        {
                                            if (lstTotalImage.Rows[j].Cells[0].Value != null)
                                            {
                                                if (lstSelImg.Items[i].Text == lstTotalImage.Rows[j].Cells[0].Value.ToString())
                                                {
                                                    lstTotalImage.Rows[j].Cells[1].Value = lstPolicy.Items[0].Text;
                                                }
                                            }
                                        }

                                    }

                                }
                                lstSelImg.Items.Clear();
                                lstPolicy.SelectedItems[0].Remove();
                                if (lstPolicy.Items.Count > 0)
                                {
                                    lstPolicy.Items[0].Selected = true;

                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "There's no file present in Import Tab...", "", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }

                    if (lstPolicy.Items.Count > 0)
                    {
                        lstPolicy.Items[0].Selected = true;
                        GetIndexDetails(lstPolicy.SelectedItems[0].Text, cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString());
                        //GetDeedVolume(lstPolicy.SelectedItems[0].Text);
                    }
                    if (lstImage.Items.Count > 0)
                    {

                        lstImage.Items[0].Focused = true;
                        lstImage.Items[0].Selected = true;
                        picMain.Height = 647;
                        picMain.Width = 625;
                        picMain.Refresh();
                        picMain.ImageLocation = null;
                        string imgPath = txtPath.Text + "\\" + lstImage.Items[0].Text + ".TIF";
                        picMain.ImageLocation = imgPath;


                        Image newImage = Image.FromFile(imgPath);
                        picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                        picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                        picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                        //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                        //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                        picMain.Refresh();
                        newImage.Dispose();
                        // GC.Collect();
                        picMain.MouseWheel += new MouseEventHandler(picMain_MouseWheel);
                        picMain.MouseHover += new EventHandler(picMain_MouseHover);
                        lstImage.Select();
                    }
                    else
                    {
                        picMain.ImageLocation = null;
                        lstImage.Select();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                    lblinfo.Text = "Error...";
                    sqlTrans.Rollback();
                }
            }
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            picMain.Image = null;

            try
            {
                for (int i = 0; i < lstImage.Items.Count; i++)
                {
                    if (lstImage.Items[i].Selected == true)
                    {
                        lstSelImg.Items.Add(lstImage.Items[i].Text.ToString());
                    }
                }
                foreach (ListViewItem eachItem in lstImage.SelectedItems)
                {
                    lstImage.Items.Remove(eachItem);
                }
                if (lstImage.Items.Count > 0)
                {

                    lstImage.Items[0].Focused = true;
                    lstImage.Items[0].Selected = true;
                    picMain.Height = 647;
                    picMain.Width = 625;
                    picMain.Refresh();
                    picMain.ImageLocation = null;
                    string imgPath = txtPath.Text + "\\" + lstImage.Items[0].Text + ".TIF";
                    picMain.ImageLocation = imgPath;


                    Image newImage = Image.FromFile(imgPath);
                    picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                    picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                    picMain.Refresh();
                    newImage.Dispose();
                    // GC.Collect();
                    picMain.MouseWheel += new MouseEventHandler(picMain_MouseWheel);
                    picMain.MouseHover += new EventHandler(picMain_MouseHover);
                    lstImage.Select();
                }
                else
                {
                    picMain.ImageLocation = null;
                    lstImage.Select();
                }
                lstImage.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void cmdRemove_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < lstSelImg.Items.Count; i++)
                {
                    if (lstSelImg.Items[i].Selected == true)
                    {
                        lstImage.Items.Add(lstSelImg.Items[i].Text.ToString());
                    }
                }
                foreach (ListViewItem eachItem in lstSelImg.SelectedItems)
                {
                    lstSelImg.Items.Remove(eachItem);
                }
                //if (lstImage.Items.Count > 0)
                //{

                //    lstImage.Items[0].Focused = true;
                //    lstImage.Items[0].Selected = true;
                //    picMain.Height = 647;
                //    picMain.Width = 625;
                //    picMain.Refresh();
                //    picMain.ImageLocation = null;
                //    string imgPath = txtPath.Text + "\\" + lstImage.Items[0].Text + ".TIF";
                //    picMain.ImageLocation = imgPath;


                //    Image newImage = Image.FromFile(imgPath);
                //    //picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                //    //picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                //    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                //    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                //    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                //    picMain.Refresh();
                //    newImage.Dispose();
                //    // GC.Collect();
                //    //picMain.MouseWheel += new MouseEventHandler(picMain_MouseWheel);
                //    //picMain.MouseHover += new EventHandler(picMain_MouseHover);
                //    //lstImage.Select();
                //}
                //else
                //{
                //    picMain.ImageLocation = null;
                //    //lstImage.Select();
                //}
                if (lstImage.Items.Count > 0)
                {
                    lstImage.Select();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void lstImage_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (lstImage.Items.Count > 0)
                {

                    if (lstImage.SelectedItems.Count > 0)
                    {

                        //lstImage.SelectedItems[0].Focused = true;
                        //lstImage.SelectedItems[0].Selected = true;
                        picMain.Height = 647;
                        picMain.Width = 625;
                        picMain.Refresh();
                        picMain.ImageLocation = null;
                        string imgPath = txtPath.Text + "\\" + lstImage.SelectedItems[0].Text + ".TIF";
                        picMain.ImageLocation = imgPath;


                        Image newImage = Image.FromFile(imgPath);
                        //picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                        //picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                        picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                        //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                        //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                        picMain.Refresh();
                        newImage.Dispose();
                        // GC.Collect();
                        //picMain.MouseWheel += new MouseEventHandler(picMain_MouseWheel);
                        //picMain.MouseHover += new EventHandler(picMain_MouseHover);
                        //lstImage.Select();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                GC.Collect();
            }
        }

        void picMain_MouseWheel(object sender, MouseEventArgs e)
        {
            //if (e.Delta < 0)
            //{
            //    ZoomIn();
            //}
            //else
            //{
            //    ZoomOut();
            //}

        }
        protected override void OnMouseWheel(MouseEventArgs mea)
        {

            //if (picMain.Image != null)
            //{
            //    if (mea.Delta > 0)
            //    {
            //        if ((picMain.Width < (15 * this.Width)) && (picMain.Height < (15 * this.Height)))
            //        {
            //            picMain.Width = (int)(picMain.Width * 1.25);
            //            picMain.Height = (int)(picMain.Height * 1.25);
            //        }
            //    }

            //    else
            //    {
            //        // Check if the pictureBox dimensions are in range (15 is the minimum and maximum zoom level)
            //        if ((picMain.Width > (this.Width / 15)) && (picMain.Height > (this.Height / 15)))
            //        {
            //            // Change the size of the picturebox, divide it by the ZOOM FACTOR
            //            picMain.Width = (int)(picMain.Width / 1.25);
            //            picMain.Height = (int)(picMain.Height / 1.25);
            //        }
            //    }
            //}
            //picMain.Refresh();
        }
        private void ZoomOut()
        {
            if (picMain.Height > (panel1.Height))
            {
                panel1.Width = picMain.Width;
                picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                picMain.Width = Convert.ToInt32(picMain.Width / ZOOMFACTOR);
                picMain.Height = Convert.ToInt32(picMain.Height / ZOOMFACTOR);
                picMain.Refresh();
            }
        }
        private void ZoomIn()
        {
            if ((picMain.Width < (MINMAX * panel1.Width)) &&
                (picMain.Height < (MINMAX * panel1.Height)))
            {
                picMain.Width = Convert.ToInt32(picMain.Width * ZOOMFACTOR);
                picMain.Height = Convert.ToInt32(picMain.Height * ZOOMFACTOR);
                picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                picMain.Refresh();
            }
        }

        private void lstImage_Leave(object sender, EventArgs e)
        {
            if (lstSelImg.Items.Count > 0)
            {

                lstSelImg.Items[0].Focused = true;
                lstSelImg.Items[0].Selected = true;
                picMain.Height = 647;
                picMain.Width = 625;
                picMain.Refresh();
                picMain.ImageLocation = null;
                string imgPath = txtPath.Text + "\\" + lstSelImg.Items[0].Text + ".TIF";
                picMain.ImageLocation = imgPath;


                Image newImage = Image.FromFile(imgPath);
                picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                picMain.Refresh();
                newImage.Dispose();
                // GC.Collect();
                picMain.MouseWheel += new MouseEventHandler(picMain_MouseWheel);
                picMain.MouseHover += new EventHandler(picMain_MouseHover);
                //lstSelImg.Select();
            }
            else
            {
                picMain.ImageLocation = null;
                //lstSelImg.Select();
            }
        }

        private void lstImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstImage_MouseClick(sender, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        }

        private void picMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ZoomIn();
            if (e.Button == MouseButtons.Right)
                ZoomOut();
        }

        private void picMain_MouseEnter(object sender, EventArgs e)
        {
            if (picMain.Focused == false)
            {
                //picMain.Focus();
            }
        }

        private void picMain_MouseHover(object sender, EventArgs e)
        {
            // picMain.Focus();
            //lstImage.Select();
        }

        private void picMain_MouseMove(object sender, MouseEventArgs e)
        {
            MouseEventArgs mouse = e as MouseEventArgs;

            //if (mouse.Button == MouseButtons.Left)
            //{
            //    Point mousePosNow = mouse.Location;

            //    int deltaX = mousePosNow.X - mouseDown.X;
            //    int deltaY = mousePosNow.Y - mouseDown.Y;

            //    int newX = picMain.Location.X + deltaX;
            //    int newY = picMain.Location.Y + deltaY;

            //    picMain.Location = new Point(newX, newY);
            //}
            //lstImage.Select();
        }

        private void lstSelImg_MouseClick(object sender, MouseEventArgs e)
        {
            if (lstSelImg.Items.Count > 0)
            {
                if (lstSelImg.SelectedItems.Count > 0)
                {

                    lstSelImg.SelectedItems[0].Focused = true;
                    lstSelImg.SelectedItems[0].Selected = true;

                    string imgPath = txtPath.Text + "\\" + lstSelImg.SelectedItems[0].Text + ".TIF";
                    picMain.Height = 647;
                    picMain.Width = 625;
                    picMain.Refresh();
                    picMain.ImageLocation = null;

                    picMain.ImageLocation = imgPath;


                    Image newImage = Image.FromFile(imgPath);
                    picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                    picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                    picMain.Refresh();
                    newImage.Dispose();
                    // GC.Collect();
                }
                lstSelImg.Select();
            }
        }

        private void lstSelImg_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstSelImg_MouseClick(sender, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        }

        private void lstTotalImage_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (lstTotalImage.Rows.Count > 0)
                {

                    string imgPath = txtPath.Text + "\\" + lstTotalImage.CurrentRow.Cells[0].Value.ToString() + ".TIF";
                    picMain.Height = 647;
                    picMain.Width = 625;
                    picMain.Refresh();
                    picMain.ImageLocation = null;

                    picMain.ImageLocation = imgPath;


                    Image newImage = Image.FromFile(imgPath);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                    //picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                    //picMain.Refresh();
                    newImage.Dispose();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());
                GC.Collect();
            }
        }

        private void lstTotalImage_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstTotalImage.Rows.Count > 0)
                {

                    string imgPath = txtPath.Text + "\\" + lstTotalImage.CurrentRow.Cells[0].Value.ToString() + ".TIF";
                    picMain.Height = 647;
                    picMain.Width = 625;
                    picMain.Refresh();
                    picMain.ImageLocation = null;

                    picMain.ImageLocation = imgPath;


                    Image newImage = Image.FromFile(imgPath);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                    //picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                    //picMain.Refresh();
                    newImage.Dispose();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());
                GC.Collect();
            }
        }

        private void lstTotalImage_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (lstTotalImage.Rows.Count > 0)
                {

                    string imgPath = txtPath.Text + "\\" + lstTotalImage.CurrentRow.Cells[0].Value.ToString() + ".TIF";
                    picMain.Height = 647;
                    picMain.Width = 625;
                    picMain.Refresh();
                    picMain.ImageLocation = null;

                    picMain.ImageLocation = imgPath;


                    Image newImage = Image.FromFile(imgPath);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                    //picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                    //picMain.Refresh();
                    newImage.Dispose();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());
                GC.Collect();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            picMain.Image = null;
            picMain.Refresh();
            if (lstCheckDeed.Items.Count > 0)
            {
                //lstPolicy.Items[0].Selected = true;
                lstCheckDeed.Items[0].Selected = true;
                lstCheckDeed.Items[0].Checked = true;
                lstCheckDeed.Items[0].Focused = true;
                lstCheckDeed.Select();
                //GetIndexDetails(lstPolicy.Items[0].Text, cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString());
                //GetDeedVolume(lstPolicy.Items[0].Text);
            }
        }

        private void lstTotalImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstTotalImage_MouseClick(sender, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
        }

        private void lstSelectedImg_MouseClick(object sender, MouseEventArgs e)
        {
            if (lstSelectedImg.Items.Count > 0)
            {
                if (lstSelectedImg.SelectedItems.Count > 0)
                {



                    string imgPath = txtPath.Text + "\\" + lstSelectedImg.SelectedItems[0].Text + ".TIF";
                    picMain.Height = 647;
                    picMain.Width = 625;
                    picMain.Refresh();
                    picMain.ImageLocation = null;

                    picMain.ImageLocation = imgPath;


                    Image newImage = Image.FromFile(imgPath);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1.1);
                    //picMain.Width = Convert.ToInt32(picMain.Height * newImage.Width / newImage.Height);

                    picMain.SizeMode = PictureBoxSizeMode.StretchImage;
                    //picMain.Width = Convert.ToInt32(picMain.Width * 1);
                    //picMain.Height = Convert.ToInt32(picMain.Height * 1);
                    picMain.Refresh();
                    newImage.Dispose();

                    lstSelectedImg.SelectedItems[0].Focused = true;
                }
            }
        }

        private void lstCheckDeed_MouseClick(object sender, MouseEventArgs e)
        {
            lstSelectedImg.Items.Clear();
            for (int i = 0; i < lstTotalImage.Rows.Count; i++)
            {
                if (lstTotalImage.Rows[i].Cells[1].Value != null)
                {
                    if (lstTotalImage.Rows[i].Cells[1].Value.ToString() == lstCheckDeed.SelectedItems[0].Text)
                    {
                        lstSelectedImg.Items.Add(lstTotalImage.Rows[i].Cells[0].Value.ToString());
                    }
                }
            }
        }

        private void lstCheckDeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCheckDeed.SelectedItems.Count > 0)
            {
                //lstPolicy.Items[0].Selected = true;
                groupBox15.Visible = true;
                groupBox23.Visible = true;
                //GetDeedVolume(lstPolicy.SelectedItems[0].Text);
            }
        }

        private void cmdremove1_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < lstSelectedImg.Items.Count; i++)
                {
                    if (lstSelectedImg.Items[i].Selected == true)
                    {
                        for (int j = 0; j < lstTotalImage.Rows.Count - 1; j++)
                        {
                            if (lstTotalImage.Rows[j].Cells[0].Value != null)
                            {
                                if (lstTotalImage.Rows[j].Cells[0].Value.ToString() == lstSelectedImg.Items[i].Text)
                                {
                                    lstTotalImage.Rows[j].Cells[1].Value = null;
                                    lstTotalImage.Rows[j].DefaultCellStyle.BackColor = Color.Yellow;
                                }
                            }
                        }
                    }
                }
                foreach (ListViewItem eachItem in lstSelectedImg.SelectedItems)
                {
                    lstSelectedImg.Items.Remove(eachItem);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void cmdadd1_Click(object sender, EventArgs e)
        {
            int indexRow = 0;
            try
            {
                for (int j = 0; j < lstTotalImage.Rows.Count - 1; j++)
                {
                    if (lstCheckDeed.SelectedItems[0].Selected == true)
                    {
                        if (lstTotalImage.CurrentRow.Cells[1].Value == null)
                        {
                            lstTotalImage.CurrentRow.Cells[1].Value = lstCheckDeed.SelectedItems[0].Text;
                            lstTotalImage.SelectedRows[j].DefaultCellStyle.BackColor = Color.GreenYellow;
                            lstSelectedImg.Items.Add(lstTotalImage.CurrentRow.Cells[0].Value.ToString());
                        }

                    }
                }






            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void CmdFinalSave_Click(object sender, EventArgs e)
        {
            if (CmdFinalSave.Enabled == true)
            {
                OdbcTransaction sqlTrans = null;
                string path = string.Empty;
                string oldFilename = string.Empty;
                string newFilename = string.Empty;
                wfeProject wfep = new wfeProject(sqlCon);
                DirectoryInfo selectedPath = new DirectoryInfo(txtPath.Text);
                try
                {
                    int filecou = lstPolicy.Items.Count;
                    int totImgcou = lstImage.Items.Count;
                    if (filecou > 0)
                    {
                        MessageBox.Show("You have one or more files there... ", "Check Again !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        tabControl1.SelectedIndex = 0;
                        lstPolicy.Select();
                        return;
                    }

                    DialogResult dr = MessageBox.Show(this, "Images are Ready to Import.Transaction Cannot be Rollback.Continue ?", "Importing Images", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.Yes)
                    {
                        lstTotalImage.Enabled = false;
                        lstSelectedImg.Enabled = false;
                        lstCheckDeed.Enabled = false;
                        cmdadd1.Enabled = false;
                        cmdremove1.Enabled = false;
                        CmdFinalSave.Enabled = false;
                        //path = wfep.GetPath(Convert.ToInt32(cmbProject.SelectedValue.ToString()), sqlTrans).Tables[0].Rows[0][0].ToString();
                        path = wfep.GetPath(Convert.ToInt32(cmbProject.SelectedValue.ToString())).Tables[0].Rows[0][0].ToString();
                        sqlTrans = sqlCon.BeginTransaction();
                        if (lstCheckDeed.Items.Count > 0)
                        {


                            for (int i = 0; i < lstCheckDeed.Items.Count; i++)
                            {

                                //if (sqlTrans.Connection == null)
                                //{
                                //    sqlTrans = sqlCon.BeginTransaction();
                                //}
                                string scanFolder = path + "\\" + cmbBatch.Text + "\\" + lstCheckDeed.Items[i].Text + "\\Scan";
                                if (!Directory.Exists(scanFolder))
                                {
                                    Directory.CreateDirectory(scanFolder);
                                }

                                List<string> fileNames = new List<string>();
                                int sequence = 1;

                                for (int j = 0; j < lstTotalImage.Rows.Count; j++)
                                {
                                    if (lstTotalImage.Rows[j].Cells[1].Value != null)
                                    {
                                        if (lstCheckDeed.Items[i].Text == lstTotalImage.Rows[j].Cells[1].Value.ToString())
                                        {
                                            fileNames.Add(lstTotalImage.Rows[j].Cells[0].Value.ToString() + ".TIF");
                                            newFilename = scanFolder + "\\" + lstCheckDeed.Items[i].Text + "_" + sequence.ToString().PadLeft(5, '0') + ".TIF";
                                            string imageName = lstCheckDeed.Items[i].Text + "_" + sequence.ToString().PadLeft(5, '0') + ".TIF";
                                            File.Copy(txtPath.Text + "\\" + lstTotalImage.Rows[j].Cells[0].Value.ToString() + ".TIF", newFilename, true);
                                            //if (sqlTrans.Connection == null)
                                            //{
                                            //    sqlTrans = sqlCon.BeginTransaction();
                                            //}
                                            insertIntoDB(imageName, sequence, sqlTrans, lstCheckDeed.Items[i].Text);
                                            sequence = sequence + 1;

                                        }
                                    }

                                }
                                if (UpdateCaseFile(sqlTrans, lstCheckDeed.Items[i].Text))
                                {

                                }
                                //if (sqlTrans.Connection == null)
                                //{
                                //    sqlTrans = sqlCon.BeginTransaction();
                                //}
                                wfePolicy wfe = new wfePolicy(sqlCon, new CtrlPolicy(Convert.ToInt32(cmbProject.SelectedValue.ToString()), Convert.ToInt32(cmbBatch.SelectedValue.ToString()), "1", lstCheckDeed.Items[i].Text));
                                wfe.UpdateTransactionLog(eSTATES.POLICY_SCANNED, crd, sqlTrans);
                                //UpdateTransactionLog(eSTATES.POLICY_SCANNED, crd, Convert.ToInt32(cmbProject.SelectedValue.ToString()), Convert.ToInt32(cmbBatch.SelectedValue.ToString()), "1", lstCheckDeed.Items[i].Text, sqlTrans);
                            }
                        }
                        //if(sqlTrans.Connection == null)
                        //{
                        //    sqlTrans = sqlCon.BeginTransaction();
                        //}
                        sqlTrans.Commit();
                        bool updatebatch = updateBatch();
                        //bool updatemeta = updateMeta();
                        MessageBox.Show(this, "Images Successfully Imported", "Import Images", MessageBoxButtons.OK, MessageBoxIcon.Information);


                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    sqlTrans.Rollback();
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int indexRow = 0;
            try
            {
                //for (int j = 0; j < lstTotalImage.Rows.Count - 1; j++)
                //{
                //    if (lstAddlPages.SelectedItems[0].Selected == true)
                //    {
                //        lstAddlallpages.CurrentRow.Cells[1].Value = lstAddlPages.SelectedItems[0].Text;

                //    }
                //}
                lstSelectedAddl.Items.Add(lstAddlallpages.CurrentRow.Cells[0].Value.ToString());
                lstAddlallpages.CurrentRow.DefaultCellStyle.BackColor = Color.Yellow;



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < lstSelectedAddl.Items.Count; i++)
                {
                    if (lstSelectedAddl.Items[i].Selected == true)
                    {
                        for (int j = 0; j < lstAddlallpages.Rows.Count - 1; j++)
                        {
                            if (lstAddlallpages.Rows[j].Cells[0].Value != null)
                            {
                                if (lstAddlallpages.Rows[j].Cells[0].Value.ToString() == lstSelectedAddl.Items[i].Text)
                                {
                                    lstAddlallpages.Rows[j].Cells[1].Value = null;
                                    lstAddlallpages.Rows[j].DefaultCellStyle.BackColor = Color.White;
                                }
                            }
                        }
                    }
                }
                foreach (ListViewItem eachItem in lstSelectedAddl.SelectedItems)
                {
                    lstSelectedAddl.Items.Remove(eachItem);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void cmdAddAddl_Click(object sender, EventArgs e)
        {

        }

        private void cmdAddl_Click(object sender, EventArgs e)
        {
            try
            {
                lblinfo.Text = "";
                List<string> fileNames = new List<string>();
                List<string> tempPath = new System.Collections.Generic.List<string>(1000);
                fbdPathAddl.ShowDialog();
                txtAddlPages.Text = fbdPathAddl.SelectedPath;
                DirectoryInfo selectedPath = new DirectoryInfo(txtAddlPages.Text);
                if (Directory.Exists(txtAddlPages.Text + "\\Backup"))
                {

                }
                else
                {
                    Directory.CreateDirectory(txtAddlPages.Text + "\\Backup");
                    DirectoryInfo selectedPath1 = new DirectoryInfo(txtAddlPages.Text);
                    foreach (FileInfo file in selectedPath.GetFiles())
                    {
                        if (file.Extension == ".TIF" || file.Extension == ".tif")
                        {
                            file.CopyTo(txtAddlPages.Text + "\\Backup\\" + file.Name);
                        }
                    }
                }

                if (selectedPath.GetFiles().Length > 0)

                    foreach (FileInfo file in selectedPath.GetFiles())
                    {
                        if (file.Extension.Equals(".tif") || file.Extension.Equals(".TIF"))
                        {
                            fileNames.Add(file.FullName);
                            tempPath.Add(txtPath.Text + "\\" + file.ToString());
                        }
                    }


                foreach (string fileName in fileNames)
                {

                    lstAddlallpages.Rows.Add();
                    lstAddlallpages.Rows[posAdd].Cells[0].Value = System.IO.Path.GetFileNameWithoutExtension(fileName);

                    posAdd = posAdd + 1;

                }
                if (lstAddlPages.Items.Count > 0)
                {
                    lstAddlPages.Items[0].Selected = true;
                }
                groupBox2.Enabled = false;

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void lstPolicy_Click(object sender, EventArgs e)
        {
            if (lstPolicy.Items.Count > 0)
            {
                //lstPolicy.Items[0].Selected = true;
                GetIndexDetails(lstPolicy.SelectedItems[0].Text, cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString());
                //GetDeedVolume(lstPolicy.SelectedItems[0].Text);
                groupBox15.Visible = true;
                groupBox23.Visible = true;
            }
        }

        private void lstCheckDeed_Click(object sender, EventArgs e)
        {
            if (lstCheckDeed.Items.Count > 0)
            {
                //lstPolicy.Items[0].Selected = true;
                GetIndexDetails(lstCheckDeed.SelectedItems[0].Text, cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString());
                //GetDeedVolume(lstCheckDeed.SelectedItems[0].Text);
            }
        }

        private void cmdImportAddladd_Click(object sender, EventArgs e)
        {
            try
            {
                for (int j = 0; j < lstTotalImage.Rows.Count - 1; j++)
                {
                    if (lstAddlPages.SelectedItems[0].Selected == true)
                    {
                        lstAddlallpages.CurrentRow.Cells[1].Value = lstAddlPages.SelectedItems[0].Text;
                    }
                }

                lstSelectedAddl.Items.Clear();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void lstAddlPages_MouseClick(object sender, MouseEventArgs e)
        {
            lstSelectedAddl.Items.Clear();
            for (int i = 0; i < lstAddlallpages.Rows.Count - 1; i++)
            {
                if (lstAddlallpages.Rows[i].Cells[1].Value != null)
                {
                    if (lstAddlallpages.Rows[i].Cells[1].Value.ToString() == lstAddlPages.SelectedItems[0].Text)
                    {
                        lstSelectedAddl.Items.Add(lstAddlallpages.Rows[i].Cells[0].Value.ToString());
                    }
                }
            }
        }

        private void cmdaddlFinalImp_Click(object sender, EventArgs e)
        {
            OdbcTransaction sqlTrans = null;
            string path = string.Empty;
            string oldFilename = string.Empty;
            string newFilename = string.Empty;
            string qcFilename = string.Empty;
            wfeProject wfep = new wfeProject(sqlCon);
            DirectoryInfo selectedPath = new DirectoryInfo(txtAddlPages.Text);
            try
            {
                DialogResult dr = MessageBox.Show(this, "Ready to move Additional Images??,Transaction cannot be rolled back, You sure to continue?", "Importing Images", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    //lstTotalImage.Enabled = false;
                    //lstSelectedImg.Enabled = false;
                    //lstCheckDeed.Enabled = false;
                    //cmdadd1.Enabled = false;
                    //cmdremove1.Enabled = false;
                    //CmdFinalSave.Enabled = false;
                    path = wfep.GetPath(Convert.ToInt32(cmbProject.SelectedValue.ToString()), sqlTrans).Tables[0].Rows[0][0].ToString();
                    sqlTrans = sqlCon.BeginTransaction();
                    if (lstAddlPages.Items.Count > 0)
                    {

                        path = wfep.GetPath(Convert.ToInt32(cmbProject.SelectedValue.ToString())).Tables[0].Rows[0][0].ToString();
                        //string scanFolder = path + "\\" + cmbBatch.Text + "\\1\\" + lstCheckDeed.SelectedItems[0].Text + "\\Scan";
                        //if (Directory.Exists(path))
                        //{
                        //    Directory.CreateDirectory(scanFolder);
                        //}
                        //else
                        //{
                        //    Directory.CreateDirectory(scanFolder);
                        //}


                        for (int i = 0; i < lstAddlPages.Items.Count; i++)
                        {


                            string scanFolder = path + "\\" + cmbBatch.Text + "\\1\\" + lstAddlPages.Items[i].Text + "\\Scan";
                            string qcFolder = path + "\\" + cmbBatch.Text + "\\1\\" + lstAddlPages.Items[i].Text + "\\QC";
                            //if (!Directory.Exists(scanFolder))
                            //{
                            //    Directory.CreateDirectory(scanFolder);
                            //}

                            List<string> fileNames = new List<string>();
                            int sequence = Convert.ToInt32(wfep.GetMaxImageSerial(lstAddlPages.Items[i].Text.ToString()).Tables[0].Rows[0][0].ToString());
                            sequence = sequence + 1;
                            for (int j = 0; j < lstAddlallpages.Rows.Count - 1; j++)
                            {
                                if (lstAddlPages.Items[i].Text == lstAddlallpages.Rows[j].Cells[1].Value.ToString())
                                {
                                    fileNames.Add(lstTotalImage.Rows[j].Cells[0].Value.ToString() + ".TIF");
                                    newFilename = scanFolder + "\\" + lstAddlPages.Items[i].Text + "_" + sequence.ToString().PadLeft(5, '0') + "_A.TIF";
                                    string imageName = lstAddlPages.Items[i].Text + "_" + sequence.ToString().PadLeft(5, '0') + "_A.TIF";
                                    File.Copy(txtPath.Text + "\\" + lstAddlallpages.Rows[j].Cells[0].Value.ToString() + ".TIF", newFilename, true);
                                    insertIntoDB(imageName, sequence, sqlTrans, lstAddlPages.Items[i].Text);
                                    sequence = sequence + 1;

                                }

                            }
                        }
                    }

                    sqlTrans.Commit();
                    MessageBox.Show(this, "Images Successfully Imported", "Import Images", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                sqlTrans.Rollback();
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void lstPolicy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPolicy.SelectedItems.Count > 0)
            {
                //lstPolicy.Items[0].Selected = true;
                groupBox15.Visible = true;
                groupBox23.Visible = true;
                //GetDeedVolume(lstPolicy.SelectedItems[0].Text);
            }
        }
        private bool insertIntoDB(string image, int imageNo, OdbcTransaction trans, string PolicyNo)
        {
            bool commitBol = true;
            if (lstCheckDeed.Items.Count > 0)
            {
                string sqlStr = string.Empty;

                OdbcCommand sqlCmd = new OdbcCommand();
                sqlStr = @"insert into image_master(proj_key,batch_key,box_number, policy_number,created_by,created_dttm,Page_name,status,photo,serial_no,page_index_name) values('" +
                            cmbProject.SelectedValue.ToString() + "','" + cmbBatch.SelectedValue.ToString() + "','1','" + PolicyNo +
                            "','" + crd.created_by + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + image + "','24','0','" + imageNo + "','" + image + "')";

                /* sqlStr = @"insert into image_import(proj_key,batch_key, filename,created_by,created_dttm,Page_name,status,photo,serial_no,page_index_name) values('" +
                             cmbProject.SelectedValue.ToString() + "','" + cmbBatch.SelectedValue.ToString() + "','" + PolicyNo +
                             "','" + frmMain.name + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + image + "','0','0','" + imageNo + "','" + image + "')"; */
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
            }

            return commitBol;

        }
        private bool UpdateCaseFile(OdbcTransaction trans, string policyNo)
        {
            OdbcCommand sqlCmd = new OdbcCommand();
            bool commitBol = true;
            if (lstCheckDeed.Items.Count > 0)
            {
                string sqlStr = "update metadata_entry set status = '2' where filename = '" + policyNo + "' and proj_code = '" + cmbProject.SelectedValue.ToString() + "' and bundle_key = '" + cmbBatch.SelectedValue.ToString() + "'";
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
            }
            return commitBol;
        }
        public bool updateBatch()
        {
            bool ret = false;
            if (ret == false)
            {
                _UpdateBatch();

                ret = true;
            }
            return ret;
        }
        public bool _UpdateBatch()
        {
            bool retVal = false;
            string sql = string.Empty;


            sql = "UPDATE bundle_master SET status = '2' WHERE proj_code = '" + cmbProject.SelectedValue.ToString() + "' AND bundle_key = '" + cmbBatch.SelectedValue.ToString() + "'";
            System.Diagnostics.Debug.Print(sql);
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon);
            if (cmd.ExecuteNonQuery() > 0)
            {
                retVal = true;
            }


            return retVal;
        }
        public bool UpdateTransactionLog(eSTATES state, Credentials prmCrd, int pk, int bk, string box, string pol, OdbcTransaction sqlTrans)
        {
            string sqlStr = null;
            // OdbcTransaction sqlTrans = null;
            bool commitBol = true;
            OdbcCommand sqlCmd = new OdbcCommand();
            bool flagTrans = false;
            switch (state)
            {
                case eSTATES.POLICY_SCANNED:
                    {
                        sqlStr = @"insert into transaction_log (proj_key,Batch_Key,Box_number,Policy_number,Scanned_user,scanned_dttm,fqc_user)" +
                        " values(" + pk + "," + bk + ",'" + box + "','" + pol + "','" + prmCrd.created_by + "','" + prmCrd.created_dttm + "','')";
                        break;
                    }
                case eSTATES.POLICY_QC:
                    {
                        sqlStr = @"update transaction_log" +
                        " set QC_User='" + prmCrd.created_by + "',Qc_DTTM='" + prmCrd.created_dttm + "' where proj_key=" + pk +
                        " and batch_key=" + bk + " and box_number='" + box + "'" +
                        " and policy_number='" + pol + "'";
                        break;
                    }
                case eSTATES.POLICY_INDEXED:
                    {
                        sqlStr = @"update transaction_log" +
                        " set Index_User='" + prmCrd.created_by + "',Index_DTTM='" + prmCrd.created_dttm + "' where proj_key=" + pk +
                        " and batch_key=" + bk + " and box_number='" + box + "'" +
                        " and policy_number='" + pol + "'";
                        break;
                    }
                case eSTATES.POLICY_FQC:
                    {
                        sqlStr = @"update transaction_log" +
                        " set Fqc_User='" + prmCrd.created_by + "',fqc_DTTM='" + prmCrd.created_dttm + "' where proj_key=" + pk +
                        " and batch_key=" + bk + " and box_number='" + box + "'" +
                        " and policy_number='" + pol + "'";
                        break;
                    }
            }
            try
            {
                if (sqlTrans == null)
                {
                    sqlTrans = sqlCon.BeginTransaction();
                    flagTrans = true;
                }
                sqlCmd.Connection = sqlCon;
                sqlCmd.Transaction = sqlTrans;
                sqlCmd.CommandText = sqlStr;
                int i = sqlCmd.ExecuteNonQuery();
                if (flagTrans)
                {
                    sqlTrans.Commit();
                }
                commitBol = true;
            }
            catch (Exception ex)
            {
                commitBol = false;
                sqlTrans.Rollback();
                sqlCmd.Dispose();
                stateLog = new MemoryStream();
                tmpWrite = new System.Text.ASCIIEncoding().GetBytes(sqlStr + "\n" + "Wfe State--" + Convert.ToString(Convert.ToInt32(state)) + "\n");
                stateLog.Write(tmpWrite, 0, tmpWrite.Length);

            }
            return commitBol;
        }
        private void cmbBatch_Validated(object sender, EventArgs e)
        {
            string batchKey = null;
            if (cmbBatch.SelectedIndex >= 0 && cmbBatch.SelectedValue != null)
            {
                batchKey = cmbBatch.SelectedValue.ToString();
                //CtrlPolicy pPolicy = new CtrlPolicy(Convert.ToInt32(cmbProject.SelectedValue.ToString()), Convert.ToInt32(cmbBatch.SelectedValue.ToString()), 1, "0");
                CtrlPolicy pPolicy = new CtrlPolicy(Convert.ToInt32(cmbProject.SelectedValue.ToString()), Convert.ToInt32(cmbBatch.SelectedValue.ToString()), "1", "0");
                wfePolicy wPolicy = new wfePolicy(sqlCon, pPolicy);

                //eSTATES[] state = new eSTATES[1];
                //state[0] = eSTATES.POLICY_CREATED;
                ds = wPolicy.GetPolicyListImport(cmbProject.SelectedValue.ToString(), cmbBatch.SelectedValue.ToString());

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count <= 0)
                    {
                        DialogResult dr = MessageBox.Show(this, "There's no filename for this batch, You sure to continue?", "Nothing Left", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                        if (dr == DialogResult.No)
                        {
                            cmbBatch.Focus();
                            return;
                        }
                    }
                }
            }
        }

        private void lstPolicy_SearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {

        }
    }
}
