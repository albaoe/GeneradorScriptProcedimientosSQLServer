using System;
using System.Windows.Forms;

namespace GeneradorScriptPaquetesOracle
{
    public partial class AddConnectionForm : Form
    {
        public AddConnectionForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string alias = "";
            string connectionString = "";
            string host = "";
            string serviceName = "";
            string userId = "";
            string password = "";
            string cypherConnection = "";
                        
            alias = aliasBox.Text;
            host = hostBox.Text;
            serviceName = serviceBox.Text;
            userId = userBox.Text;
            password = passwordBox.Text;

            connectionString = "Data Source="+host+";Initial Catalog="+userId+";User Id="+userId+";Password="+password+";Persist Security Info=true;Connect Timeout=3000;";

            cypherConnection = SecureIt.EncryptString(SecureIt.ToSecureString(connectionString));

            if (!Properties.ConnectionStrings.Default.Alias.Contains(alias))
            {
                Properties.ConnectionStrings.Default.ConnectionString.Add(cypherConnection);
                Properties.ConnectionStrings.Default.Alias.Add(alias);
                Properties.ConnectionStrings.Default.Owners.Add(userId);
                Properties.ConnectionStrings.Default.Save();
                this.Close();
            }

            else {
                MessageBox.Show("Ese alias ya existe. Por favor, elija otro alias o elimine el existente", "Error al guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }
    }
}
