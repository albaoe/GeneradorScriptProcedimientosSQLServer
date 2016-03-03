using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace GeneradorScriptProcedimientosSQLServer
{
    public partial class MainForm : Form
    {
        string sqldb = "";
        ConfigForm m;
        StreamWriter file;

        public MainForm()
        {
            InitializeComponent();
            fillEnvironments();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            
            SqlConnection conn = new SqlConnection(null);
            string rutaFichero = "";
            int totalLines = 0;

            string[] proceduresNames = new string[checkedListBox1.CheckedItems.Count]; //Nombre de los paquetes seleccionados

            if (textBox2.Text == "")
                rutaFichero = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".sql";    //Ruta del fichero si no se ha introducido una ruta (la carpeta donde se ejecuta)
            else
                rutaFichero = textBox2.Text + "_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".sql";     //Ruta del fichero si se ha introdudico una ruta

            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                proceduresNames[i] = checkedListBox1.CheckedItems[i].ToString();    //Cogemos los nombres de los procedimientos seleccionados
            }

            file = new StreamWriter(rutaFichero, true);     //Abrimos el fichero
            file.NewLine = "\r";

            for (int i = 0; i < proceduresNames.Length; i++)
            {

                conn = new SqlConnection(sqldb);    //Conectamos a la base de datos con la cadena de conexion

                totalLines = proceduresNames.Length;

                label1.Text = "";
                progressBar1.Value = 0;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = totalLines;

                try
                {
                    conn.Open();    //Abrimos la conexión                    

                    SqlCommand cmd = new SqlCommand();    //Comando que mandaremos a la base de datos

                    cmd.Connection = conn;

                    file.WriteLine("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = '" + proceduresNames[i] + "')");
                    file.WriteLine("DROP PROCEDURE " + proceduresNames[i]);    //Escribimos el drop
                    file.WriteLine("GO");

                    cmd.CommandText = "select OBJECT_NAME(OBJECT_ID), definition from sys.sql_modules where objectproperty(OBJECT_ID, 'IsProcedure') = 1 and OBJECT_NAME(OBJECT_ID) = '" + proceduresNames[i] + "'";
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader dr = cmd.ExecuteReader();      //Resultados de la query

                    while (dr.Read())
                    {
                        if (progressBar1.Value < totalLines)
                            progressBar1.Increment(1);
                        label1.Text = proceduresNames[i] + " -> Escribiendo procedimiento...";
                        await file.WriteAsync(dr.GetString(1).Replace("[dbo].",""));   //Escribimos en el fichero el contenido del procedimiento
                    }

                    file.WriteLine(Environment.NewLine);

                }
                catch (Exception ex)
                {
                    label1.Text = ex.Message;
                }
            }

            file.Flush();
            file.Dispose();         //Cerramos el fichero

            conn.Dispose();         //Cerramos la conexión

            progressBar1.Value = progressBar1.Maximum;

            label1.Text = "¡Completado!";

        }

        private void comboBox1_SelectionChangeCommited(object sender, EventArgs e)
        {
            string owner = "";
            int index = 0;
            string[] aliases = new string[Properties.ConnectionStrings.Default.Alias.Count];
            string[] connections = new string[Properties.ConnectionStrings.Default.ConnectionString.Count];
            string[] owners = new string[Properties.ConnectionStrings.Default.Owners.Count];

            
            Properties.ConnectionStrings.Default.ConnectionString.CopyTo(connections, 0);
            Properties.ConnectionStrings.Default.Alias.CopyTo(aliases, 0);
            Properties.ConnectionStrings.Default.Owners.CopyTo(owners, 0);
            
            for(int i=0; i < aliases.Length; i++)
            {
                if (aliases[i] == comboBox1.Text)
                    index = i;
            }

            sqldb = SecureIt.ToInsecureString(SecureIt.DecryptString(connections[index]));         //Obtenemos la cadena de conexión y el owner correspondientes
            owner = owners[index];             

            SqlConnection conn = new SqlConnection(sqldb);    //Conectamos a la base de datos con la cadena de carga

            try
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand();

                cmd.Connection = conn;

                cmd.CommandText = "select Routine_Name from "+owner+".information_schema.routines where routine_type = 'PROCEDURE' and Left(Routine_Name, 3) NOT IN ('sp_', 'xp_', 'ms_') and routine_name like 'P\\_%' ESCAPE '\\' order by routine_name";     //Obtenemos el nombre de los paquetes de la base de datos
                cmd.CommandType = CommandType.Text;

                SqlDataReader dr = cmd.ExecuteReader();

                checkedListBox1.Items.Clear();

                while (dr.Read())
                {
                    checkedListBox1.Items.Add(dr.GetString(0));         //Añadimos los elementos a la lista              
                }

                checkedListBox1.Update();

                conn.Dispose();
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        private void configButton_Click(object sender, EventArgs e)
        {
            m = new ConfigForm(this);     
            m.Show();
        }

        public void fillEnvironments()
        {
            comboBox1.Items.Clear();
            comboBox1.BeginUpdate();
            if (Properties.ConnectionStrings.Default.Alias != null)
            {
                foreach (string alias in Properties.ConnectionStrings.Default.Alias)
                {
                    comboBox1.Items.Add(alias);
                }
            }
            comboBox1.EndUpdate();
            comboBox1.Refresh();
        }
        
    }
}
