using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Task {
	
	[SerializeField]
	public string taskName;
	
	[SerializeField]
	public string taskDescription;

	[SerializeField]
	public Rect taskPosition;

	[SerializeField]
	public string taskOwner;

	[SerializeField]
	public bool isEditing = false;

	[SerializeField]
	public GameObject taskDisplayObject;

	[SerializeField]
	public enum Status { Completed, Started, Researching, Blocked, NotStarted };

	[SerializeField]
	public Status taskStatus = Status.NotStarted;


	public Task(string _Name, string _Description, string _owner, GameObject _gameobject = null)
	{
		taskName = _Name;
		taskDescription = _Description;
		taskOwner = _owner;
		taskPosition = new Rect(Screen.width/2, Screen.height/2, 200, 380);
		taskDisplayObject = _gameobject;
		taskStatus = Status.NotStarted;
	}

}
