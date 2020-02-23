    using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays some information as well as the source Texture2D on top of the
/// detected image.
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class CustomTrackedImageInfoManager : MonoBehaviour {

    [SerializeField]
    [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
    Texture2D m_DefaultTexture;

    /// <summary>
    /// If an image is detected but no source texture can be found,
    /// this texture is used instead.
    /// </summary>
    /// 
    private Camera m_MainCamera;
    public Texture2D defaultTexture
    {
        get { return m_DefaultTexture; }
        set { m_DefaultTexture = value; }
    }

    ARTrackedImageManager m_TrackedImageManager;



    private List<CornerSandboxParenter> m_parenters = new List<CornerSandboxParenter>();

    private ARTrackedImage[] m_arImages = new ARTrackedImage[0];

    void Awake() {
        m_MainCamera = Camera.main;
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        InvokeRepeating("CheckForReparenting", 1.0f, 1.0f);
    }

    void OnEnable() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable() {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void UpdateInfo(ARTrackedImage trackedImage, Texture2D texture = null) {
        var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
        var planeGo = planeParentGo.transform.GetChild(0).gameObject;

        if (texture) {
            // Set the texture
            trackedImage.name = texture.name;
            CornerSandboxParenter sender = trackedImage.GetComponentInChildren<CornerSandboxParenter>();
            m_parenters.Add(sender);

            sender.SetCornerTypeFromString(texture.name);

            var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
            material.mainTexture = texture;
        }

        // Disable the visual plane if it is not being tracked
        if (trackedImage.trackingState != TrackingState.None) {
            planeGo.SetActive(true);

            // The image extents is only valid when the image is being tracked
            trackedImage.transform.localScale = new Vector3(trackedImage.size.x, trackedImage.size.x, trackedImage.size.y);
        } else {
            planeGo.SetActive(false);
        }
    }

    private bool isVisibleOnScreen(Vector3 position) {

        return (position.z > 0 && (position.x < 1 && position.x > 0) && (position.y < 1 && position.y > 0));
    }

    void CheckForReparenting() {
        float closestDistance = float.MaxValue;
        int closestIndex = -1;
        for (int i = 0; i < m_arImages.Length; i++) {

            float currDistance = Vector3.SqrMagnitude(m_MainCamera.transform.position - m_arImages[i].transform.position);
            Vector3 screenPosition = m_MainCamera.WorldToViewportPoint(m_arImages[i].transform.position);
            if (isVisibleOnScreen(screenPosition) && currDistance < closestDistance) {
                closestDistance = currDistance;
                closestIndex = i;
            }
        }
        if(closestIndex > 0) {
            m_parenters[closestIndex].SetSandboxAsChild();
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {

        m_arImages = eventArgs.updated.ToArray();

        foreach (var trackedImage in eventArgs.added) {
            UpdateInfo(trackedImage, trackedImage.referenceImage.texture);
        }

     
    }
}
