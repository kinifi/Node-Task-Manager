using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class TaskManager: EditorWindow {

	public TaskDatabase m_Task;

	//asset path
	public string DataPath = "Assets/Editor/TaskManager/";
	public string CurrentDataPath = null;


	public string newBoardName = null;
	public string currentBoardName;

	public bool n_HideSideBar = true;
	public bool n_HideCreateAsset = false;
	public string n_FileLocation = null;
	public string n_Name;
	public string n_Desc;
	public string n_Owner;
	public GameObject n_GameObject;

	public static Texture2D Background;
	public Vector2 scrollPosition;
	public Vector2 mousePos;

	//asset preview
	public GameObject gameObjectToDisplay;
	public Editor gameObjectEditor;


	[MenuItem("Window/Task Manager")]
	static void ShowEditor() {
		
		TaskManager editor = EditorWindow.GetWindow<TaskManager>();
		editor.minSize = new Vector2 (800, 600);

		editor.Init();
	}
	
	public void Init() 
	{
		//get the directory we are in so we can make our paths relative
		//Assetdatabase.LoadAsset requires a relative path
		CurrentDataPath = System.Environment.CurrentDirectory + "/";

		//load the background texture
		Background = AssetDatabase.LoadAssetAtPath (DataPath + "Textures/background.png", typeof(Texture2D)) as Texture2D;
	}

	void OnEnable() {
		
	}

	void LoadOldDatabase()
	{
		m_Task = (TaskDatabase)AssetDatabase.LoadAssetAtPath( n_FileLocation , typeof(TaskDatabase));
		Debug.Log("Loaded Existing Database");
	}

	void CreateNewDatabase()
	{
		if(newBoardName != "" && newBoardName != null && newBoardName != " ")
		{
			m_Task = ScriptableObject.CreateInstance<TaskDatabase>();
			AssetDatabase.CreateAsset(m_Task, DataPath + "TaskData/" + newBoardName + ".asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			currentBoardName = newBoardName;
			newBoardName = null;
		}
	}


	void DeleteDatabase()
	{
		bool confirm = EditorUtility.DisplayDialog("Close Board?", "Are you sure you want to Close this Task Board?", "Yes", "No");

		if(confirm == true)
		{
			m_Task = null;
			newBoardName = null;
			currentBoardName = null;
			n_FileLocation = null;
			Debug.Log("Closed Board");
		}
		else
		{
			Debug.Log("Cancelled Closing Board");
		}
	}


	void OnGUI() {
		
		//DrawNodeCurve(window1, window2); // Here the curve is drawn under the windows

		if (Event.current.type == EventType.Repaint) 
		{
			if(Background == null)
			{
				Background = AssetDatabase.LoadAssetAtPath (DataPath + "Textures/background.png", typeof(Texture2D)) as Texture2D;
			}

			GUI.DrawTexture(new Rect(0,0, Screen.width, Screen.height) ,Background, ScaleMode.ScaleAndCrop);
		}


		//draw the main area 
		GUILayout.BeginArea(new Rect(0,0, Screen.width, Screen.height));

		//start the scroll view
		scrollPosition = GUI.BeginScrollView(new Rect(0, 0, Screen.width, Screen.height), scrollPosition, new Rect(0, 0, Screen.width*4, Screen.height-15), false, false);


		//load the task windows
		if(m_Task != null)
		{
			LoadTasks();
		}

		GUI.EndScrollView();
		//GUILayout.EndScrollView();
		GUILayout.EndArea();

		GUILayout.Label("Board Position: " + scrollPosition.ToString());
		GUILayout.BeginHorizontal();

		if(GUILayout.Button("Snap To Left", "minibuttonleft", GUILayout.Width(75)))
		{
			scrollPosition = new Vector2(0, 0);
		}

		if(GUILayout.Button("Snap To Right", "minibuttonright", GUILayout.Width(80)))
		{
			scrollPosition = new Vector2(Screen.width*4, 0);	
		}

		GUILayout.EndHorizontal();


		//draw the sidebar
		sideBarGUI();

		//check for input so we can create new tasks
		InputEvents();



	}

	void InputEvents()
	{

		Event e = Event.current;
		if (e.type == EventType.MouseDown || e.type == EventType.MouseUp)
		{
			//set the mouse position
			mousePos = new Vector2(e.mousePosition.x, e.mousePosition.y);

			//check which mouse button was pressed
			if (e.button == 1) 
			{ 

				if(m_Task != null)
				{
					//Right click -> Show Menu
					GenericMenu menu = new GenericMenu ();
					menu.AddItem (new GUIContent ("Create Task"), false, CreateNodeTask, "createTask");
					//set the mouse position

					menu.ShowAsContext ();
				}
				e.Use();
			}
			
		}
	}
	

	void CreateNodeTask(object obj)
	{

		//create a blank task 
		m_Task.Add(new Task("", "", ""));
		//set the task location so its in the view
		m_Task.database[m_Task.database.Count-1].taskPosition = new Rect(scrollPosition.x,
		                                                                 m_Task.database[m_Task.database.Count-1].taskPosition.y,
		                                                               m_Task.database[m_Task.database.Count-1].taskPosition.size.x,
		                                                               m_Task.database[m_Task.database.Count-1].taskPosition.size.y);
		Repaint();
	}

	//Called when the Task Manager window is focused with the mouse
	void OnFocus()
	{
		Repaint();
	}

	//displays the side bar
	void sideBarGUI()
	{

		if(m_Task == null)
		{
			//display the board we want to load
			GUILayout.BeginArea(new Rect(Screen.width-250,0, 250, Screen.height));
			EditorGUILayout.BeginVertical("GroupBox");

			EditorGUILayout.Space();
			///CREATING BOARD
			EditorGUILayout.LabelField("Create A New Board");
			newBoardName = EditorGUILayout.TextField("Board Name: ", newBoardName);

			if(GUILayout.Button("Create New Board"))
			{
				if(newBoardName != "" && newBoardName != null && newBoardName != " ")
				{
					CreateNewDatabase();
					//Debug.Log("Created New Board");
				}
				else
				{
					Debug.LogError("Enter A Valid Board Name");
				}
			}

			EditorGUILayout.Space();

			//END Creating BOARD

			///loading a board
			if(n_FileLocation == null)
			{
				EditorGUILayout.LabelField("No Board Selected");
			}
			else
			{
				EditorGUILayout.LabelField(n_FileLocation);
			}
			if(GUILayout.Button("Select Board"))
			{
				n_FileLocation = EditorUtility.OpenFilePanel("Select A Board","Assets/Editor/TaskManager/TaskData/","asset");
				if (n_FileLocation.Length != 0) 
				{

					if(n_FileLocation.Contains("Assets"))
					{
						n_FileLocation = n_FileLocation.Replace(CurrentDataPath, "");
						Debug.Log(n_FileLocation);
					}

					//load the board here
					LoadOldDatabase();
				}
			}
			if(GUILayout.Button("Clear File Path"))
			{
				n_FileLocation = null;
				newBoardName = null;
			}
			///END Loading Board


			EditorGUILayout.EndVertical();
			GUILayout.EndArea();
		}
		else
		{

			if(n_HideSideBar == false)
			{
				GUILayout.BeginArea(new Rect(Screen.width-250,0, 250, Screen.height));
				EditorGUILayout.BeginVertical("GroupBox");

				EditorGUILayout.Space();

				if(m_Task != null)
				{
					EditorGUILayout.LabelField("Board Data");
					EditorGUILayout.LabelField("Number of Tasks: " + m_Task.database.Count);
				}

				EditorGUILayout.Space();
				EditorGUILayout.Space();

				if (GUILayout.Button ("Add Task", GUILayout.Height(30)))
				{

					m_Task.Add(new Task("", "", ""));
					EditorUtility.SetDirty(m_Task);
					Repaint();
				}

				EditorGUILayout.Space();


				if (gameObjectToDisplay != null) 
				{

					//if(gameObjectEditor != null)
					//{
						EditorGUILayout.LabelField("Asset From Task");
						gameObjectEditor = Editor.CreateEditor(gameObjectToDisplay);
						gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(250, 150), EditorStyles.miniButton);

					//}
				}		

				EditorGUILayout.Space();
				EditorGUILayout.Space();
				
				if(GUILayout.Button("Hide SideBar"))
				{
					n_HideSideBar = true;
				}

				if(GUILayout.Button("Close Board"))
				{
					DeleteDatabase();
				}

				EditorGUILayout.EndVertical();
				GUILayout.EndArea();
			}
			else
			{
				GUILayout.BeginArea(new Rect(Screen.width-250,0, 250, Screen.height));
				EditorGUILayout.BeginHorizontal("GroupBox");

				if(GUILayout.Button("Show Sidebar"))
				{
					n_HideSideBar = false;
				}

				EditorGUILayout.EndHorizontal();
				GUILayout.EndArea();
			}

		}
		
	}

	//Create a window for every single task in the list 
	void LoadTasks()
	{

		if(m_Task.database.Count > 0)
		{

			BeginWindows();
			//create all the tasks
			for (int i = 0; i < m_Task.database.Count; i++) 
			{
				//create a window for each task in the list
				m_Task.database[i].taskPosition = GUI.Window(i, m_Task.database[i].taskPosition, DrawNodeWindow, "Title: " + m_Task.database[i].taskName);
			}

			EndWindows();

		}
	}






	void DrawNodeWindow(int id) {

		Event e = Event.current;
		if (e.type == EventType.MouseDown)
		{
			if (e.button == 0) 
			{
				if(m_Task.database[id].taskDisplayObject != null)
				{
					gameObjectEditor = null;
					gameObjectToDisplay = m_Task.database[id].taskDisplayObject;
				}
				else
				{
					gameObjectToDisplay = null;
					gameObjectEditor = null;
				}


			}

		}


		//Draw all the content
		if(m_Task.database[id].isEditing == false)
		{
			//Have everything in an edit viewing mode here
			EditorGUILayout.LabelField("Title: ");
			m_Task.database[id].taskName = EditorGUILayout.TextField(m_Task.database[id].taskName);

			//the task description
			EditorGUILayout.LabelField("Description: ");
			m_Task.database[id].taskDescription = EditorGUILayout.TextArea(m_Task.database[id].taskDescription, GUILayout.Height(80));

			//display Task Status
			EditorGUILayout.LabelField("Task Status: ");
			m_Task.database[id].taskStatus = (Task.Status)EditorGUILayout.EnumPopup("", m_Task.database[id].taskStatus);

			//display the Owner field
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Owner: ");
			m_Task.database[id].taskOwner = EditorGUILayout.TextField(m_Task.database[id].taskOwner);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Select A GameObject");
			EditorGUILayout.LabelField("Related to the Task");
			m_Task.database[id].taskDisplayObject = (GameObject) EditorGUILayout.ObjectField(m_Task.database[id].taskDisplayObject, typeof(GameObject), true);
			EditorGUILayout.Space();

			if(GUILayout.Button("Done Editing"))
			{
				m_Task.database[id].isEditing = true;
				m_Task.database[id].taskPosition = new Rect(m_Task.database[id].taskPosition.x, m_Task.database[id].taskPosition.y, 200, 180);

				EditorUtility.SetDirty(m_Task);
			}

			if(GUILayout.Button("Delete Task"))
			{
				m_Task.database.Remove(m_Task.database[id]);
				EditorUtility.SetDirty(m_Task);
			}


		}
		else
		{
			//display mode

			//display the description
			EditorGUILayout.LabelField(m_Task.database[id].taskDescription, GUILayout.Height(40));
			EditorGUILayout.Space();

			//display Task Status
			EditorGUILayout.LabelField("Task Status: ");
			EditorGUILayout.EnumPopup("", m_Task.database[id].taskStatus);

			if(m_Task.database[id].taskDisplayObject != null)
			{
				EditorGUILayout.LabelField("Asset in Viewable in SideBar");
			}

			//Display the owner name
			EditorGUILayout.BeginHorizontal("minibutton");
			EditorGUILayout.LabelField("Owner: " + m_Task.database[id].taskOwner);
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.Space();
			//Edit the task
			if(GUILayout.Button("Edit Task"))
			{
				m_Task.database[id].isEditing = false;
				m_Task.database[id].taskPosition = new Rect(m_Task.database[id].taskPosition.x, m_Task.database[id].taskPosition.y, 200, 360);
			}
		}

		GUI.DragWindow(new Rect(0, 0, Screen.width, Screen.height));

	}

	void DrawRealNodeCurve(Rect start, Rect end)
	{
		DrawNodeCurve(start, end, new Vector2(1.0f, 0.5f), new Vector2(1.0f, 0.5f));
	}
	
	void DrawNodeCurve(Rect start, Rect end, Vector2 vStartPercentage, Vector2 vEndPercentage )
	{
		Vector3 startPos = new Vector3(start.x + start.width * vStartPercentage.x, start.y + start.height * vStartPercentage.y, 0);
		Vector3 endPos = new Vector3(end.x + end.width * vEndPercentage.x, end.y + end.height * vEndPercentage.y, 0);
		Vector3 startTan = startPos + Vector3.right * (-50 + 100 * vStartPercentage.x) + Vector3.up * (-50 + 100 * vStartPercentage.y);
		Vector3 endTan = endPos + Vector3.right * (-50 + 100 * vEndPercentage.x) + Vector3.up * (-50 + 100 * vEndPercentage.y);
		Color shadowCol = new Color(0, 0, 0, 0.06f);
		for (int i = 0; i < 3; i++) // Draw a shadow
			Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
		Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 2);
	}
}
