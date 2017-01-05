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

    string selectedFileAndPath = "";
    S3DArchive loadedArchive = null;
    Vector2 artFileScroll;
    Vector2 wldFileScroll;

    private void OnGUI()
    {
        selectedFileAndPath = EditorGUILayout.TextField("Selected S3D file:", selectedFileAndPath);

        if (GUILayout.Button("Select File"))
        {
            selectedFileAndPath = EditorUtility.OpenFilePanelWithFilters("Load S3D File", "", S3DArchive.archiveFileExtEditor);
        }

        if (!string.IsNullOrEmpty(selectedFileAndPath))
        {
            if (GUILayout.Button("Load File"))
            {
                loadedArchive = S3DArchive.Load(selectedFileAndPath);
            }
        }

        GUILayout.Space(5);

        if (loadedArchive != null)
        {
            GUILayout.Label("Loaded file " + loadedArchive.FileName + " Art files: " + loadedArchive.ArtFiles.Count + " WLD files: " + loadedArchive.WLDFiles.Count);

            GUILayout.Label("Files present:");

            artFileScroll = GUILayout.BeginScrollView(artFileScroll);

            foreach (KeyValuePair<string, S3DArchive.S3DArchiveEntry> entry in loadedArchive.ArtFiles)
            {
                GUILayout.Label(entry.Key);

                if (entry.Value.GetImage() != null)
                {
                    GUILayout.Label(entry.Value.GetImage());
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(2);

            wldFileScroll = GUILayout.BeginScrollView(wldFileScroll);

            foreach (KeyValuePair<string, S3DArchive.S3DArchiveEntry> entry in loadedArchive.WLDFiles)
            {
                GUILayout.Label(entry.Key);

                if (entry.Value.GetImage() != null)
                {
                    GUILayout.Label(entry.Value.GetImage());
                }
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Clear"))
            {
                loadedArchive = null;
            }

        }

    }
}