using UnityEngine;
using System.Collections;

/*
 * 	Clase: CeldaAlta
 * 
 * 	INCOMPLETO
 *  Esta clase no esta implementada todavia.
 *
 */

public class CeldaAlta {

	public int indice_x;
	public int indice_z;
	public int indice_y_max;
	public int indice_y_min;

	public bool es_solida;

	public bool rendered;
	public GameObject rendered_celda;

	public CeldaAlta(int indice_x, int indice_z, int indice_y_max, int indice_y_min, bool es_solida = false) {

		this.indice_x = indice_x;
		this.indice_z = indice_z;
		this.indice_y_min = indice_y_min;
		this.indice_y_max = indice_y_max;
		this.es_solida = es_solida;
		rendered = false;

	}

	public void linkRender(GameObject rendered_celda) {
		rendered = true;
		this.rendered_celda = rendered_celda;
	}
	
	public void unlinkRender () {
		rendered = false;
	}

}
