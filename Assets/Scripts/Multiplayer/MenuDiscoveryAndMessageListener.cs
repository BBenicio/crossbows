using System;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi.Nearby;
#endif

namespace Multiplayer
{
	#if UNITY_ANDROID
	public class MenuDiscoveryAndMessageListener : IDiscoveryListener, IMessageListener
	{
		private MultiplayerMenu menu;

		public MenuDiscoveryAndMessageListener (MultiplayerMenu menu) {
			this.menu = menu;
		}

		public void OnEndpointFound (EndpointDetails discoveredEndpoint) {
			Logger.LogInfo (string.Format ("Found Endpoint: {0} {1}", discoveredEndpoint.Name, discoveredEndpoint.EndpointId));

			menu.AddEndpoint (discoveredEndpoint);
		}

		public void OnEndpointLost (string lostEndpointId) {
			Logger.LogInfo (string.Format ("Lost Endpoint: {0}", lostEndpointId));

			menu.RemoveEndpoint (lostEndpointId);
		}

		public void OnMessageReceived (string remoteEndpointId, byte[] data, bool isReliableMessage) {
			Logger.LogInfo (string.Format ("Recieved message '{0}' from {1}", data, remoteEndpointId));
		}

		public void OnRemoteEndpointDisconnected (string remoteEndpointId) {
			Logger.LogInfo (string.Format ("Endpoint disconnected {0}", remoteEndpointId));
		}
	}
	#else
	public class MenuDiscoveryAndMessageListener
	{
	}
	#endif
}

