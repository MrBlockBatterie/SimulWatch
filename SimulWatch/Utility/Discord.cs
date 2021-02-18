using System;
using DiscordRPC;
using DiscordRPC.Logging;

namespace SimulWatch.Utility
{
    public class Discord
    {
        public Discord()
        {
            /*
	Create a Discord client
	NOTE: 	If you are using Unity3D, you must use the full constructor and define
			 the pipe connection.
	*/
            var client = new DiscordRpcClient("my_client_id");			
	
            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };
		
            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };
	
            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = "Example Project",
                State = "csharp example",
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "Lachee's Discord IPC Library",
                    SmallImageKey = "image_small"
                }
            });	
        }
    }
}