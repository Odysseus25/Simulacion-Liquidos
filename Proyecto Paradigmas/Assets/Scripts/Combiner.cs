using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Mesh/Combine Children")]
public class Combiner : MonoBehaviour {

	int vertexSoFar = 0;
	int vertexLimit = 65000;
	int iterador = 0;
	bool done = false;
	
	void Start()
	{
		Matrix4x4 myTransform = transform.worldToLocalMatrix;
		Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
		MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
		//Debug.Log("largo de renderers> "+meshRenderers.Length);

		foreach (var meshRenderer in meshRenderers)
		{
			foreach (var material in meshRenderer.sharedMaterials)
				if (material != null && !combines.ContainsKey(material))
					combines.Add(material, new List<CombineInstance>());
		}
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		//Debug.Log ("largo de la lista: "+meshFilters.Length);
		int iteradorPasado = 0;

		while(iterador < meshFilters.Length-1){
			combineTerrain(meshFilters, myTransform, combines, iteradorPasado);
			vertexSoFar = 0;
			iteradorPasado = iterador;
			combines.Clear();
		}
		DeleteChildren();
	}

	void combineTerrain(MeshFilter[] meshFilters, Matrix4x4 myTransform, Dictionary<Material, List<CombineInstance>> combines, int iteradorPasado){

			MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in meshRenderers)
			{
				foreach (var material in meshRenderer.sharedMaterials)
					if (material != null && !combines.ContainsKey(material))
						combines.Add(material, new List<CombineInstance>());
			}


		for(int i = iterador; i < meshFilters.Length; i++){
			if(vertexSoFar < vertexLimit){
				iterador++;
				if (meshFilters[i].sharedMesh == null)
					continue;
				var filterRenderer = meshFilters[i].GetComponent<Renderer>();
				if (filterRenderer.sharedMaterial == null)
					continue;
				if (filterRenderer.sharedMaterials.Length > 1)
					continue;
				CombineInstance ci = new CombineInstance
				{
					mesh = meshFilters[i].sharedMesh,
					transform = myTransform*meshFilters[i].transform.localToWorldMatrix
				};
				combines[filterRenderer.sharedMaterial].Add(ci);
				vertexSoFar += meshFilters[i].mesh.vertexCount; 
				Destroy(filterRenderer);
				//Debug.Log("Iterador: "+i);
			}
			//Debug.Log(vertexSoFar);
		}


		foreach(Material m in combines.Keys)
		{
				var go = new GameObject("Combined mesh");
				go.transform.parent = transform;
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				
				var filter = go.AddComponent<MeshFilter>();
				filter.mesh.CombineMeshes(combines[m].ToArray(), true, true);
				
				var arenderer = go.AddComponent<MeshRenderer>();
				arenderer.material = m;
		}

	}

	void DeleteChildren(){
		var children = new List<GameObject>();
		foreach (Transform child in transform) {
			if (child.name != "Combined mesh")
				children.Add (child.gameObject);
		}
		children.ForEach(child => Destroy(child)); 

	}
}