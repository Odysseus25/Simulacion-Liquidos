using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 	Clase: EspRenderer
 * 
 * 	Se encarga de construir y actualizar la visualización del entorno en cada tick. 
 *  Cada celda se representa como un GameObject en el mundo.
 *
 */

public class EspRenderer : MonoBehaviour {

	public GameObject celda_prefab;
	public Material solid_material;
	public Material fluid_material;

	public CeldaPool pool;
	public CeldaPool terrain_pool;

	public List<Celda> scheduled_cells; 

	bool deleted_from_actives = false;
	bool deleted_from_scheduled = false;

	//el terreno
	public GameObject terrain;
	//celdas de terreno combinadas
	public Transform combinedMeshParent;
	public Transform individualTerrainParent;

	
	//numero de vertices permitidos por grupo
	[System.NonSerialized]
	public int vertexLimit = 65000;
	
	//lista que contiene todas las celdas unificadas del terreno
	[System.NonSerialized]
	public List<GameObject> combinedterrainList = new List<GameObject>();
	
	//lista que contiene todos los gameObjects que representan el terreno.
	[System.NonSerialized]
	public List<GameObject> allTerrain = new List<GameObject>();

	/*
	 * 	Funcion: inicializar()
	 * 
	 * 	Inicializa las estructuras de datos necesarias para rendering (el pool de celdas y el pool de celdas de terreno).
	 *  Luego ejecuta las operaciones de render para el terreno y el espacio.
	 */
	public void inicializar(Espacio espacio) {
		int tamano_pool = (int)(espacio.tamano_x * espacio.tamano_z * espacio.tamano_y);
		pool.inicializar (tamano_pool);
		terrain_pool.inicializar (tamano_pool);
		renderTerreno (espacio);
		renderEspacio (espacio);
	}

	/*
	 * 	Funcion: inicializarScheduledCells()
	 * 
	 * 	Inicializa la lista de celdas programadas.
	 *  La lista de celdas programadas <scheduled_cells> en una lista intermedia, donde se colocan temporalmente 
	 *  las nuevas celdas para ser visualizadas (rendered) en la funcion renderEspacio.
	 */
	public void inicializarScheduledCells() {
		scheduled_cells = new List<Celda> ();
	}

	/*
	 * 	Funcion: renderTerreno()
	 * 
	 * 	Recorre la lista de celdas de terreno del entorno, ejecutando la operacion de render para cada una.
	 */
	public void renderTerreno(Espacio espacio) {
		for (int i = 0; i < Controlador.espacio.celdas_de_terreno.Count; ++i) {
			renderCeldaDeTerreno(Controlador.espacio.celdas_de_terreno[i]);
		}
	}

	/*
	 * 	Funcion: renderEspacio()
	 * 
	 * 	Recorre la lista de celdas de activas del entorno (con saturacion > 0), ejecutando la operacion de render para cada una.
	 *  Luego recorre la lista de celdas programadas, ejecutando la operacion de render para cada una.
	 */
	public void renderEspacio(Espacio espacio) {
		for (int i = 0; i < Controlador.espacio.celdas_activas.Count; ++i) {
			renderCelda(Controlador.espacio.celdas_activas[i], i);
			if(deleted_from_actives) {
				deleted_from_actives = false;
				i -= 1;
			}
		}

		for (int i = 0; i < scheduled_cells.Count; ++i) {
			renderNewCelda(scheduled_cells[i], i);
			if(deleted_from_scheduled) {
				deleted_from_scheduled = false;
				i -= 1;
			}
		}
	}

	/*
	 *	Funcion: renderNewCelda()
	 *
	 *	Si <celda> no ha sido rendered, y <celda> no esta vacia (saturacion > 0), ejecuta la operacion de render para la nueva celda. 
	 */
	public void renderNewCelda(Celda celda, int sc_index) {
		if (!celda.rendered) {
			if (!celda.estaVacia()) {
				renderNuevaCelda(celda, sc_index);
			}
		} 
	}

	/*
	 *	Funcion: renderCelda()
	 *
	 *	Si <celda> ya ha sido rendered, y <celda> esta vacia (saturacion = 0), ejecuta la operacion de unrender para <celda>.
	 *	Si <celda> no esta vacia (saturacion > 0), ejecuta la operacion de actualizacion de render para <celda>. 
	 */
	public void renderCelda(Celda celda, int act_index) {
		if (celda.rendered) {
			if (celda.estaVacia()) {
				unrenderCelda(celda, act_index);
			}
			else {
				actualizarCelda(celda);
			}
		}
	}

	/*
	 *	Funcion: renderCeldaDeTerreno()
	 *
	 *	Si <celda> no ha sido rendered, y <celda> es solida, ejecuta la operacion de render para la nueva celda solida (de terreno).
	 */
	public void renderCeldaDeTerreno(Celda celda) {
		if (!celda.rendered) {
			if (celda.es_solido) {
				renderNuevaCeldaSolida(celda);
			}
		} 
	}

	/*
	 *	Funcion: unrenderCelda()
	 *
	 *	Destruye el GameObject correspondiente a <celda>.
	 */
	public void unrenderCelda(Celda celda, int act_index) {
		GameObject target = celda.getObjRendered();
		pool.destruirCelda (target);
		celda.unlinkRender (act_index);
		deleted_from_actives = true;
	}

	/*
	 *	Funcion: renderNuevaCelda()
	 *
	 *	Obtiene un nuevo GameObject del pool de celdas, lo relaciona a <celda> y ejecuta la operacion de render para <celda>.
	 */
	public void renderNuevaCelda(Celda celda, int sc_index) {
		GameObject nuevaCelda = pool.instanciarCelda (new Vector3 (celda.indice_x, celda.indice_y, celda.indice_z));
		//nuevaCelda.GetComponent<MeshRenderer> ().material = fluid_material;
		celda.linkRender (nuevaCelda, sc_index);
		deleted_from_scheduled = true;
		actualizarCelda (celda);
	}

	/*
	 *	Funcion: renderNuevaCelda()
	 *
	 *	Obtiene un nuevo GameObject del pool de celdas de terreno, lo relaciona a <celda> y ejecuta la operacion de render para <celda>.
	 */
	public void renderNuevaCeldaSolida(Celda celda) {
		if (!celda.rendered) {
			GameObject nuevaCelda = terrain_pool.instanciarCelda (new Vector3 (celda.indice_x, celda.indice_y, celda.indice_z));
			//nuevaCelda.GetComponent<MeshRenderer> ().material = solid_material;
			celda.linkRenderTerreno (nuevaCelda);
			//nuevaCelda.transform.parent = individualTerrainParent;
			allTerrain.Add(nuevaCelda);
			actualizarCelda (celda);
		}
	}

	/*
	 *	Funcion: actualizarCelda()
	 *
	 *	Actualiza la visualizacion de <celda>. Esto consiste en:
	 *		-Actualizar el GameObject correspondiente a <celda> para reflejar el nivel de saturacion de la celda.
	 *		-Actualizar la posicion del GameObject correspondiente a <celda> para reflejar la posicion de la celda en el entorno.
	 */
	public void actualizarCelda(Celda celda) {
		Transform rcelda_trans = celda.rendered_celda.transform;
		rcelda_trans.localScale = new Vector3(1, celda.getSaturacion(), 1);
		rcelda_trans.position = new Vector3 (celda.indice_x, celda.indice_y + celda.getSaturacion()/2 - 0.5f, celda.indice_z);
	}

	/*
	 *	Funcion: CombineTerrain()
	 *
	 *
	 *	
	 *		
	 */
	public void CombineTerrain(GameObject combinedTerrainObj){
		List<CombineInstance> terrainList = new List<CombineInstance>();
		CombineInstance combine = new CombineInstance();

		int verticesActuales = 0;
		int meshListCounter = 0;

		for(int i = 0; i < allTerrain.Count; i++){
			allTerrain[i].SetActive(false);
			//allTerrain[i].GetComponent<TerrainData>().listPos = meshListCounter;

			MeshFilter[] meshFilters = allTerrain[i].GetComponentsInChildren<MeshFilter>(true);

			for(int j = 0; j < meshFilters.Length; j++){
				MeshFilter meshFilter = meshFilters[j];
				combine.mesh = meshFilter.mesh;
				combine.transform = meshFilter.transform.localToWorldMatrix;

				//se agrega a la lista 
				terrainList.Add(combine);

				verticesActuales += meshFilter.mesh.vertexCount;
			}

			//se excede el limite de vertices?
			if(verticesActuales > vertexLimit){
				i =-1; 
				terrainList.RemoveAt(terrainList.Count - 1);

				CreateCombinedMesh(terrainList, combinedTerrainObj, combinedterrainList);
				terrainList.Clear();
				verticesActuales = 0;
				meshListCounter += 1;
			}
		}
		CreateCombinedMesh(terrainList, combinedTerrainObj, combinedterrainList);
	}


	public void CreateCombinedMesh(List<CombineInstance> meshDataList, GameObject meshHolderObj, List<GameObject> combinedHolderList){
		//crear nuevo mesh combinado
		Mesh newMesh = new Mesh();
		newMesh.CombineMeshes(meshDataList.ToArray());

		//crear nuevo objeto que va a contener el mesh combinado
		GameObject combinedMeshHolder = Instantiate(meshHolderObj, Vector3.zero, Quaternion.identity) as GameObject;

		combinedMeshHolder.transform.parent = combinedMeshParent;

		//agregamos el mesh
		combinedMeshHolder.GetComponent<MeshFilter>().mesh = newMesh;
		combinedHolderList.Add(combinedMeshHolder);
	}
}






















