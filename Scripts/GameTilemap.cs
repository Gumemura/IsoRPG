using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameTilemap : MonoBehaviour
{
	public Transform ball;

	[Range(0, 1)]
	public float obstacleCadence;//the lower the value, more gaps(aka obstacles) on map
	public float scale;//scale of perlin noise
	private float offset; //moves the perlin (map seed)

	private Grid gameGrid; //grid that will host the tilemaps

	private Vector3Int origin;//origin of grid (home plate)

	//the tilemaps
	private Tilemap walkableFloorTM;
	private Tilemap obstaclesTM;
	private Tilemap obstaclesFillTM;
	private Tilemap decorationTM;

	//array with all coordinates of tilemaps
	private List<Vector3Int> allCoords3Int = new List<Vector3Int>();
	private List<Vector3> walkableArea  = new List<Vector3>();

	private Vector3Int gridSize;
	void Start()
	{
		//getting tilesmap
		gameGrid = GetComponent<Grid>();

		walkableFloorTM = transform.Find("WalkableFloor").GetComponent<Tilemap>();
		obstaclesFillTM = transform.Find("ObstacleFill").GetComponent<Tilemap>();
		obstaclesTM = transform.Find("Obstacle").GetComponent<Tilemap>();
		decorationTM = transform.Find("Decoration").GetComponent<Tilemap>();

		walkableFloorTM.CompressBounds();//compressing to delimit tilemap bounds to cells where tiles are present

		gridSize = walkableFloorTM.size;

		origin = walkableFloorTM.origin;//origin = home plate

		Vector3Int cell;
		Vector3 convertedCell;

		//filling arrays with all map's cell
		for(int i = 0; i < gridSize.x; i++){
			for(int j = 0; j < gridSize.y; j++){
				cell = origin + new Vector3Int(i, j, 0);

				offset = Random.Range(0, 100);
				float xCoord = (float)i / gridSize.x * scale + offset;
				float yCoord = (float)j / gridSize.y * scale + offset;


				print(Mathf.PerlinNoise(xCoord, yCoord));
				if(Mathf.PerlinNoise(xCoord, yCoord) < obstacleCadence){
					print("ssdd");
					creatingGaps(cell);
				}

				if(walkableFloorTM.HasTile(origin + new Vector3Int(i, j, 0))){
					convertedCell = walkableFloorTM.GetCellCenterWorld(cell);

					allCoords3Int.Add(cell);
					walkableArea.Add(convertedCell);
				}
			}
		}

	}

	private void creatingGaps(Vector3Int cellToRemove){
		walkableFloorTM.SetTile(cellToRemove, null);
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
