using UnityEngine;

public class DestroyPuzzlePiece : MonoBehaviour
{
    public void OnGrabClicked()
    {
		Debug.Log("OnGrabClicked: " + gameObject.name);
        Destroy(gameObject);
    }
}
