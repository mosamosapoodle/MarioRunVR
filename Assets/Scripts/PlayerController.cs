﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour 
{
	bool isGround = true;
	PlayerAngleController pac;

	public float thrust;
	[SerializeField] Rigidbody rb;
	public int hp = 1;

	public GameObject killer;

	GameObject[] killerPositions;


	bool isRunning = false;

	bool isDead = false;

	AudioClip getDeadSound;
	AudioClip getJumpSound;

	AudioSource audioSource;

	Vector3 startPosition;

	enum Achievement {
		NotYet,
		FirstPoint,
		SecondPoint,
	};

	Achievement achievement;
	PlayerController playerController;

	void Start() 
	{
		pac = GetComponent<PlayerAngleController> ();

		startPosition = new Vector3 (0, 1.7f, 2);

		rb = GetComponent<Rigidbody>();

		killerPositions = GameObject.FindGameObjectsWithTag ("Position");

		getDeadSound = Resources.Load<AudioClip>("Audio/dead");
		getJumpSound = Resources.Load<AudioClip>("Audio/jump");

		audioSource = this.GetComponent<AudioSource>();

		Vector3 goal1 = transform.position + (new Vector3 (0, 1.7f, 41f));
		Vector3 goal2 = transform.position + (new Vector3 (0, 1.7f, 69f));

		switch (achievement) {
			case Achievement.FirstPoint:
				this.transform.position = goal1;
					break;
			case Achievement.SecondPoint:
				this.transform.position = goal2;
				break;
			default:
				break;
		}
		playerController = GetComponent<PlayerController> ();
	}

	void Update() {
		if (this.transform.position.y < -4) {
			isDead = true;
			audioSource.PlayOneShot(getDeadSound);
			Invoke ("MainScene", getDeadSound.length);
		}

		if (this.transform.position.z > 20 && this.transform.position.z < 80) {
			if (SceneManager.GetActiveScene ().name == "Main") {
				StartCoroutine ("GenerateKiller");
			}
		}

		// if achieved first point once
		if (this.transform.position.z >= 42f) {
			achievement = Achievement.FirstPoint;
		}
		if (this.transform.position.z >= 70f) {
			achievement = Achievement.SecondPoint;
		}
		if (isGround == true && Input.GetKeyDown (KeyCode.Space)) {
			playerController.JumpSound ();
			isGround = false;
		}
	}

	IEnumerator GenerateKiller () {
		if (isRunning) {
			yield break;
		}

		int key = Random.Range (0, killerPositions.Length);
		Instantiate (
			killer,
			killerPositions[key].transform.position,
			Quaternion.identity
		);
		isRunning = true;
		yield return new WaitForSeconds (6f);
	}

	void FixedUpdate() 
	{
		if (Input.GetKey (KeyCode.Space) && Input.GetKey("up")) {
			rb.AddForce (transform.forward * thrust);
		}
	}

	void OnCollisionEnter(Collision other){
		if (other.gameObject.name == "KuribouCollider" || other.gameObject.name == "PakkunBody" || other.gameObject.name == "Dossun") {
			hp--;
			if (hp == 0) {
				pac.enabled = false;
				isDead = true;
				audioSource.PlayOneShot(getDeadSound);
				Invoke ("MainScene", getDeadSound.length);
			} else {
				this.transform.localScale -= new Vector3 (0.0f, 0.3f, 0.0f);
				this.transform.position -= new Vector3 (0.0f, 0.2f, 0.0f);
				Destroy (other.transform.parent.gameObject);
			}
		} else if (other.gameObject.name == "Shroom(Clone)") {
			hp++;
			this.transform.position += new Vector3 (0.0f, 0.2f, 0.0f);
			this.transform.localScale += new Vector3 (0.0f, 0.3f, 0.0f);
		}

		if (other.gameObject.tag == "Goal") {
			KuppaScene ();
//			StartCoroutine ("ExplodeAsGoalEffect");
//			Invoke("Kuppa", 2f);
		}
		if (other.gameObject.tag == "Ground") {
			isGround = true;
		}
	}

	void MainScene () {
		SceneManager.LoadScene ("Main");
	}

	void KuppaScene () {
		SceneManager.LoadScene ("Kuppa");
	}

	void ResetGameScene () {
		pac.enabled = true;
		Achievement tmp1 = achievement;

		this.transform.position = startPosition;

		achievement = tmp1;
	}

	IEnumerator ExplodeAsGoalEffect () {
		GameObject explosion = killer.GetComponent<KillerScript> ().GetExplosion ();

		for (int i = 0; i < 100; i++) {
			Instantiate (explosion, transform.position, Quaternion.identity);
			yield return new WaitForSeconds (0.5f);	
		}
	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.name == "Dossun") {
			hp--;
			if (hp == 0) {
				isDead = true;
				audioSource.PlayOneShot(getDeadSound);
				Invoke ("MainScene", getDeadSound.length);
			} else {
				this.transform.localScale -= new Vector3 (0.0f, 0.3f, 0.0f);
				this.transform.position -= new Vector3 (0.0f, 0.2f, 0.0f);
			}
		}
	}

	public void JumpSound () {
		audioSource.PlayOneShot(getJumpSound);
	}

	public bool whetherDead () {
		return isDead;
	}
}