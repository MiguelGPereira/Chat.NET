using System;

public delegate void NewClientHandler(ClientInstance newClient);
public delegate void ChatRequestHandler(ClientInstance clientInst, string destination);

[Serializable]
public class ClientInstance
{
    public ClientInstance(int id, string name, string address)
    {
        Id = id;
        Name = name;
        Address = address;
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}

public interface IServer
{
    event NewClientHandler newClientEvent;
    event ChatRequestHandler chatReqEvent;
    ClientInstance AddNewClient(string name, string password, string address);
    bool CreateNewChatRequest(ClientInstance clientInst, string destination);
}

public class Intermediate : MarshalByRefObject
{
    public event NewClientHandler newClientEvent;
    public event ChatRequestHandler chatReqEvent;

    public Intermediate(IServer server)
    {
        server.newClientEvent += FireNewClient;
        server.chatReqEvent += FireChatRequest;
    }

    public void FireNewClient(ClientInstance client)
    {
        newClientEvent(client);
    }

    public void FireChatRequest(ClientInstance client, string destination)
    {
        chatReqEvent(client, destination);
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }
}
