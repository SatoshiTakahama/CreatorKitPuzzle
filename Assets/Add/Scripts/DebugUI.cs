using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System;
using System.Text;

public class DebugUI : MonoBehaviour
{
    public Transform displayTransform;

	int inMenu;
	private Text m_textFPS;
	private Text m_textLog;
	private Text m_textPosition;
	private Text m_textRotation;

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
        m_textFPS = AddLabelFormat("-", DebugUIBuilderHand.DEBUG_PANE_CENTER);
        m_textPosition = AddLabelFormat("-", DebugUIBuilderHand.DEBUG_PANE_CENTER);
        m_textRotation = AddLabelFormat("-", DebugUIBuilderHand.DEBUG_PANE_CENTER);

        //Right Panel
        AddLabelFormat("Debug Log", DebugUIBuilderHand.DEBUG_PANE_RIGHT);
        m_textLog = AddLabelFormat("", DebugUIBuilderHand.DEBUG_PANE_RIGHT, 1800.0f, 20);
        Application.logMessageReceived += HandleLog;

        inMenu = 1;
	}

	private float elapsed = 0;
	private int tick = 0;

	private void updateFPS(Text fpsText)
	{
		tick++;
		elapsed += Time.deltaTime;
		if (elapsed >= 1f)
		{
			float fps = tick / elapsed;
			tick = 0;
			elapsed = 0;
			if (fpsText != null)
			{
				StringBuilder sb = new StringBuilder(16);
				fpsText.text = sb.AppendFormat("{0:F1} FPS", fps).ToString();
			}
		}
	}

    private bool viewInRect = true;

    private void HandleLog(string logText, string stackTrace, LogType logType)
    {
        if (string.IsNullOrEmpty(logText))
            return;

        m_textLog.text += logText + System.Environment.NewLine;

        if (viewInRect && m_textLog.verticalOverflow == VerticalWrapMode.Truncate)
            AdjustText(m_textLog);
    }

    private void AdjustText(Text t)
    {
        TextGenerator generator = t.cachedTextGenerator;
        var settings = t.GetGenerationSettings(t.rectTransform.rect.size);
        generator.Populate(t.text, settings);

        int countVisible = generator.characterCountVisible;
        if (countVisible == 0 || t.text.Length <= countVisible)
            return;

        int truncatedCount = t.text.Length - countVisible;
        var lines = t.text.Split('\n');
        foreach (string line in lines)
        {
            // 見切れている文字数が0になるまで、テキストの先頭行から消してゆく
            t.text = t.text.Remove(0, line.Length + 1);
            truncatedCount -= (line.Length + 1);
            if (truncatedCount <= 0)
                break;
        }
    }

	private void updateTransform(Transform displayTransform, Text textPosition, Text textRotation)
	{
		if (displayTransform != null)
		{

			float worldPosX = displayTransform.position.x;
			float worldPosY = displayTransform.position.y;
			float worldPosZ = displayTransform.position.z;
			float worldRotX = displayTransform.rotation.x;
			float worldRotY = displayTransform.rotation.y;
			float worldRotZ = displayTransform.rotation.z;
//			float localPosX = displayTransform.localPosition.x;
//			float localPosY = displayTransform.localPosition.y;
//			float localPosZ = displayTransform.localPosition.z;
//			float localRotX = displayTransform.localRotation.x;
//			float localRotY = displayTransform.localRotation.y;
//			float localRotZ = displayTransform.localRotation.z;

			if (textPosition != null)
			{
				textPosition.text = "Pos X: " + worldPosX.ToString("0.0") + " Y: " + worldPosY.ToString("0.0") + " Z: " + worldPosZ.ToString("0.0");
			}
			if (textRotation != null)
			{
				textRotation.text = "Rot X: " + worldRotX.ToString("0.0") + " Y: " + worldRotY.ToString("0.0") + " Z: " + worldRotZ.ToString("0.0");
			}
		}
	}

    void Update()
    {
//        if(OVRInput.GetDown(OVRInput.Button.Two))
//        {
//            if (inMenu == 0)
//            {
//	            inMenu = 1;
//	            int[] panels = {DebugUIBuilderHand.PLAY_PANE_CENTER, DebugUIBuilderHand.PLAY_PANE_RIGHT};
//            	DebugUIBuilderHand.instance.ShowActivePanels(panels);
//            }
//            else if (inMenu == 1)
//            {
//	            inMenu = 2;
//	            int[] panels = {DebugUIBuilderHand.DEBUG_PANE_CENTER, DebugUIBuilderHand.DEBUG_PANE_RIGHT};
//            	DebugUIBuilderHand.instance.ShowActivePanels(panels);
//            }
//            else
//            {
//	            inMenu = 0;
//            	DebugUIBuilderHand.instance.Hide();
//            }
//        }
        if(inMenu > 0)
        {
            updateFPS(m_textFPS);
            updateTransform(displayTransform, m_textPosition, m_textRotation);
        }
    }
}
