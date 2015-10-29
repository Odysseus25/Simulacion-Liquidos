using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 	Clase: Celda
 * 
 * 	Unidad minima de granularidad del entorno. Si vemos el entorno como un grafico tridimensional, cada punto del
 *  grafico representa una celda.
 *
 */

public class Celda {

	public int indice_x;			// posicion que ocupa la celda en la dimension X del entorno
	public int indice_y;			// posicion que ocupa la celda en la dimension Y del entorno
	public int indice_z;			// posicion que ocupa la celda en la dimension Z del entorno

	public float saturacion;		// concentracion de liquido en la celda (0.0 vacia, 1.0 llena)
	public bool es_solido;			// es solida? (las celdas de terreno son solidas, i.e. el liquido nunca se desplaza hacia ellas)
	public bool esta_en_aire;		

	public bool es_celda_alta;
	public CeldaAlta celda_alta;

	public bool rendered;					// esta celda esta visualizada (rendered) por EspRenderer?
	public bool scheduled_for_rendering;	// esta celda esta programada para ser visualizada por EspRenderer?
	public GameObject rendered_celda;		// referencia al GameObject utilizado por EspRenderer para visualizar esta celda en particular

	/*
	 *	Constructor de Celda
	 *
	 *	Inicializa la celda.
	 */
	public Celda(int indice_x, int indice_y, int indice_z) {

		this.indice_x = indice_x;
		this.indice_y = indice_y;
		this.indice_z = indice_z;
		saturacion = 0f;
		es_solido = false;
		esta_en_aire = false;
		es_celda_alta = false;
		rendered = false;
		scheduled_for_rendering = false;

	}

	/*
	 * 	Funcion no implementada.
	 */
	public void linkCeldaAlta(CeldaAlta celda_alta) {
		this.celda_alta = celda_alta;
		es_celda_alta = true;
	}

	/*
	 * 	Devuelve la saturacion de la celda, la cual representa el porcentaje de liquido que contiene la celda.
	 */
	public float getSaturacion() {
		return saturacion;
	}

	/*
	 * 	Funcion: setSaturacion()
	 * 
	 * 	Establece la saturacion de la celda a <valor>.
	 * 
	 * 	Si la celda no esta visualizada (rendered) por EspRenderer, ni esta programada para ser visualizada, significa que
	 *  anteriormente esta celda tenia saturacion = 0. En este caso se agrega la celda a la lista de celdas programadas 
	 *  (<scheduled_cells>) para ser visualizada por EspRenderer durante el proximo tick de rendering.
	 * 	
	 * 	De lo contrario la celda ya esta visualizada (existe un GameObject en la escena que la representa), y por lo tanto es 
	 * 	una celda activa. EspRenderer se encarga de actualizar la visualizacion de las celdas activas por su cuenta, y por esto
	 *  no hace falta agregarla a la lista de celdas programadas.
	 */
	public void setSaturacion(float valor) {
		if (!rendered && !scheduled_for_rendering) {
			scheduled_for_rendering = true;
			Controlador.esp_renderer.scheduled_cells.Add (this);
			Controlador.espacio.saturacion_total += valor;
		} 
		else {
			Controlador.espacio.saturacion_total -= saturacion;
			Controlador.espacio.saturacion_total += valor;
		}
		saturacion = valor;
		estaEnAire ();	// recalcular
	}
	
	public bool esCeldaSolida() {
		return es_solido;
	}

	/*
	 * 	Funcion: estaEnAire()
	 * 
	 * 	Devuelve true si la celda esta en el aire, falso si no.
	 * 
	 * 	Si la celda abajo de la celda actual existe (i.e. la celda actual tiene indice_y > 0), la celda abajo no es solida 
	 * 	y la celda abajo tiene saturacion > 0, entonces el devuelve el resultado de llamar la funcion estaEnAire() de la 
	 * 	celda abajo.
	 * 
	 * 	Si no se cumple la condicion anterior, devuelve el resultado de la funcion puedeFluirHaciaAbajo().
	 */
	public bool estaEnAire() {

		Celda abajo = getCeldaAbajo ();
		if (abajo != null && !abajo.es_solido && abajo.saturacion > 0f) {
			return abajo.estaEnAire();
		}

		if (puedeFluirHaciaAbajo()) {
			esta_en_aire = true;
		} 
		else {
			esta_en_aire = false;	
		}

		return esta_en_aire;

	}

	/*
	 *	Funcion: setSolida()
	 *
	 *	Establece la celda actual como solida, y la agrega a la lista de celdas de terreno del entorno.
	 */
	public void setSolida() {
		es_solido = true;
		saturacion = 1f;
		Controlador.espacio.celdas_de_terreno.Add (this);
	}

	/*
	 * 	Funcion: unsetSolida()
	 * 
	 * 	Establece la celda actual como no solida.
	 * 	
	 * 	NOTA: la funcion esta incompleta; se debe ademas remover la celda de la lista de celdas de terreno del entorno.
	 * 	Esta funcion no se utiliza por ahora.
	 */
	public void unsetSolida() {
		es_solido = false;
		saturacion = 0f;
	}

	/*
	 *	Funcion: estaVacia()
	 *
	 *	Devuelve verdadero si la celda esta vacia (saturacion == 0), falso si no.
	 */
	public bool estaVacia() {
		if(saturacion > 0 || es_celda_alta) {
			return false;
		}
		return true;
	}

	/*
	 * 	Funcion: distribuirLiquido()
	 * 
	 * 	Ejecuta las operaciones de distribucion de liquido vertical y horizontalmente para la celda actual.
	 */
	public void distribuirLiquido() {
		distribuirLiquidoVerticalmente ();
		distribuirLiquidoHorizontalmente ();
	}

	/*
	 * 	Funcion: distribuirLiquidoVerticalmente()
	 * 
	 * 	Ejecuta la operacion de distribucion de liquido vertical para la celda actual.
	 */
	public void distribuirLiquidoVerticalmente() {
		if (puedeFluirHaciaAbajo ()) {
			procesarFlujoVertical();	
		}
	}

	/*
	 * 	Funcion: distribuirLiquidoHorizontalmente()
	 * 
	 * 	Ejecuta la operacion de distribucion de liquido horizontal para la celda actual.
	 */
	public void distribuirLiquidoHorizontalmente() {
		Celda abajo = getCeldaAbajo ();
		if (abajo != null && !abajo.es_solido && abajo.estaEnAire()) {
			return;
		}
		if (!puedeFluirHaciaAbajo () && saturacion > 0f) {
			procesarFlujoHorizontal();
		}
	}

	/*
	 * 	Funcion: procesarFlujoVertical()
	 * 
	 * 	Ejecuta la operacion de distribucion de liquido hacia la celda abajo de la celda actual.
	 */
	void procesarFlujoVertical() {
		Celda abajo = getCeldaAbajo ();
		float intercambio = Mathf.Min (1f - abajo.saturacion, saturacion);
		setSaturacion (saturacion - intercambio);
		abajo.setSaturacion (abajo.saturacion + intercambio);
	}

	/*
	 * 	Funcion: procesarFlujoHorizontal()
	 * 
	 * 	Ejecuta la operacion de distribucion de liquido hacia las celdas contiguas a la celda actual (pero no la de abajo).
	 */
	void procesarFlujoHorizontal() {
		float sat = saturacion;
		List<Celda> vecinas = getCeldasContiguas ();
		for (int i = 0; i < vecinas.Count; ++i) {
			sat += vecinas[i].saturacion;
		}
		sat = sat / (vecinas.Count + 1);
		if (sat > Controlador.espacio.sensibilidad) {
			for (int i = 0; i < vecinas.Count; ++i) {
				vecinas[i].setSaturacion(sat);
			}
			setSaturacion(sat);
		}
	}

	void ajustarSaturacion(float saturacion) {
		float diferencia = Mathf.Abs(this.saturacion - saturacion);

		if (saturacion > this.saturacion) {
			diferencia = Mathf.Min (diferencia, Controlador.espacio.tasa_transferencia_saturacion);
		} 
		else {
			diferencia = -1 * Mathf.Min (diferencia, Controlador.espacio.tasa_transferencia_saturacion);
		}

		this.saturacion += diferencia;
		
	}

	/*
	 * 	Funcion: puedeFluirHaciaAbajo()
	 * 
	 * 	Devuelve verdadero si el liquido en la celda actual puede fluir hacia la celda abajo, falso si no.
	 */
	bool puedeFluirHaciaAbajo() {
		Celda abajo = getCeldaAbajo ();
		if(abajo != null && !abajo.esCeldaSolida() && abajo.saturacion < 1f) {
			return true;
		}
		return false;
	}

	/*
	 * 	Funcion: getCeldasContiguas()
	 * 
	 * 	Devuelve una lista con las celdas contiguas a la celda actual.
	 */
	List<Celda> getCeldasContiguas() {

		List<Celda> celdas = new List<Celda> ();
		if(indice_x > 0) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x-1, indice_y, indice_z);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}
		if(indice_z > 0) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x, indice_y, indice_z-1);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}
		if(indice_x < Controlador.espacio.tamano_x - 1) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x+1, indice_y, indice_z);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}
		if(indice_z < Controlador.espacio.tamano_z - 1) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x, indice_y, indice_z+1);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}


		// Diagonales. No esta implementado.
		/*if(indice_x > 0 && indice_z > 0) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x-1, indice_y, indice_z-1);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}

		if(indice_x > 0 && indice_z < Controlador.espacio.tamano_z - 1) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x-1, indice_y, indice_z+1);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}

		if(indice_x < Controlador.espacio.tamano_x - 1 && indice_z > 0) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x+1, indice_y, indice_z-1);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}

		if(indice_x < Controlador.espacio.tamano_x - 1 && indice_z < Controlador.espacio.tamano_z - 1) {
			Celda tcelda = Controlador.espacio.getCelda(indice_x+1, indice_y, indice_z+1);
			if(!tcelda.es_solido) {
				celdas.Add (tcelda);
			}
		}*/


		return celdas;

	}

	// Devuelve la celda abajo de la celda actual.
	Celda getCeldaAbajo() {
		if (indice_y > 0) {
			return Controlador.espacio.getCelda(indice_x, indice_y-1, indice_z);
		}
		return null;
	}

	List<Celda> getCeldaArriba() {
		List<Celda> celdas = new List<Celda> ();
		if (indice_y < Controlador.espacio.tamano_y - 1) {
			celdas.Add (Controlador.espacio.getCelda(indice_x, indice_y + 1, indice_z));
		}
		return celdas;
	}

	// Devuelve el GameObject que representa la celda actual en la escena.
	public GameObject getObjRendered() {
		if(rendered) {
			return rendered_celda;
		}
		return null;
	}

	/*
	 *	Funcion: linkRender()
	 *
	 *	Relaciona la celda actual con <rendered_celda>, el cual es el GameObject que representa la celda en la escena.
	 *	Remueve la celda actual de la lista de celdas programadas, y agrega la celda actual a la lista de celdas activas.
	 */
	public void linkRender(GameObject rendered_celda, int sc_index) {
		if (!rendered) {
			this.rendered_celda = rendered_celda;
			rendered = true;
			scheduled_for_rendering = false;
			Controlador.esp_renderer.scheduled_cells.RemoveAt(sc_index);
			Controlador.espacio.celdas_activas.Add (this);
		}
	}

	/*
	 * 	Funcion: unlinkRender()
	 * 
	 * 	Remueve la relacion entre la celda actual y el GameObject que la representa en el entorno. Tambien remueve la celda
	 *  actual de la lista de celdas activas.
	 */
	public void unlinkRender (int act_index) {
		rendered = false;
		this.rendered_celda = null;
		Controlador.espacio.celdas_activas.RemoveAt (act_index);
	}

	/*
	 *	Funcion: linkRenderTerreno()
	 *
	 *	Relaciona la celda de terreno actual con <rendered_celda>, el cual es el GameObject que representa la celda en la escena.
	 */
	public void linkRenderTerreno(GameObject rendered_celda) {
		if (!rendered) {
			this.rendered_celda = rendered_celda;
			rendered = true;
			scheduled_for_rendering = false;
		}
	}

}
