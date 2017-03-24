using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;

class Client
{

    static string address = "";

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
            
            string address = getClientTCPAddress();
            Console.WriteLine("port->"+ address);
            /*
             * Pede ao servidor que lhe crie uma instancia
             */
            self = server.AddNewClient(name, password, address);
            if(self != null)
            {
                Console.WriteLine("[Client]: Joined! (Id=" + self.Id.ToString() + ", Name=" + self.Name + ")");
                Client.address = address;
                string destination = Console.ReadLine();
                server.CreateNewChatRequest(self, destination);
                Console.ReadLine();
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

    static void OnNewChatRequest(ClientInstance client, string destination)
    {
        Console.WriteLine("[Client Chat Request]: "+ "broadcast received");
        if(destination == Client.address)
        {
            Console.WriteLine("[Client Chat Request]: Client '"+client.Name+"' requested a chat!");
        }
    }

    /*
     * Usa o comando netstat (precisa de estar ativado nas funcionalidades extras do windows)
     * para obter a porta do processo tcp corrente
     */
    static string getClientTCPAddress()
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
                //string port_number = localAddress.Split(':')[1];
                int pid = Convert.ToInt16(tokens[5]);
                if (pid == clientProcessID)
                {
                    return localAddress;
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
