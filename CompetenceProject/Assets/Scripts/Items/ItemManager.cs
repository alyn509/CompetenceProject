using UnityEngine;
using System.Collections.Generic;
using System;

public class ItemManager : MonoBehaviour {

    List<Vector3> dropPoints = new List<Vector3>();
    public GameObject gunObject;
    public GameObject ammoObject;
    List<int> usedDrops = new List<int>();

    public void DropItems(List<Vector3> _dropPoints)
    {
        this.dropPoints = _dropPoints;
        for (int i = 0; i < 10; i++)
        {
            int point = 1;
            while (usedDrops.Contains(point))
            {
                point = (int)UnityEngine.Random.Range(0, dropPoints.Count - 1);
            }
            usedDrops.Add(point);

            GameObject drop;
            if (usedDrops.Count == 0)
                drop = Instantiate(gunObject, dropPoints[point], Quaternion.identity) as GameObject;
            else
                drop = Instantiate(ammoObject, dropPoints[point] + new Vector3(0, -6f, 0), Quaternion.identity) as GameObject;
            drop.AddComponent<Item>();
        }
    }


}
