using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine;

public class PlaneSelectManager : MonoBehaviour
{

    //refs
    public GameObject quadPrefab;
    public Camera arCam;
    public FieldManager field;
    public GameFlowManager flowManager;
    

    private GameMaster gm;
    private DebugBox dbox;

    public GameObject gameUI;
    public GameObject preGameUI;

#if UNITY_EDITOR
    public GameObject sceneQuad;
#endif

    void Start() {
        gm = GameMaster.instance;
        dbox = gm.dbox;
    }


    public void SelectPlaneButton() {
        dbox.PrintLine("Button pressed");
#if UNITY_EDITOR
        PrepStuff(sceneQuad);
#else
        if (gm.arPlaneList.Count == 0) {
            dbox.PrintLine("Need at least one detected plane");
        }
        else {
            dbox.PrintLine("Setup Started");
            ARPlane maxPlane = null;
            float maxArea = -1;
            foreach (ARPlane plane in gm.arPlaneList) {
                if (plane.size.x * plane.size.y > maxArea) {
                    maxArea = plane.size.x * plane.size.y;
                    maxPlane = plane;
                }
            }

            GameObject quad = Instantiate(quadPrefab, maxPlane.center, Quaternion.identity);
            quad.transform.localScale = new Vector3(maxPlane.size.x, quad.transform.localScale.y, maxPlane.size.y);

            quad.transform.rotation = Quaternion.Euler(90, 0, 0);

            PrepStuff(quad);
        }
#endif
    }


    void PrepStuff(GameObject quad) {
        field.playField = quad;
        field.Init();
        UpdateUI();
    }

    void UpdateUI() {
        preGameUI.SetActive(false);
        gameUI.SetActive(true);
    }

}
