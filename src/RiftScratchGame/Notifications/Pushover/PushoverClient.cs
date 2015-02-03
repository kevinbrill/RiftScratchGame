using System;
using RestSharp;

namespace RiftScratchGame.Notifications.Pushover
{
	public class PushOverClient
	{
		private const string url = "https://api.pushover.net/1/messages.json";
		private const string key = "aHR7yfwFTX1kzyGNGC1vNpPi7oZbQL";
		private RestClient client;

		public string UserKey {
			get;
			private set;
		}

		public PushOverClient (string userKey)
		{
			UserKey = userKey;

			client = new RestClient (url);
		}

		public void SendMessage( string message, string title = null)
		{
			var request = new RestRequest (Method.POST);

			request.AddParameter ("token", key, ParameterType.GetOrPost);
			request.AddParameter ("user", UserKey);
			request.AddParameter ("message", message);

			if (title != null) {
				request.AddParameter ("title", title);
			}

			client.Execute (request);
		}
	}
}

