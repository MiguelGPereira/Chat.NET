using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


public partial class ChatView : Form
{

    IServer server;
    ClientInstance self;
    string otherUser;

    MessageEventRepeater msgEvRepeater;
    delegate void ReceiveMessageDelegate(string message);

    ChatClosedEventRepeater chatClosEvRepeater;
    delegate void ClosedChatDelegate();

    //is this chat still active?
    bool active = false;

    public ChatView(IServer serv, ClientInstance cI, string _otherUser)
    {
        server = serv;
        self = cI;
        otherUser = _otherUser;
        InitializeComponent();
        Client.inter.newMessage += Client.handleNewChatMessage;
        msgEvRepeater = new MessageEventRepeater();
        msgEvRepeater.messageEvent += new MessageDelegate(MessageReceived);
        server.messageEvent += new MessageDelegate(msgEvRepeater.Repeater);
        chatClosEvRepeater = new ChatClosedEventRepeater();
        chatClosEvRepeater.chatClosedEvent += new ChatClosedDelegate(UserLoggedOut);
        server.chatClosedEvent += new ChatClosedDelegate(chatClosEvRepeater.Repeater);
        active = true;
    }

    private void ChatView_Load(object sender, EventArgs e)
    {
        this.Text = "Talking with " + otherUser;
    }

    private void button1_Click(object sender, EventArgs e)
    {
        string textToSend = richTextBox2.Text;
        richTextBox2.Clear();
        richTextBox1.Text += "\n" + self.Name + " said: " + textToSend;
        Client.inter.chat.addMessage(self, textToSend);
        server.MessageNotification(Operation.NewMessage, textToSend, otherUser);
    }

    void MessageReceived(Operation op, string message, string destinationName)
    {
        ReceiveMessageDelegate msgRec;

        switch (op)
        {
            case Operation.NewMessage:
                if (self.Name.Equals(destinationName) && active)
                {
                    msgRec = new ReceiveMessageDelegate(NewMessage);
                    BeginInvoke(msgRec, message);
                }
                break;
        }
    }

    private void NewMessage(string message)
    {
        string textToAdd = otherUser + " said: " + message;
        richTextBox1.Text += "\n" + textToAdd;
    }

    private void UserLoggedOut(Operation op, string destinationName)
    {
        ClosedChatDelegate closedChat;

        switch (op)
        {
            case Operation.ChatClosed:
                if (self.Name.Equals(destinationName) && active)
                {
                    closedChat = new ClosedChatDelegate(OtherUserLoggedOutActions);
                    BeginInvoke(closedChat);
                }
                break;
        }
    }

    void OtherUserLoggedOutActions()
    {
        richTextBox1.Text += "\n" + otherUser + " has ended this conversation";
        button1.Enabled = false;
        active = false;
    }

    private void ChatView_FormClosing(object sender, FormClosingEventArgs e)
    {
        server.ChatClosedNotification(Operation.ChatClosed, otherUser);
        Client.inter.newMessage -= Client.handleNewChatMessage;
        msgEvRepeater.messageEvent -= new MessageDelegate(MessageReceived);
        server.messageEvent -= new MessageDelegate(msgEvRepeater.Repeater);
        chatClosEvRepeater.chatClosedEvent -= new ChatClosedDelegate(UserLoggedOut);
        server.chatClosedEvent -= new ChatClosedDelegate(chatClosEvRepeater.Repeater);
    }

    public void OustideOrderToClose()
    {
        button1.Enabled = false;
        active = false;
        server.ChatClosedNotification(Operation.ChatClosed, otherUser);
        Client.inter.newMessage -= Client.handleNewChatMessage;
        msgEvRepeater.messageEvent -= new MessageDelegate(MessageReceived);
        server.messageEvent -= new MessageDelegate(msgEvRepeater.Repeater);
        chatClosEvRepeater.chatClosedEvent -= new ChatClosedDelegate(UserLoggedOut);
        server.chatClosedEvent -= new ChatClosedDelegate(chatClosEvRepeater.Repeater);
    }
}
