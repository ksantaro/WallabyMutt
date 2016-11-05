using UnityEngine;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour{

	public bool isHitboxActice = false;
	public int  damage = 1;

	public string[] tagsToHit = new string[1];

//	public abstract void Attack ();

//	public abstract void Hit (Collider2D c);

}
