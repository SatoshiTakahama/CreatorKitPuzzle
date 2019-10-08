using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System;
using System.Text;
using Example;
using VrGrabber;

public class PlayUI : MonoBehaviour
{
    public SceneCompletion sceneCompletion;
    public Transform distTransform;

	int inMenu;
    private List<GameObject> m_puzzlePiecePrefabs = new List<GameObject>();
	private GameObject m_selectedPuzzlePiece;
	private int m_mode;
    private PuzzlePiecesList m_puzzlePieces = new PuzzlePiecesList();
    private GameObject[] m_grabbers;

	private const int MODE_EDIT = 0;
	private const int MODE_PLAY = 1;
	private const string m_saveFileName = "test";

	private Text AddLabelFormat(string label, int targetCanvas = 0, float panelWidth = 600.0f, int panelHeight = 1)
	{
        //Debug.Log("AddLabelFormat : "+label + " targetCanvas="+targetCanvas);
        RectTransform rt = DebugUIBuilderHand.instance.AddLabel(label, targetCanvas);
        rt.sizeDelta = new Vector2 (panelWidth, rt.rect.height * panelHeight);
        Text text = rt.GetComponent<Text>();
        text.alignment = TextAnchor.MiddleLeft;
        text.resizeTextForBestFit = false;
		return text;
	}

	void Start ()
    {
        //Center Panel
        DebugUIBuilderHand.instance.AddButton("Play Mode", PlayModeButtonPressed, DebugUIBuilderHand.EDIT_PANE_CENTER);
        DebugUIBuilderHand.instance.AddButton("Save", SaveButtonPressed, DebugUIBuilderHand.EDIT_PANE_CENTER);
        DebugUIBuilderHand.instance.AddButton("Load", LoadButtonPressed, DebugUIBuilderHand.EDIT_PANE_CENTER);
        DebugUIBuilderHand.instance.AddButton("Delete", DeleteButtonPressed, DebugUIBuilderHand.EDIT_PANE_CENTER);

        //Right Panel
        AddLabelFormat("Select PuzzlePieces", DebugUIBuilderHand.EDIT_PANE_RIGHT);
    	//PuzzlePieceを表示
		string[] tags = {"DynamicPuzzlePiece","PuzzlePiece"};
		foreach (var tag in tags)
		{
			GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
			foreach (var obj in objects)
			{
				Debug.Log("puzzlePieces: " + obj.name);
				m_puzzlePiecePrefabs.Add(obj);
				DebugUIBuilderHand.instance.AddRadio(obj.name, "group", delegate(Toggle t) { RadioPressed(obj.name, "group", t); }, DebugUIBuilderHand.EDIT_PANE_RIGHT) ;
				obj.SetActive(false);
			}
		}

        //Center Panel
        DebugUIBuilderHand.instance.AddButton("Edit Mode", EditModeButtonPressed, DebugUIBuilderHand.PLAY_PANE_CENTER);
        DebugUIBuilderHand.instance.AddButton("Restart", RestartButtonPressed, DebugUIBuilderHand.PLAY_PANE_CENTER);

        inMenu = 0;
        m_mode = MODE_EDIT;
        m_grabbers = GameObject.FindGameObjectsWithTag("Grabber");
        selectePuzzlePiece("DominoBlock");

        //TestSaveLoad();
	}

    private void TestSaveLoad()
    {
		//以下は動作確認用
        Debug.Log("DataPath : "+UnityEngine.Application.persistentDataPath);
        GameObject pieceInstance = Instantiate(m_selectedPuzzlePiece, distTransform.position, Quaternion.identity) as GameObject;
        pieceInstance.SetActive(true);
		m_puzzlePieces.Add(m_selectedPuzzlePiece.name, pieceInstance);
        SaveButtonPressed();
        LoadButtonPressed();
    }

    private void PlayModeButtonPressed()
    {
        //"Play Mode"に遷移する
        m_mode = MODE_PLAY;
		if (inMenu == 1)
		{
			UpdateMode(m_mode);
		}
        //Marbleが転がるようにする
		string[] tags = {"Marble","DynamicPuzzlePiece"};
		UpdateRigidbody(tags, true, false);
        //パズルのオブジェクトは移動できないようにする(つかめないようにする)
		UpdateGrabber(false);
    }

    private void SaveButtonPressed()
    {
		m_puzzlePieces.Save();
        var file = new DataFilePlain();
        file.Save(m_puzzlePieces, m_saveFileName);
    }

    private void DeleteButtonPressed()
    {
		System.IO.File.Delete(@m_saveFileName);
    }

    private void LoadButtonPressed()
    {
        var file = new DataFilePlain();
        PuzzlePiecesList loadPieces = file.Load<PuzzlePiecesList>(m_saveFileName);

		Debug.Log("load");
		if (loadPieces != null)
		{
	        //現在の情報をすべてクリアする
	        foreach (var piece in m_puzzlePieces.puzzlePieces)
	        {
				if (piece != null)
				{
					Debug.Log("delete puzzlePieces: " + piece.name);
					Destroy(piece.obj);
				}
	        }
	        m_puzzlePieces.Clear();
	        //Loadした情報に更新する
	        foreach (var piece in loadPieces.puzzlePieces)
	        {
				if (piece != null)
				{
					Debug.Log("add puzzlePieces: " + piece.name);
					foreach (var prefab in m_puzzlePiecePrefabs)
					{
						if (prefab.name == piece.name)
						{
					        GameObject pieceInstance = Instantiate(prefab, piece.position, piece.rotation) as GameObject;
					        pieceInstance.SetActive(true);
							m_puzzlePieces.Add(prefab.name, pieceInstance);
							break;
						}
					}
				}
	        }
	        //Goal/Marbleの配置を更新する
			m_puzzlePieces.Load(loadPieces.goal, loadPieces.marble);
		}
    }

    private void RadioPressed(string radioLabel, string group, Toggle t)
    {
    	selectePuzzlePiece(radioLabel);
    }

    private void selectePuzzlePiece(string name)
    {
    	//生成するPuzzlePieceを選択する
		foreach (var piece in m_puzzlePiecePrefabs)
		{
			if (piece.name == name)
			{
				m_selectedPuzzlePiece = piece;
		        //TODO プレビュー表示を更新する
				break;
			}
		}
    }

    private void EditModeButtonPressed()
    {
        //"Edit Mode"に遷移する
        m_mode = MODE_EDIT;
		if (inMenu == 1)
		{
			UpdateMode(m_mode);
		}
        //Marbleが転がらないようにする
		string[] tags = {"Marble","DynamicPuzzlePiece"};
		UpdateRigidbody(tags, false, true);
        //パズルのオブジェクトを移動できるようにする(つかめるようにする)
		UpdateGrabber(true);
    }

    private void UpdateRigidbody(string[] tags, bool useGravity, bool isKinematic)
    {
		foreach (var tag in tags)
		{
			GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
			foreach (var obj in objects)
			{
				Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
				rigidbody.useGravity = useGravity;
				rigidbody.isKinematic = isKinematic;
			}
		}
    }

    private void UpdateGrabber(bool active)
    {
		foreach (var obj in m_grabbers)
		{
			obj.SetActive(active);
		}
    }

    private void RestartButtonPressed()
    {
        sceneCompletion.ReloadLevel();
        //"Edit Mode"に戻ってしまう？モードも保存が必要では？
    }

	private void UpdateMode(int mode)
	{
		if (mode == MODE_EDIT)
		{
			int[] panels = {DebugUIBuilderHand.EDIT_PANE_CENTER, DebugUIBuilderHand.EDIT_PANE_RIGHT};
			DebugUIBuilderHand.instance.ShowActivePanels(panels);
		}
		else if (inMenu == MODE_PLAY)
		{
			int[] panels = {DebugUIBuilderHand.PLAY_PANE_CENTER};
			DebugUIBuilderHand.instance.ShowActivePanels(panels);
		}
	}

    void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.Two))
        {
            if (inMenu == 0)
            {
	            inMenu = 1;
	            UpdateMode(m_mode);
            }
            else if (inMenu == 1)
            {
	            inMenu = 2;
	            int[] panels = {DebugUIBuilderHand.DEBUG_PANE_CENTER, DebugUIBuilderHand.DEBUG_PANE_RIGHT};
            	DebugUIBuilderHand.instance.ShowActivePanels(panels);
            }
            else
            {
	            inMenu = 0;
            	DebugUIBuilderHand.instance.Hide();
            }
        }
        if(OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
			if (m_selectedPuzzlePiece != null)
			{
		        Vector3 position = distTransform.position + distTransform.forward * 1.0f + new Vector3(0, 3.0f, 0);
		        GameObject pieceInstance = Instantiate(m_selectedPuzzlePiece, position, Quaternion.identity) as GameObject;
		        pieceInstance.SetActive(true);
				m_puzzlePieces.Add(m_selectedPuzzlePiece.name, pieceInstance);
		    }
        }
        if(OVRInput.GetDown(OVRInput.Button.Two))
        {
        }
    }
}

[Serializable]
public class PuzzlePiecesList
{
    public List<Piece> puzzlePieces = new List<Piece>();
    public Piece goal = new Piece();
    public Piece marble = new Piece();
    public void Add(string name, GameObject puzzlePieceInstance)
    {
        Piece addPiece = new Piece();
        addPiece.name = name;
        addPiece.obj = puzzlePieceInstance;
        puzzlePieces.Add(addPiece);
    }
    public void Clear()
    {
        puzzlePieces.Clear();
    }
    public void Save()
    {
        foreach (var piece in puzzlePieces)
        {
			if (piece != null)
			{
				Debug.Log("puzzlePieces: " + piece.name);
			}
			piece.position = piece.obj.transform.localPosition;
			piece.rotation = piece.obj.transform.localRotation;
		}
        //Goal/Marbleは削除しないので別管理にする
		goal.position = GameObject.FindGameObjectWithTag("Goal").transform.localPosition;
		goal.rotation = GameObject.FindGameObjectWithTag("Goal").transform.localRotation;
		marble.position = GameObject.FindGameObjectWithTag("Marble").transform.localPosition;
		marble.rotation = GameObject.FindGameObjectWithTag("Marble").transform.localRotation;
		Debug.Log("save");
    }
    public void Load(Piece goal, Piece marble)
    {
        //Goal/Marbleは削除しないので別管理にする
		if (goal != null)
		{
			GameObject.FindGameObjectWithTag("Goal").transform.localPosition = goal.position;
			GameObject.FindGameObjectWithTag("Goal").transform.localRotation = goal.rotation;
		}
		if (marble != null)
		{
			GameObject.FindGameObjectWithTag("Marble").transform.localPosition = marble.position;
			GameObject.FindGameObjectWithTag("Marble").transform.localRotation = marble.rotation;
		}
    }
}

[Serializable]
public class Piece
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    [NonSerialized]
    public GameObject obj;
}
