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
            divName = _GetBundleDetails(projKey, bundleKey).Rows[0][4].ToString();
            divCode = _GetBundleDetails(projKey, bundleKey).Rows[0][4].ToString();

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

        public DataTable _GetFileCaseInDetails(string proj, string bundle)
        {
            DataTable dt = new DataTable();
            string sql = "select distinct proj_code, bundle_Key,item_no,filename,ps_name,ps_code,div_name,div_code from metadata_entry where proj_code = '" + proj + "' and batch_key = '" + bundle + "' order by item_no";
            OdbcCommand cmd = new OdbcCommand(sql, sqlCon, txn);
            OdbcDataAdapter odap = new OdbcDataAdapter(cmd);
            odap.Fill(dt);
            return dt;
        }

        private void Files_Load(object sender, EventArgs e)
        {

        }
    }
}
