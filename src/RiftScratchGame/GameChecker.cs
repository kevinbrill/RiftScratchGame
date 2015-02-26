using System;
using System.Collections.Generic;
using rift.net.Models.Games;
using System.Timers;
using rift.net.Models;
using rift.net;
using System.Linq;
using RiftScratchGame.Notifications.Pushover;
using log4net;

namespace RiftScratchGame
{
	public class GameChecker
	{
		private static ILog logger = LogManager.GetLogger (typeof(GameChecker));
		private Dictionary<string, Game> _availableGames = new Dictionary<string, Game>();
		private Timer timer;
		private ScratchGameClient _client;
		private PushOverClient _pushOverClient;

		public GameChecker (Session session, PushOverClient pushOverClient = null)
		{
			this._client = new ScratchGameClient (session);
			this._pushOverClient = pushOverClient;
		}

		public void Start() {

			CheckGames ();

			SetTimer ();
		}

		private void CheckGames() {

			logger.Info ("Checking for new or removed games now.");

			var games = _client.ListGames ().ToDictionary (x => x.Name);

			if (_availableGames.Any ()) {

				// Check for any games that are now available that were not
				//  the last time we checked
				foreach (var game in games) {

					if (!_availableGames.ContainsKey (game.Key)) {

						var message = string.Format ("It looks like '{0}' is a new game available for play!", game.Value.Name);

						if (this._pushOverClient != null) {
							this._pushOverClient.SendMessage (message, "New game");
						}

						logger.Info (message);
					}						
				}

				// Now, check for any games that were removed
				foreach (var game in _availableGames) {

					if (!games.ContainsKey (game.Key)) {

						var message = string.Format ("It looks like '{0}' was removed from play.", game.Value.Name);

						if (this._pushOverClient != null) {
							this._pushOverClient.SendMessage (message, "Game removed");
						}

						logger.Info (message);
					}						
				}
			}

			_availableGames = games;
		}

		private void SetTimer() {

			if (timer != null) {
				timer.Stop ();
				timer.Dispose ();
			}

			timer = new Timer (3600000);
			timer.Elapsed+= (sender, e) => {
				CheckGames();
			};

			timer.Start ();
		}
	}
}

