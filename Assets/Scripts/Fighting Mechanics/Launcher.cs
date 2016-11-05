using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour, I_Ability {

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

	// Use this for initialization
	void Start () {
		
	}

	public void Attack () {

		switch (launchDirection) {
			case LaunchDirection.Right: launchingVector = Vector2.right;
			break;
			case LaunchDirection.Up: launchingVector = Vector2.up;
			break;
		}

		spawnedProjectile = (GameObject)Instantiate(projectile, transform.position, transform.rotation);

		projectileProperties =  spawnedProjectile.GetComponent<Projectile>();
		projectileProperties.startingPoint = transform.position;
		projectileProperties.launchVector = this.launchingVector;


	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
