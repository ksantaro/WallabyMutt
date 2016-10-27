using UnityEngine;
using System.Collections;

public class EnemyDeath : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (!this.GetComponent<Health>().isAlive) {

			if(this.GetComponent<ItemDropper>())
				this.GetComponent<ItemDropper>().DropItem();
			
			Destroy(this.gameObject);

		}
	}
}
