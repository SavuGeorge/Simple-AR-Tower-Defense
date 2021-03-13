using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

public class PlacementManager : MonoBehaviour
{

    //settings
    public LayerMask floorMask;
    public LayerMask structureMask;

    //refs
    //scripts
    public FieldManager field;
        //prefabs
    public GameObject hitMarker;

        //UI
    public Button PlaceTowerButton;
    private Text PlaceTowerText;
    private Image PlaceTowerImage;

    public Button PlaceWallButton;
    private Text PlaceWallText;
    private Image PlaceWallImage;

    public Button DemolishButton;
    private Text DemolishText;
    private Image DemolishImage;

        //misc
    private Camera mainCam;
    public Camera arCam;
    private DebugBox dbox;
    //private GameMaster gm;   // TODO: swap back to regular gm
    private GameMaster gm;
    

    //state 
    public enum PlaceStateType { None, Tower, Wall, Demolish}
    [HideInInspector]
    public PlaceStateType placeState = PlaceStateType.None;



    void Start() {
        gm = GameMaster.instance;
        dbox = gm.dbox;
        mainCam = Camera.main;

        PlaceTowerText = PlaceTowerButton.gameObject.GetComponentInChildren<Text>();
        PlaceTowerImage = PlaceTowerButton.gameObject.GetComponent<Image>();
        PlaceWallText = PlaceWallButton.gameObject.GetComponentInChildren<Text>();
        PlaceWallImage = PlaceWallButton.gameObject.GetComponent<Image>();
        DemolishText = DemolishButton.gameObject.GetComponentInChildren<Text>();
        DemolishImage = DemolishButton.gameObject.GetComponent<Image>();

    }

    List<ARRaycastHit> hits = new List<ARRaycastHit>();


    Touch curTouch;
    void Update() {


#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) {
            if ((placeState == PlaceStateType.Wall) || (placeState == PlaceStateType.Tower)) {
                Vector2 screenPos = Input.mousePosition;
                Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
                RaycastHit hitInfo;
                Ray ray = mainCam.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, floorMask)) {
                    if (placeState == PlaceStateType.Wall) {
                        field.BuildAt(FieldManager.buildType.Wall, hitInfo.point);
                    }
                    if (placeState == PlaceStateType.Tower) {
                        field.BuildAt(FieldManager.buildType.Tower, hitInfo.point);
                    }
                }
            }
            else if (placeState == PlaceStateType.Demolish) {
                Vector2 screenPos = Input.mousePosition;
                Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
                RaycastHit hitInfo;
                Ray ray = mainCam.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, structureMask)) {
                    field.BuildAt(FieldManager.buildType.Demolish, hitInfo.point);
                }
            }
        }

#else
        if(Input.touchCount > 0) {
            curTouch = Input.GetTouch(0);
            Vector2 screenPos = curTouch.position;
            Vector3 worldPos = arCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
            RaycastHit hitInfo;
            Ray ray = arCam.ScreenPointToRay(screenPos);
            if ((placeState == PlaceStateType.Wall) || (placeState == PlaceStateType.Tower)) {
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, floorMask)) {
                    if (placeState == PlaceStateType.Wall) {
                        field.BuildAt(FieldManager.buildType.Wall, hitInfo.point);
                    }
                    if (placeState == PlaceStateType.Tower) {
                        field.BuildAt(FieldManager.buildType.Tower, hitInfo.point);
                    }
                }
            }
            else if(placeState == PlaceStateType.Demolish) {
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, structureMask)) {
                    field.BuildAt(FieldManager.buildType.Demolish, hitInfo.point);
                }
            }
        }
#endif

    }



    public void BuildTowerButton() {
        ResetButtons();
        if(placeState == PlaceStateType.Tower) {
            placeState = PlaceStateType.None;
        }
        else {
            placeState = PlaceStateType.Tower;
            PlaceTowerText.text = "Stop Placing Towers";
            PlaceTowerImage.color = Color.green;
        }
    }


    public void BuildWallButton() {
        ResetButtons();
        if (placeState == PlaceStateType.Wall) {
            placeState = PlaceStateType.None;
        }
        else {
            placeState = PlaceStateType.Wall;
            PlaceWallText.text = "Stop Placing Walls";
            PlaceWallImage.color = Color.green;
        }
    }

    public void DemolishButtonF() {
        ResetButtons();
        if (placeState == PlaceStateType.Demolish) {
            placeState = PlaceStateType.None;
        }
        else {
            placeState = PlaceStateType.Demolish;
            DemolishText.text = "Stop Demolishing";
            DemolishImage.color = Color.red;
        }
    }

    public void ResetButtons() {
        PlaceTowerText.text = "Start Placing Towers";
        PlaceTowerImage.color = Color.white;
        PlaceWallText.text = "Start Placing Walls";
        PlaceWallImage.color = Color.white;
        DemolishText.text = "Start Demolishing";
        DemolishImage.color = Color.white;
    }

    /*public void BuildTowerButton() {
        placingFunkyTower = !placingFunkyTower;
    }*/



}
