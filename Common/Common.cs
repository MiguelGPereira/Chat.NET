using System;

public delegate void HandlerNotify(ClientInstance newClientEvent);

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
    event HandlerNotify newClientEvent;
    ClientInstance AddNewClient(string name, string password, string address);
}

public class Intermediate : MarshalByRefObject
{
    public event HandlerNotify newClientEvent;

    public Intermediate(IServer server)
    {
        server.newClientEvent += FireNewClient;
    }

    public void FireNewClient(ClientInstance client)
    {
        newClientEvent(client);
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }
}
public interface IChat
{
    event HandlerNotify newClientEvent;
}