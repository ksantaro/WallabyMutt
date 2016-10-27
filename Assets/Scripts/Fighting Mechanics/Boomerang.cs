using UnityEngine;
using System.Collections;
using System.Linq;


public class Boomerang : WeaponBase {

	public float distance = 2f;
	public GameObject startingPoint;

	private Animator ANIM;

	// Use this for initialization
	void Start () {
		ANIM = GetComponent<Animator>();
	}

	void OnTriggerEnter2D (Collider2D c) {

		Hit(c);
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void Attack ()
	{
		Debug.Log("Boom");
	}

	public override void Hit (Collider2D c)
	{
		GameObject otherGO = c.gameObject;

		if (tagsToHit.Contains(otherGO.tag)) {

			if (otherGO.GetComponent<Health>()) {

				otherGO.GetComponent<Health>().Hurt(damage);

			}


		}

	}




}
