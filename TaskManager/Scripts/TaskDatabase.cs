using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class TaskDatabase : ScriptableObject {

	[SerializeField]
	public List<Task> database;

	void OnEnable() {

		if( database == null )
		{
			database = new List<Task>();
		}

	}

	public void Add( Task _task ) {
		database.Add( _task );
	}
	public void Remove( Task _task ) {
		database.Remove( _task );
	}

}
