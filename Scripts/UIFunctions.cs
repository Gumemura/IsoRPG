using UnityEngine.Tilemaps;
using UnityEngine;

public class UIFunctions : MonoBehaviour
{
    public Tilemap grid;

    public void turnGridOnOff(){
        grid.gameObject.SetActive(!grid.gameObject.active);
    }
}
