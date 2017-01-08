﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Repository of commonly used prefabs.
/// </summary>
[AddComponentMenu("Gameplay/ObjectPool")]
public class ObjectPool : MonoBehaviour
{

    public static ObjectPool instance { get; private set; }

    #region member
    /// <summary>
    /// Member class for a prefab entered into the object pool
    /// </summary>
    [Serializable]
    public class ObjectPoolEntry
    {
        /// <summary>
        /// The race this model represents
        /// </summary>
        [SerializeField]
        public EQBrowser.Race Race;

        /// <summary>
        /// the object to pre instantiate - they should be arranged in gender. index 0 = male, 1 = female, 2 = it/genderless
        /// </summary>
        [SerializeField]
        public GameObject[] Prefab;

        /// <summary>
        /// quantity of object to pre-instantiate
        /// </summary>
        [SerializeField]
        public int Count;
    }
    #endregion

    /// <summary>
    /// The object prefabs which the pool can handle
    /// by The amount of objects of each type to buffer.
    /// </summary>
    public ObjectPoolEntry[] Entries;

    public Dictionary<EQBrowser.Race, ObjectPoolEntry> _pool;

    /// <summary>
    /// The pooled objects currently available.
    /// Indexed by the index of the objectPrefabs
    /// </summary>
    [HideInInspector]
    public List<GameObject>[] Pool;

    public List<GameObject> spawnlist;

    System.Diagnostics.Stopwatch _startTimer;

    /// <summary>
    /// The container object that we will keep unused pooled objects so we dont clog up the editor with objects.
    /// </summary>
    protected GameObject ContainerObject;
    void Awake()
    {
        _startTimer = new System.Diagnostics.Stopwatch();
        _startTimer.Start();

        DontDestroyOnLoad(transform.gameObject);
    }

    void OnEnable()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        ContainerObject = new GameObject();
        DontDestroyOnLoad(ContainerObject);

        //Loop through the object prefabs and make a new list for each one.
        //We do this because the pool can only support prefabs set to it in the editor,
        //so we can assume the lists of pooled objects are in the same order as object prefabs in the array
        Pool = new List<GameObject>[Entries.Length];

        for (int i = 0; i < Entries.Length; i++)
        {
            var objectPrefab = Entries[i];

            //create the repository
            Pool[i] = new List<GameObject>();

            //fill it
            for (int n = 0; n < objectPrefab.Count; n++)
            {
                    var newObj = Instantiate(objectPrefab.Prefab) as GameObject;
                    DontDestroyOnLoad(newObj);
                    newObj.name = objectPrefab.Prefab.name;
                    PoolObject(newObj);
            }
        }

        _startTimer.Stop();
        Debug.LogWarningFormat("ObjectPool Time: {0}s", _startTimer.ElapsedMilliseconds * 0.001f);
        _startTimer = null;
    }



    /// <summary>
    /// Gets a new object for the name type provided.  If no object type exists or if onlypooled is true and there is no objects of that type in the pool
    /// then null will be returned.
    /// </summary>
    /// <returns>
    /// The object for type.
    /// </returns>
    /// <param name='objectType'>
    /// Object type.
    /// </param>
    /// <param name='onlyPooled'>
    /// If true, it will only return an object if there is one currently pooled.
    /// </param>
    public GameObject GetObjectForType(string objectType, bool onlyPooled, float x, float y, float z, int spawnId, EQBrowser.Race race, string name, float heading, int deity, float size, byte NPC, byte curHp, byte maxHp, byte level, EQBrowser.Gender gender)
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            var prefab = Entries[i].Prefab;

            if (prefab.name != objectType)
                continue;

            if (Pool[i].Count > 0)
            {
                GameObject pooledObject = Pool[i][0];
                Pool[i].RemoveAt(0);
                pooledObject.transform.parent = null;
                pooledObject.SetActiveRecursively(true);
                Vector3 pos = new Vector3(x, y, z);
                pooledObject.transform.position = pos;
                //heading
                float h = Mathf.Lerp(360, 0, heading / 255f);
                //					pooledObject.transform.eulerAngles.y = h;
                pooledObject.transform.localEulerAngles = new Vector3(0, h, 0);
                if (NPC == 0) { size = 1.4f; pooledObject.transform.localScale = new Vector3(size, size, size); }
                if (NPC == 1) { pooledObject.transform.localScale = new Vector3((size / 4f), (size / 4f), (size / 4f)); }
                pooledObject.name = spawnId.ToString();

                NPCController controller = pooledObject.GetComponent<NPCController>();

                controller.RaceID = race;
                controller.spawnId = spawnId;
                controller.name = name;// Player's Name
                controller.prefabName = prefab.name;
                controller.x = x;// x coord
                controller.y = y;// y coord
                controller.z = z;// z coord
                controller.heading = heading;// heading
                controller.deity = deity;// Player's Deity
                controller.size = size;// Model size
                controller.NPC = (NPCController.NPCType)NPC;// 0=player,1=npc,2=pc corpse,3=npc corpse,a
                controller.curHp = curHp;// Current hp %%% wrong
                controller.maxHp = maxHp;// Current hp %%% wrong
                controller.level = level;// Spawn Level
                controller.gender = gender;// Gender (0=male, 1=female)
                spawnlist.Add(pooledObject);

                return pooledObject;
            }
        }

        //If we have gotten here either there was no object of the specified type or non were left in the pool with onlyPooled set to true
        return null;
    }

    public GameObject GetObjectForType(bool onlyPooled, float x, float y, float z, int spawnId, EQBrowser.Race race, string name, float heading, int deity, float size, byte NPC, byte curHp, byte maxHp, byte level, EQBrowser.Gender gender)
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            var prefab = Entries[i].Prefab;

            if (prefab.name != objectType)
                continue;

            if (Pool[i].Count > 0)
            {
                GameObject pooledObject = Pool[i][0];
                Pool[i].RemoveAt(0);
                pooledObject.transform.parent = null;
                pooledObject.SetActiveRecursively(true);
                Vector3 pos = new Vector3(x, y, z);
                pooledObject.transform.position = pos;
                //heading
                float h = Mathf.Lerp(360, 0, heading / 255f);
                //					pooledObject.transform.eulerAngles.y = h;
                pooledObject.transform.localEulerAngles = new Vector3(0, h, 0);
                if (NPC == 0) { size = 1.4f; pooledObject.transform.localScale = new Vector3(size, size, size); }
                if (NPC == 1) { pooledObject.transform.localScale = new Vector3((size / 4f), (size / 4f), (size / 4f)); }
                pooledObject.name = spawnId.ToString();

                NPCController controller = pooledObject.GetComponent<NPCController>();

                controller.RaceID = race;
                controller.spawnId = spawnId;
                controller.name = name;// Player's Name
                controller.prefabName = prefab.name;
                controller.x = x;// x coord
                controller.y = y;// y coord
                controller.z = z;// z coord
                controller.heading = heading;// heading
                controller.deity = deity;// Player's Deity
                controller.size = size;// Model size
                controller.NPC = (NPCController.NPCType)NPC;// 0=player,1=npc,2=pc corpse,3=npc corpse,a
                controller.curHp = curHp;// Current hp %%% wrong
                controller.maxHp = maxHp;// Current hp %%% wrong
                controller.level = level;// Spawn Level
                controller.gender = gender;// Gender (0=male, 1=female)
                spawnlist.Add(pooledObject);

                return pooledObject;
            }
        }

        //If we have gotten here either there was no object of the specified type or non were left in the pool with onlyPooled set to true
        return null;
    }

    /// <summary>
    /// Pools the object specified.  Will not be pooled if there is no prefab of that type.
    /// </summary>
    /// <param name='obj'>
    /// Object to be pooled.
    /// </param>
    public void PoolObject(GameObject obj)
    {

        for (int i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].Prefab.name != obj.name)
                continue;

            obj.SetActiveRecursively(false);

            obj.transform.parent = ContainerObject.transform;
            DontDestroyOnLoad(ContainerObject);
            Pool[i].Add(obj);

            return;
        }
    }
}


