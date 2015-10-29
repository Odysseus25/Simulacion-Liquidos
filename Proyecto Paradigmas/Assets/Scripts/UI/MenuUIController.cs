using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * 	Clase: MenuUIController
 * 
 * 	
 *
 */

public class MenuUIController : MonoBehaviour {

	public void selectCanyonHeightmap() {
		PersistentData.GlobalData.heightmapSelected = true;
		PersistentData.GlobalData.heightmap = Heightmaps.Canyon;
		startSimulation ();
	}

	public void selectPoolHeightmap() {
		PersistentData.GlobalData.heightmapSelected = true;
		PersistentData.GlobalData.heightmap = Heightmaps.Pool;
		startSimulation ();
	}

	public void selectPlaygroundHeightmap() {
		PersistentData.GlobalData.heightmapSelected = true;
		PersistentData.GlobalData.heightmap = Heightmaps.Playground;
		startSimulation ();
	}

	void startSimulation() {
		Application.LoadLevel("simulacion");
	}

}
