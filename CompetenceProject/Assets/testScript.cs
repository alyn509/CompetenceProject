using UnityEngine;
using System.Collections;

public class testScript : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
	    this.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        this.gameObject.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;
    }
	
	// Update is called once per frame
	void Update () {
        this.gameObject.GetComponent<NavMeshObstacle>().carving = true;
        this.gameObject.GetComponent<NavMeshObstacle>().carveOnlyStationary = true;
    }
}
