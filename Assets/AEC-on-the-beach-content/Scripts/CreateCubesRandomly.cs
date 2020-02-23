using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCubesRandomly : MonoBehaviour
{
    [SerializeField]
    private int heightLimit = 1000;
    [SerializeField]
    private int lowLimit = 600;


    public void Checkhouses(Vector3[] vertices, Vector3[] normals, Building[] buildings)
    {

        for (int z = 0; z < 32; z++)
        {
            for (int x = 0; x < 24; x++)
            {
                var buildingIndex = z * 24 + x;
                var meshIndex = z * (640 / 32) * 480 + x * (480 / 24);
                var b = buildings[buildingIndex];
                float height = vertices[meshIndex].y;
                if (height >= lowLimit && height <= heightLimit)
                {
                    var pos = vertices[meshIndex];
                    b.transform.position = pos * 0.02f;
                    b.gameObject.SetActive(true);
                }
                else
                {
                    b.gameObject.SetActive(false);
                }
            }
        }
    }
}
