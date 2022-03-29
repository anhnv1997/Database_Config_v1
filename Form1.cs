using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using SQLConns;
using FileXMLs;
using CryptorEngines;

namespace ConnectionConfig
{
    public partial class Form1 : Form
    {
        SQLConn[] sqls = null;
        SqlConnection connection;
        public Form1()
        {
            InitializeComponent();
            try
            {
                if (File.Exists(Application.StartupPath + "\\SQLConn.xml"))
                {
                    FileXML.ReadXMLSQLConn(Application.StartupPath + "\\SQLConn.xml", ref sqls);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("frmConnectionConfig: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtServerName.Text == "" || cbDatabaseName.Text == "" || txtLogin.Text == "")
            {
                MessageBox.Show("Các trường dữ liệu không được trống", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            List<SQLConn> sqlconnlist = new List<SQLConn>();
            SQLConn sqlconn = null;
            sqlconn = new SQLConn("", txtServerName.Text, cbDatabaseName.Text, txtLogin.Text, CryptorEngine.Encrypt(txtPassword.Text, true), cbAuthentication.Text);
            sqlconnlist.Add(sqlconn);
            sqls = new SQLConn[sqlconnlist.Count];
            sqlconnlist.CopyTo(sqls);
            FileXML.WriteXMLSQLConn(Application.StartupPath + @"\SQLConn.xml", sqls);
            MessageBox.Show("Save Config Success", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCheckConnection_Click(object sender, EventArgs e)
        {
            connection = new SqlConnection(GetConnectStr(txtServerName.Text,"master",cbAuthentication.Text,txtLogin.Text,txtPassword.Text));
            try
            {
                connection.Open();
                cbDatabaseName.Items.Clear();
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT name from sys.databases";
                SqlDataReader dataReader = cmd.ExecuteReader();
                cbDatabaseName.Items.Clear();
                cbDatabaseName.Items.Clear();
                while (dataReader.Read())
                {
                    cbDatabaseName.Items.Add(dataReader["name"].ToString());
                }
                cbDatabaseName.SelectedIndex = 0;
                btnSave.Enabled = true;
                MessageBox.Show("Connect Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connect to SQL SERVER FALSE: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                if (sqls != null && sqls.Length > 0)
                {
                    cbDatabaseName.Items.Add(sqls[0].SQLDatabase);
                    txtServerName.Text = sqls[0].SQLServerName;
                    cbDatabaseName.Text = sqls[0].SQLDatabase;
                    txtLogin.Text = sqls[0].SQLUserName;
                    txtPassword.Text = CryptorEngine.Decrypt(sqls[0].SQLPassword, true);
                    cbAuthentication.Text = sqls[0].SQLAuthentication;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("frmConnectionConfig_Load: " + ex.Message);
            }
        }

        private static bool IsServerConnected(SqlConnection connection, string connectionString)
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private string GetConnectStr(string serverName, string databaseName, string authentication, string username, string password)
        {
            if (authentication == "Window Authentication")
            {
                return $"data source={serverName};initial Catalog ={databaseName}; Integrated Security=True";

            }
            return $"data source={serverName};initial Catalog ={databaseName}; user id ={username};password={password}";
        }
    }
}
