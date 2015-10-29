using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
 * 	Clase: UIController
 * 
 * 	Maneja la entrada del usuario a traves de los elementos de interfaz grafica (UI).
 */

public class UIController : MonoBehaviour {

	public Text liquid_increase_rate;		// tasa de incremento de liquido, en celdas por tick.
	public Text total_saturation;			// monto total de liquido en el entorno (suma de la saturacion de todas las celdas activas)
	public Text active_cells;				// numero de celdas activas en el entorno (con saturacion > 0)

	bool sac_active = false;		// spawnAtCenter active?
	bool sar_active = false;		// spawnAtRandom active?
	bool sic_active = false;		// spawnAroundCenter active?

	// actualiza los elementos de texto que despliegan informacion sobre el entorno
	void updateUI() {
		liquid_increase_rate.text = "" + Mathf.RoundToInt(Controlador.espacio.tasa_incr_sat) + " c/t";
		total_saturation.text = "" + Mathf.RoundToInt(Controlador.espacio.saturacion_total);
		active_cells.text = "" + Controlador.esp_renderer.pool.getCeldasActivas ();
	}
	
	// se actualizan los elementos de texto periodicamente
	void Update () {
		updateUI ();
	}

	// si cualquier metodo de introduccion de liquido esta activo, se le dice al entorno que ejecute la introduccion de liquido
	public void checkSpawn() {
		if (sac_active) {
			Controlador.espacio.spawnAtCenter ();
		}
		if (sar_active) {
			Controlador.espacio.spawnAtRandom ();
		}
		if (sic_active) {
			Controlador.espacio.spawnInCircle ();
		}
	}

	// activar/desactivar metodo de introduccion de liquido: spawnAtCenter
	public void spawnAtCenter() {
		sac_active = !sac_active;
	}

	// activar/desactivar metodo de introduccion de liquido: spawnAtRandom
	public void spawnAtRandom() {
		sar_active = !sar_active;
	}

	// activar/desactivar metodo de introduccion de liquido: spawnInCircle (a veces referido tambien como spawnAroundCenter)
	public void spawnInCircle() {
		sic_active = !sic_active;
	}

	// salir de la aplicacion
	public void quitProgram() {
		Application.Quit();
	}

}
