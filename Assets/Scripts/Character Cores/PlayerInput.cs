using UnityEngine;
using System.Collections;


public class PlayerInput : MonoBehaviour {

	public float xMove { get; private set; }
	public float yMove { get; private set; }
	public bool  jump  { get; private set; }
	public bool  whack { get; private set; }

	private I_Ability iAbility;
	private PlayerMovement PM;

  

	/*player movement script */

	// Use this for initialization
	void Start () {

		iAbility = GetComponent<I_Ability>();
		PM = GetComponent<PlayerMovement>();
        
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
<<<<<<< Updated upstream
		if (xMove > 0)
            GetComponent<PlayerMovement>().moveRight();
        else if (xMove < 0)
            GetComponent<PlayerMovement>().moveLeft();
        else if (xMove == 0)
            GetComponent<PlayerMovement>().slowDown();
        if (jump || yMove > 0)
            GetComponent<PlayerMovement>().jump();
        
=======

		if (PM) {

			if (xMove > 0)
			{
				PM.moveRight();
			}
			else if (xMove < 0)
			{
				PM.moveLeft();
			}
			else if (xMove == 0)
			{
				PM.slowDown();
			}
		}

        if (whack) {
        	iAbility.Attack();
        }
>>>>>>> Stashed changes
	}
}
