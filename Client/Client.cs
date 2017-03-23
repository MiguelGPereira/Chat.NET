using System;
using System.Collections;
using System.Runtime.Remoting;

class Client
{
    static void Main(string[] args)
    {
        RemotingConfiguration.Configure("Client.exe.config", false);
        IServer server = (IServer)RemoteNew.New(typeof(IServer));
        Intermediate inter = new Intermediate(server);
        inter.newClientEvent += OnNewClient;

        ClientInstance self = null;
        while(self == null)
        {
            Console.WriteLine("[Client] Enter username:");
            string name = Console.ReadLine();
            Console.WriteLine("[Client] Enter password:");
            string password = Console.ReadLine();
            self = server.AddNewClient(name, password);
            if(self != null)
            {
                Console.WriteLine("[Client]: Joined! Data is (Id=" + self.Id.ToString() + ", Name=" + self.Name + ")");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("There is already a username with a different password");
            }
        }
        

        inter.newClientEvent -= OnNewClient;
        server.newClientEvent -= inter.FireNewClient;
    }

    static void OnNewClient(ClientInstance client)
    {
        Console.WriteLine("[Client Joined]: Event handler called for " + client.Name + " Id: " + client.Id.ToString());
    }
}

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
