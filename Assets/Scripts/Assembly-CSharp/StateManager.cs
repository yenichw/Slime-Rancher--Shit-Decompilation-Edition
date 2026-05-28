public interface StateManager
{
	void SetState(string stateName, bool setEnabled = true);

	void Reset();

	void CombineStates();

	bool IsEnabled(string stateName);
}
