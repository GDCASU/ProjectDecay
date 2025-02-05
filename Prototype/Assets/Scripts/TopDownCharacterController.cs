﻿using UnityEngine;
using System.Collections;

public class TopDownCharacterController : MonoBehaviour {

	public GameObject character;

	float mPlayerRotation;
	static float mBaseSpeed;
	static float mSpeedBonus;
	Vector2 unitVelo;
	enum WeaponModes: int{sword, gun}
	int numOfWeaponModes =2;
	static WeaponModes currentWeapon;
	static float baseAttackStrength = 1;


	static float noise = 2.0f;

	//
	//Get Hit Vars Below:
	//
	bool hurt;
	static float lifeRemaining;
	public static float maxLife = 10f;

	float hurtStateTime;
	float hurtStateTimeRemaining;
	//MeshRenderer MRtoMessWith;
	public SpriteRenderer MRtoMessWith;
	static float inDamageRatio;
	bool youHaveDied;

	// Use this for initialization
	void Start () {
		youHaveDied = false;
		mSpeedBonus = 1;
		mPlayerRotation= 0;
		mBaseSpeed = 60;
		unitVelo = Vector2.zero;
		currentWeapon = WeaponModes.sword;
		//MRtoMessWith = this.gameObject.GetComponent<MeshRenderer>();
	//
	//Get Hit Vars Below:
	//
		hurt = false;
		lifeRemaining = maxLife;
		hurtStateTime = 2;
		inDamageRatio = 1f;
		hurtStateTimeRemaining = hurtStateTime;
	}
	
	// Update is called once per frame
	void Update () {
		if(hurt){
			countdownHurtTime();
		}
		if(!youHaveDied){
			Control();
			Move();
			Weapons();
		}
	}



	void Control(){
		unitVelo = Vector2.zero;
		/*if (Input.GetKey(KeyCode.W)){
			unitVelo.y+=1;
		}
		if (Input.GetKey(KeyCode.A)){
			unitVelo.x-=1;
		}
		if (Input.GetKey(KeyCode.S)){
			unitVelo.y-=1;
		}
		if (Input.GetKey(KeyCode.D)){
			unitVelo.x+=1;
		}
		unitVelo.Normalize();
		*/
		if (Input.GetKey(KeyCode.UpArrow)){
			unitVelo.y+=1;
		}
		if (Input.GetKey(KeyCode.LeftArrow)){
			unitVelo.x-=1;
		}
		if (Input.GetKey(KeyCode.DownArrow)){
			unitVelo.y-=1;
		}
		if (Input.GetKey(KeyCode.RightArrow)){
			unitVelo.x+=1;
		}
		unitVelo.Normalize();
		/*float x = Input.GetAxis("Horizontal") * mBaseSpeed * Time.deltaTime;
		float z = Input.GetAxis("Vertical")* mBaseSpeed * Time.deltaTime;
		character.transform.Translate(x,0,z);
		*/
	}

	void Move(){
		Vector3 vec3From2 = new Vector3(getEffectiveVelo().x, 0, getEffectiveVelo().y);
		gameObject.transform.position+= vec3From2;
		// rotate based on orientation of velo
		if (vec3From2 != Vector3.zero)transform.rotation = Quaternion.LookRotation(vec3From2);
	}

	public Vector2 getEffectiveVelo(){
		return unitVelo * (mBaseSpeed +  mSpeedBonus)* Time.deltaTime; // Insert modifier list here if need be
	}

	public GameObject swordPrefab;
	public GameObject currentSword;
	public GameObject cannonBallPrefab;
	public GameObject currentCannonBall;

	void Weapons(){
		if(Input.GetKeyUp(KeyCode.Tab)){
			if(currentWeapon== WeaponModes.sword){
				currentWeapon = WeaponModes.gun;
			}else{
				currentWeapon = WeaponModes.sword;
			}
		}
		if(Input.GetKeyDown(KeyCode.Space)){
			if(currentWeapon==WeaponModes.sword)sword();
			if(currentWeapon==WeaponModes.gun)cannon();
		}
	}

	void sword(){
		if(currentSword!=null) Destroy(currentSword);
		currentSword = Instantiate(swordPrefab, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
		currentSword.transform.parent = gameObject.transform;
		currentSword.transform.localPosition += new Vector3(2,-0.5f,0);
	}


	void cannon(){
		currentCannonBall = Instantiate(cannonBallPrefab, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
		currentCannonBall.transform.parent = gameObject.transform;
		currentCannonBall.transform.localPosition += new Vector3(0,0.75f,2.5f);
		currentCannonBall.GetComponent<Rigidbody>().velocity = (gameObject.transform.forward+(Vector3.up *0.05f))* 40; 
		currentCannonBall.transform.parent = null;
	}

	public static void SetNoise(float newNoise)
	{
		noise = newNoise;
	}

	public static float GetNoise()
	{
		return noise;
	}

	/*Set a new max speed for player*/
	public static void SetSpeedBonus(float newBonus)
	{
		mSpeedBonus = newBonus;
	}

	public static void SetBaseAttackStrength(float newStrength)
	{
		baseAttackStrength = newStrength;
	}

	public static float GetEffectiveAttackStrength()
	{
		float outVal = baseAttackStrength;
		if(currentWeapon == WeaponModes.gun){
			outVal-=0.5f;
		}else if(currentWeapon == WeaponModes.sword){
			outVal+=2;
		}
		return outVal;
	}

	
	public static void SetInDamageRatio(float newInDamageRatio)
	{
		inDamageRatio = newInDamageRatio;
	}

//NEED TO HAVE A CATCH FOR GOING OVER MAX LIFE
	public static void IncreaseLife(float increase)
	{
		lifeRemaining += increase;
	}

	void OnTriggerEnter(Collider collider) {
		if(!hurt && (collider.gameObject.tag =="enemy" || collider.gameObject.tag =="enemyCrow" || collider.gameObject.tag == "enemyGrozzle" || collider.gameObject.tag == "carnivore"||collider.gameObject.tag == "enemyNoxiousCrawler"||collider.gameObject.tag == "HAZARD")){
			float damageDealt = effectiveDamageTabulation(collider);
			lifeRemaining-= damageDealt;
			if(lifeRemaining<=0){
				Debug.Log("YOU DIED");
				handleDeath();
				//Destroy(this.gameObject);
			}
			healthGUIScript.setHearts((int)lifeRemaining);
		startHurtState();
		}
	}


	void handleDeath(){
		youHaveDied = true;
		//Application.LoadLevel("Menu");
	}


void OnGUI(){
	if (youHaveDied){
		GUI.Window(0, new Rect((Screen.width/2)-150, (Screen.height/2)-75, 300, 250), ShowGUI, "GAME OVER");
	}
}
 
void ShowGUI(int windowID){
    GUI.Label(new Rect(65, 40, 200, 100), "YOU HAVE DIED, PRESS THE BUTTON BELOW TO RETURN TO MAIN MENU");
	if (GUI.Button(new Rect(125, 150, 100, 30), "MAIN MENU")){
		Application.LoadLevel("Menu");
	}
}






	const float mEnemcyGenericInDamage = 1.0f;
	const float mEnemcyGrozzleInDamage = 3.0f;
	const float mEnemcyCrowInDamage = 1.0f;
	const float mEnemcyCarnivoreInDamage = 2.0f;
	const float mEnemcyNoxiousCrawlerInDamage = 2.0f;
	const float mHazardDamage = 1.0f;




	float effectiveDamageTabulation(Collider collider){
		float retVal = 0;
		if(collider.gameObject.tag =="enemy") retVal=mEnemcyGenericInDamage;
		if(collider.gameObject.tag =="enemyCrow") retVal=mEnemcyCrowInDamage;
		if(collider.gameObject.tag == "enemyGrozzle") retVal = mEnemcyGrozzleInDamage;
		if(collider.gameObject.tag == "carnivore") retVal=mEnemcyCarnivoreInDamage;
		if(collider.gameObject.tag == "enemyNoxiousCrawler"){
			retVal=mEnemcyNoxiousCrawlerInDamage;
			collider.gameObject.GetComponent<NoxiousCrawlerEnemyScript>().reproduce();
			Destroy(collider.gameObject);
		}
		if(collider.gameObject.tag == "HAZARD"){
			retVal=mHazardDamage;
		}
		return retVal * inDamageRatio;
	}
    void startHurtState(){
    	hurt = true;
    	hurtStateTimeRemaining = hurtStateTime;
    	Debug.Log("IT IS NOW HURT" + " Life Remaining is " + lifeRemaining);

    }
    void countdownHurtTime(){
    	hurtStateTimeRemaining-= Time.deltaTime;
    	MRtoMessWith.enabled = ((Time.frameCount%3)!=0) ? false : true;
    	if(hurtStateTimeRemaining<=0){
    		 hurt = false;
    		 Debug.Log("IT IS NO LONGER HURT");
    		 MRtoMessWith.enabled = true;
    		 //MRtoMessWith.enabled = false;
    	}
    }
}
