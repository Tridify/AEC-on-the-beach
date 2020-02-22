using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCubesRandomly : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> houses = new List<GameObject>();
    [SerializeField]
    private GameObject hs;
    [SerializeField]
    private float minDistance = 30;
    [SerializeField]
    private float size = 200;
    [SerializeField]
    private int amountOfHouses = 5;

    private Vector3 FindNewPos(float height) {
        Vector3 newPos = Vector3.zero;
        Collider[] neighbours = new Collider[0];
        do {
            // draw a new position
            newPos = new Vector3(Random.Range(-size, size), height, Random.Range(-size, size));
            // get neighbours inside minDistance:
            neighbours = Physics.OverlapSphere(newPos, minDistance);
            // if there's any neighbour inside range, repeat the loop:
        } while (neighbours.Length > 0);

        return newPos; // otherwise return the new position
    }
 
    public void CreateHouses(int height)
    {
        for (int a = 0; a < amountOfHouses; a++){
            Vector3 newPos = FindNewPos(height);
            GameObject newObject = Instantiate(hs, newPos, transform.rotation);
            houses.Add(newObject);
        }
    }
}
