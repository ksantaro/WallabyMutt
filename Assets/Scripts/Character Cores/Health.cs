using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

	public float health 	= 100f;
	public float maxHealth  = 100f;
	public bool  invinsible = false;
	
	private bool  alive 	= true;
	private float lastHealth;


	public bool isAlive {
		get { return alive; }
	}

	public float HealthPercent {
		get {
			return Mathf.Clamp01(health / maxHealth) * 100f;
		}
		set {
			health = (value / 100f) * maxHealth;
		}
	}

	public float MaxHealth {
		get { return maxHealth; }
	}

	public float CurrentHealth {
		get { return health; }
	}

	// Use this for initialization
	void Start () {
		lastHealth = health = maxHealth;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		
		HealthReport ();
		
		if (!alive) {

//			Debug.Log("Not alive anymore"); 
//			EventManager.OutOfHeatlh(this.gameObject);
//			Debug.Log("Just pass the event"); 
//			gameObject.SendMessage("Dead", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	// Main Healing
	public void Heal (float points) {

		health = Mathf.Min (maxHealth, health + points);

	}
	
	// Main Hurting
	public void Hurt (float points) {
		
		if (!invinsible) {
			health = Mathf.Max (0f, health - points);
		}
	}
	
	//Mod health by percentage
	public void HealByPercent (float percent) {
	
		health = Mathf.Min (maxHealth, health + ((percent / 100f) * maxHealth));
	}

	public void HurtByPercent (float percent) {
		
		if (!invinsible)
			health = Mathf.Max (0f, health - ((percent / 100f) * maxHealth));
	}


		
	void HealthReport () {
		
		if (lastHealth == health)
		return;

		alive = health > 0;
		
		Debug.Log (transform.root.name + (health < lastHealth ? " lost " : " gained ") + Mathf.Abs (health - lastHealth) + " health.");
		
		if (!alive)
			Debug.LogWarning (transform.root.name + " is out.");
					
		lastHealth = health;
	
	}
}
