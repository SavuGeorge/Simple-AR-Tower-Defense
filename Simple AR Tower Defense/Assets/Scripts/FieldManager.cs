using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{

    public float tileSize = 0.05f;


    //refs
    public GameObject playField;
    public Vector3 pivotWorld;
    private MeshRenderer fieldMesh;
    public Material fieldMaterial;
    public GameObject towerPrefab;
	private float towerRange;
    public GameObject wallPrefab;
	public GameObject PortalPrefab;
	public GameObject PlayerBasePrefab;

	public GameObject testPathPrefab;

	private GameFlowManager flowManager;
	

    public enum buildType { Tower, Wall, Demolish }

	[HideInInspector]
	public int[][] aiMatrix;
	[HideInInspector]
    public GameObject[][] refMatrix;
	private int[][] towerAreaMatrix;
	[HideInInspector]
    public int tilesX, tilesY;

	[HideInInspector]
	public Vector2Int portalPos;
	[HideInInspector]
	public Vector3 worldPortalPos;
	[HideInInspector]
	public Vector2Int basePos;
	[HideInInspector]
	public Vector2 worldBasePos;


	public void Init() {
		InitField();
		SetupPortal();
		SetupPlayerBase();
		flowManager = GameFlowManager.instance;
		towerRange = towerPrefab.GetComponent<Tower>().range;
	}

#if UNITY_EDITOR
	public bool butonel = false;
	void Update() {
		if (butonel) {
			butonel = false;
			TestPath();
		}
	}
#endif

	public void ProcessTowerAreaMatrix() {
		for (int x = 0; x < tilesX; x++) {
			for (int y = 0; y < tilesY; y++) {
				Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
				towerAreaMatrix[y][x] = 0;
				foreach (Tower t in flowManager.towerList) {
					if (Vector3.Distance(worldPos, t.transform.position) <= towerRange) {
						towerAreaMatrix[y][x]++; // 1 per tower in range of tile
					}
				}
			}
		}
	}

	public void BuildAt(buildType type, Vector3 pos) {

		Vector2Int localCoord = WorldToGrid(pos);

		pos = GridToWorld(localCoord); // snaps to grid


		if (type == buildType.Demolish) {
			GameObject obj = refMatrix[localCoord.x][localCoord.y];
			Tower tScript = obj.GetComponent<Tower>();
			if (tScript != null) {
				tScript.Demolish();
			}
			else {
				Destroy(refMatrix[localCoord.x][localCoord.y], 0.01f);
			}
			aiMatrix[localCoord.y][localCoord.x] = 0;
		}
		else {
			if (refMatrix[localCoord.x][localCoord.y] == null) {
				GameObject newObj = null;
				if (type == buildType.Tower) {
					newObj = Instantiate(towerPrefab, pos, Quaternion.identity);
					flowManager.towerList.Add(newObj.GetComponent<Tower>());
					flowManager.ChangeGold(-flowManager.towerCost);
				}
				else if (type == buildType.Wall) {
					newObj = Instantiate(wallPrefab, pos, Quaternion.identity);
					flowManager.ChangeGold(-flowManager.wallCost);
				}
				refMatrix[localCoord.x][localCoord.y] = newObj;
				aiMatrix[localCoord.y][localCoord.x] = 1;
				Vector3 newPos = newObj.transform.position;
				newPos.y += newObj.transform.localScale.y / 2;
				newObj.transform.position = newPos;
			}
		}

    }

	public Vector2Int WorldToGrid(Vector3 pos) {
		pos.x = pos.x - rem(pos.x, tileSize);
		pos.z = pos.z - rem(pos.z, tileSize);
		Vector2 localPos = new Vector2(pos.x - pivotWorld.x, pos.z - pivotWorld.z);
		Vector2Int ret = new Vector2Int((int)Mathf.Round(localPos.x / tileSize), (int)Mathf.Round(localPos.y / tileSize));
		return ret;
	}

	public Vector3 GridToWorld(Vector2Int grid) {
		return new Vector3(grid.x * tileSize + pivotWorld.x + tileSize/2, pivotWorld.y, grid.y * tileSize + pivotWorld.z + tileSize/2);
	}

	void SetupPortal() {
		Vector2Int localCoord = new Vector2Int(Random.Range(0, tilesX), 0);
		Vector3 worldPos = GridToWorld(localCoord);
		GameObject newObj;
		newObj = Instantiate(PortalPrefab, worldPos, Quaternion.identity);
		Vector3 newPos = newObj.transform.position;
		newPos.y += newObj.transform.localScale.y / 2;
		newObj.transform.position = newPos;
		portalPos = localCoord;
		worldPortalPos = GridToWorld(portalPos);
	}

	void SetupPlayerBase() {
		Vector2Int localCoord = new Vector2Int((int)tilesX/2, (int)tilesY/2);
		Vector3 worldPos = GridToWorld(localCoord);
		GameObject newObj;
		newObj = Instantiate(PlayerBasePrefab, worldPos, Quaternion.identity);
		basePos = localCoord;
		worldBasePos = worldPos;
	}

	List<GameObject> testObjs = new List<GameObject>();

#if UNITY_EDITOR
	public void TestPath() {
		int[] s = new int[2] { portalPos.x, portalPos.y };
		int[] e = new int[2] { basePos.x, basePos.y };
		List<Vector2> path = new Astar(aiMatrix, s, e, "Euclidean", towerAreaMatrix).result;

		foreach(GameObject old in testObjs) {
			Destroy(old,0.01f);
		}

		testObjs = new List<GameObject>();

		foreach(Vector2 p in path) {
			Vector3 worldPos = new Vector3(p.x * tileSize + pivotWorld.x + tileSize/2, pivotWorld.y, p.y * tileSize + pivotWorld.z + tileSize/2);
			GameObject obj = Instantiate(testPathPrefab, worldPos, Quaternion.identity);
			testObjs.Add(obj);
		}
	}
#endif

	public List<Vector2Int> GetPath(Vector2Int st, Vector2Int en, string type) {
		int[] s = new int[2] { st.x, st.y };
		int[] e = new int[2] { en.x, en.y };
		List<Vector2> path = new Astar(aiMatrix, s, e, type, towerAreaMatrix).result;
		List<Vector2Int> ret = new List<Vector2Int>();
		foreach(Vector2 v in path) {
			ret.Add(new Vector2Int((int)v.x, (int)v.y));
		}
		return ret;
	}

    void InitField() {
        Vector3 scale = playField.transform.localScale;
        playField.transform.localScale = new Vector3(scale.x - scale.x % tileSize, scale.y - scale.y % tileSize, scale.z);
        Vector3 pos = playField.transform.position;
        playField.transform.position = new Vector3(pos.x - pos.x % tileSize, pos.y , pos.z - pos.z % tileSize);

        fieldMesh = playField.GetComponent<MeshRenderer>();

        tilesX = (int)(scale.x / tileSize);
        tilesY = (int)(scale.y / tileSize); 
        fieldMaterial.SetInt("_GridSizeX", tilesX);
        fieldMaterial.SetInt("_GridSizeY", tilesY);

        pivotWorld = new Vector3(pos.x - scale.x/2, pos.y, pos.z - scale.y / 2);

        refMatrix = new GameObject[tilesX][];
        for(int i = 0; i < tilesX; i++) {
            refMatrix[i] = new GameObject[tilesY];
        }

		aiMatrix = new int[tilesY][];
		towerAreaMatrix = new int[tilesY][];
		for(int i = 0; i < tilesY; i++) {
			aiMatrix[i] = new int[tilesX];
			towerAreaMatrix[i] = new int[tilesX];
		}

        fieldMesh.material = fieldMaterial;
    }


    float rem(float a, float b) { 
        return a - Mathf.Floor(a / b) * b;
    }

}



public class Astar {
	public List<Vector2> result = new List<Vector2>();
	private string find;
	private int[][] towerMatrix;

	private class _Object {
		public int x {
			get;
			set;
		}
		public int y {
			get;
			set;
		}
		public double f {
			get;
			set;
		}
		public double g {
			get;
			set;
		}
		public int v {
			get;
			set;
		}
		public _Object p {
			get;
			set;
		}
		public _Object(int x, int y) {
			this.x = x;
			this.y = y;
		}
	}


	private _Object[] successors(int x, int y, int[][] grid, int rows, int cols) {
		int N = y - 1;
		int S = y + 1;
		int E = x + 1;
		int W = x - 1;

		bool xN = N > -1 && grid[N][x] == 0;
		bool xS = S < rows && grid[S][x] == 0;
		bool xE = E < cols && grid[y][E] == 0;
		bool xW = W > -1 && grid[y][W] == 0;

		int i = 0;

		_Object[] result = new _Object[8];

		if (xN) {
			result[i++] = new _Object(x, N);
		}
		if (xE) {
			result[i++] = new _Object(E, y);
		}
		if (xS) {
			result[i++] = new _Object(x, S);
		}
		if (xW) {
			result[i++] = new _Object(W, y);
		}

		_Object[] obj = result;

		return obj;
	}

	private double diagonal(_Object start, _Object end) {
		return Mathf.Max(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
	}

	private double euclidean(_Object start, _Object end) {
		var x = start.x - end.x;
		var y = start.y - end.y;

		return Mathf.Sqrt(x * x + y * y);
	}

	private double manhattan(_Object start, _Object end) {
		return Mathf.Abs(start.x - end.x) + Mathf.Abs(start.y - end.y);
	}

	private double sneaky(_Object start, _Object end) {
		return towerMatrix[end.y][end.x];
	}

	public Astar(int[][] grid, int[] s, int[] e, string f, int[][] tMatrix) {
		this.find = (f == null) ? "Diagonal" : f;

		grid[s[1]][s[0]] = 0;
		grid[e[1]][e[0]] = 0;

		towerMatrix = tMatrix;

		int cols = grid[0].Length;
		int rows = grid.Length;
		int limit = cols * rows;
		int length = 1;

		List<_Object> open = new List<_Object>();
		open.Add(new _Object(s[0], s[1]));
		open[0].f = 0;
		open[0].g = 0;
		open[0].v = s[0] + s[1] * cols;

		_Object current;

		List<int> list = new List<int>();

		double distanceS;
		double distanceE;

		int i;
		int j;

		double max;
		int min;

		_Object[] next;
		_Object adj;

		_Object end = new _Object(e[0], e[1]);
		end.v = e[0] + e[1] * cols;

		bool inList;

		do {
			max = limit;
			min = 0;

			for (i = 0; i < length; i++) {
				if (open[i].f < max) {
					max = open[i].f;
					min = i;
				}
			}

			current = open[min];
			open.RemoveAt(min);

			if (current.v != end.v) {
				--length;
				next = successors(current.x, current.y, grid, rows, cols);

				for (i = 0, j = next.Length; i < j; ++i) {
					if (next[i] == null) {
						continue;
					}

					(adj = next[i]).p = current;
					adj.f = adj.g = 0;
					adj.v = adj.x + adj.y * cols;
					inList = false;

					foreach (int key in list) {
						if (adj.v == key) {
							inList = true;
						}
					}

					if (!inList) {
						if (this.find == "DiagonalFree" || this.find == "Diagonal") {
							distanceS = diagonal(adj, current);
							distanceE = diagonal(adj, end);
						}
						else if (this.find == "Euclidean" || this.find == "EuclideanFree") {
							distanceS = euclidean(adj, current);
							distanceE = euclidean(adj, end);
						}
						else if(this.find == "Sneaky") {
							distanceS = sneaky(adj, current);
							distanceE = sneaky(adj, end);
						}
						else {
							distanceS = manhattan(adj, current);
							distanceE = manhattan(adj, end);
						}



						adj.f = (adj.g = current.g + distanceS) + distanceE;
						open.Add(adj);
						list.Add(adj.v);
						length++;
					}
				}
			}
			else {
				i = length = 0;
				do {
					this.result.Add(new Vector2(current.x, current.y));
				}
				while ((current = current.p) != null);
				result.Reverse();
			}
		}
		while (length != 0);
	}
}
