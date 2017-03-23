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
    string[] names = { "Peter", "John", "George", "Mary", "Michael", "Anthony" };
    List<Client> clients = new List<Client>();

    public event HandlerNotify newClientEvent;

    public override object InitializeLifetimeService()
    {
        //loadClients();
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
                string name = line.Split('/')[0];
                string password = line.Split('/')[1];

                Client client = new Client(name, password);
                clients.Add(client);

                line = sr.ReadLine();
            }
            sr.Close();
        }
        catch (System.IO.FileNotFoundException e) { }
        
    }

    public ClientInstance AddNewClient(string name, string password)
    {
        Client client = getClientByName(name);
        string userevent = "login";

        if(client == null)
        {
            client = new Client(name, password);
            saveClient(client);
            userevent = "signup";
        }
        else if(client.Password != password)
        {
            return null;
        }

        ClientInstance clientInst = new ClientInstance(nr, name);
        Console.WriteLine("[Server]: New client "+userevent+" (" + name + ")");
        nr += 1;

        if (newClientEvent != null)
        {
            Delegate[] invkList = newClientEvent.GetInvocationList();

            foreach (HandlerNotify handler in invkList)
            {
                Console.WriteLine("[Server]: Join Event triggered: invoking handler to inform client x");
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
}

class Client
{
    public Client(string name, string password)
    {
        Name = name;
        Password = password;
    }
    public string Name { get; set; }
    public string Password { get; set; }

    public string Info()
    {
        return Name + "/" + Password;
    }
}
