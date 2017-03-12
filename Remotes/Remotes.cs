using System;
using System.Threading;

public class Server : MarshalByRefObject, IServer
{
    private int nr = 1;
    string[] names = { "Peter", "John", "George", "Mary", "Michael", "Anthony" };

    public event HandlerNotify newClientEvent;

    public override object InitializeLifetimeService()
    {
        Console.WriteLine("[Entities]: InitilizeLifetimeService");
        return null;
    }

    public ClientInstance AddNewClient()
    {
        ClientInstance client = new ClientInstance(nr, names[(nr - 1) % names.Length]);
        Console.WriteLine("[Server]: New client joined (" + nr.ToString() + ")");
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
                        handler(client);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("[TriggerEvent]: Exception");
                        newClientEvent -= handler;
                    }
                }).Start();
            }
        }
        return client;
    }
}
