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

public enum Operation { ClientOn, ClientOff, NewChat, NewMessage, ChatClosed};

public delegate void AlterDelegate(Operation op, ClientInstance clientInst);
public delegate void ChatDelegate(Operation op, ClientInstance clientOrigin, ClientObj clientDestination);
public delegate void MessageDelegate(Operation op, string message, string destinationName);
public delegate void ChatClosedDelegate(Operation op, string destinationName);

public interface IServer
{
    event NewClientHandler newClientEvent;
    event ChatRequestHandler chatReqEvent;
    ClientInstance AddNewClient(string name, string password, string port);
    bool CreateNewChatRequest(ClientInstance clientInst, string destination);
    bool CreateNewChatRequest(ClientInstance clientInst, ClientObj clientDestination);
    List<ClientObj> GetClientsOnline();
    void MessageNotification(Operation op, string message, string destinationName);
    void ChatClosedNotification(Operation op, string destinationName);
    void ClientLogout(ClientInstance clientInst);

    event AlterDelegate alterEvent;
    event ChatDelegate chatEvent;
    event MessageDelegate messageEvent;
    event ChatClosedDelegate chatClosedEvent;
}


public class AlterEventRepeater : MarshalByRefObject
{
    public event AlterDelegate alterEvent;

    public override object InitializeLifetimeService()
    {
        return null;
    }

    public void Repeater(Operation op, ClientInstance clientInst)
    {
        if (alterEvent != null)
            alterEvent(op, clientInst);
    }
}

public class ChatEventRepeater : MarshalByRefObject
{
    public event ChatDelegate chatEvent;

    public override object InitializeLifetimeService()
    {
        return null;
    }

    public void Repeater(Operation op, ClientInstance clientInst, ClientObj clientDestination)
    {
        if (chatEvent != null)
            chatEvent(op, clientInst, clientDestination);
    }
}

public class MessageEventRepeater : MarshalByRefObject
{
    public MessageDelegate messageEvent;

    public override object InitializeLifetimeService()
    {
        return null;
    }

    public void Repeater(Operation op, string message, string destinationName)
    {
        if (messageEvent != null)
            messageEvent(op, message, destinationName);
    }
}

public class ChatClosedEventRepeater : MarshalByRefObject
{
    public ChatClosedDelegate chatClosedEvent;

    public override object InitializeLifetimeService()
    {
        return null;
    }

    public void Repeater(Operation op, string destinationName)
    {
        if (chatClosedEvent != null)
            chatClosedEvent(op, destinationName);
    }
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
        try
        {
            NewMessage(source, message);
        }catch(Exception e)
        {
            Console.WriteLine("Error with chat");
        }
        
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
