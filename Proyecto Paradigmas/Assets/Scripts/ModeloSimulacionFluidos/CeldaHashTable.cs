using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * 	Clase: CeldaHashTable
 * 
 * 	INCOMPLETO
 *  Esta clase no esta implementada todavia
 *
 */

public class CeldaHashTable {

	/*public int elements;
	public int subentries_per_entry;
	public int elements_per_subentry;
	public int num_entries;
	public int num_subentries;
	public List<Vector3> indexes_saved;
	
	private List<Celda>[,] hash_table;
	
	public CeldaHashTable(int num_entries, int num_subentries, int elements, int cells_x, int cells_y, int cells_z) {
		this.elements = elements;
		this.num_entries = num_entries;
		this.num_subentries = num_subentries;
		this.indexes_saved = new List<Vector3> ();




		this.subentries_per_entry = cells_x / num_entries;
		this.elements_per_subentry = subentries_per_entry / num_subentries + 1;

		
		hash_table = new List<Celda>[num_entries];
		for (int i = 0; i < num_entries; ++i) {
			hash_table[i] = new List<Celda> ();
		}
		
	}
	
	/*public void addElem(Celda cell) {
		int entry_index = 
		int elem_index = cell.index_y * world_chunks_per_side + chunk.index_x;
		List<TerrainChunk> entry = hash_table [getEntryIndex (elem_index)];
		
		for(int i = 0; i < entry.Count; ++i) {
			if(entry[i].checkMatch(chunk.index_x, chunk.index_y)) {
				// already exists
				return;
			}
		}
		
		indexes_saved.Add (new Vector2(chunk.index_x, chunk.index_y));
		entry.Add (chunk);
		
	}
	
	public TerrainChunk getElem(int index_x, int index_y) {
		int elem_index = index_y * world_chunks_per_side + index_x;
		List<TerrainChunk> entry = hash_table [getEntryIndex (elem_index)];
		
		for (int i = 0; i < entry.Count; ++i) {
			if(entry[i].checkMatch(index_x, index_y)) {
				if(elemIsValid(entry[i])) {
					return entry[i];
				}
			}
		}
		
		return null;

	}
	
	public void removeElem(TerrainChunk chunk) {
		int elem_index = chunk.index_y * world_chunks_per_side + chunk.index_x;
		List<TerrainChunk> entry = hash_table [getEntryIndex (elem_index)];
		
		for (int i = 0; i < entry.Count; ++i) {
			if(entry[i].checkMatch(chunk.index_x, chunk.index_y)) {
				indexes_saved.Remove (new Vector2(chunk.index_x, chunk.index_y));
				entry.RemoveAt(i);
				return;
			}
		}
		
	}

	public List<Vector2> getSavedIndexes() {
		return indexes_saved;
	}
	
	public int getEntryIndex(int elem_index) {
		return (int) (elem_index / elems_per_entry);
	}
	
	public bool elemIsValid(TerrainChunk chunk) {
		if (chunk.index_x >= 0 && chunk.index_y >= 0 && chunk.index_x < world_chunks_per_side && chunk.index_y < world_chunks_per_side) {
			return true;
		}
		return false;
	}*/

}


