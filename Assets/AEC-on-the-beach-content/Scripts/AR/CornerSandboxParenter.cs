using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CornerType {
    Red,
    Green,
    Blue,
    Purple
}
public class CornerSandboxParenter : MonoBehaviour {

    public CornerType cornerType = CornerType.Red;
    public Vector3 sandboxOffset = Vector3.zero;

    private GameObject sandbox;

    [SerializeField]
    [Tooltip("Sandbox side length in meters")]
    private float sandboxSideLength = 0.75f;

    [SerializeField]
    [Tooltip("Sandbox side height in meters")]
    private float sandboxHeight = 0.2f;

    const float qrCodeSideLength = 0.1f;


    private Transform planeParent;


    private void Awake() {
        sandbox = GameObject.Find("Sandbox");
        // Get the only rotating object as parent
        planeParent = transform.GetChild(0).transform.GetChild(0);
    }

    public void SetCornerTypeFromString(string name) {
        switch (name) {
            case "Red":
                cornerType = CornerType.Red;
                break;
            case "Green":
                cornerType = CornerType.Green;
                break;
            case "Blue":
                cornerType = CornerType.Blue;
                break;
            case "Purple":
                cornerType = CornerType.Purple;
                break;
            default:
                cornerType = CornerType.Red;
                break;
        }

        AssignOffset();
        SetSandboxAsChild();
    }

    public string GetName() {
        switch (cornerType) {
            case CornerType.Red:
                return "Red";
            case CornerType.Green:
                return "Green";
            case CornerType.Blue:
                return "Blue";
            case CornerType.Purple:
                return "Purple";
            default:
                return "Red";
        }
    }

    public void AssignOffset() {
        float sideOffset = (sandboxSideLength * 0.5f) + (qrCodeSideLength * 0.5f);
        float heightOffset = -sandboxHeight;
        switch (cornerType) {
            case CornerType.Red:
                sandboxOffset = new Vector3(-sideOffset, heightOffset, sideOffset); //Lower Right
                break;
            case CornerType.Green:
                sandboxOffset = new Vector3(sideOffset, heightOffset, -sideOffset); //Upper Left
                break;
            case CornerType.Blue:
                sandboxOffset = new Vector3(sideOffset, heightOffset, sideOffset); //Upper Right
                break;
            case CornerType.Purple:
                sandboxOffset = new Vector3(-sideOffset, heightOffset, -sideOffset); //Lower Left
                break;
            default:
                sandboxOffset = new Vector3(-sideOffset, heightOffset, sideOffset); //Lower Right
                break;
        }
    }

    public void SetSandboxAsChild() {
        if(sandbox.transform.parent != planeParent) {
            sandbox.transform.SetParent(planeParent);
            sandbox.transform.rotation = Quaternion.identity;
            sandbox.transform.localPosition = Vector3.zero;
            sandbox.transform.Translate(sandboxOffset);
        }
    }

    void Update() {
        if (Input.touchCount >= 2) {
            if ((Input.touches[0].phase == TouchPhase.Began) || (Input.touches[1].phase == TouchPhase.Began)) {
                string name = GetName();
                Debug.Log(name + " parentName:" + transform.parent.name);
                Debug.Log(name + " position:" + transform.position);
            }
        }
    }
}
