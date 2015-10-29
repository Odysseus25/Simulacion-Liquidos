using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 	Clase: CeldaPool
 * 
 * 	Estructura de datos utilizada para guardar GameObjects que corresponden a celdas no asignadas.
 *  Cuando EspRenderer necesita visualizar (render) una nueva celda, EspRenderer obtiene un GameObject de alguna
 *  instancia de esta clase, y EspRenderer la relaciona a la celda que se quiere visualizar.
 *
 *	Instanciar nuevos GameObjects en la escena es una operacion lenta, asi que en la medida de lo posible es mejor
 *  evitar hacerlo en tiempo de ejecucion. Por ello, al inicio de la aplicacion instanciamos muchos GameObjects y 
 *  los guardamos en un Pool.
 * 
 *  Asi, EspRenderer puede obtener GameObjects del Pool en tiempo de ejecucion en vez de instanciar nuevos GameObjects.
 *  Esto hace que la operacion de render de nuevas celdas sea mucho mas rapida.
 */

public class CeldaPool : MonoBehaviour {

	public GameObject celda_prefab;

	Stack<GameObject> pool;
	int tamano;

	public void inicializar(int tamano) {
		pool = new Stack<GameObject> ();
		agregarCeldas (tamano);
		this.tamano = tamano;
	}

	public GameObject instanciarCelda(Vector3 posicion) {

		if (pool.Count > 0) {
			GameObject celda;
			celda = pool.Pop ();
			celda.SetActive (true);
			celda.transform.position = posicion;
			return celda;
		} 
		else {
			Debug.LogError("No hay mas celdas!");
		}

		return null;

	}

	public void destruirCelda(GameObject celda) {
		celda.SetActive (false);
		pool.Push (celda);
	}

	public int getCeldasActivas() {
		return tamano - pool.Count;
	}

	void agregarCeldas(int numero) {
		for (int i = 0; i < numero; ++i) {
			pool.Push (instanciarNuevaCelda());
		}
	}

	GameObject instanciarNuevaCelda() {
		GameObject celda = (GameObject) Instantiate(celda_prefab);
		celda.transform.parent = transform;
		celda.SetActive (false);
		return celda;
	}

}
