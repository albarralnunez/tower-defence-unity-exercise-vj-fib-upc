﻿using UnityEngine;
using System.Collections.Generic;


public class Town : MonoBehaviour {

	public GameObject[] buildings;
	public GameObject[] walls;
	public GameObject selection;
	public float spaceBetweenBuildings = 15;
	public float secondsToBuild = 10;
	public int people = 5;
	public int radius = 150;
	
	Queue<GameObject> sideBuildings;
	Queue<GameObject> wallsBuilt;
	int level =1;
	int wallLevel =0;
	float timecont =0;
	GameObject selectionHighlight;

	// Use this for initialization
	void Start () {
		sideBuildings= new Queue<GameObject>();
		wallsBuilt= new Queue<GameObject>();
		int indx = (int)Random.Range(0,buildings.Length);
		GameObject building = (GameObject) Instantiate(buildings[indx], transform.position, buildings[indx].transform.rotation);
		building.transform.parent = transform;
		sideBuildings.Enqueue(building);
	}

	void Awake () {
		selectionHighlight = (GameObject) Instantiate(selection, selection.transform.position, transform.rotation);
		selectionHighlight.transform.localScale = new Vector3(radius, 1, radius);
		selectionHighlight.SetActive(false);	
	}

	void Update () {
		timecont += Time.deltaTime;
		if(timecont>= secondsToBuild) {
			timecont = 0;
			Vector3 pos = new Vector3(0,0,0);
			bool found = false;
			while(!found) {
				GameObject sideBuilding = sideBuildings.Peek();
				for(int i=0; i< 4 && !found; ++i) {
					pos = sideBuilding.transform.position;
					switch(i) {
						case 0: pos.x -= spaceBetweenBuildings; break;
						case 1: pos.z -= spaceBetweenBuildings; break;
						case 2: pos.x += spaceBetweenBuildings; break;
						case 3: pos.z += spaceBetweenBuildings; break;
					}
					found = true;
					for(int j = 0; j< transform.childCount && found; ++j) {
						Vector3 posch = transform.GetChild(j).position;
						if(pos.x == posch.x && pos.z == posch.z) found = false;
					}
				}
				if(!found) sideBuildings.Dequeue();
				else {
					int b = Random.Range(0,buildings.Length);
					GameObject building = (GameObject) Instantiate(buildings[b], new Vector3(pos.x, 0, pos.z), buildings[b].transform.rotation);
					building.transform.parent = transform;
					sideBuildings.Enqueue(building);
				}
			}
			if(wallLevel>0) {
				destroyWalls();
				buildWalls();
			}
		}
	}

	/*
	// Update is called once per frame
	void Update () {
		timecont += Time.deltaTime;
		if(timecont>= secondsToBuild) {
			timecont = 0;
			bool found = false;
			int rnd = Random.Range(0,transform.childCount);
			Vector3 pos = Vector3.zero;
			while(!found) {
				int indx = Random.Range (0,4);
				for(int i=0; i< 4; ++i) {
					pos = transform.GetChild(rnd).position;
					switch(indx) {
						case 0: pos.x -= spaceBetweenBuildings; break;
						case 1: pos.z -= spaceBetweenBuildings; break;
						case 2: pos.x += spaceBetweenBuildings; break;
						case 3: pos.z += spaceBetweenBuildings; break;
					}
					indx++; if(indx>3) indx =0;
					found =true;
					for(int j = 0; j< transform.childCount && found; ++j) {
						Vector3 posch = transform.GetChild(j).position;
						if(j!= rnd && pos.x == posch.x && pos.z == posch.z) found = false;
					}
				}
				++rnd; if(rnd> transform.childCount-1) rnd =0;
			}
			int b = Random.Range(0,buildings.Length);
			GameObject building = (GameObject) Instantiate(buildings[b], new Vector3(pos.x, 0, pos.z), buildings[b].transform.rotation);
			building.transform.parent = transform;
		}
	}
	 */

	public void setHighlight(bool active) {
		selectionHighlight.SetActive(active);
	}

	public int getLevel() {
		return level;
	}

	public int getPeople() {
		return Mathf.Max(people, people+transform.childCount-2);
	}

	public void wallLevelUp() {
		++wallLevel; 
		if(wallLevel>walls.Length) wallLevel = walls.Length;
		destroyWalls ();
		buildWalls ();
	}

	void destroyWalls() {
		while(wallsBuilt.Count>0) {
			Destroy (wallsBuilt.Dequeue());
		}
	}

	void buildWalls() {
		foreach(GameObject building in sideBuildings) {
			Vector3 pos = new Vector3 (0,0,0);
			for(int i=0; i< 4; ++i) {
				pos = building.transform.position;
				switch(i) {
				case 0: pos.x -= spaceBetweenBuildings; break;
				case 1: pos.z -= spaceBetweenBuildings; break;
				case 2: pos.x += spaceBetweenBuildings; break;
				case 3: pos.z += spaceBetweenBuildings; break;
				}
				bool found = true;
				for(int j = 0; j< transform.childCount && found; ++j) {
					Vector3 posch = transform.GetChild(j).position;
					if(pos.x == posch.x && pos.z == posch.z) found = false;
				}
				if(found) {
					switch(i) {
					case 0: wallsBuilt.Enqueue((GameObject)Instantiate(walls[wallLevel-1], new Vector3(pos.x+walls[wallLevel-1].transform.position.x+spaceBetweenBuildings/2, 0, pos.z+walls[wallLevel-1].transform.position.z), Quaternion.Euler(new Vector3(0,90,0)))); break;
					case 1: wallsBuilt.Enqueue((GameObject)Instantiate(walls[wallLevel-1], new Vector3(pos.x+walls[wallLevel-1].transform.position.x, 0, pos.z+walls[wallLevel-1].transform.position.z+spaceBetweenBuildings/2), Quaternion.Euler(new Vector3(0,0,0)))); break;
					case 2: wallsBuilt.Enqueue((GameObject)Instantiate(walls[wallLevel-1], new Vector3(pos.x+walls[wallLevel-1].transform.position.x-spaceBetweenBuildings/2, 0, pos.z+walls[wallLevel-1].transform.position.z), Quaternion.Euler(new Vector3(0,90,0)))); break;
					case 3: wallsBuilt.Enqueue((GameObject)Instantiate(walls[wallLevel-1], new Vector3(pos.x+walls[wallLevel-1].transform.position.x, 0, pos.z+walls[wallLevel-1].transform.position.z-+spaceBetweenBuildings/2), Quaternion.Euler(new Vector3(0,0,0)))); break;
					}
				}
			}
		}

	}

}