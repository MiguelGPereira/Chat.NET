using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;

public class Server : MarshalByRefObject, IServer
{
    public Server()
    {
        loadClients();
    }

    private int nr = 1;
    List<Client> clients = new List<Client>();

    public event NewClientHandler newClientEvent;
    public event ChatRequestHandler chatReqEvent;

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Entities]: InitilizeLifetimeService");
        return null;
    }

    private Client getClientByName(string name)
    {
        foreach (Client client in clients)
        {
            if(client.Name == name)
            {
                return client;
            }
        }

        return null;
    }

    private void saveClient(Client client)
    {
        clients.Add(client);

        FileStream db = new FileStream(@"users.txt", FileMode.Append, FileAccess.Write, FileShare.None);
        StreamWriter sw = new StreamWriter(db);
        sw.WriteLine(client.Info());
        sw.Flush();
        sw.Close();
    }

    private void loadClients()
    {
        try
        {
            FileStream db = new FileStream(@"users.txt", FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader sr = new StreamReader(db);
            string line = sr.ReadLine();
            while (line != null)
            {
                string name = line.Split('%')[0];
                string password = line.Split('%')[1];
                string address = line.Split('%')[2];

                Client client = new Client(name, password, address);
                clients.Add(client);

                line = sr.ReadLine();
            }
            sr.Close();
        }
        catch (System.IO.FileNotFoundException e) { }
        
    }

    /*
     * Recebe um pedido de criacao de instancia de um novo cliente e informa
     * todos os clientes sobre isso
     */ 
    public ClientInstance AddNewClient(string name, string password, string address)
    {
        Client client = getClientByName(name);
        string userevent = "login";

        if(client == null)
        {
            client = new Client(name, password, address);
            saveClient(client);
            userevent = "signup";
        }
        else if(client.Password != password)
        {
            return null;
        }

        ClientInstance clientInst = new ClientInstance(nr, name, address);
        Console.WriteLine("[Server]: New client "+userevent+" (" + name + ")");
        nr += 1;

        if (newClientEvent != null)
        {
            Delegate[] invkList = newClientEvent.GetInvocationList();

            foreach (NewClientHandler handler in invkList)
            {
                Console.WriteLine("[Server]: Join Event triggered: invoking handler to inform client");
                new Thread(() =>
                {
                    try
                    {
                        handler(clientInst);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("[TriggerEvent]: Exception");
                        newClientEvent -= handler;
                    }
                }).Start();
            }
        }
        return clientInst;
    }

    public bool CreateNewChatRequest(ClientInstance clientInst, string destination)
    {
        if (chatReqEvent != null)
        {
            Delegate[] invkList = chatReqEvent.GetInvocationList();

            foreach (ChatRequestHandler handler in invkList)
            {
                Console.WriteLine("[Server]: Chat Event triggered: invoking handler to inform client");
                new Thread(() =>
                {
                    try
                    {
                        handler(clientInst, destination);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("[TriggerEvent]: Exception");
                        chatReqEvent -= handler;
                    }
                }).Start();
            }
        }
        return true;
    }

    /*public void CreateNewChatRequest(string name, string address)
    {
        if (newClientRequest != null)
        {
            Delegate[] invkList = newClientRequest.GetInvocationList();

            foreach (NewClientHandler handler in invkList)
            {
                Console.WriteLine("[Server]: invoking handler to inform client of request");
                new Thread(() =>
                {
                    try
                    {
                        handler(clientInst);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("[TriggerEvent]: Exception");
                        newClientEvent -= handler;
                    }
                }).Start();
            }
        }
    }
}*/

    class Client
{
    public Client(string name, string password, string address)
    {
        Name = name;
        Password = password;
        Address = address;
    }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Address { get; set; }

    public string Info()
    {
        return Name + "%" + Password + "%" + Address;
    }
}

public class Chat : MarshalByRefObject, IChat
{
    public event NewClientHandler newClientEvent;
    public Chat()
    {
        Console.WriteLine("chat room on");
    }

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Entities]: InitilizeLifetimeService");
        return null;
    }
}
}
