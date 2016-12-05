using UnityEngine;
using System.Collections;
using System.Linq;


public class Boomerang : Projectile, IDamagable {


	public float spinAnimationMultiplier = 1;

	private Rigidbody2D RB;
	private Animator ANIM;
	private AudioSource ADS;


	// Use this for initialization
	void Start () {
		RB 		= GetComponent<Rigidbody2D>();
		ANIM 	= GetComponent<Animator>();
		ADS 	= GetComponent<AudioSource>();

		OnSpawn();

	}

	void OnTriggerEnter2D (Collider2D c) {

		if (isHitboxActice)
			Hit(c);
		else if (!isHitboxActice)
			Miss();
		
	}
	
	// Update is called once per frame
	void Update () {
		TravelDistanceCheck();
	}

	public void Hit (Collider2D c)
	{
		GameObject otherGO = c.gameObject;
		
		if (tagsToHit.Contains(otherGO.tag)) {
			if (otherGO.GetComponentInParent<Health>()) {

				otherGO.GetComponentInParent<Health>().Hurt(damage);
				OnHit();
			}
		}
	}

	void OnSpawn () {
		RB.velocity = launchVector * speed;

		if (travelingSound) {

			if (!ADS.isPlaying) {
				ADS.clip = travelingSound;
				ADS.Play();
			}
		}

	}


	void TravelDistanceCheck () {

		if (Vector2.Distance(transform.position, startingPoint) >= maxTravelDistance) {
//			Destroy(this.gameObject);

			Miss();
			
		}
		
	}

	void OnHit () {
		isHitboxActice = false;
		
		RB.gravityScale = 0.5f;
		RB.isKinematic = false;

		ADS.clip = impactSound;
		ADS.Play();

		Destroy(gameObject, 3);
		
	}

	void Miss () {

		Destroy(gameObject, 3);
		
	}

		






}
