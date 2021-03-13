using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GameMaster : MonoBehaviour
{

    [SerializeField]
    ARSession m_session;
    [SerializeField]
    ARPlaneManager planeManager;

    public static GameMaster instance;

    //refs
    public DebugBox dbox;
    public Camera arCam;





    [HideInInspector]
    public List<ARPlane> arPlaneList;

    void Awake() {
        instance = this;
    }

    IEnumerator Start() {
        
        if ((ARSession.state == ARSessionState.None) || (ARSession.state == ARSessionState.CheckingAvailability)) {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported) {
            // Start some fallback experience for unsupported devices
        }
        else {
            // Start the AR session
            StartSession();
        }
    }

    void StartSession() {
        m_session.enabled = true;
        planeManager.planesChanged += planesChangedHandler;
    }

    void planesChangedHandler(ARPlanesChangedEventArgs data) {
        foreach(ARPlane added in data.added) {
            arPlaneList.Add(added);
        }
        foreach(ARPlane removed in data.removed) {
            arPlaneList.Remove(removed);
        }

    }

    public void EndPlaneTracking() {
        //planeManager.enabled = false;
    }
    
}
