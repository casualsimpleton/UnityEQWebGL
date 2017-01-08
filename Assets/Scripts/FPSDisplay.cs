using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EQBrowser; 

public class FPSDisplay : MonoBehaviour
{
    int CurFPS = 0;
	float msec = 0.0f;
    float packetTimer = 0f;

    void Start()
    {
        StartCoroutine(FPSCoRoutine(0.5f));
    }

    private IEnumerator FPSCoRoutine(float frequency)
    {
        for (; ; )
        {
            // Capture frame-per-second
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(frequency);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;

            // Display it
            CurFPS = Mathf.RoundToInt(frameCount / timeSpan);
        }
    }

	void Update()
	{
		//deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        packetTimer += Time.deltaTime;

        //Been a second yet?
        if (packetTimer > 1f)
        {
            WorldConnect.PacketsLatestSecond = 0;
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
        string text = string.Format("{0:0} ms ({1:0.} fps)\np/s: {2} tot. p: {3}\nunhand. p: {4}", WorldConnect.PingTime, CurFPS, WorldConnect.PacketsLatestSecond, WorldConnect.PacketsTotal, WorldConnect.PacketsUnhandledType);
		GUI.Label(rect, text, style);
	}
}