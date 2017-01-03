using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EQBrowser; 

public class FPSDisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
	float msec = 0.0f;
    float packetTimer = 0f;

	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        packetTimer += Time.deltaTime;

        //Been a second yet?
        if (packetTimer > 1f)
        {
            WorldConnect.packetsLatestSecond = 0;
            packetTimer = 0;
        }
	}
 
	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
 
		GUIStyle style = new GUIStyle();
 
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (1.0f, 0.92f, 0.016f, 1.0f);
		float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)\np/s: {2} tot. p: {3}\nunhand. p: {4}", msec, fps, WorldConnect.packetsLatestSecond, WorldConnect.packetsTotal, WorldConnect.packetsUnhandledType);
		GUI.Label(rect, text, style);
	}
}