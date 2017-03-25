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
    List<ClientObj> clients = new List<ClientObj>();
    List<ClientObj> clientsOnline = new List<ClientObj>();

    public event NewClientHandler newClientEvent;
    public event ChatRequestHandler chatReqEvent;

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Entities]: InitilizeLifetimeService");
        return null;
    }

    private ClientObj getClientByName(string name, string port)
    {
        foreach (ClientObj client in clients)
        {
            if (client.Name == name)
            {
                client.Port = port;
                return client;
            }
        }

        return null;
    }

    private void saveClient(ClientObj client)
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
                string port = line.Split('%')[2];

                ClientObj client = new ClientObj(name, password, port);
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
    public ClientInstance AddNewClient(string name, string password, string port)
    {
        ClientObj client = getClientByName(name, port);
        string userevent = "login";

        if (client == null)
        {
            client = new ClientObj(name, password, port);
            saveClient(client);
            userevent = "signup";
        }
        else if (client.Password != password)
        {
            return null;
        }

        ClientInstance clientInst = new ClientInstance(nr, name, port);
        Console.WriteLine("[Server]: New client " + userevent + " (" + name + ")");
        clientsOnline.Add(client);
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
                        handler(clientInst, clientsOnline);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[TriggerEvent]: Exception");
                        Console.WriteLine(e.Message);
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
}



