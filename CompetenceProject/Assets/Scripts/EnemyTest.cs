using UnityEngine;
using System.Collections;

public class EnemyTest : MonoBehaviour {

    public Vector3 moveTo;
    public Vector3 currentPos;
    public bool move = false;

	// Use this for initialization
	void Start () {
        currentPos = this.transform.position;
        moveTo = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(currentPos, moveTo) < 1f)
        {
            move = false;
        }
	
	}
}
