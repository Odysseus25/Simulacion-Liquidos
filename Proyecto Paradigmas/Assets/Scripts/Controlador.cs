using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
 * 	Clase: Controlador
 * 
 * 	Esta es la rutina principal de la aplicación. Si le ayuda, puede verlo como el "Main" del programa. 
 * 	La ejecución empieza en el método Start(), y desde ahí se inicializan los demás componentes.
 * 
 * 	La lógica de la simulación transcurre de forma cíclica. Es decir, una vez que la aplicación inicia, 
 *	esta se ejecuta por repeticiones, ciclos, o ticks, hasta que el usuario detenga la simulación. 
 *
 */

public class Controlador : MonoBehaviour {

	public int tamano_x;		// tamaño de la dimensión X del entorno (en celdas)
	public int tamano_y;  		// tamaño de la dimensión Y del entorno (en celdas)
	public int tamano_z;		// tamaño de la dimensión Z del entorno (en celdas)
	public float sensibilidad = 0.1f;

	public Heightmaps heightmap;	// mapa de alturas seleccionado para la simulacion

	public Transform center;
	public Slider sdelay_slider;

	public static Controlador controlador;
	public static Espacio espacio;
	public static EspRenderer esp_renderer; 
	public static MapLoader map_loader;
	int frame_delay = 0;
	int frames = 0;
	bool simulation_paused = false;

	public static bool debugflag1 = false;

	public UIController uicontroller;

	/*
	 *	Funcion: getHeightmapUrl()
	 *
	 *	Devuelve la ruta en el filesystem del mapa de alturas dado por <hm>.
	 */
	string getHeightmapUrl(Heightmaps hm) {
		switch (hm) {
			case Heightmaps.Canyon:
				return "/Resources/maps/hm5.png";
			case Heightmaps.Hills:
				return "/Resources/maps/hm3.png";
			case Heightmaps.Pool:
				return "/Resources/maps/hm1.png";
			case Heightmaps.Playground:
				return "/Resources/maps/hm8.png";
			default:
				return "/Resources/maps/hm5.png";
		}
	}

	/*
	 *	Funcion: Start()
	 *
	 *	En Unity, la funcion Start() de cualquier script que herede de MonoBehaviour se ejecuta automaticamente al inicio del programa.
	 *
	 *	-Guarda el mapa de alturas seleccionado como dato persistente, para que permanezca a traves del cambio de escenas desde "inicio"
	 * 	 hasta "simulacion"
	 * 	-Carga el mapa de alturas seleccionado.
	 *  -Carga e inicializa el renderer y el espacio de simulacion.
	 */
	void Start () {

		controlador = this;

		if (PersistentData.GlobalData != null && PersistentData.GlobalData.heightmapSelected) {
			heightmap = PersistentData.GlobalData.heightmap;
		}

		map_loader = new MapLoader (tamano_y, tamano_x, tamano_z, getHeightmapUrl(heightmap));

		esp_renderer = GameObject.Find ("Renderer").GetComponent<EspRenderer> ();
		esp_renderer.inicializarScheduledCells ();

		Espacio esp = new Espacio(tamano_x, tamano_y, tamano_z, sensibilidad);

		espacio.rend = esp_renderer;
		esp_renderer.inicializar (espacio);
	
	}

	/*
	 * 	Funcion: tick()
	 * 
	 *	Cada tick se divide en tres etapas o subrutinas, en las cuales:
 	 *	-Se introduce líquido en el entorno, si es requerido (checkSpawn)
 	 *	-Se simula un único tick de flujo de líquido para cada celda activa en el entorno (simularTick)
 	 *	-Se actualiza la visualización gráfica del entorno, o "rendering" (renderEspacio)
	 */
	void tick() {
		uicontroller.checkSpawn ();
		espacio.simularTick ();
		esp_renderer.renderEspacio (espacio);
	}

	/*
	 * 	Funcion: togglePauseSimulation()
	 * 
	 * 	Detiene/reanuda la simulacion.
	 */
	void togglePauseSimulation() {
		simulation_paused = !simulation_paused;
	}

	/*
	 *	Funcion: Update()
	 *
	 *	En Unity, la funcion Update() de cualquier script que herede de MonoBehaviour se ejecuta automaticamente cada ciclo del programa.
	 */
	void Update() {
		if (Input.GetKeyDown (KeyCode.Space)) {
			togglePauseSimulation ();
		}
		if (Input.GetKeyDown (KeyCode.Q)) {
			debugflag1 = true;
		}
	}

	/*
	 *	Funcion: FixedUpdate()
	 *
	 *	En Unity, la funcion FixedUpdate() de cualquier script que herede de MonoBehaviour se ejecuta automaticamente cada ciclo del
	 * 	programa, similar a la funcion Update(). La diferencia es que FixedUpdate() se ejecuta en intervalos de tiempo iguales, mientras
	 *  Update() se ejecuta luego de terminar el ciclo anterior, sin considerar tiempos.
	 */
	void FixedUpdate () {
		updateFrameDelay ();
		if (frames > frame_delay) {
			if (!simulation_paused) {
				tick ();
			}
			frames = 0;
		}
		++frames;
	}

	/*
	 *	Funcion: updateFrameDelay()
	 *
	 *	Modifica <frame_delay> para reflejar la velocidad de ejecucion de ciclos, segun el slider de velocidad en la interfaz grafica.
	 */
	public void updateFrameDelay() {
		frame_delay = Mathf.FloorToInt(sdelay_slider.value);
	}

}
