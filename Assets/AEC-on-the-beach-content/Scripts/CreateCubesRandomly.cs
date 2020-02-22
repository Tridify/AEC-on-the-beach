using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCubesRandomly : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> houses = new List<GameObject>();

    public GameObject[] hs;
    [SerializeField]
    private float minDistance = 10;
    [SerializeField]
    private int amountOfHouses = 20;

    private Vector3 FindNewPos(Vector3[] vertices, Vector3[] normals, int xSize, int zSize) {  //x 640, z 480
        Vector3 newPos = Vector3.zero;
        Collider[] neighbours = new Collider[0];
        int misses = 0;
        do {
            int row = (int)Random.Range(15, xSize-15);
            int zPos = (int)Random.Range(15, zSize-15);
            float angle = Vector3.Dot(Vector3.up, normals[row * zSize + zPos]);  //check if vertes is flat
            if(angle >= 0.8f)
            {
                float height = vertices[row * zSize + zPos].y;
                // draw a new position
                newPos = new Vector3(row, height, zPos);
                // get neighbours inside minDistance:
                neighbours = Physics.OverlapSphere(newPos, minDistance);
                // if there's any neighbour inside range, repeat the loop:
            }else
            {
                neighbours = new Collider[1];
                misses++;
                if(misses > 200) //if there is no place for house
                {
                    break;
                }
            }
        } while (neighbours.Length > 0);

        return newPos; // otherwise return the new position
    }
 
    public void CreateHouses(Vector3[] vertices, Vector3[] normals, int xSize, int zSize)
    {
        int index = 0;
        for (int a = 0; a < amountOfHouses; a++){
            Vector3 newPos = FindNewPos(vertices, normals, xSize, zSize);
            if(newPos == Vector3.zero)
            {
                Debug.Log("no more room for houses");
                return;
            }else
            {
                GameObject newObject = Instantiate(hs[index], newPos*0.02f, transform.rotation);
                houses.Add(newObject);
                index++;
                if(index >= hs.Length)
                {
                    index = 0;
                }
            }
        }
    }
}
