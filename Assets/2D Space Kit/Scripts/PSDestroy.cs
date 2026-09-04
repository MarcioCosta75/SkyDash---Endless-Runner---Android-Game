using UnityEngine;
using System.Collections;

public class PSDestroy : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
