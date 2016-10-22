using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {

    public ItemType itemType;
    public Vector3 realPosition;
    public Coord gridPosition;
    GameObject player;

    public Item(ItemType type, Vector3 _realPosition, Coord _gridPosition)
    {
        this.itemType = type;
        this.realPosition = _realPosition;
        this.gridPosition = _gridPosition;
    }

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider collision)
    {
        //If we collided with a player, add item to player.
        if (collision.transform.tag == "Player")
        {
            player.GetComponent<BasicMovement>().PickupItem(itemType);
        }
        Destroy(gameObject);
    }
}

public enum ItemType { Gun, Ammo }