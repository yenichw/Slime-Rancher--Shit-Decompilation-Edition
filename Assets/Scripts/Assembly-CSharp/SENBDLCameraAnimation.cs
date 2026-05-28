using System;
using UnityEngine;

public class SENBDLCameraAnimation : MonoBehaviour
{
	private Vector3 randomRotation;

	private Vector3 randomModRotation;

	private void Start()
	{
		randomRotation = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
		randomRotation = Vector3.Normalize(randomRotation);
		randomModRotation = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
		randomModRotation = Vector3.Normalize(randomModRotation);
	}

	private void Update()
	{
		float num = 15f + Mathf.Pow(Mathf.Cos(Time.time * (float)Math.PI / 15f) * 0.5f + 0.5f, 3f) * 35f;
		Vector3 position = Quaternion.Euler(randomRotation * Time.time * 25f) * (Vector3.up * num);
		base.transform.position = position;
		base.transform.LookAt(Vector3.zero);
	}
}
