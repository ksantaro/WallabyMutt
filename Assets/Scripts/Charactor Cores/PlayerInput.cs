using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	public float xMove { get; private set; }
	public float yMove { get; private set; }
	public bool  jump  { get; private set; }
	public bool  whack { get; private set; }


	/*player movement script */

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		UpdateInput();
		AssignInputToAction();
	
	}

	void UpdateInput () {

		xMove = Input.GetAxis("horizontal");
		yMove = Input.GetAxis("vertical");
		jump  = Input.GetKeyDown(KeyCode.Space);
		whack = Input.GetKeyDown(KeyCode.J);


		
	}

	void AssignInputToAction () {
		
	}
}
