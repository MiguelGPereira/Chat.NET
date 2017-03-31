using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;

public partial class OnlineList : Form
{
    IServer server;
    List<ClientObj> clientsOnline;
    ClientInstance self;

    AlterEventRepeater evRepeater;
    delegate void LBAddDelegate(ClientInstance clientInst);
    delegate void LBRemoveDelegate(ClientInstance clientInst);

    ChatEventRepeater chEvRepeater;
    delegate void ChatViewStartDelegate(ClientInstance clientInst);

    ChatView viewChat;

    public OnlineList(IServer serv, ClientInstance cI)
    {
        server = serv;
        self = cI;
        clientsOnline = server.GetClientsOnline();

        InitializeComponent();

        evRepeater = new AlterEventRepeater();
        evRepeater.alterEvent += new AlterDelegate(ClientChange);
        server.alterEvent += new AlterDelegate(evRepeater.Repeater);

        chEvRepeater = new ChatEventRepeater();
        chEvRepeater.chatEvent += new ChatDelegate(ChatRequested);
        server.chatEvent += new ChatDelegate(chEvRepeater.Repeater);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        string client = (string)listBox1.SelectedItem;
        if (client != null)
        {
            ClientObj destinationClient = null;
            clientsOnline = server.GetClientsOnline();
            foreach (ClientObj clientObj in clientsOnline)
            {
                if (client.Equals(clientObj.Name))
                {
                    destinationClient = clientObj;
                }
            }
            if (destinationClient != null)
            {
                string destination = destinationClient.Port;
                //server.CreateNewChatRequest(self, destination);
                server.CreateNewChatRequest(self, destinationClient);
                Client.inter.ConnectChat((Chat)RemotingServices.Connect(typeof(Chat),
                            "tcp://localhost:" + Client.port + "/Client/Chat"
                            ));
                //Client.inter.newMessage += Client.handleNewChatMessage;
                /*
                 *Fecha form atual e abre novo
                 */
                //this.Hide();
                var chatView = new ChatView(server, self, destinationClient.Name);
                //onlineList.Closed += (s, args) => this.Close();
                viewChat = chatView;
                chatView.Show();
            }
        }
    }

    private void OnlineList_Load(object sender, EventArgs e)
    {
        listBox1.Items.Clear();
        foreach (ClientObj client in clientsOnline)
        {
            if (!client.Name.Equals(self.Name))
            {
                listBox1.Items.Add(client.Name);
            }
        }
        this.Text = self.Name + "'s Online List";
    }

    /*
     * Recebe o registo de  clientes
     */
    void ClientChange(Operation op, ClientInstance clientInst)
    {
        LBAddDelegate lbAdd;
        LBRemoveDelegate lbRemove;

        switch (op)
        {
            case Operation.ClientOn:
                lbAdd = new LBAddDelegate(NewClient);
                BeginInvoke(lbAdd, clientInst);
                break;

            case Operation.ClientOff:
                if (clientInst != null)
                {
                    lbRemove = new LBRemoveDelegate(RemoveClient);
                    BeginInvoke(lbRemove, clientInst);
                }
                break;
        }
    }

    private void NewClient(ClientInstance clientInst)
    {
        if (!clientInst.Name.Equals(self.Name))
        {
            listBox1.Items.Add(clientInst.Name);
        }
        clientsOnline = server.GetClientsOnline();
    }

    private void RemoveClient(ClientInstance clientInst)
    {
        listBox1.Items.Remove(clientInst.Name);
        clientsOnline = server.GetClientsOnline();
    }

    void ChatRequested(Operation op, ClientInstance clientInst, ClientObj clientDestination)
    {
        ChatViewStartDelegate chViStart;
        switch (op)
        {
            case Operation.NewChat:
                if (!clientInst.Name.Equals(self.Name) && clientDestination.Name.Equals(self.Name))
                {
                    chViStart = new ChatViewStartDelegate(OpenChatViewOnRequest);
                    BeginInvoke(chViStart, clientInst);
                }
                break;
        }
    }

    private void OpenChatViewOnRequest(ClientInstance clientInst)
    {
        Client.inter.ConnectChat((Chat)RemotingServices.Connect(typeof(Chat),
                            "tcp://localhost:" + clientInst.Address + "/Client/Chat"
                            ));
        /*
         *Fecha form atual e abre novo
         */
        //this.Hide();
        var chatView = new ChatView(server, self, clientInst.Name);
        viewChat = chatView;
        //onlineList.Closed += (s, args) => this.Close();
        chatView.Show();
    }

    private void OnlineList_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (viewChat != null)
        {
            viewChat.OustideOrderToClose();
            viewChat.Close();
        }
        server.ClientLogout(self);
    }
}

