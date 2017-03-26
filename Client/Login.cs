using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Login : Form
{
    IServer server;
    ClientInstance self;

    public Login(IServer serv)
    {
        server = serv;
        InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        string name = textBox1.Text;
        string password = textBox2.Text;

        if (name != null && password != null)
        {
            string port = Client.getClientTCPAddressPort();
            Console.WriteLine("[Client]Port: " + port);
            /*
             * Pede ao servidor que lhe crie uma instancia
             */
            self = server.AddNewClient(name, password, port);
            if (self != null)
            {
                Console.WriteLine("[Client]: Joined! (Id=" + self.Id.ToString() + ", Name=" + self.Name + ")");
                Client.port = port;
                Client.name = name;
                Client.self = self;

                /*
                 *Fecha form atual e abre novo
                 */
                this.Hide();
                var onlineList = new OnlineList(server, self);
                onlineList.Closed += (s, args) => this.Close();
                onlineList.Show();
            }else
            {
                label3.Visible = true;
            }
        }
    }
}
