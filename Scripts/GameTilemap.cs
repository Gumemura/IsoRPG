using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; 

public class GameTilemap : MonoBehaviour
{
	[Header("Debug")]
	public TileBase blueDebug;
	public TextMeshProUGUI gridCoords;

	private int PLAIN = 1;
	private int FOREST = 2;
	private int SNOW = 3;
	private int DESERT = 4;
	private int CITY = 5;
	private int LAVA = 6;
	private int CAVE = 7;

	public Transform ball;

	[Header("Obstacle Cadence")]
	public float chanceRiverTurn = .1f;//chance of river to turn 90° on his path
	[Range(.1f, .35f)]
	public float obstacleCadence;//the lower the value, more gaps(aka obstacles) on map
	private float offset; //moves the perlin (map seed)

	private Grid gameGrid; //grid that will host the tilemaps

	private Vector3Int origin;//origin of grid (home plate)

	//the tilemaps
	private Tilemap walkableFloorTM;
	private Tilemap obstaclesFillTM;
	private Tilemap decorationTM;
	private Tilemap visibleGridTM;

	//array with all coordinates of tilemaps
	private List<Vector3Int> allCoords3Int = new List<Vector3Int>();
	private List<Vector3> walkableArea  = new List<Vector3>();

	private Vector3Int gridSize;

	[Header("Grid")]
	public TileBase visibleGrid;

	[Header("Plain/Forest")]
	public TileBase treesGrassTile;
	public Transform[] treesGrassObstacles;
	public Transform[] treesGrassDecoration;

	[Header("Snow")]
	public TileBase snowTile;
	public Transform[] snowObstacles;
	public Transform[] snowDecoration;

	[Header("Desert")]
	public TileBase desertTile;
	public Transform[] desertObstacles;
	public Transform[] desertDecoration;

	[Header("City")]
	public TileBase cityTile;
	public Transform[] cityObstacles;
	public Transform[] cityDecoration;

	[Header("Lava")]
	public TileBase lavaTile;
	public Transform[] lavaObstacles;
	public Transform[] lavaDecoration;

	[Header("Cave")]
	public TileBase caveTile;
	public Transform[] caveObstacles;
	public Transform[] caveDecoration;

	[Header("River Forest/Plains")]
	public TileBase riverPlainsSEtNW;
	public TileBase riverPlainsSWtNE;
	public TileBase riverPlainsHomePlate;
	public TileBase riverPlains1thBase;
	public TileBase riverPlains2thBase;
	public TileBase riverPlains3thBase;

	//TBI = to be implemented
	[Header("River Snow TBI")]
	public TileBase riverSnowSEtNW;

	[Header("River Desert TBI")]
	public TileBase riverDesertSEtNW;

	[Header("River Lava TBI")]
	public TileBase riverLavaSEtNW;

	[Header("River Cave TBI")]
	public TileBase riverCaveSEtNW;

	void Start()
	{
		//getting tilesmap
		gameGrid = GetComponent<Grid>();

		walkableFloorTM = transform.Find("WalkableFloor").GetComponent<Tilemap>();
		obstaclesFillTM = transform.Find("ObstacleFill").GetComponent<Tilemap>();
		decorationTM = transform.Find("Decoration").GetComponent<Tilemap>();
		visibleGridTM = transform.Find("VisibleGrid").GetComponent<Tilemap>();

		walkableFloorTM.CompressBounds();//compressing to delimit tilemap bounds to cells where tiles are present

		gridSize = walkableFloorTM.size;

		origin = walkableFloorTM.origin;//origin = home plate

		riverMaker(FOREST);
		mapGenerator(FOREST);
	}

	Vector3Int convertedMouse;
	void Update(){
		convertedMouse = gameGrid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition - new Vector3(0,0,-10)));
		//convertedMouse = new Vector3Int(convertedMouse.x, convertedMouse.y, 0);
		gridCoords.text = convertedMouse.ToString();

		if(Input.GetMouseButtonDown(0)){
			if(visibleGridTM.GetTile(convertedMouse)){
				walkableFloorTM.SetTile(convertedMouse, blueDebug);
			}	
		}
	}

	//creates a river in the map
	private void riverMaker(int mapBiome){
		TileBase riverSEtNW;// \
		TileBase riverSWtNE;// /
 		TileBase riverHP;
		TileBase river1b;
		TileBase river2b;
		TileBase river3b;

		riverSEtNW = riverPlainsSEtNW;
		riverSWtNE = riverPlainsSWtNE;
 		riverHP = riverPlainsHomePlate;
		river1b = riverPlains1thBase;
		river2b = riverPlains2thBase;
		river3b = riverPlains3thBase;

		/*if(mapBiome == PLAIN || mapBiome == FOREST){//only diference btw plain and forest is the amount of forest
			riverSEtNW = riverPlainsSEtNW;
			riverSWtNE = riverPlainsSWtNE;
	 		riverHP = riverPlainsHomePlate;
			river1b = riverPlains1thBase;
			river2b = riverPlains2thBase;
			river3b = riverPlains3thBase;
		}else if(mapBiome == SNOW){

		}else if(mapBiome == DESERT){

		}else if(mapBiome == LAVA){

		}else{//cave

		}*/

		// 0 = SE
		// 1 = NE
		// 2 = NW
		// 3 = SW
		int startingSide = Random.Range(0, 4);

		int ySE = origin.y;
		int xNE = origin.x + gridSize.x - 1;
		int yNW = origin.y + gridSize.y - 1;
		int xSW = origin.x;

		Vector3Int riverCoords;
		int xCounter = 0, yCounter = 0;
		int riverMainFlow = 0;

		TileBase activeTile;

		if(startingSide == 0){
			riverCoords = new Vector3Int(Random.Range(xSW, xNE), ySE, 0);
			yCounter = riverMainFlow = 1;
		}else if(startingSide == 1){
			riverCoords = new Vector3Int(xNE, Random.Range(ySE, yNW), 0);
			xCounter = riverMainFlow = -1;
		}else if(startingSide == 2){
			riverCoords = new Vector3Int(Random.Range(xSW, xNE), yNW, 0);
			yCounter = riverMainFlow = -1;
		}else{
			riverCoords = new Vector3Int(xSW, Random.Range(ySE, yNW), 0);
			xCounter = riverMainFlow = 1;
		}

		bool riverDone = false;
		int turningDirection;

		while(!riverDone){
			activeTile = yCounter == 0? riverSWtNE: riverSEtNW;

			if(Random.Range(0f, 1f) < chanceRiverTurn){
				turningDirection = Random.Range(0f, 1f) < .5f? 1: -1;

				if(startingSide == 0 || startingSide == 2){
					xCounter = xCounter == 0? turningDirection: 0;
					yCounter = yCounter == 0? riverMainFlow: 0;

					if(riverMainFlow == 1 && xCounter == 1){
						activeTile = river3b;
					}else if(riverMainFlow == 1 && (xCounter == -1 || xCounter == 0)){
						activeTile = river2b;
					}else if(riverMainFlow == -1 && (xCounter == 1 || xCounter == 0)){
						activeTile = riverHP;
					}else if(riverMainFlow == -1 && xCounter == -1){
						activeTile = river1b;
					}
				}else{
					xCounter = xCounter == 0? riverMainFlow: 0;
					yCounter = yCounter == 0? turningDirection: 0;

					if(riverMainFlow == 1 && yCounter == 1){
						activeTile = river1b;
					}else if(riverMainFlow == 1 && (yCounter == -1 || yCounter == 0)){
						activeTile = river3b;
					}else if(riverMainFlow == -1 && (yCounter == 1 || yCounter == 0)){
						activeTile = riverHP;
					}else if(riverMainFlow == -1 && yCounter == -1){
						activeTile = river3b;
					}
				}
			}

			obstaclesFillTM.SetTile(riverCoords, activeTile);
			walkableFloorTM.SetTile(riverCoords, null);

			riverCoords += new Vector3Int(xCounter, yCounter, 0);
			if(riverCoords.y < ySE || riverCoords.y > yNW || riverCoords.x > xNE || riverCoords.x < xSW){
				riverDone = true;
			}
		}
	}

	private void mapGenerator(int mapBiome){
		/*
		CREATES THE MAP

		fills the tilemap with designated tile

		PARAMETERS GOES FOR:
		1 plain
		2 forest 
		3 snow
		4 desert
		5 city (few buildings)
		6 farm
		7 lava
		8 cave
		*/
		Vector3Int cell;
		Vector3 convertedCell;

		TileBase tile;
		Transform[] obstacles;
		Transform[] decoration;

		int selectedObstacle;

		if(mapBiome == PLAIN || mapBiome == FOREST){//only diference btw plain and forest is the amount of forest
			obstacleCadence = .2f;
			if(mapBiome == FOREST){
				obstacleCadence = .3f;
			}
			tile = treesGrassTile;
			obstacles = treesGrassObstacles;
		}else if(mapBiome == SNOW){
			tile = snowTile;
			obstacles = snowObstacles;

		}else if(mapBiome == DESERT){
			obstacleCadence = .2f;
			tile = desertTile;
			obstacles = desertObstacles;

		}else if(mapBiome == CITY){
			tile = cityTile;
			obstacles = cityObstacles;

		}else if(mapBiome == LAVA){
			tile = treesGrassTile;
			obstacles = lavaObstacles;

		}else{
			tile = lavaTile;
			obstacles = caveObstacles;
		}

		//filling arrays with all map's cell
		for(int i = 0; i < gridSize.x; i++){
			for(int j = 0; j < gridSize.y; j++){
				cell = origin + new Vector3Int(i, j, 0);
				convertedCell = walkableFloorTM.GetCellCenterWorld(cell);

				offset = Random.Range(0, 100);
				float xCoord = (float)i / gridSize.x + offset;
				float yCoord = (float)j / gridSize.y + offset;

				visibleGridTM.SetTile(cell, visibleGrid);

				//creating gaps on map where will be inserted obstacles
				if(Mathf.PerlinNoise(xCoord, yCoord) < obstacleCadence){
					walkableFloorTM.SetTile(cell, null);

					if(obstaclesFillTM.GetTile(cell) == null){
						selectedObstacle = Random.Range(0, obstacles.Length);//selecting a random object to fill the cell (creates cell variation)

						//placing visible obstacles
						obstaclesFillTM.SetTile(cell, tile);
						Instantiate(obstacles[selectedObstacle], convertedCell, Quaternion.identity, transform.Find("Obstacles"));
					}
				}else{
					if(obstaclesFillTM.GetTile(cell) == null){
						walkableFloorTM.SetTile(cell, tile);

						allCoords3Int.Add(cell);
						walkableArea.Add(convertedCell);
					}
				}
			}
		}
	}

	public List<Vector3> getWalkableArea(){
		//all walkable area
		return walkableArea;
	} 

	public List<Vector3Int> getWalkableArea3Int(){
		//all walkable area as Vector3
		return allCoords3Int;
	}
}
