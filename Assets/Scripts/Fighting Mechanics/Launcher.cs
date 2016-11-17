using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour, IAbility {

	public enum LaunchDirection {
		Right,
		Up
	}

	public GameObject projectile;
	public LaunchDirection launchDirection = LaunchDirection.Right;
	public float fireIntervals = 0.5f;

	private Projectile projectileProperties;
	private Vector2 launchingVector;
	private GameObject spawnedProjectile;

	private bool canFire = true;


	// Use this for initialization
	void Start () {
		
	}


	public void Attack () {

		if (!canFire)
		return;


		switch (launchDirection) {
			case LaunchDirection.Right: launchingVector = transform.right;
			break;
			case LaunchDirection.Up: 	launchingVector = transform.up;
			break;
		}

		spawnedProjectile = (GameObject)Instantiate(projectile, transform.position, transform.rotation);

		projectileProperties =  spawnedProjectile.GetComponent<Projectile>();
		projectileProperties.startingPoint = transform.position;
		projectileProperties.launchVector = this.launchingVector;

		StartCoroutine("FireDelay");


	}

	private IEnumerator FireDelay () {
		canFire = false;
		yield return new WaitForSeconds(fireIntervals);
		canFire = true;
	}


}
