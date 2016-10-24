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
	void FixedUpdate () {

		UpdateInput();
		AssignInputToAction();
	
	}

	void UpdateInput () {

		xMove = Input.GetAxis("Horizontal");
		yMove = Input.GetAxis("Vertical");
		jump  = Input.GetKeyDown(KeyCode.Space);
		whack = Input.GetKeyDown(KeyCode.J);


		
	}

	void AssignInputToAction () {
		if (xMove > 0)
        {
            PlayerMovement.moveRight(GetComponent<Rigidbody2D>());
        }
        else if (xMove < 0)
        {
            PlayerMovement.moveLeft(GetComponent<Rigidbody2D>());
        }
        else if (xMove == 0)
        {
            PlayerMovement.slowDown(GetComponent<Rigidbody2D>());
        }
	}
}
