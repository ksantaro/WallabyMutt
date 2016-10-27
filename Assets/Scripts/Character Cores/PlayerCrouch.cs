using UnityEngine;
using System.Collections;

public class PlayerCrouch : MonoBehaviour {

	public int height; //public for unity


	public void Crouch() {
		this.height = this.height / 2;
	}

	public void Stand() {
		this.height = this.height * 2;
	}


	// Use this for initialization
	void Start () {
		height = 6;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
