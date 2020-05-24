using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
#if UNITY_EDITOR
using input = GoogleARCore.InstantPreviewInput;
#endif
public class ARController : MonoBehaviour
{
    
    //we will fill this with planes that ARCore detected in the current frame
    private List<DetectedPlane> m_NewTrackedPlanes = new List<DetectedPlane>();
    public GameObject GridPrefab;
    public GameObject Portal;
    public GameObject ARCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        return;
        //check ARCore session status
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
        //the following function will fill m_NewTrackedPlanes with the planes that ARcore detected in the current frame
        Session.GetTrackables<DetectedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

        //Instaitiate a grid for each trackedplane in m_NewTrackedPlanes
        for (int i = 0; i < m_NewTrackedPlanes.Count; ++i)
        {
            GameObject grid = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, transform);
            //This function will set the position of grid and modify the vertices of the attached mesh
            grid.GetComponent<GridVisualiser>().Initialize(m_NewTrackedPlanes[i]);
        }

        //check if the user touches the screen
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        //let's now check if the user touched any of the tracked palnes
        TrackableHit hit;
        if (Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon, out hit))
        {
            // lets now place the portal on the top of the tracked plane  that we touched

            //enable the portal
            Portal.SetActive(true);

            //create  anew anchor

            Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

            //set the position of the portal to be the same as the hit position
            Portal.transform.position = hit.Pose.position;
            Portal.transform.rotation = hit.Pose.rotation;

            //we want the portal to face the camera
            Vector3 cameraPosition = ARCamera.transform.position;

            //The portal should only rotate around the y axis
            cameraPosition.y = hit.Pose.position.y;

            //Rotate the portal to face the camera
            Portal.transform.LookAt(cameraPosition, Portal.transform.up);

            //ARCore will keep understanding the world and update teh anchors accordingly hence we need to attach our portal to the anchor
            Portal.transform.parent = anchor.transform;
        }
    }
}
