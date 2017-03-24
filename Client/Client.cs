using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;

class Client
{

    static string address = "";
    static string name = "";
    static ClientInstance instance = null;

    static void Main(string[] args)
    {
        RemotingConfiguration.Configure("Client.exe.config", false);
        IServer server = (IServer)RemoteNew.New(typeof(IServer));
        Intermediate inter = new Intermediate(server);
        inter.newClientEvent += OnNewClient;
        inter.chatReqEvent += OnNewChatRequest;

        ClientInstance self = null;
        while(self == null)
        {
            Console.WriteLine("[Client] Enter username:");
            string name = Console.ReadLine();
            Console.WriteLine("[Client] Enter password:");
            string password = Console.ReadLine();
            
            string address = getClientTCPAddressPort();
            Console.WriteLine("[Client]Port: "+ address);
            /*
             * Pede ao servidor que lhe crie uma instancia
             */
            self = server.AddNewClient(name, password, address);
            if(self != null)
            {
                Console.WriteLine("[Client]: Joined! (Id=" + self.Id.ToString() + ", Name=" + self.Name + ")");
                Client.address = address;
                Client.name = name;
                Client.instance = self;

                Console.WriteLine("Want to make a connection? (y/n)");
                string res = Console.ReadLine();
                if(res == "y")
                {
                    string destination = Console.ReadLine();
                    server.CreateNewChatRequest(self, destination);

                    Chat chat = (Chat)RemotingServices.Connect(typeof(Chat),
                    "tcp://localhost:" + Client.address + "/Client/Chat"
                    );
                    chat.NewMessage += handleNewChatMessage;
                    while (true)
                    {
                        Console.WriteLine("Write new message");
                        string message = Console.ReadLine();
                        string source = name;
                        chat.addMessage(self, message);
                    }
                }
                else
                {
                    while (true)
                    {
                        Console.ReadKey();
                    }
                }
                
            }
            else
            {
                Console.WriteLine("There is already a username with a different password");
            }
        }
        

        inter.newClientEvent -= OnNewClient;
        server.newClientEvent -= inter.FireNewClient;
        inter.chatReqEvent -= OnNewChatRequest;
        server.chatReqEvent -= inter.FireChatRequest;
    }

    /*
     * Recebe o registo de novos clientes
     */
    static void OnNewClient(ClientInstance client)
    {
        Console.WriteLine("[Client Joined]: " + client.Name + " in: " + client.Address);
    }


    /*
     * Recebe o pedido para chat
     */
    static void OnNewChatRequest(ClientInstance client, string destination)
    {
        Console.WriteLine("[Client Chat Request]: "+ "broadcast received");
        if(destination == Client.address)
        {
            Console.WriteLine("[Client Chat Request]: Client '"+client.Name+"' requested a chat!");
            Chat chat = (Chat)RemotingServices.Connect(typeof(Chat),
            "tcp://localhost:"+client.Address+"/Client/Chat"
            );
            chat.NewMessage += handleNewChatMessage;
            while (true)
            {
                Console.WriteLine("Write new message");
                string message = Console.ReadLine();
                string source = Client.name;
                chat.addMessage(Client.instance, message);
            }
            
        }
    }

    /*
     * Imprime mensagens recebidas no chat por quem recebe o chat request
     * (quem inicia interage atraves da classe Chat)
     */
     static void handleNewChatMessage(ClientInstance source, string message)
    {
        Console.WriteLine("Message from '" + source.Name + "': " + message);
    }

    /*
     * Usa o comando netstat (precisa de estar ativado nas funcionalidades extras do windows)
     * para obter a porta do processo tcp corrente
     */
    static string getClientTCPAddressPort()
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

/*
 * Representa o objecto remoto e parte daqui a interação do cliente que faz o chat request
 */
public class Chat : MarshalByRefObject
{
    public delegate void NewMessageHandler(ClientInstance source, string message);
    public event NewMessageHandler NewMessage;

    public Chat() {}

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Entities]: InitilizeLifetimeService");
        return null;
    }

    public void addMessage(ClientInstance source, string message)
    {
        NewMessage(source, message);
    }
}
