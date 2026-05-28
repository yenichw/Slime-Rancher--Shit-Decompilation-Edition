using UnityEngine;

public class MergeChildMeshes : MonoBehaviour
{
	public Material mergedMaterial;

	private void Start()
	{
		base.gameObject.AddComponent<MeshRenderer>().material = mergedMaterial;
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		int num = componentsInChildren.Length;
		CombineInstance[] array = new CombineInstance[num];
		for (int i = 0; i < num; i++)
		{
			array[i].mesh = componentsInChildren[i].sharedMesh;
			Log.Info("source mesh", "item", i, "vertexCount", componentsInChildren[i].sharedMesh.vertexCount);
			array[i].transform = componentsInChildren[i].transform.localToWorldMatrix;
			componentsInChildren[i].gameObject.SetActive(value: false);
		}
		Log.Info("We wrote some stuff3", "count", array.Length);
		Mesh mesh = new Mesh();
		MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		meshFilter.mesh.CombineMeshes(array);
		Log.Info("Our combined mesh", "vertexCount", mesh.vertexCount);
		base.transform.rotation = Quaternion.identity;
		base.transform.position = Vector3.zero;
		base.transform.gameObject.SetActive(value: true);
	}
}
