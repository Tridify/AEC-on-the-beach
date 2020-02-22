using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCubesRandomly : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> building = new List<GameObject>();

    public GameObject[] hs;
    [SerializeField]
    private float minDistance = 10;
    [SerializeField]
    private int amountOfHouses = 20;

    private Vector3 FindNewPos(Vector3 vertices, float xPos, float zPos)
    {
        Vector3 newPos = new Vector3(xPos, vertices.y, zPos);
        Collider[] neighbours = Physics.OverlapSphere(newPos, minDistance);
        if (neighbours.Length > 0)
        {
            return Vector3.zero;
        }else
        {
            return newPos;
        }
    }
 
    public void CreateHousesInBegin(Vector3[] vertices, Vector3[] normals, int xSize, int zSize)
    {
        int index = 0;
        for (int a = 0; a < amountOfHouses; a++)
        {
            Vector3 newPos = Vector3.zero;
            int indexerOfCVertice;
            do
            {
                int row = (int)Random.Range(15, xSize - 15);
                int zPos = (int)Random.Range(15, zSize - 15);
                indexerOfCVertice = row * zSize + zPos;
                float angle = Vector3.Dot(Vector3.up, normals[indexerOfCVertice]);  //check if vertes is flat
                if (angle >= 0.8f)
                {
                    newPos = FindNewPos(vertices[indexerOfCVertice], row, zPos);
                }

            } while (newPos == Vector3.zero);

            GameObject newObject = Instantiate(hs[index], newPos*0.02f, transform.rotation);
            newObject.GetComponent<HouseInfo>().indexOfVertice = indexerOfCVertice;
            newObject.GetComponent<HouseInfo>().heightNow = vertices[indexerOfCVertice].y;
            building.Add(newObject);
            index++;
            if(index >= hs.Length)
            {
                index = 0;
            }
        }
    }


    public void Checkhouses(Vector3[] vertices, Vector3[] normals, int xSize, int zSize)
    {
        foreach (GameObject b in building)
        {
            Vector3 newPos = Vector3.zero;
            int indexerOfCVertice;
            if (b.GetComponent<HouseInfo>().heightNow == vertices[b.GetComponent<HouseInfo>().indexOfVertice].y && Vector3.Dot(Vector3.up, normals[b.GetComponent<HouseInfo>().indexOfVertice]) >= 0.8f)
            {
                
            }else
            {
                do
                {
                    int row = (int)Random.Range(15, xSize - 15);
                    int zPos = (int)Random.Range(15, zSize - 15);
                    indexerOfCVertice = row * zSize + zPos;
                    float angle = Vector3.Dot(Vector3.up, normals[indexerOfCVertice]);  //check if vertes is flat
                    if (angle >= 0.8f)
                    {
                        newPos = FindNewPos(vertices[indexerOfCVertice], row, zPos);
                    }
                } while (newPos == Vector3.zero);
                b.transform.position = newPos * 0.02f;
                b.GetComponent<HouseInfo>().heightNow = vertices[indexerOfCVertice].y;
                b.GetComponent<HouseInfo>().indexOfVertice = indexerOfCVertice;
            }   
        }
    }
}
