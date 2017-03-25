using System;
using System.Collections.Generic;

public delegate void NewClientHandler(ClientInstance newClient, List<ClientObj> clients);
public delegate void ChatRequestHandler(ClientInstance clientInst, string destination);

public delegate void NewMessageHandler(ClientInstance source, string message);

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
    ClientInstance AddNewClient(string name, string password, string port);
    bool CreateNewChatRequest(ClientInstance clientInst, string destination);
}


/*
 * Classe que serve para intermediar a passagem de eventos a clientes
 */
public class Intermediate : MarshalByRefObject
{
    public event NewClientHandler newClientEvent;
    public event ChatRequestHandler chatReqEvent;

    public event NewMessageHandler newMessage;

    public Chat chat;

    public Intermediate(IServer server)
    {
        server.newClientEvent += FireNewClient;
        server.chatReqEvent += FireChatRequest;
    }

    public void ConnectChat(Chat chat)
    {
        this.chat = chat;
        this.chat.NewMessage += FireNewMessage;
    }

    public void FireNewMessage(ClientInstance client, string message)
    {
        newMessage(client, message);
    }

    public void FireNewClient(ClientInstance client, List<ClientObj> clients)
    {
        newClientEvent(client, clients);
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

/*
 * Representa o objecto remoto do chat usado
 * na interação cliente-cliente
 */
public class Chat : MarshalByRefObject
{
    //public delegate void NewMessageHandler(ClientInstance source, string message);
    public event NewMessageHandler NewMessage;

    public Chat() { }

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

/*
 * classe auxiliar para gerir a List de clientes 
 */
[Serializable]
public class ClientObj
{
    public ClientObj(string name, string password, string port)
    {
        Name = name;
        Password = password;
        Port = port;
    }
    public string Name { get; set; }
    public string Password { get; set; }
    public string Port { get; set; }

    public string Info()
    {
        return Name + "%" + Password + "%" + Port;
    }
}