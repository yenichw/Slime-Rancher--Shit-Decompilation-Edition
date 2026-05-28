using UnityEngine;

public class SectrTestWorldGenerator : MonoBehaviour
{
	private readonly string[] PATHS = new string[2] { "SectrTestDynamic_OtherChunk", "SectrTestDynamic_OtherChunk2" };

	private void Awake()
	{
		for (int i = -30; i <= 30; i += 10)
		{
			for (int j = 10; j <= 50; j += 10)
			{
				CreateRandomSector(i, j, 10f, 10f);
			}
		}
	}

	private void CreateRandomSector(float x, float z, float width, float height)
	{
		Vector3 vector = new Vector3(x, 0f, z);
		Vector3 size = new Vector3(width, 20f, height);
		GameObject obj = new GameObject("TerrainBlock(" + x + "," + z + ")");
		obj.transform.position = vector;
		SECTR_Sector sECTR_Sector = obj.AddComponent<SECTR_Sector>();
		sECTR_Sector.OverrideBounds = true;
		sECTR_Sector.BoundsOverride = new Bounds(vector, size);
		sECTR_Sector.Frozen = true;
		SECTR_Chunk sECTR_Chunk = obj.AddComponent<SECTR_Chunk>();
		sECTR_Chunk.NodeName = "Assets/Scene/test/SectrTestDynamic/Chunks/" + (sECTR_Chunk.ScenePath = PickSubSceneName()) + ".unity";
	}

	private string PickSubSceneName()
	{
		return PATHS[Random.Range(0, PATHS.Length)];
	}

	private void Update()
	{
	}
}
