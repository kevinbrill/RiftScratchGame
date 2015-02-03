using System;
using rift.net.Models;
using rift.net.Models.Games;
using rift.net;
using log4net;
using System.Configuration;
using RiftScratchGame.Notifications.Pushover;
using System.Timers;

namespace RiftScratchGame
{
	public class GameRunner
	{
		private static ILog logger = LogManager.GetLogger (typeof(GameRunner));

		private ScratchGameClient gameClient;
		private Timer timer;
		private Character character;
		private Game game;
		private PushOverClient pushOverClient;

		public GameRunner (Session session, PushOverClient pushOverClient = null)
		{
			gameClient = new ScratchGameClient (session);
			this.pushOverClient = pushOverClient;
		}

		public void Start(Game game, Character character)
		{
			this.character = character;
			this.game = game;

			PlayGames ();
		}

		private void PlayGames()
		{
			var gameStatus = gameClient.GetAccountGameInfo ();

			while (gameStatus.AvailablePoints > 0) {

				var prizes = gameClient.Play (game, character.Id);

				if (prizes.Count == 0) {
					logger.InfoFormat ("Sorry, I just played {0} and you didn't win anything.  Better luck next time!", game.Name);
				} else {
					foreach (var prize in prizes) {

						logger.InfoFormat ("Congratulations!  I just played {0} and you won {1} {2}", game.Name, prize.Quantity, prize.Name);

						if (pushOverClient != null) {
							pushOverClient.SendMessage (string.Format ("You won {0} {1}", prize.Quantity, prize.Name), string.Format ("{0} rewards", game.Name));
						}
					}
				}

				gameStatus = gameClient.GetAccountGameInfo ();
			}

			logger.InfoFormat ("Your next game will be played at {0}", DateTime.Now.AddSeconds (gameStatus.SecondsUntilNextPoint));

			SetTimer (gameStatus.SecondsUntilNextPoint);
		}

		private void SetTimer( int secondsUntilNextPoint ) {

			if (timer != null) {
				timer.Stop ();
				timer.Dispose ();
			}

			timer = new Timer ((secondsUntilNextPoint + 30) * 1000);
			timer.Elapsed+= (sender, e) => {
				PlayGames();
			};

			timer.Start ();
		}
	}
}

