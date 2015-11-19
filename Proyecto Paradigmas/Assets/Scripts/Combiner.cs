using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Mesh/Combine Children")]
public class Combiner : MonoBehaviour {

	int vertexSoFar = 0;
	int vertexLimit = 65000;
	//IEnumerator currentObjc;
	int iterador = 0;

	void Start()
	{
		Matrix4x4 myTransform = transform.worldToLocalMatrix;
		Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
		MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
		foreach (var meshRenderer in meshRenderers)
		{
			foreach (var material in meshRenderer.sharedMaterials)
				if (material != null && !combines.ContainsKey(material))
					combines.Add(material, new List<CombineInstance>());
		}

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		//meshFilters.Length
		for(int i = iterador; i < meshFilters.Length; i++){
			Debug.Log("Iterador: "+i);
			iterador += 1;
		}

		foreach(var filter in meshFilters)
		{
			vertexSoFar += filter.mesh.vertexCount; 
			//filter es un objeto
			if(vertexSoFar < vertexLimit){
				if (filter.sharedMesh == null)
					continue;
				var filterRenderer = filter.GetComponent<Renderer>();
				if (filterRenderer.sharedMaterial == null)
					continue;
				if (filterRenderer.sharedMaterials.Length > 1)
					continue;
				CombineInstance ci = new CombineInstance
				{
					mesh = filter.sharedMesh,
					transform = myTransform*filter.transform.localToWorldMatrix
				};
				combines[filterRenderer.sharedMaterial].Add(ci);
				
				Destroy(filterRenderer);
			}

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


		DeleteChildren();
	}

	void DeleteChildren(){
		var children = new List<GameObject>();
		foreach (Transform child in transform) {
			if (child.name != "Combined mesh")
				children.Add (child.gameObject);
		}
		children.ForEach(child => Destroy(child)); 
		Debug.Log(vertexSoFar);
	}
}

/*meter los foreach en un metodo y pasarle de parametro la lista con los objetoc que quedan
 * declarar variable global ienumerator que se va a modificar al final del metodo nuevo, el metodo start
 * tiene que poder modificar la lista de objetos (copiar la lista desde el punto en donde se quedo la ultima en adelante)
 * y al final de todo eliminar los hijos originales*/
