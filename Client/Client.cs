using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Windows.Forms;

class Client
{

    static public string port = "";
    static public string name = "";
    static public ClientInstance self = null;
    static public Intermediate inter = null;

    //static void Main(string[] args)
    //{
        //    RemotingConfiguration.Configure("Client.exe.config", false);
        //    IServer server = (IServer)RemoteNew.New(typeof(IServer));
        //    inter = new Intermediate(server);
        //    inter.newClientEvent += OnNewClient;
        //    inter.chatReqEvent += OnNewChatRequest;

        [STAThread]
    static void Main()
    {
        RemotingConfiguration.Configure("Client.exe.config", false);
        IServer server = (IServer)RemoteNew.New(typeof(IServer));
        inter = new Intermediate(server);
        inter.newClientEvent += OnNewClient;
        inter.chatReqEvent += OnNewChatRequest;
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Login(server));


        //    while (self == null)
        //    {
        //        Console.WriteLine("[Client] Enter username:");
        //        name = Console.ReadLine();
        //        Console.WriteLine("[Client] Enter password:");
        //        string password = Console.ReadLine();

        //        port = getClientTCPAddressPort();
        //        Console.WriteLine("[Client]Port: " + port);

        //        /*
        //         * Pede ao servidor que lhe crie uma instancia
        //         */
        //        self = server.AddNewClient(name, password, port);
        //        if (self != null)
        //        {
        //            Console.WriteLine("[Client]: Joined! (Id=" + self.Id.ToString() + ", Name=" + self.Name + ")");

        //            Console.WriteLine("Want to make a connection? (y/n)");
        //            string res = Console.ReadLine();
        //            if (res == "y")
        //            {
        //                string destination = Console.ReadLine();
        //                server.CreateNewChatRequest(self, destination);

        //                inter.ConnectChat((Chat)RemotingServices.Connect(typeof(Chat),
        //                    "tcp://localhost:" + Client.port + "/Client/Chat"
        //                    ));
        //                inter.newMessage += handleNewChatMessage;
        //                while (true)
        //                {
        //                    Console.WriteLine("Write new message");
        //                    string message = Console.ReadLine();
        //                    string source = name;
        //                    inter.chat.addMessage(self, message);
        //                }
        //                inter.newMessage -= handleNewChatMessage;
        //            }
        //            else
        //            {
        //                while (true)
        //                {
        //                    Console.ReadKey();
        //                }
        //            }

        //        }
        //        else
        //        {
        //            Console.WriteLine("There is already a username with a different password");
        //        }
        //    }


        inter.newClientEvent -= OnNewClient;
        server.newClientEvent -= inter.FireNewClient;
        inter.chatReqEvent -= OnNewChatRequest;
        server.chatReqEvent -= inter.FireChatRequest;
    }

    /*
     * Recebe o registo de novos clientes
     */
    static void OnNewClient(ClientInstance client, List<ClientObj> clients)
    {
        Console.WriteLine("[Client Joined]: Update clients list:");
        foreach (ClientObj c in clients)
        {
            Console.WriteLine("Client '" + c.Name + "' in port '" + c.Port + "'");
        }
    }


    /*
     * Recebe o pedido para chat
     */
    static void OnNewChatRequest(ClientInstance client, string destination)
    {
        Console.WriteLine("[Client Chat Request]: " + "broadcast received");
        //if (destination == Client.port)
        //{
        //    Console.WriteLine("[Client Chat Request]: Client '" + client.Name + "' requested a chat!");

        //    inter.ConnectChat((Chat)RemotingServices.Connect(typeof(Chat),
        //        "tcp://localhost:" + client.Address + "/Client/Chat"
        //        ));
        //    inter.newMessage += handleNewChatMessage;
        //    while (true)
        //    {
        //        Console.WriteLine("Write new message");
        //        string message = Console.ReadLine();
        //        string source = name;
        //        inter.chat.addMessage(Client.self, message);
        //    }
        //    inter.newMessage -= handleNewChatMessage;
        //}
    }

    /*
     * Imprime mensagens recebidas no chat
     */
    static public void handleNewChatMessage(ClientInstance source, string message)
    {
        Console.WriteLine("Message from '" + source.Name + "': " + message);
        
    }

    /*
     * Usa o comando netstat (precisa de estar ativado nas funcionalidades extras do windows)
     * para obter a porta do processo tcp corrente
     */
    static public string getClientTCPAddressPort()
    {
        int clientProcessID = Process.GetCurrentProcess().Id;

        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = "netstat.exe";
        p.StartInfo.Arguments = "-ano";
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        string[] rows = Regex.Split(output, "\r\n");
        foreach (string row in rows)
        {
            string[] tokens = Regex.Split(row, "\\s+");
            if (tokens.Length > 4 && (tokens[1].Equals("TCP")))
            {
                string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                string port_number = localAddress.Split(':')[1];
                int pid = Convert.ToInt16(tokens[5]);
                if (pid == clientProcessID)
                {
                    return port_number;
                }
            }
        }
        return null;
    }
}

/*
 * Cria ligacoes a objectos remotos
 */
class RemoteNew
{
    private static Hashtable types = null;

    private static void InitTypeTable()
    {
        types = new Hashtable();
        foreach (WellKnownClientTypeEntry entry in RemotingConfiguration.GetRegisteredWellKnownClientTypes())
            types.Add(entry.ObjectType, entry);
    }

    public static object New(Type type)
    {
        if (types == null)
            InitTypeTable();
        WellKnownClientTypeEntry entry = (WellKnownClientTypeEntry)types[type];
        if (entry == null)
            throw new RemotingException("Type not found!");
        return RemotingServices.Connect(type, entry.ObjectUrl);
    }
}


