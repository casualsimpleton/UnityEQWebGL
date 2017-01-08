//PrefabListing
//Contains entries for all prefabs that need to be instantiated at runtime. Suggested it goes on a separate gameobject for better flexibility
//CasualSimpleton

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using EQBrowser;

public class PrefabListing : MonoBehaviour
{
    [Serializable]
    public class PrefabListEntry
    {
        /// <summary>
        /// What race this represents
        /// </summary>
        [SerializeField]
        public EQBrowser.Race Race;

        /// <summary>
        /// Object to instantiate when needed - Arrange in order of gender. Index 0 = male, Index 1 = female, Index 2 = It/Genderless
        /// </summary>
        [SerializeField]
        public GameObject[] Prefab;

        /// <summary>
        /// Number of instantiate and keep in the pool
        /// TODO - Probably should tie this to some kind of zone specific listings. No gnolls in Gfay, and no snakes in Temple of Veeshan
        /// </summary>
        [SerializeField]
        public int Count;
    }

    public PrefabListEntry[] Prefabs;

    public static PrefabListing Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(transform.gameObject);
    }
}