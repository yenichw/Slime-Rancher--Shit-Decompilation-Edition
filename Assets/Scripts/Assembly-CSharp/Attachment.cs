using UnityEngine;

public interface Attachment
{
	void Init(GameObject source, GameObject target, Vector3 attachPoint, bool causeFear, float intermediateHeight);
}
