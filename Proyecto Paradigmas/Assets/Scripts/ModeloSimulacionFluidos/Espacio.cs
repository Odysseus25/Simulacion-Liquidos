using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/*
 * 	Clase: Espacio
 * 
 * 	Estructura de datos que contiene el entorno de simulacion; <grid>, una cuadricula tridimensional de Celdas.
 *  <grid> es una Lista de Arreglos bidimensionales de celdas, de manera que la altura (Y) del entorno es dinamica (la lista),
 *  mientras la anchura (X) y profundidad (Z) del entorno es fija (las dimensiones de los arreglos contenidos en la lista).
 *
 */
public class Espacio {
	
	public float saturacion_total = 0;
	public int tamano_x;				// tamano de la dimension X (anchura) del entorno
	public int tamano_y;				// tamano de la dimension Y (altura) del entorno
	public int tamano_z;				// tamano de la dimension Z (profundidad) del entorno
	public float sensibilidad;	
	public float tasa_transferencia_saturacion = 0.15f;		// tasa a la que se distribuye liquido desde una celda hacia otra, en un solo tick
	public float tasa_incr_sat = 0f;
	public List<Celda[,]> grid;					// estructura que contiene todas las celdas del entorno
	public List<Celda> celdas_activas;			// lista de celdas activas, i.e. con saturacion > 0
	public List<Celda> celdas_de_terreno;		// lista de celdas de terreno

	public EspRenderer rend;
	public ManualResetEvent[] threads;	//necesario para el threadpool
	private ManualResetEvent doneEvent;
	//private Celda cellda;

	bool simulacionEnProgreso = false;
	Vector3 sicStartPosition;
	int testDelay;
	float sat_tot_tick_anterior;
	float sat_tot_tick_actual;

	/*
	 * 	Constructor
	 * 
	 * 	Inicializa las estructuras de datos del entorno y genera el terreno en el entorno (celdas naranjas).
	 */
	public Espacio(int tamano_x, int tamano_y, int tamano_z, float sensibilidad) {

		Controlador.espacio = this;

		this.tamano_x = tamano_x;
		this.tamano_y = tamano_y;
		this.tamano_z = tamano_z;
		this.sensibilidad = sensibilidad;
		threads = new ManualResetEvent[100];

		Controlador.controlador.center.position = new Vector3 (tamano_x/2, tamano_y/4, tamano_z/2);
		celdas_de_terreno = new List<Celda> ();
		celdas_activas = new List<Celda> ();
		inicializarGrid ();

		sicStartPosition = new Vector3 (tamano_x/4, tamano_y-1, tamano_z/4);
		testDelay = 0;

		generarTerreno ();

		/*
		grid [9] [15, 15].setSaturacion (1f);

		grid [2, 2, 0].setCeldaAlta (2, 0, true, this);
		setCeldaAlta (2, 0, 2, 0, true);
		*/

	}

	/*
	 * 	Funcion: inicializarGrid()
	 * 
	 * 	Inicializa <grid>, la estructura de datos que contiene todas las celdas del entorno.
	 * 
	 * 	<grid> es una lista, donde cada entrada contiene una matriz bidimensional de celdas. Cada entrada de la lista representa
	 * 	un plano de celdas con el mismo <indice_y>. Estos son almacenados como matrices bidimensionales de celdas, donde las 
	 *  dimensiones representan el <indice_x> y el <indice_z> de cada celda.
	 * 
	 * 	Ejemplo:	grid[1][2,3] contiene la celda con <indice_y> = 1, <indice_x> = 2, <indice_z> = 3.
	 */
	void inicializarGrid() {

		grid = new List<Celda[,]>();

		for (int y = 0; y < tamano_y; ++y) {
			grid.Add (new Celda[tamano_x, tamano_z]);
			for (int x = 0; x < tamano_x; ++x) {
				for (int z = 0; z < tamano_z; ++z) {
					grid[y][x, z] = new Celda(x, y, z);
				}	
			}
		}

	}

	/*
	 *	Funcion: generarTerreno()
	 *
	 *	Genera el terreno en el entorno, el cual consiste de columnas de celdas solidas cuya base es y = 0, y cuya altura (en Y) 
	 *	esta determinada por el mapa de alturas cargado por MapLoader.
	 */
	void generarTerreno() {

		for (int i = 0; i < Controlador.map_loader.size_x; i++) {
			for (int j = 0; j < Controlador.map_loader.size_z; j++) {
				agregarColumnaSolida(i, Controlador.map_loader.Levels[j, i], j);
			}
		}
	}

	/*
	 *	Funcion: agregarColumnaSolida()
	 *
	 *	Genera una columna de celdas solidas (de terreno) en el entorno. La columna tiene <indice_x> = x, <indice_z> = z, su
	 *	base es <indice_y> = 0 y su altura es <indice_y> = y.
	 */
	void agregarColumnaSolida(int x, int y, int z) {
		for (int i = 0; i < y; ++i) {
			grid [i] [x, z].setSolida ();
		}
	}

	/*
	 *	Funcion: spawnAtCenter()
	 *	
	 *	Simula la introduccion de liquido en las celdas del centro del entorno.
	 */
	public void spawnAtCenter() {

		Celda cell;

		cell = getCelda (tamano_x/2, tamano_y-1, tamano_z/2);
		cell.setSaturacion(1f);

		cell = getCelda (tamano_x/2 - 1, tamano_y-1, tamano_z/2);
		cell.setSaturacion(1f);
		
		cell = getCelda (tamano_x/2, tamano_y-1, tamano_z/2 - 1);
		cell.setSaturacion(1f);
		
		cell = getCelda (tamano_x/2 - 1, tamano_y-1, tamano_z/2 - 1);
		cell.setSaturacion(1f);

		cell = getCelda (tamano_x/2, tamano_y-2, tamano_z/2);
		cell.setSaturacion(1f);
		
		cell = getCelda (tamano_x/2 - 1, tamano_y-2, tamano_z/2);
		cell.setSaturacion(1f);
		
		cell = getCelda (tamano_x/2, tamano_y-2, tamano_z/2 - 1);
		cell.setSaturacion(1f);
		
		cell = getCelda (tamano_x/2 - 1, tamano_y-2, tamano_z/2 - 1);
		cell.setSaturacion(1f);

	}

	/*
	 *	Funcion: spawnAtRandom()
	 *	
	 *	Simula la introduccion de liquido en celdas aleatorias del entorno.
	 */
	public void spawnAtRandom() {

		if (testDelay > 0) {

			testDelay = 0;

			float min_height = 0.7f;
			Vector3 indexes = new Vector3 (UnityEngine.Random.Range (0, tamano_x), UnityEngine.Random.Range (tamano_y * min_height, tamano_y), UnityEngine.Random.Range (0, tamano_z));
			Celda cell = getCelda ((int)indexes.x, (int)indexes.y, (int)indexes.z);
			if (cell.estaVacia () && !cell.esCeldaSolida ()) {
				cell.setSaturacion(1f);
			}

		}

		testDelay++;

	}

	/*
	 *	Funcion: spawnInCircle()
	 *	
	 *	Simula la introduccion de liquido en celdas alrededor del centro del entorno.
	 */
	public void spawnInCircle() {

		if (testDelay > 0) {

			testDelay = 0;

			Vector3 indexes = sicStartPosition;
			if(indexes.x <= tamano_x/4 && indexes.z < tamano_z/4*3) {
				indexes.z++;
			}
			if(indexes.z >= tamano_z/4*3 && indexes.x < tamano_x/4*3) {
				indexes.x++;
			}
			if(indexes.x >= tamano_x/4*3 && indexes.z > tamano_z/4) {
				indexes.z--;
			}
			if(indexes.z <= tamano_z/4 && indexes.x > tamano_x/4) {
				indexes.x--;
			}
			
			Celda cell = getCelda ((int) indexes.x, (int) indexes.y, (int) indexes.z);
			if(cell.estaVacia() && !cell.esCeldaSolida()) {
				cell.setSaturacion(1f);
			}
			
			sicStartPosition = indexes;

		}

		testDelay++;

	}

	// Retorna la celda con <indice_x> = x, <indice_y> = y, <indice_z> = z
	public Celda getCelda(int indice_x, int indice_y, int indice_z) {
		return grid[indice_y][indice_x, indice_z];
	}

	// Funcion no implementada
	/*public void setCeldaAlta (int indice_x, int indice_z, int indice_y_max, int indice_y_min, bool es_solida) {
	
		CeldaAlta celda_alta = new CeldaAlta (indice_x, indice_z, indice_y_max, indice_y_min, es_solida);

		for (int y = indice_y_min; y < indice_y_max+1; ++y) {
			Celda celda = getCelda(indice_x, y, indice_z);
			celda.linkCeldaAlta(celda_alta);
			if(simulacionEnProgreso) {
				rend.unrenderCelda(celda);
			}
		}

	}*/

	/*
	 *	Funcion: simularTick() 
	 *
	 *	Simula la distribucion de liquido de todas las celdas activas en un tick.
	 */
	public void simularTick() {
		simulacionEnProgreso = true;
		//Debug.Log (celdas_activas.Count);
		if(celdas_activas.Count != 0){
			for (int i = 0; i < celdas_activas.Count; ++i) {
				//cellda = celdas_activas[i];
				threads[i%100] = new ManualResetEvent(false);
				//Celda cell = celdas_activas[i];
				ThreadPool.QueueUserWorkItem(simularTickCelda, celdas_activas[i]);
			}
			//WaitHandle.WaitAll(threads);
		}

		float sat_tot_tick_actual = saturacion_total;
		if (sat_tot_tick_actual - sat_tot_tick_anterior < 0.1f) {
			tasa_incr_sat = 0f;
		} 
		else {
			tasa_incr_sat = sat_tot_tick_actual - sat_tot_tick_anterior;
		}

		sat_tot_tick_anterior = saturacion_total;
	}


	/*ManualResetEvent waithandle = new ManualResetEvent(false);
	waithandles.Add(waithandle);
	Thread liquid  = new Thread(new ThreadStart(() => {	simularTickCelda(celdas_activas[i]);
		waithandle.Set();
	}));
	liquid.Start();
	//simularTickCelda(celdas_activas[i])
	//a;adir un tread por cada una de las celdas activas 
}
WaitHandle.WaitAll(waithandles.ToArray());*/

	/*
	 * 	Funcion: simularTickCelda()
	 * 
	 * 	Simula la distribucion de liquido desde <celda> hacia sus celdas vecinas.
	 * 
	 * 	<celda> no debe ser solida, y su saturacion debe ser mayor que 0 (de lo contrario no hay liquido que distribuir en <celda>).
	 */
	void simularTickCelda(object state) {
		Celda celda = (Celda)state;
		//Celda celda = cellda;
		if (!celda.es_solido && celda.saturacion > 0f) {

			// DEBUG BREAKPOINT
			if(Controlador.debugflag1) {
				if(true) {
					int i = 0;
				}
			}
			// END OF DEBUG BREAKPOINT

			celda.distribuirLiquido ();
			doneEvent.Set();
		}
	}

}
