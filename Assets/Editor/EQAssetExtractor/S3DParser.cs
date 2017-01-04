//An attempt to implement a S3D parser inside Unity. I suck at decyphering data files, so this is heavily based on EQ-Zip's work
//CasualSimpleton
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using EQBrowser;

public class S3DParser : EditorWindow
{
    [MenuItem("EQ Assets/S3D Parser")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(S3DParser));
    }

    private void OnGUI()
    {
    }
}