using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[ExecuteInEditMode]
public class HealthBarUI : MonoBehaviour {

	public GameObject healthContainerUI;
	
	public int maxBars = 10;
	public int curBars = 10;

	public float healthNormalized = 0f;

	public GameObject   healthBarSegmentPrefab;
//	public List<Image>  healthBarImages;
	public Image[]		healthBarImages;

//	private CanvasScaler  parentCS;
//	private RectTransform containersTransform;
	private Health health;
	private Image  containersImage;
	private HorizontalLayoutGroup HLG;
//	private RectTransform segmentsTransform;

//	private int lastMaxHealthBars = 0;

//	private int curChildImages = 0;
//	private int newChildImagesCount = 0;

	


	// Use this for initialization
	void Start () {

		InitUI();
	
	}

	void LateUpdate () {
		CheckHealth();
	}


	void InitUI () {

		if (!healthContainerUI) {

			Debug.LogError("Health Container UI was not assigned.");
			return;
		}

		if (!healthBarSegmentPrefab) {

			Debug.LogError("Health Bar Segment Prefab was not assigned.");
			return;

		}

		health = GetComponent<Health>();
		maxBars = (int)health.MaxHealth;

//		parentCS 			= healthContainerUI.GetComponentInParent<CanvasScaler>();
//		containersTransform = healthContainerUI.GetComponent<RectTransform>();
		containersImage		= healthContainerUI.GetComponent<Image>();
//		segmentsTransform   = healthBarSegment.GetComponent<RectTransform>();
		HLG					= healthContainerUI.GetComponent<HorizontalLayoutGroup>();



		HLG.enabled = true;

		List<Image> tempList = new List<Image>(healthContainerUI.GetComponentsInChildren<Image>());

		foreach (Transform child in healthContainerUI.transform) {

			Destroy(child.gameObject);
		}

		tempList.Clear();


		for (int c = 0; c < maxBars; c++) {
			
			GameObject newHealthbar = (GameObject)Instantiate(healthBarSegmentPrefab, healthContainerUI.transform);

			tempList.Add(newHealthbar.GetComponent<Image>());

			newHealthbar.GetComponent<RectTransform>().localScale =  new Vector3(1f, 1f);
			newHealthbar = null;
			
		}


		healthBarImages = tempList.ToArray();
		containersImage.color = Color.clear;
		curBars = maxBars;
		healthNormalized = (float)( curBars / maxBars );
		
	}

	void CheckHealth () {

		int healthDif = (int)health.CurrentHealth - curBars;

		if ((int)health.CurrentHealth > curBars)
		{
			GainHealth(healthDif);
		}
		else if ((int)health.CurrentHealth < curBars) {
			LoseHealth(healthDif);
		}

		curBars = (int)health.CurrentHealth;
	}

//	public void GainHealth () {
//		GainHealth(1);
//	}
//
//	public void LostHealth () {
//		LostHealth(1);
//	}

	public void GainHealth(int points) {

//		HLG.enabled = false;

		points = Mathf.Abs(points);

		for (int p = 0; p < points; p++) {

			if (curBars >= maxBars)
				break;

			healthBarImages[curBars].color = Color.white;
			curBars++;
			curBars = Mathf.Clamp(curBars, 0, maxBars);

		}

//		curBars += points;
		
		healthNormalized = ((float) curBars / (float) maxBars );

	}

	public void LoseHealth(int points) {

//		HLG.enabled = false;

		points = Mathf.Abs(points);

		for (int p = 0; p < points; p++) {

			if (curBars <= 0)
				break;

			curBars--;
			healthBarImages[curBars].color = Color.clear;

			curBars = Mathf.Clamp(curBars, 0, maxBars);
		}

//		curBars -= points;

		healthNormalized = ((float) curBars / (float) maxBars );

	}


}
