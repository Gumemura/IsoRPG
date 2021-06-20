using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameTilemap : MonoBehaviour
{
	private int PLAIN = 1;
	private int FOREST = 2;
	private int SNOW = 3;
	private int DESERT = 4;
	private int CITY = 5;
	private int LAVA = 6;
	private int CAVE = 7;

	public Transform ball;

	[Range(.1f, .35f)]
	public float obstacleCadence;//the lower the value, more gaps(aka obstacles) on map
	private float offset; //moves the perlin (map seed)

	private Grid gameGrid; //grid that will host the tilemaps

	private Vector3Int origin;//origin of grid (home plate)

	//the tilemaps
	private Tilemap walkableFloorTM;
	private Tilemap obstaclesTM;
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

	[Header("River")]
	public TileBase riverSEtNW;
	public TileBase riverSWtNE;
	//turning river

	void Start()
	{
		//getting tilesmap
		gameGrid = GetComponent<Grid>();

		walkableFloorTM = transform.Find("WalkableFloor").GetComponent<Tilemap>();
		obstaclesFillTM = transform.Find("ObstacleFill").GetComponent<Tilemap>();
		obstaclesTM = transform.Find("Obstacle").GetComponent<Tilemap>();
		decorationTM = transform.Find("Decoration").GetComponent<Tilemap>();
		visibleGridTM = transform.Find("VisibleGrid").GetComponent<Tilemap>();

		walkableFloorTM.CompressBounds();//compressing to delimit tilemap bounds to cells where tiles are present

		gridSize = walkableFloorTM.size;

		origin = walkableFloorTM.origin;//origin = home plate

		mapGenerator(FOREST);

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
					selectedObstacle = Random.Range(0, obstacles.Length);
					walkableFloorTM.SetTile(cell, null);

					//placing visible obstacles
					obstaclesFillTM.SetTile(cell, tile);
					Instantiate(obstacles[selectedObstacle], convertedCell, Quaternion.identity, obstaclesTM.transform);
				}else{
					walkableFloorTM.SetTile(cell, tile);


					allCoords3Int.Add(cell);
					walkableArea.Add(convertedCell);
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
