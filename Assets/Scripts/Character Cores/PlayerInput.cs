using UnityEngine;
using System.Collections;


public class PlayerInput : MonoBehaviour {

	public float xMove { get; private set; }
	public float yMove { get; private set; }
	public bool  jump  { get; private set; }
	public bool  whack { get; private set; }

	private IAbility iAbility;
	private PlayerMovement PM;

  

	/*player movement script */

	// Use this for initialization
	void Start () {

		iAbility = GetComponent<IAbility>();
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
		whack = Input.GetKey(KeyCode.J);

	}

	void AssignInputToAction () {        

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
	}
}
