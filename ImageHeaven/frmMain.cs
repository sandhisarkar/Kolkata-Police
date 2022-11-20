using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using NovaNet.Utils;
using System.Data.Odbc;
using System.Reflection;
using System.Data.OleDb;
using System.Globalization;
using LItems;
using NovaNet;
using NovaNet.wfe;

namespace ImageHeaven
{
    public partial class frmMain : Form
    {
        static wItem wi;
        //NovaNet.Utils.dbCon dbcon;
        frmMain mainForm;
        OdbcConnection sqlCon = null;
        public Credentials crd;
        static int colorMode;
        dbCon dbcon;

        //
        NovaNet.Utils.GetProfile pData;
        NovaNet.Utils.ChangePassword pCPwd;
        NovaNet.Utils.Profile p;
        public static NovaNet.Utils.IntrRBAC rbc;
        private short logincounter;
        //
        OdbcTransaction txn;

        public static string projKey;
        public static string bundleKey;
        public static string projectName = null;
        public static string batchName = null;
        public static string boxNumber = null;
        public static string projectVal = null;
        public static string batchVal = null;

        public static string name;

        public static int height;
        public static int width;

        public frmMain()
        {
            InitializeComponent();
        }

        public frmMain(OdbcConnection pCon)
        {
            InitializeComponent();

            sqlCon = pCon;

            logincounter = 0;

            ImageHeaven.Program.Logout = false;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            height = pictureBox1.Height;
            width = pictureBox1.Width;

            int k;
            dbcon = new NovaNet.Utils.dbCon();
            try
            {
                string dllPaths = string.Empty;

                menuStrip1.Visible = false;


                if (sqlCon.State == ConnectionState.Open)
                {
                    pData = getData;
                    pCPwd = getCPwd;
                    rbc = new NovaNet.Utils.RBAC(sqlCon, dbcon, pData, pCPwd);
                    //string test = sqlCon.Database;
                    GetChallenge gc = new GetChallenge(getData);




                    gc.ShowDialog(this);

                    crd = rbc.getCredentials(p);
                    AssemblyName assemName = Assembly.GetExecutingAssembly().GetName();
                    this.Text = "B'Zer - Kolkata Police" + "           Version: " + assemName.Version.ToString() + "    Database name: " + sqlCon.Database.ToString() + "    Logged in user: " + crd.userName;

                    name = crd.userName;
                    if (crd.role == ihConstants._ADMINISTRATOR_ROLE || crd.role == "Supervisor")
                    {
                        menuStrip1.Visible = true;
                        newToolStripMenuItem.Visible = true;
                        newToolStripMenuItem.Enabled = true;
                        projectToolStripMenuItem.Enabled = true;
                        projectToolStripMenuItem.Visible = true;
                        batchToolStripMenuItem.Enabled = true;
                        batchToolStripMenuItem.Visible = true;
                        exitToolStripMenuItem.Visible = true;
                        exitToolStripMenuItem.Enabled = true;

                        transactionsToolStripMenuItem.Visible = true;
                        dataEntryToolStripMenuItem.Visible = true;
                        dataEntryToolStripMenuItem.Enabled = true;
                        batchUploadToolStripMenuItem.Visible = true;
                        batchUploadToolStripMenuItem.Enabled = true;
                        bundleScanToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Visible = true;
                        imageImportToolStripMenuItem.Enabled = true;
                        imageQualityControlToolStripMenuItem.Visible = true;
                        imageQualityControlToolStripMenuItem.Enabled = true;
                        qualityControlFinalToolStripMenuItem.Visible = true;
                        qualityControlFinalToolStripMenuItem.Enabled = true;
                        toolStripMenuItem1.Enabled = true;
                        toolStripMenuItem1.Visible = true;
                        exportToolStripMenuItem.Enabled = true;
                        exportToolStripMenuItem.Visible = true;


                        toolsToolStripMenuItem.Enabled = true;
                        toolsToolStripMenuItem.Visible = true;
                        configurationToolStripMenuItem.Visible = true;
                        newPasswordToolStripMenuItem.Visible = true;
                        newUserToolStripMenuItem.Visible = true;
                        onlineUsersToolStripMenuItem.Visible = true;
                        officeNameConfigurationToolStripMenuItem.Visible = true;

                        toolStrip1.Visible = true;
                        toolStripButton1.Visible = false;
                        toolStripButton3.Visible = true;
                        toolStripButton2.Visible = false;
                        toolStripButton4.Visible = true;

                        configurationToolStripMenuItem.Visible = true;

                        helpToolStripMenuItem.Visible = true;

                        aboutToolStripMenuItem.Visible = true;

                        auditToolStripMenuItem.Visible = true;
                        partIIToolStripMenuItem.Visible = true;
                        partIToolStripMenuItem.Visible = true;
                        logoutToolStripMenuItem.Visible = true;

                        reportToolStripMenuItem.Visible = true;
                        dashboardToolStripMenuItem.Visible = true;
                    }
                    else if (crd.role == "Scan")
                    {

                        menuStrip1.Visible = true;
                        newToolStripMenuItem.Visible = false;
                        newToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Visible = false;
                        batchToolStripMenuItem.Enabled = false;
                        batchToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Enabled = false;

                        transactionsToolStripMenuItem.Visible = true;
                        dataEntryToolStripMenuItem.Visible = false;
                        dataEntryToolStripMenuItem.Enabled = false;
                        batchUploadToolStripMenuItem.Visible = false;
                        batchUploadToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Visible = true;
                        imageImportToolStripMenuItem.Enabled = true;
                        imageQualityControlToolStripMenuItem.Visible = false;
                        imageQualityControlToolStripMenuItem.Enabled = false;
                        qualityControlFinalToolStripMenuItem.Visible = false;
                        qualityControlFinalToolStripMenuItem.Enabled = false;
                        toolStripMenuItem1.Enabled = false;
                        toolStripMenuItem1.Visible = false;
                        exportToolStripMenuItem.Enabled = false;
                        exportToolStripMenuItem.Visible = false;


                        toolsToolStripMenuItem.Enabled = true;
                        toolsToolStripMenuItem.Visible = true;
                        configurationToolStripMenuItem.Visible = false;
                        newPasswordToolStripMenuItem.Visible = true;
                        newUserToolStripMenuItem.Visible = false;
                        onlineUsersToolStripMenuItem.Visible = false;
                        officeNameConfigurationToolStripMenuItem.Visible = false;

                        toolStrip1.Visible = false;
                        toolStripButton1.Visible = false;
                        toolStripButton3.Visible = false;
                        toolStripButton2.Visible = false;
                        toolStripButton4.Visible = false;

                        configurationToolStripMenuItem.Visible = false;

                        helpToolStripMenuItem.Visible = true;

                        aboutToolStripMenuItem.Visible = true;

                        auditToolStripMenuItem.Visible = false;
                        partIIToolStripMenuItem.Visible = false;
                        partIToolStripMenuItem.Visible = false;
                        logoutToolStripMenuItem.Visible = true;

                        reportToolStripMenuItem.Visible = true;
                        dashboardToolStripMenuItem.Visible = true;
                    }
                    else if (crd.role == "QC")
                    {
                        menuStrip1.Visible = true;
                        newToolStripMenuItem.Visible = false;
                        newToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Visible = false;
                        batchToolStripMenuItem.Enabled = false;
                        batchToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Enabled = false;

                        transactionsToolStripMenuItem.Visible = true;
                        dataEntryToolStripMenuItem.Visible = false;
                        dataEntryToolStripMenuItem.Enabled = false;
                        batchUploadToolStripMenuItem.Visible = false;
                        batchUploadToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Enabled = false;
                        imageQualityControlToolStripMenuItem.Visible = true;
                        imageQualityControlToolStripMenuItem.Enabled = true;
                        qualityControlFinalToolStripMenuItem.Visible = false;
                        qualityControlFinalToolStripMenuItem.Enabled = false;
                        toolStripMenuItem1.Enabled = false;
                        toolStripMenuItem1.Visible = false;
                        exportToolStripMenuItem.Enabled = false;
                        exportToolStripMenuItem.Visible = false;


                        toolsToolStripMenuItem.Enabled = true;
                        toolsToolStripMenuItem.Visible = true;
                        configurationToolStripMenuItem.Visible = false;
                        newPasswordToolStripMenuItem.Visible = true;
                        newUserToolStripMenuItem.Visible = false;
                        onlineUsersToolStripMenuItem.Visible = false;
                        officeNameConfigurationToolStripMenuItem.Visible = false;

                        toolStrip1.Visible = true;
                        toolStripButton1.Visible = false;
                        toolStripButton3.Visible = true;
                        toolStripButton2.Visible = false;
                        toolStripButton4.Visible = false;

                        configurationToolStripMenuItem.Visible = false;

                        helpToolStripMenuItem.Visible = true;

                        aboutToolStripMenuItem.Visible = true;

                        auditToolStripMenuItem.Visible = false;
                        partIIToolStripMenuItem.Visible = false;
                        partIToolStripMenuItem.Visible = false;
                        logoutToolStripMenuItem.Visible = true;

                        reportToolStripMenuItem.Visible = true;
                        dashboardToolStripMenuItem.Visible = true;
                    }
                    else if (crd.role == "Metadata Entry")
                    {
                        menuStrip1.Visible = true;
                        newToolStripMenuItem.Visible = false;
                        newToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Visible = false;
                        batchToolStripMenuItem.Enabled = false;
                        batchToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Enabled = false;

                        transactionsToolStripMenuItem.Visible = true;
                        dataEntryToolStripMenuItem.Visible = true;
                        dataEntryToolStripMenuItem.Enabled = true;
                        batchUploadToolStripMenuItem.Visible = false;
                        batchUploadToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Enabled = false;
                        imageQualityControlToolStripMenuItem.Visible = false;
                        imageQualityControlToolStripMenuItem.Enabled = false;
                        qualityControlFinalToolStripMenuItem.Visible = false;
                        qualityControlFinalToolStripMenuItem.Enabled = false;
                        toolStripMenuItem1.Enabled = false;
                        toolStripMenuItem1.Visible = false;
                        exportToolStripMenuItem.Enabled = false;
                        exportToolStripMenuItem.Visible = false;


                        toolsToolStripMenuItem.Enabled = true;
                        toolsToolStripMenuItem.Visible = true;
                        configurationToolStripMenuItem.Visible = false;
                        newPasswordToolStripMenuItem.Visible = true;
                        newUserToolStripMenuItem.Visible = false;
                        onlineUsersToolStripMenuItem.Visible = false;
                        officeNameConfigurationToolStripMenuItem.Visible = false;

                        toolStrip1.Visible = false;
                        toolStripButton1.Visible = false;
                        toolStripButton3.Visible = false;
                        toolStripButton2.Visible = false;
                        toolStripButton4.Visible = false;

                        configurationToolStripMenuItem.Visible = false;

                        helpToolStripMenuItem.Visible = true;

                        aboutToolStripMenuItem.Visible = true;

                        auditToolStripMenuItem.Visible = false;
                        partIIToolStripMenuItem.Visible = false;
                        partIToolStripMenuItem.Visible = false;
                        logoutToolStripMenuItem.Visible = true;

                        reportToolStripMenuItem.Visible = true;
                        dashboardToolStripMenuItem.Visible = true;
                    }
                    else if (crd.role == "Audit 1")
                    {
                        menuStrip1.Visible = true;
                        newToolStripMenuItem.Visible = false;
                        newToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Visible = false;
                        batchToolStripMenuItem.Enabled = false;
                        batchToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Enabled = false;

                        transactionsToolStripMenuItem.Visible = false;
                        dataEntryToolStripMenuItem.Visible = false;
                        dataEntryToolStripMenuItem.Enabled = false;
                        batchUploadToolStripMenuItem.Visible = false;
                        batchUploadToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Enabled = false;
                        imageQualityControlToolStripMenuItem.Visible = false;
                        imageQualityControlToolStripMenuItem.Enabled = false;
                        qualityControlFinalToolStripMenuItem.Visible = false;
                        qualityControlFinalToolStripMenuItem.Enabled = false;
                        toolStripMenuItem1.Enabled = false;
                        toolStripMenuItem1.Visible = false;
                        exportToolStripMenuItem.Enabled = false;
                        exportToolStripMenuItem.Visible = false;


                        toolsToolStripMenuItem.Enabled = true;
                        toolsToolStripMenuItem.Visible = true;
                        configurationToolStripMenuItem.Visible = false;
                        newPasswordToolStripMenuItem.Visible = true;
                        newUserToolStripMenuItem.Visible = false;
                        onlineUsersToolStripMenuItem.Visible = false;
                        officeNameConfigurationToolStripMenuItem.Visible = false;

                        toolStrip1.Visible = false;
                        toolStripButton1.Visible = false;
                        toolStripButton3.Visible = false;
                        toolStripButton2.Visible = false;
                        toolStripButton4.Visible = false;

                        configurationToolStripMenuItem.Visible = false;

                        helpToolStripMenuItem.Visible = true;

                        aboutToolStripMenuItem.Visible = true;

                        auditToolStripMenuItem.Visible = true;
                        partIIToolStripMenuItem.Visible = false;
                        partIToolStripMenuItem.Visible = true;
                        logoutToolStripMenuItem.Visible = true;

                        reportToolStripMenuItem.Visible = true;
                        dashboardToolStripMenuItem.Visible = true;
                    }
                    else if (crd.role == "Audit 2")
                    {
                        menuStrip1.Visible = true;
                        newToolStripMenuItem.Visible = false;
                        newToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Visible = false;
                        batchToolStripMenuItem.Enabled = false;
                        batchToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Enabled = false;

                        transactionsToolStripMenuItem.Visible = false;
                        dataEntryToolStripMenuItem.Visible = false;
                        dataEntryToolStripMenuItem.Enabled = false;
                        batchUploadToolStripMenuItem.Visible = false;
                        batchUploadToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Enabled = false;
                        imageQualityControlToolStripMenuItem.Visible = false;
                        imageQualityControlToolStripMenuItem.Enabled = false;
                        qualityControlFinalToolStripMenuItem.Visible = false;
                        qualityControlFinalToolStripMenuItem.Enabled = false;
                        toolStripMenuItem1.Enabled = false;
                        toolStripMenuItem1.Visible = false;
                        exportToolStripMenuItem.Enabled = false;
                        exportToolStripMenuItem.Visible = false;


                        toolsToolStripMenuItem.Enabled = true;
                        toolsToolStripMenuItem.Visible = true;
                        configurationToolStripMenuItem.Visible = false;
                        newPasswordToolStripMenuItem.Visible = true;
                        newUserToolStripMenuItem.Visible = false;
                        onlineUsersToolStripMenuItem.Visible = false;
                        officeNameConfigurationToolStripMenuItem.Visible = false;

                        toolStrip1.Visible = false;
                        toolStripButton1.Visible = false;
                        toolStripButton3.Visible = false;
                        toolStripButton2.Visible = false;
                        toolStripButton4.Visible = false;

                        configurationToolStripMenuItem.Visible = false;

                        helpToolStripMenuItem.Visible = true;

                        aboutToolStripMenuItem.Visible = true;

                        auditToolStripMenuItem.Visible = true;
                        partIIToolStripMenuItem.Visible = true;
                        partIToolStripMenuItem.Visible = false;

                        logoutToolStripMenuItem.Visible = true;

                        reportToolStripMenuItem.Visible = true;
                        dashboardToolStripMenuItem.Visible = true;
                    }
                    else if (crd.role == "Fqc")
                    {
                        menuStrip1.Visible = true;
                        newToolStripMenuItem.Visible = false;
                        newToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Enabled = false;
                        projectToolStripMenuItem.Visible = false;
                        batchToolStripMenuItem.Enabled = false;
                        batchToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Visible = false;
                        exitToolStripMenuItem.Enabled = false;

                        transactionsToolStripMenuItem.Visible = true;
                        dataEntryToolStripMenuItem.Visible = false;
                        dataEntryToolStripMenuItem.Enabled = false;
                        batchUploadToolStripMenuItem.Visible = false;
                        batchUploadToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Enabled = false;
                        bundleScanToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Visible = false;
                        imageImportToolStripMenuItem.Enabled = false;
                        imageQualityControlToolStripMenuItem.Visible = false;
                        imageQualityControlToolStripMenuItem.Enabled = false;
                        qualityControlFinalToolStripMenuItem.Visible = true;
                        qualityControlFinalToolStripMenuItem.Enabled = true;
                        toolStripMenuItem1.Enabled = false;
                        toolStripMenuItem1.Visible = false;
                        exportToolStripMenuItem.Enabled = false;
                        exportToolStripMenuItem.Visible = false;


                        toolsToolStripMenuItem.Enabled = true;
                        toolsToolStripMenuItem.Visible = true;
                        configurationToolStripMenuItem.Visible = false;
                        newPasswordToolStripMenuItem.Visible = true;
                        newUserToolStripMenuItem.Visible = false;
                        onlineUsersToolStripMenuItem.Visible = false;
                        officeNameConfigurationToolStripMenuItem.Visible = false;

                        toolStrip1.Visible = true;
                        toolStripButton1.Visible = false;
                        toolStripButton3.Visible = false;
                        toolStripButton2.Visible = false;
                        toolStripButton4.Visible = true;

                        configurationToolStripMenuItem.Visible = false;

                        helpToolStripMenuItem.Visible = true;

                        aboutToolStripMenuItem.Visible = true;

                        auditToolStripMenuItem.Visible = false;
                        partIIToolStripMenuItem.Visible = false;
                        partIToolStripMenuItem.Visible = false;
                        logoutToolStripMenuItem.Visible = true;

                        reportToolStripMenuItem.Visible = true;
                        dashboardToolStripMenuItem.Visible = true;
                    }
                    //else
                    //{
                    //    menuStrip1.Visible = true;
                    //    newToolStripMenuItem.Visible = false;
                    //    newToolStripMenuItem.Enabled = false;
                    //    projectToolStripMenuItem.Enabled = false;
                    //    projectToolStripMenuItem.Visible = false;
                    //    batchToolStripMenuItem.Enabled = false;
                    //    batchToolStripMenuItem.Visible = false;
                    //    exitToolStripMenuItem.Visible = false;
                    //    exitToolStripMenuItem.Enabled = false;

                    //    transactionsToolStripMenuItem.Visible = false;
                    //    dataEntryToolStripMenuItem.Visible = false;
                    //    dataEntryToolStripMenuItem.Enabled = false;
                    //    batchUploadToolStripMenuItem.Visible = false;
                    //    batchUploadToolStripMenuItem.Enabled = false;
                    //    bundleScanToolStripMenuItem.Enabled = false;
                    //    bundleScanToolStripMenuItem.Visible = false;
                    //    imageImportToolStripMenuItem.Visible = false;
                    //    imageImportToolStripMenuItem.Enabled = false;
                    //    imageQualityControlToolStripMenuItem.Visible = false;
                    //    imageQualityControlToolStripMenuItem.Enabled = false;
                    //    qualityControlFinalToolStripMenuItem.Visible = false;
                    //    qualityControlFinalToolStripMenuItem.Enabled = false;
                    //    toolStripMenuItem1.Enabled = false;
                    //    toolStripMenuItem1.Visible = false;
                    //    exportToolStripMenuItem.Enabled = false;
                    //    exportToolStripMenuItem.Visible = false;


                    //    toolsToolStripMenuItem.Enabled = true;
                    //    toolsToolStripMenuItem.Visible = true;
                    //    configurationToolStripMenuItem.Visible = false;
                    //    newPasswordToolStripMenuItem.Visible = true;
                    //    newUserToolStripMenuItem.Visible = false;
                    //    onlineUsersToolStripMenuItem.Visible = false;

                    //    toolStrip1.Visible = false;
                    //    toolStripButton1.Visible = false;
                    //    toolStripButton3.Visible = false;
                    //    toolStripButton2.Visible = false;
                    //    toolStripButton4.Visible = false;

                    //    configurationToolStripMenuItem.Visible = false;

                    //    helpToolStripMenuItem.Visible = true;

                    //    aboutToolStripMenuItem.Visible = true;

                    //    auditToolStripMenuItem.Visible = false;
                    //    partIIToolStripMenuItem.Visible = false;
                    //    partIToolStripMenuItem.Visible = false;


                    //    logoutToolStripMenuItem.Visible = true;
                    //}
                }
            }
            catch (DBConnectionException dbex)
            {
                //MessageBox.Show(dbex.Message, "Image Heaven", MessageBoxButtons.OK, MessageBoxIcon.Error);
                string err = dbex.Message;
                this.Close();
            }
        }
        void getData(ref NovaNet.Utils.Profile prmp)
        {
            int i;
            p = prmp;
            for (i = 1; i <= 2; i++)
            {
                if (rbc.authenticate(p.UserId, p.Password) == false)
                {
                    if (logincounter == 2)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        logincounter++;
                        GetChallenge ogc = new GetChallenge(getData);
                        ogc.ShowDialog(this);
                    }
                }
                else
                {
                    if (rbc.CheckUserIsLogged(p.UserId))
                    {

                        p = rbc.getProfile();
                        crd = rbc.getCredentials(p);
                        if (crd.role != ihConstants._ADMINISTRATOR_ROLE)
                        {
                            rbc.LockedUser(p.UserId, crd.created_dttm);
                        }
                        break;
                    }
                    else
                    {
                        p.UserId = null;
                        p.UserName = null;
                        GetChallenge ogc = new GetChallenge(getData);
                        AssemblyName assemName = Assembly.GetExecutingAssembly().GetName();
                        this.Text = "B'Zer - Kolkata Police" + "           Version: " + assemName.Version.ToString() + "    Database name: " + sqlCon.Database.ToString() + "    Logged in user: " + crd.userName;
                        ogc.ShowDialog(this);
                    }
                }
            }
        }
        void getCPwd(ref NovaNet.Utils.Profile prmpwd)
        {
            p = prmpwd;
            rbc.changePassword(p.UserId, p.UserName, p.Password);
        }

        private void projectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                frmProject dispProject;
                wi = new wfeProject(sqlCon);
                dispProject = new frmProject(wi, sqlCon, crd);
                dispProject.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
