using UnityEngine;
using System.Collections;

public enum Heightmaps {Canyon, Hills, Pool, Playground};

/*
 * 	Clase: PersistentData
 * 
 * 	Contiene informacion persistentemente a travez de diferentes escenas.
 *  Guarda la informacion del heightmap seleccionado para transferirla desde la escena inicial a la escena de simulacion.
 *
 */

public class PersistentData : MonoBehaviour {

	public static PersistentData GlobalData;

	public bool heightmapSelected = false;
	public Heightmaps heightmap;

	void Awake() {
		GlobalData = this;
		DontDestroyOnLoad (gameObject);
	}

}
