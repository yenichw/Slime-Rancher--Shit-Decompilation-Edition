using UnityEngine;

public abstract class MessageOfTheDayProvider : ScriptableObject
{
	public delegate void SuccessHandler(MessageOfTheDay message);

	public delegate void ErrorHandler();

	public void Get(SuccessHandler onSuccess, ErrorHandler onError)
	{
		RetrieveMessage(onSuccess, onError);
	}

	protected abstract void RetrieveMessage(SuccessHandler onSuccess, ErrorHandler onError);
}
