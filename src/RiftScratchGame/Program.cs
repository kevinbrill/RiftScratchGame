using System;
using System.Configuration;
using rift.net;
using System.Linq;
using RiftScratchGame.Notifications.Pushover;
using log4net;
using System.Timers;
using rift.net.Models;

namespace RiftScratchGame
{
	class MainClass
	{
		private static readonly ILog logger = LogManager.GetLogger (typeof(MainClass));

		public static void Main (string[] args)
		{
			log4net.Config.XmlConfigurator.Configure ();

			if ((args == null) || (args.Length == 0)) 
			{
				Console.WriteLine ("Usage: RiftScratchGame /u <username> /pa <password> /c <character name@shard name> /g <game> /pu <pushover key>");
				return;
			}

			// Clear the console.
			Console.Clear ();

			var arguments = Args.Configuration.Configure<CommandObject> ().CreateAndBind (args);

			var sessionFactory = new SessionFactory ();
			Session session;

			// Login
			try 
			{
				session = sessionFactory.Login (arguments.Username, arguments.Password);
			} 
			catch( AuthenticationException ex )
			{
				logger.Error (ex.Message);
				return;
			}
			catch (Exception ex) 
			{
				logger.Error ("An unexpected error occurred", ex);
				return;
			}

			// Create a new client
			var riftClient = new RiftClientSecured (session);
			var gameClient = new ScratchGameClient (session);
			PushOverClient pushOverClient = arguments.PushoverKey != null ? new PushOverClient (arguments.PushoverKey) : null;

			var characters = riftClient.ListCharacters ();
			var character = characters.FirstOrDefault (x => x.FullName == arguments.Character);

			if (character == null) {

				var characterList = string.Join(System.Environment.NewLine, characters.OrderBy(x=>x.Shard.Name).ThenBy(x=>x.Name).Select(x=>string.Format("\t{0}", x.FullName)));

				logger.ErrorFormat ("Unable to locate {0} in your list of characters.  Please use one of the following:", arguments.Character);
				logger.ErrorFormat (characterList);

				return;
			}

			var games = gameClient.ListGames ();
			var game = games.FirstOrDefault (x => x.Name == arguments.Game);

			if (game == null) {
				var currentGames = string.Join (System.Environment.NewLine, games.Select (x => string.Format("\t{0}", x.Name)));

				logger.ErrorFormat ("It doesn't look like '{0}' is a valid game. The following is a list of valid games:", arguments.Game);
				logger.Error (currentGames);

				return;
			}

			logger.InfoFormat ("Hello {0}@{1}!  Are you up for a game of {2}?", character.Name, character.Shard.Name, game.Name);

			if (pushOverClient != null) {
				logger.InfoFormat ("I see you're using PushOver.  I'll let you know there whenever you win something.");
			}

			var runner = new GameRunner (session, pushOverClient);
			var waiter = new Waiter ();

			// Start up the game runner
			runner.Start (game, character);

			// Wait for an interrupt
			waiter.Wait ();
		}
	}
}