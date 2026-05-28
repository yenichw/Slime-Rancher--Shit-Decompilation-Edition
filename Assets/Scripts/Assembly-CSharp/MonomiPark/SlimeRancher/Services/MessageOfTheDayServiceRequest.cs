using System;
using System.IO;
using System.Xml.Serialization;
using MonomiPark.SlimeRancher.Services.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace MonomiPark.SlimeRancher.Services
{
	public class MessageOfTheDayServiceRequest
	{
		public delegate void SuccessHandler(MessageOfTheDayV01 message, Texture2D image);

		public delegate void ErrorHandler();

		private readonly string url;

		private readonly int timeout;

		private bool isError;

		private bool isComplete;

		private bool isInitiated;

		private MessageOfTheDayV01 message;

		private Texture2D image;

		private UnityWebRequest messageRequest;

		private UnityWebRequest imageRequest;

		public event SuccessHandler OnSuccess;

		public event ErrorHandler OnError;

		public bool IsError()
		{
			return isError;
		}

		public bool IsComplete()
		{
			return isComplete;
		}

		public MessageOfTheDayV01 GetResultMessage()
		{
			return message;
		}

		public Texture2D GetResultImage()
		{
			return image;
		}

		public MessageOfTheDayServiceRequest(string url, int timeout)
		{
			this.url = url;
			this.timeout = timeout;
		}

		public void Begin()
		{
			if (isComplete || isInitiated)
			{
				throw new Exception("MessageOfTheDayServiceRequest is already complete or has been initiated. Create a new instance for a new request.");
			}
			isInitiated = true;
			messageRequest = UnityWebRequest.Get(url);
			messageRequest.timeout = timeout;
			messageRequest.SendWebRequest().completed += OnMessageRequestComplete;
		}

		private void OnMessageRequestComplete(AsyncOperation operation)
		{
			if (operation is UnityWebRequestAsyncOperation operation2)
			{
				OnMessageRequestComplete(operation2);
			}
		}

		private void OnMessageRequestComplete(UnityWebRequestAsyncOperation operation)
		{
			if (!operation.webRequest.isNetworkError && !operation.webRequest.isHttpError)
			{
				ProcessRequest(operation.webRequest);
				return;
			}
			Log.Debug("Initial message request failed.", "message", operation.webRequest.error);
			operation.webRequest.Dispose();
			ProcessFailedRequest();
		}

		private void ProcessRequest(UnityWebRequest request)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(MessageOfTheDayV01));
			MessageOfTheDayV01 messageOfTheDayV;
			using (TextReader textReader = new StringReader(request.downloadHandler.text))
			{
				messageOfTheDayV = (MessageOfTheDayV01)xmlSerializer.Deserialize(textReader);
			}
			LoadImage(messageOfTheDayV);
		}

		private void LoadImage(MessageOfTheDayV01 message)
		{
			imageRequest = UnityWebRequest.Get(message.ImageUrl);
			imageRequest.timeout = timeout;
			imageRequest.downloadHandler = new DownloadHandlerTexture();
			imageRequest.SendWebRequest().completed += delegate(AsyncOperation op)
			{
				OnImageRequestComplete(op, message);
			};
		}

		private void OnImageRequestComplete(AsyncOperation operation, MessageOfTheDayV01 message)
		{
			if (operation is UnityWebRequestAsyncOperation operation2)
			{
				OnImageRequestComplete(operation2, message);
			}
		}

		private void OnImageRequestComplete(UnityWebRequestAsyncOperation operation, MessageOfTheDayV01 message)
		{
			if (!operation.webRequest.isHttpError && !operation.webRequest.isNetworkError)
			{
				SetMessageResults(message, DownloadHandlerTexture.GetContent(operation.webRequest));
				return;
			}
			Log.Debug("Image request failed.", "message", operation.webRequest.error);
			ProcessFailedRequest();
		}

		private void ProcessFailedRequest()
		{
			isError = true;
			isComplete = true;
			message = null;
			image = null;
			if (this.OnError != null)
			{
				this.OnError();
			}
		}

		private void SetMessageResults(MessageOfTheDayV01 message, Texture2D image)
		{
			this.message = message;
			this.image = image;
			isError = false;
			isComplete = true;
			if (this.OnSuccess != null)
			{
				this.OnSuccess(this.message, image);
			}
		}

		~MessageOfTheDayServiceRequest()
		{
			if (messageRequest != null)
			{
				messageRequest.Dispose();
			}
			if (imageRequest != null)
			{
				imageRequest.Dispose();
			}
		}
	}
}
