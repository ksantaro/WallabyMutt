using UnityEngine;
using System.Collections;

public class ItemDropper : MonoBehaviour {

	public GameObject item;
	public Transform  dropPoint;
	public Vector2 launchDirection = new Vector2(0, 1);
	public float   launchForce;


	private GameObject tempGO;
	private Rigidbody2D itemRB;


	public void DropItem () {

		if (!item) {
			return;
		}

		tempGO = (GameObject)Instantiate(item, dropPoint.position, Quaternion.identity);

		itemRB = tempGO.GetComponent<Rigidbody2D>();
		itemRB.velocity = launchDirection * launchForce;

		Destroy(tempGO, 5f);
	}

}
