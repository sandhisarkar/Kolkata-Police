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
using nControls;

namespace ImageHeaven
{
    public partial class Files : Form
    {
        public static int index;

        Credentials crd = new Credentials();
        //Credentials crd = new Credentials();
        //private OdbcConnection sqlCon;
        OdbcTransaction txn;
        string name = frmMain.name;
        OdbcConnection sqlCon = null;
        public static bool _modeBool;

        public static DataLayerDefs.Mode _mode = DataLayerDefs.Mode._Edit;

        public static string projKey;
        public static string bundleKey;
        public static string casefileNo;
        public static string filename;

        public static string category;
        public static string psName;
        public static string psCode;
        public static string divName;
        public static string divCode;

        public static string item;

        public Files()
        {
            InitializeComponent();
        }

        public Files(OdbcConnection pCon, DataLayerDefs.Mode mode, OdbcTransaction pTxn, Credentials prmCrd)
        {
            InitializeComponent();
            sqlCon = pCon;
            crd = prmCrd;

            txn = pTxn;

            projKey = frmEntrySummary.projKey;
            bundleKey = frmEntrySummary.bundleKey;

            category = _GetBundleDetails(projKey, bundleKey).Rows[0][4].ToString();
            psName = _GetBundleDetails(projKey, bundleKey).Rows[0][5].ToString();
            psCode = _GetBundleDetails(projKey, bundleKey).Rows[0][6].ToString();
            divName = _GetBundleDetails(projKey, bundleKey).Rows[0][7].ToString();
            divCode = _GetBundleDetails(projKey, bundleKey).Rows[0][8].ToString();

            if (mode == DataLayerDefs.Mode._Edit)
            {


                deLabel3.Text = _GetBundleDetails(projKey, bundleKey).Rows[0][3].ToString();
                deLabel5.Text = "Category : " + _GetBundleDetails(projKey, bundleKey).Rows[0][4].ToString();

                int count = _GetFileCaseInDetails(projKey, bundleKey).Rows.Count;

                for (int i = 0; i < count; i++)
                {

                    string filename = _GetFileCaseInDetails(projKey, bundleKey).Rows[i][3].ToString();

                    //add row
                    string[] row = { filename };
                    var listItem = new ListViewItem(row);

                    lstDeeds.Items.Add(listItem);
                }



                _mode = mode;
            }
        }

        public DataTable _GetBundleDetails(string proj, string bundle)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct proj_code, bundle_Key, bundle_name as 'Batch Name', bundle_code as 'Batch Code',category,ps_name,ps_code,div_name,div_code from bundle_master where proj_code = '" + proj + "' and bundle_key = '" + bundle + "' ";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon, txn);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }
        private void formatForm()
        {

            this.deTextBox1.AutoCompleteCustomSource = GetSuggestions("metadata_entry", "filename", projKey, bundleKey);
            this.deTextBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.deTextBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;

        }

        private void formatEntryForm()
        {

            this.deTextBox1.AutoCompleteCustomSource = GetSuggestions("metadata_entry", "filename", projKey, bundleKey);
            this.deTextBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.deTextBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;

        }
        public DataTable _GetFileCaseInDetails(string proj, string bundle)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct proj_code, bundle_Key,item_no,filename,ps_name,ps_code,div_name,div_code from metadata_entry where proj_code = '" + proj + "' and bundle_key = '" + bundle + "' order by item_no";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon, txn);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }
        public AutoCompleteStringCollection GetSuggestions(string tblName, string fldName, string projKey, string bundleKey)
        {
            AutoCompleteStringCollection x = new AutoCompleteStringCollection();
            string sql = "Select distinct " + fldName + " from " + tblName + " where proj_code = '" + projKey + "' AND bundle_key = '" + bundleKey + "'";
            DataSet ds = new DataSet();
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon, txn);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    x.Add(ds.Tables[0].Rows[i][0].ToString().Trim());
                }
            }

            return x;
        }
        private void Files_Load(object sender, EventArgs e)
        {
            if (sqlCon.State == ConnectionState.Closed || sqlCon.State == ConnectionState.Broken)
            {
                sqlCon.Open();
            }
            if (_mode == DataLayerDefs.Mode._Add)
            {
                formatForm();
                if (lstDeeds.Items.Count > 0)
                {
                    lstDeeds.Items[0].Selected = true;
                    lstDeeds.Items[0].Focused = true;
                    lstDeeds.Select();
                    lstDeeds.Items[0].EnsureVisible();
                }
            }
            if (_mode == DataLayerDefs.Mode._Edit)
            {
                formatEntryForm();
                if (lstDeeds.Items.Count > 0)
                {
                    lstDeeds.Items[0].Selected = true;
                    lstDeeds.Items[0].Focused = true;
                    lstDeeds.Select();
                    lstDeeds.Items[0].EnsureVisible();
                }
            }
        }

        private void Files_KeyUp(object sender, KeyEventArgs e)
        {
            if (sqlCon.State == ConnectionState.Closed || sqlCon.State == ConnectionState.Broken)
            {
                sqlCon.Open();
            }
            if (e.KeyCode == Keys.Escape)
            {

                this.Close();
            }
        }

        private void deTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(deTextBox1.Text.ToUpper().Trim()))
                {
                    for (int i = 0; i < lstDeeds.Items.Count; i++)
                    {
                        if (lstDeeds.Items[i].ToString().Contains(deTextBox1.Text.ToUpper().Trim()))
                        {
                            lstDeeds.Items[i].Selected = true;
                            lstDeeds.Items[i].Focused = true;
                            lstDeeds.Select();
                            lstDeeds.Items[i].EnsureVisible();
                            //lstDeeds.SetSelected(i, true);
                        }
                    }
                }
            }
        }

        private void deTextBox1_Enter(object sender, EventArgs e)
        {
            deTextBox1.SelectAll();
        }

        private void cmdSearch_Click(object sender, EventArgs e)
        {
            string text = deTextBox1.Text;

            for (int i = 0; i < lstDeeds.Items.Count; i++)
            {
                if (lstDeeds.Items[i].SubItems[0].Text.Equals(text))
                {

                    lstDeeds.Items[i].Selected = true;
                    lstDeeds.Items[i].Focused = true;
                    lstDeeds.Select();
                    lstDeeds.Items[i].EnsureVisible();
                    return;
                }
                else
                {
                    lstDeeds.Items[i].Selected = false;
                }

            }
        }

        private void lstDeeds_MouseClick(object sender, MouseEventArgs e)
        {
            if (crd.role == ihConstants._ADMINISTRATOR_ROLE)
            {
                if (_GetFileCaseDetailsIndividualStatus(projKey, bundleKey, lstDeeds.SelectedItems[0].Text).Rows[0][0].ToString() == "0")
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        if (lstDeeds.FocusedItem.Bounds.Contains(e.Location) == true)
                        {
                            cmsDeeds.Show(Cursor.Position);
                        }
                    }
                }
            }
        }

        public DataTable _GetFileCaseDetailsIndividualStatus(string proj, string bundle, string fileName)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct status from metadata_entry where proj_code = '" + proj + "' and bundle_key = '" + bundle + "' and filename = '" + fileName + "'  ";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon, txn);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }

        private void lstDeeds_DoubleClick(object sender, EventArgs e)
        {
            if (lstDeeds.SelectedItems.Count > 0)
            {
                index = lstDeeds.FocusedItem.Index;

                filename = lstDeeds.Items[index].SubItems[0].Text;

                if (_mode == DataLayerDefs.Mode._Edit)
                {
                    //this.Hide();
                    //EntryForm frm = new EntryForm(sqlCon, _mode, filename, txn, crd);
                    //frm.ShowDialog(this);
                    this.SetTopLevel(false);
                    if (category.ToString() == "General Diary")
                    {
                        frmGD fm1 = new frmGD(projKey, bundleKey, sqlCon, crd, DataLayerDefs.Mode._Edit, filename, "Entry");
                        fm1.ShowDialog();
                    }
                    else if (category.ToString() == "FIR")
                    {
                        frmFIR fm1 = new frmFIR(projKey, bundleKey, sqlCon, crd, DataLayerDefs.Mode._Edit, filename, "Entry");
                        fm1.ShowDialog();
                    }
                    else if (category.ToString() == "Crime Index")
                    {
                        frmCI fm1 = new frmCI(projKey, bundleKey, sqlCon, crd, DataLayerDefs.Mode._Edit, filename, "Entry");
                        fm1.ShowDialog();
                    }
                    else if (category.ToString() == "Malkhana Register")
                    {

                    }
                    else if (category.ToString() == "Case Records")
                    {

                    }
                    else if (category.ToString() == "General Register")
                    {

                    }
                    this.SetTopLevel(true);

                    lstDeeds.Items.Clear();

                    int count = _GetFileCaseInDetails(projKey, bundleKey).Rows.Count;

                    for (int i = 0; i < count; i++)
                    {

                        string filename = _GetFileCaseInDetails(projKey, bundleKey).Rows[i][3].ToString();

                        //add row
                        string[] row = { filename };
                        var listItem = new ListViewItem(row);

                        lstDeeds.Items.Add(listItem);
                    }
                    if (lstDeeds.Items.Count > 0)
                    {
                        lstDeeds.Items[0].Selected = true;
                        lstDeeds.Items[0].Focused = true;
                        lstDeeds.Select();
                        lstDeeds.Items[0].EnsureVisible();
                    }
                    else
                    {
                        fileRemarks.Text = string.Empty;
                    }

                }
            }
        }

        private void lstDeeds_KeyUp(object sender, KeyEventArgs e)
        {
            if (lstDeeds.Items.Count > 0)
            {
                if (lstDeeds.SelectedItems.Count > 0)
                {
                    if (e.Control == true && e.KeyCode == Keys.O)
                    {
                        lstDeeds_DoubleClick(sender, e);
                    }
                    if (e.KeyCode == Keys.Enter)
                    {
                        //lstDeeds_DoubleClick(sender, e);
                    }
                    if (e.KeyCode == Keys.Space)
                    {
                        lstDeeds_DoubleClick(sender, e);
                    }
                }
            }
        }

        public DataTable _GetFileCaseDetailsIndividual(string proj, string bundle, string fileName)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct proj_code, bundle_Key,item_no,filename,ps_name,ps_code,div_name,div_code,date_format(GD_startdate,'%Y-%m-%d'),date_format(GD_enddate,'%Y-%m-%d'),GD_start_serial,GD_end_serial,GD_serial_date,FIR_caseno,CI_case_no,date_format(CI_date,'%Y-%m-%d'),CR_case_no,date_format(CR_date,'%Y-%m-%d')," +
                "date_format(MR_date,'%Y-%m-%d'),MR_serial_no,MR_case_no from metadata_entry where proj_code = '" + proj + "' and bundle_key = '" + bundle + "' and filename = '" + fileName + "' ";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon, txn);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }

        private void lstDeeds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDeeds.SelectedItems.Count > 0)
            {
                //string casefileno = lstDeeds.SelectedItems[0].SubItems[0].Text;
                //string item = lstDeeds.SelectedItems[0].SubItems[1].Text;

                string filename = lstDeeds.SelectedItems[0].SubItems[0].Text;

                if (category.ToString() == "General Diary")
                {
                    string psName = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][4].ToString();
                    string psCode = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][5].ToString();
                    string divName = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][6].ToString();
                    string divCode = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][7].ToString();
                    string gdstartDate = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][8].ToString();
                    string gdendDate = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][9].ToString();
                    string gdsatrtserail = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][10].ToString();
                    string gdendSerial = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][11].ToString();


                    fileRemarks.Text = "Category : " + category + "\nDivision Name : " + divName + "\nDivision Code : " + divCode +
                        "\nPS Name: " + psName + "\nPS Code : " + psCode + "\nGD Start Date : " + gdstartDate + "\nGD End Date : " + gdendDate + "\nGD Start Serial :" + gdsatrtserail + "\nGD End Serial : " + gdendSerial;
                }
                else if (category.ToString() == "FIR")
                {
                    string psName = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][4].ToString();
                    string psCode = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][5].ToString();
                    string divName = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][6].ToString();
                    string divCode = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][7].ToString();
                    string gdserialDate = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][12].ToString();
                    string fircaseno = _GetFileCaseDetailsIndividual(projKey, bundleKey, filename).Rows[0][13].ToString();
                    

                    fileRemarks.Text = "Category : " + category + "\nDivision Name : " + divName + "\nDivision Code : " + divCode +
                        "\nPS Name: " + psName + "\nPS Code : " + psCode + "\nGD Serial Date : " + gdserialDate + "\nFIR Case No : " + fircaseno;
                }
                else if (category.ToString() == "Crime Index")
                {

                }
                else if (category.ToString() == "Malkhana Register")
                {

                }
                else if (category.ToString() == "Case Records")
                {

                }
                else if (category.ToString() == "General Register")
                {

                }
                else
                {
                    fileRemarks.Text = "";
                }
            }
            else
            {
                fileRemarks.Text = "";
            }
        }
    }
}
