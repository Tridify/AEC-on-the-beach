using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class CornerObject {

    public GameObject obj;
    public bool hasTexture;

    public CornerObject(GameObject gObject, bool value = false) {
        obj = gObject;
        hasTexture = value;
    }
}

[RequireComponent(typeof(ARTrackedImageManager))]
public class CornerImageManager : MonoBehaviour {

    [SerializeField]
    private GameObject[] m_CornerPrefabs;

    private ARTrackedImageManager m_TrackedImageManager;

    private Dictionary<string, CornerObject> m_CornerObjects = new Dictionary<string, CornerObject>();

    void Awake() {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        // setup all game objects in dictionary
        foreach (GameObject arObject in m_CornerPrefabs) {
            GameObject newARObject = Instantiate(arObject, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            m_CornerObjects.Add(arObject.name, new CornerObject(newARObject));
        }

    }

    void OnEnable() {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable() {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }


    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (ARTrackedImage trackedImage in eventArgs.added) {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated) {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed) {

            m_CornerObjects[trackedImage.name].obj.SetActive(false);
        }
    }

    private void UpdateARImage(ARTrackedImage trackedImage) {
        // Assign and Place Game Object
        AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position, trackedImage.referenceImage.texture, trackedImage.size);

        //Debug.Log($"trackedImage.referenceImage.name: {trackedImage.referenceImage.name}");
    }

    void AssignGameObject(string name, Vector3 newPosition, Texture2D texture, Vector3 size) {

        if (m_CornerPrefabs != null) {
            GameObject goARObject = m_CornerObjects[name].obj;


            goARObject.SetActive(true);
            goARObject.transform.position = newPosition;
            foreach (CornerObject cornerGo in m_CornerObjects.Values) {
                //Debug.Log($"Go in arObjects.Values: {go.name}");
                if (!cornerGo.hasTexture) {
                    cornerGo.hasTexture = true;

                    var material = cornerGo.obj.GetComponentInChildren<MeshRenderer>().material;
                    material.mainTexture = texture;
                }
                // The image extents is only valid when the image is being tracked
                cornerGo.obj.transform.localScale = new Vector3(size.x * 0.1f, size.y * 0.1f, size.y * 0.1f);
                if (cornerGo.obj.name != name) {
                cornerGo.obj.SetActive(false);
                }
            }
        }
    }

}
