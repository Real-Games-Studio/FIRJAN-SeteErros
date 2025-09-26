using System;
using UnityEngine;
using Lando;
using UnityEngine.Events;

namespace _4._NFC_Firjan.Scripts.NFC
{
	public class NFCReceiver : MonoBehaviour
	{
		/// <summary>
		/// Evento chamado quando o Nfc é encostado no leitor
		/// </summary>
		/// <returns><see cref="string">Nome do nfc</see>,<see cref="string">Nome do leitor</see></returns>
		public UnityEvent<string,string> OnNFCConnected;
		/// <summary>
		/// Evento chamado quando o Nfc é desencostado do leitor
		/// </summary>
		public UnityEvent OnNFCDisconnected;
		/// <summary>
		/// Evento chamado quando o Leitor de Nfc é conectado do windows
		/// </summary>
		/// <returns><see cref="string">Nome do leitor</see></returns>
		public UnityEvent<string> OnNFCReaderConnected;
		/// <summary>
		/// Evento chamado quando o Leitor de nfc é desconectado do windoes
		/// </summary>
		public UnityEvent OnNFCReaderDisconected;
		private  Cardreader _cardReader = new Cardreader();

		private void Awake()
		{
			_cardReader.CardConnected += OnCardConnectedHandler;
			_cardReader.CardDisconnected += OnCardDisconnectedHandler;
			_cardReader.CardreaderConnected += OnCardReaderConnectedHandler;
			_cardReader.CardreaderDisconnected += OnCardReaderDisconnectedHandler;
			_cardReader.StartWatch();
		}

		private void OnCardReaderDisconnectedHandler(object sender, CardreaderEventArgs e)
		{
			OnNFCReaderDisconected.Invoke();
		}

		private void OnCardReaderConnectedHandler(object sender, CardreaderEventArgs e)
		{
			OnNFCReaderConnected.Invoke(e.CardreaderName);
		}

		private void OnCardDisconnectedHandler(object sender, CardreaderEventArgs e)
		{
			OnNFCDisconnected.Invoke();
		}

		private void OnCardConnectedHandler(object sender, CardreaderEventArgs e)
		{
			OnNFCConnected.Invoke(e.Card.Id,e.CardreaderName);
		}

		private void OnDestroy()
		{
			_cardReader.StopWatch();
		}
	}
}