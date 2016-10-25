using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[ExecuteInEditMode]
public class HealthBarUI : MonoBehaviour {

	public GameObject healthContainerUI;
	
	public int maxHealth = 10;
	public int curHealth = 10;

	public float healthbarLengthNormalized = 1f;

	public GameObject   healthBarSegmentPrefab;
//	public List<Image>  healthBarImages;
	public Image[]		healthBarImages;

	private CanvasScaler  parentCS;
	private RectTransform containersTransform;
	private Image		  containersImage;
	private HorizontalLayoutGroup HLG;
	private RectTransform segmentsTransform;

	private int lastMaxHealthBars = 0;

	private int curChildImages = 0;
	private int newChildImagesCount = 0;

	


	// Use this for initialization
	void Start () {

		Init();
	
	}


	void Init () {

		if (!healthContainerUI) {

			Debug.LogError("Health Container UI was not assigned.");
			return;
		}

		if (!healthBarSegmentPrefab) {

			Debug.LogError("Health Bar Segment Prefab was not assigned.");
			return;

		}

		parentCS 			= healthContainerUI.GetComponentInParent<CanvasScaler>();
		containersTransform = healthContainerUI.GetComponent<RectTransform>();
		containersImage		= healthContainerUI.GetComponent<Image>();
//		segmentsTransform   = healthBarSegment.GetComponent<RectTransform>();
		HLG					= healthContainerUI.GetComponent<HorizontalLayoutGroup>();



		HLG.enabled = true;

		List<Image> tempList = new List<Image>(healthContainerUI.GetComponentsInChildren<Image>());

		foreach (Transform child in healthContainerUI.transform) {

			Destroy(child.gameObject);
		}

		tempList.Clear();


		for (int c = 0; c < maxHealth; c++) {
			
			GameObject newHealthbar = (GameObject)Instantiate(healthBarSegmentPrefab, healthContainerUI.transform);

			tempList.Add(newHealthbar.GetComponent<Image>());

			newHealthbar.GetComponent<RectTransform>().localScale =  new Vector3(1f, 1f);
			newHealthbar = null;
			
		}


		healthBarImages = tempList.ToArray();
		containersImage.color = Color.clear;
		curHealth = maxHealth;
		healthbarLengthNormalized = (float)( curHealth / maxHealth );
		
	}

	public void GainHealth(int points) {

//		HLG.enabled = false;

		curHealth += points;
		curHealth = Mathf.Clamp(curHealth, 0, maxHealth);

		healthBarImages[curHealth - 1].color = Color.white;

		healthbarLengthNormalized = ((float) curHealth / (float) maxHealth );

	}

	public void LostHealth(int points) {

//		HLG.enabled = false;

		curHealth -= points;
		curHealth = Mathf.Clamp(curHealth, 0, maxHealth);

		healthBarImages[curHealth].color = Color.clear;

		healthbarLengthNormalized = ((float) curHealth / (float) maxHealth );

	}


}
