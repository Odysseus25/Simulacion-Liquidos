using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * 	Clase: MapLoader
 * 
 * 	Construye un mapa de niveles <Levels> a partir del archivo de imagen en la ruta <FilePath>.
 *  La imagen debe estar en escala de grises y en formato PNG.
 *  El tono de gris de cada pixel en la imagen es convertido a un entero que representa el nivel de ese pixel.
 *
 */

public class MapLoader {

	string FilePath;			// Ruta de la imagen
	Texture2D loadedImage;		// Imagen cargada

	public int size_x;
	public int size_z;
	public int level_count;		// Cantidad de niveles permitidos

	public int[,] Levels;		// Matriz de niveles de la imagen
	float level_difference;

	public MapLoader(int level_count, int size_x, int size_z, string image_path) {
		this.level_count = level_count;
		initSizes (size_x, size_z);
		initAttributes(level_count);
		openFile(image_path);
		setLevels ();
	}

	// Carga el archivo en la ruta <path> y lo guarda en <loadedImage>.
	public void openFile(string path) {

		FilePath = Application.dataPath + path;
		if (System.IO.File.Exists (FilePath)) {
			byte[] bytes = System.IO.File.ReadAllBytes (FilePath);
			loadedImage = new Texture2D (1, 1);
			loadedImage.LoadImage (bytes);
		} 
		else {
			Debug.LogError("Unable to open file: "+path);
		}

	}
	
	private Vector3 convertColorFromStandard(Vector3 color_standard) {
		return new Vector3(color_standard.x/255.0f, color_standard.y/255.0f, color_standard.z/255.0f);
	}

	private Vector3 convertColorToStandard(Vector3 color_norm) {
		return new Vector3( (int) (color_norm.x*255.0f), (int) (color_norm.y*255.0f), (int) (color_norm.z*255.0f) );
	}

	// Asigna a cada entrada de <Levels> un nivel segun el tono de gris del mismo pixel en la imagen cargada.
	private void setLevels() {

		Color[] pix = loadedImage.GetPixels();
		Levels = new int[size_z, size_x];
		for (int i = 0; i < size_z; ++i) {
			for(int j = 0; j < size_x; ++j) {
				Levels[i, j] = getLevelFromGrayscale(pix[i * size_z + j].grayscale);
			}
		}

	}

	// Convierte <grayscale_value>, un tono de gris en forma de flotante (0.0 = negro, 1.0 = blanco) a un nivel entero.
	private int getLevelFromGrayscale(float grayscale_value) {
		return (int) (grayscale_value * level_count);
	}


	private void initSizes(int size_x, int size_z) {
		this.size_x = size_x;
		this.size_z = size_z;
	}

	private void initAttributes(int level_count) {
		this.level_count = level_count;
		this.level_difference = 1f / level_count;
	}



}
