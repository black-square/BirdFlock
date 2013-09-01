using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
	
	public Object prefab;
	// Use this for initialization
	void Start () {
		float count = 5; 
		float size = 0.2f;
		
		for( int i = 0; i < count; ++i )
			for( int j = 0; j < count; ++j )
				Instantiate(prefab, new Vector3 (size * i - size*count/2, size * j - size*count/2, 1), Quaternion.identity); 
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
