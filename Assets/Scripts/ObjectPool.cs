using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Repository of commonly used prefabs.
/// </summary>
[AddComponentMenu("Gameplay/ObjectPool")]
public class ObjectPool : MonoBehaviour
{
    #region OLD
    //public static ObjectPool instance { get; private set; }

    //#region member
    ///// <summary>
    ///// Member class for a prefab entered into the object pool
    ///// </summary>
    //[Serializable]
    //public class ObjectPoolEntry
    //{
    //    /// <summary>
    //    /// The race this model represents
    //    /// </summary>
    //    [SerializeField]
    //    public EQBrowser.Race Race;

    //    /// <summary>
    //    /// the object to pre instantiate - they should be arranged in gender. index 0 = male, 1 = female, 2 = it/genderless
    //    /// </summary>
    //    [SerializeField]
    //    public GameObject Prefab;

    //    /// <summary>
    //    /// quantity of object to pre-instantiate
    //    /// </summary>
    //    [SerializeField]
    //    public int Count;
    //}
    //#endregion

    ///// <summary>
    ///// The object prefabs which the pool can handle
    ///// by The amount of objects of each type to buffer.
    ///// </summary>
    //public ObjectPoolEntry[] Entries;
    
    ///// <summary>
    ///// The pooled objects currently available.
    ///// Indexed by the index of the objectPrefabs
    ///// </summary>
    //[HideInInspector]
    //public List<GameObject>[] Pool;

    //public List<GameObject> spawnlist;

    //System.Diagnostics.Stopwatch _startTimer;

    /// <summary>
    /// The container object that we will keep unused pooled objects so we dont clog up the editor with objects.
    /// </summary>
    //protected GameObject ContainerObject;
    
    //void Awake()
    //{
    //    _startTimer = new System.Diagnostics.Stopwatch();
    //    _startTimer.Start();

    //    DontDestroyOnLoad(transform.gameObject);
    //}

    //void OnEnable()
    //{
    //    instance = this;
    //}

    //// Use this for initialization
    //void Start()
    //{
    //    ContainerObject = new GameObject();
    //    DontDestroyOnLoad(ContainerObject);

    //    //Loop through the object prefabs and make a new list for each one.
    //    //We do this because the pool can only support prefabs set to it in the editor,
    //    //so we can assume the lists of pooled objects are in the same order as object prefabs in the array
    //    Pool = new List<GameObject>[Entries.Length];

    //    for (int i = 0; i < Entries.Length; i++)
    //    {
    //        var objectPrefab = Entries[i];

    //        //create the repository
    //        Pool[i] = new List<GameObject>();

    //        //fill it
    //        for (int n = 0; n < objectPrefab.Count; n++)
    //        {
    //                var newObj = Instantiate(objectPrefab.Prefab) as GameObject;
    //                DontDestroyOnLoad(newObj);
    //                newObj.name = objectPrefab.Prefab.name;
    //                PoolObject(newObj);
    //        }
    //    }

    //    _startTimer.Stop();
    //    Debug.LogWarningFormat("ObjectPool Time: {0}s", _startTimer.ElapsedMilliseconds * 0.001f);
    //    _startTimer = null;
    //}



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
    //public GameObject GetObjectForType(string objectType, bool onlyPooled, float x, float y, float z, int spawnId, EQBrowser.Race race, string name, float heading, int deity, float size, byte NPC, byte curHp, byte maxHp, byte level, EQBrowser.Gender gender)
    //{
    //    for (int i = 0; i < Entries.Length; i++)
    //    {
    //        var prefab = Entries[i].Prefab;

    //        if (prefab.name != objectType)
    //            continue;

    //        if (Pool[i].Count > 0)
    //        {
    //            GameObject pooledObject = Pool[i][0];
    //            Pool[i].RemoveAt(0);
    //            pooledObject.transform.parent = null;
    //            pooledObject.SetActiveRecursively(true);
    //            Vector3 pos = new Vector3(x, y, z);
    //            pooledObject.transform.position = pos;
    //            //heading
    //            float h = Mathf.Lerp(360, 0, heading / 255f);
    //            //					pooledObject.transform.eulerAngles.y = h;
    //            pooledObject.transform.localEulerAngles = new Vector3(0, h, 0);
    //            if (NPC == 0) { size = 1.4f; pooledObject.transform.localScale = new Vector3(size, size, size); }
    //            if (NPC == 1) { pooledObject.transform.localScale = new Vector3((size / 4f), (size / 4f), (size / 4f)); }
    //            pooledObject.name = spawnId.ToString();

    //            NPCController controller = pooledObject.GetComponent<NPCController>();

    //            controller.RaceID = race;
    //            controller.spawnId = spawnId;
    //            controller.name = name;// Player's Name
    //            controller.prefabName = prefab.name;
    //            controller.x = x;// x coord
    //            controller.y = y;// y coord
    //            controller.z = z;// z coord
    //            controller.heading = heading;// heading
    //            controller.deity = deity;// Player's Deity
    //            controller.size = size;// Model size
    //            controller.NPC = (NPCController.NPCType)NPC;// 0=player,1=npc,2=pc corpse,3=npc corpse,a
    //            controller.curHp = curHp;// Current hp %%% wrong
    //            controller.maxHp = maxHp;// Current hp %%% wrong
    //            controller.level = level;// Spawn Level
    //            controller.gender = gender;// Gender (0=male, 1=female)
    //            spawnlist.Add(pooledObject);

    //            return pooledObject;
    //        }
    //    }

    //    //If we have gotten here either there was no object of the specified type or non were left in the pool with onlyPooled set to true
    //    return null;
    //}

    //public GameObject GetObjectForType(string objectType, bool onlyPooled, float x, float y, float z, int spawnId, EQBrowser.Race race, string name, float heading, int deity, float size, byte NPC, byte curHp, byte maxHp, byte level, EQBrowser.Gender gender)
    //{
    //    for (int i = 0; i < Entries.Length; i++)
    //    {
    //        var prefab = Entries[i].Prefab;

    //        if (prefab.name != objectType)
    //            continue;

    //        if (Pool[i].Count > 0)
    //        {
    //            GameObject pooledObject = Pool[i][0];
    //            Pool[i].RemoveAt(0);
    //            pooledObject.transform.parent = null;
    //            pooledObject.SetActiveRecursively(true);
    //            Vector3 pos = new Vector3(x, y, z);
    //            pooledObject.transform.position = pos;
    //            //heading
    //            float h = Mathf.Lerp(360, 0, heading / 255f);
    //            //					pooledObject.transform.eulerAngles.y = h;
    //            pooledObject.transform.localEulerAngles = new Vector3(0, h, 0);
    //            if (NPC == 0) { size = 1.4f; pooledObject.transform.localScale = new Vector3(size, size, size); }
    //            if (NPC == 1) { pooledObject.transform.localScale = new Vector3((size / 4f), (size / 4f), (size / 4f)); }
    //            pooledObject.name = spawnId.ToString();

    //            NPCController controller = pooledObject.GetComponent<NPCController>();

    //            controller.RaceID = race;
    //            controller.spawnId = spawnId;
    //            controller.name = name;// Player's Name
    //            controller.prefabName = prefab.name;
    //            controller.x = x;// x coord
    //            controller.y = y;// y coord
    //            controller.z = z;// z coord
    //            controller.heading = heading;// heading
    //            controller.deity = deity;// Player's Deity
    //            controller.size = size;// Model size
    //            controller.NPC = (NPCController.NPCType)NPC;// 0=player,1=npc,2=pc corpse,3=npc corpse,a
    //            controller.curHp = curHp;// Current hp %%% wrong
    //            controller.maxHp = maxHp;// Current hp %%% wrong
    //            controller.level = level;// Spawn Level
    //            controller.gender = gender;// Gender (0=male, 1=female)
    //            spawnlist.Add(pooledObject);

    //            return pooledObject;
    //        }
    //    }

    //    //If we have gotten here either there was no object of the specified type or non were left in the pool with onlyPooled set to true
    //    return null;
    //}

    /// <summary>
    /// Pools the object specified.  Will not be pooled if there is no prefab of that type.
    /// </summary>
    /// <param name='obj'>
    /// Object to be pooled.
    /// </param>
    //public void PoolObject(GameObject obj)
    //{

    //    for (int i = 0; i < Entries.Length; i++)
    //    {
    //        if (Entries[i].Prefab.name != obj.name)
    //            continue;

    //        obj.SetActiveRecursively(false);

    //        obj.transform.parent = ContainerObject.transform;
    //        DontDestroyOnLoad(ContainerObject);
    //        Pool[i].Add(obj);

    //        return;
    //    }
    //}
    #endregion

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// We need a way to differentiate between gendered models, so while the ObjectPool has a dictionary sorted by race,
    /// this allows us a way to store per gender in a flexible and fast to access format
    /// </summary>
    [Serializable]
    public class GenderedStorage
    {
        //Using a list here since it's generally pretty fast if adding/removing from the end. The list shouldn't be changing must
        //at runtime outside of the instantiation periods.
        //http://stackoverflow.com/questions/13211277/performance-differences-so-dramatic
        //Could still be looked at for performance though specifically to the game, claims of actual testing here
        //If performance becomes a giant issue, might want to store by gender-as-int instead of enum since there's some GC involved
        protected Dictionary<EQBrowser.Gender, List<GameObject>> _storage;

        public GenderedStorage()
        {
            _storage = new Dictionary<EQBrowser.Gender, List<GameObject>>();
        }

        /// <summary>
        /// Adds an entry based on gender, should already be checked for race
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="newGO"></param>
        public void AddStorage(EQBrowser.Gender gender, GameObject newGO)
        {
            List<GameObject> GOs = null;
            bool present = _storage.TryGetValue(gender, out GOs);

            if (present)
            {
                GOs.Add(newGO);
            }
            else
            {
                GOs = new List<GameObject>();
                GOs.Add(newGO);
                _storage.Add(gender, GOs);
            }
        }

        /// <summary>
        /// Get an entry, returns true if one available, and false if not available
        /// </summary>
        /// <param name="gender"></param>
        /// <param name="go"></param>
        /// <returns></returns>
        public bool GetGOFromStorage(EQBrowser.Gender gender, out GameObject GO)
        {
            List<GameObject> GOs;
            bool present = _storage.TryGetValue(gender, out GOs);

            //None of that gender is present
            if(!present)
            {
                GO = null;
                return false;
            }
            else
            {
                //No presently instantiated GOs are available
                if(GOs.Count < 1)
                {
                    GO = null;
                    return false;
                }

                //Remove from the rear in the list otherwise we'll have to update the whole list internally
                int count = GOs.Count;
                GO = GOs[count - 1];
                GOs.RemoveAt(count - 1);

                return true;
            }
        }

        public void Cleanup()
        {
            //Foreachs are a bit heavy on the GC, so be wary when to do this
            foreach (KeyValuePair<EQBrowser.Gender, List<GameObject>> kvp in _storage)
            {
                kvp.Value.Clear();
            }

            _storage.Clear();
        }
    }

    public static ObjectPool Instance { get; private set; }

    /// <summary>
    /// Timer for keeping track of how long it takes to load (so we don't have to always use the profiler)
    /// </summary>
    protected System.Diagnostics.Stopwatch _startTimer;

    /// <summary>
    /// The container object that we will keep unused pooled objects so we dont clog up the editor with objects.
    /// </summary>
    protected GameObject ContainerObject;

    protected Dictionary<int, NPCController> _spawnList;

    //TODO - if performance becomes a big issue, may want to use race-as-int instead of enum - CasualSimpleton
    protected Dictionary<EQBrowser.Race, GenderedStorage> _pool;

    void Awake()
    {
        Instance = this;

        _startTimer = new System.Diagnostics.Stopwatch();
        //_startTimer.Start();

        DontDestroyOnLoad(transform.gameObject);
    }

    void OnEnable()
    {
        Instance = this;
    }

    void Start()
    {
        BeginInit();
    }

    void ClearPool()
    {
        //TODO - This could be optimized where if we need some things again in the new zone, to not clear it

        //Foreachs are guarenteed to hit the GC, so becareful when to do this
        foreach (KeyValuePair<EQBrowser.Race, GenderedStorage> kvp in _pool)
        {
            kvp.Value.Cleanup();
        }

        _pool.Clear();

        //Trigger a GC collection since we're transitioning
        System.GC.Collect();
    }

    void BeginInit()
    {
        _startTimer.Reset();
        _startTimer.Start();
        if (ContainerObject == null)
        {
            ContainerObject = new GameObject();
            ContainerObject.name = "ObjectPoolContainer";
        }

        DontDestroyOnLoad(ContainerObject);

        if (_pool == null)
        {
            _pool = new Dictionary<EQBrowser.Race, GenderedStorage>();
        }

        if (_spawnList == null)
        {
            _spawnList = new Dictionary<int, NPCController>();
        }

        //Let's clear the existing one and then trigger a GC at the end since we're zoning
        ClearPool();

        //TODO - We should probably pull data from a zone/global listing so we don't instantiate everything everytime. We just need pooled GOs for mobs that appear in the current zone
        int count = PrefabListing.Instance.Prefabs.Length;

        for (int i = 0; i < count; i++)
        {
            PrefabListing.PrefabListEntry entry = PrefabListing.Instance.Prefabs[i];

            for (int j = 0; j < entry.Prefab.Length; j++)
            {
                //No entry for that gender, skip it
                if (entry.Prefab[j] == null)
                {
                    continue;
                }

                for (int k = 0; k < entry.Count; k++)
                {
                    GameObject newGO = Instantiate(entry.Prefab[j]) as GameObject;
                    DontDestroyOnLoad(newGO);
                    newGO.name = entry.Prefab[j].name;
                    AddToPool((EQBrowser.Race)i, (EQBrowser.Gender)j, newGO);
                }
            }
        }
        
        _startTimer.Stop();
        Debug.LogWarningFormat("ObjectPool Time: {0}s", _startTimer.ElapsedMilliseconds * 0.001f);
        _startTimer = null;
    }

    public void AddToPool(EQBrowser.Race race, EQBrowser.Gender gender, GameObject GO)
    {
        GO.SetActive(false);
        GO.transform.parent = ContainerObject.transform;

        GenderedStorage storage = null;
        bool present = _pool.TryGetValue(race, out storage);

        if (present)
        {
            storage.AddStorage(gender, GO);
        }
        else
        {
            storage = new GenderedStorage();
            storage.AddStorage(gender, GO);

            _pool.Add(race, storage);
        }
    }

    /// <summary>
    /// Gets a new object based on race and gender. Will instantiate a new one if none is available.
    /// </summary>
    /// <returns>A suitable GameObject for that race and gender</returns>
    public GameObject GetObjectByRaceAndGender(EQBrowser.Race race, EQBrowser.Gender gender,
        float x, float y, float z,
        float deltaX, float deltaY, float deltaZ, float deltaH,
        int spawnID, string name, float heading, int deity,
        float size, NPCController.NPCType npcType,
        byte curHP, byte maxHP, byte level)
    {
        GameObject GO = null;
        GenderedStorage storage = null;
        bool present = _pool.TryGetValue(race, out storage);
        string prefabName = "(Unnamed Prefab)";

        if (present)
        {
            present = storage.GetGOFromStorage(gender, out GO);

            if (!present)
            {
                //The race is present, but none available for that gender. 
                //Before we use a default, we'll check if that prefab exists
                if (PrefabListing.Instance.Prefabs[(int)race].Prefab[(int)gender] != null)
                {
                    //We found it, so just instantiate a new one
                    GO = Instantiate(PrefabListing.Instance.Prefabs[(int)race].Prefab[(int)gender]) as GameObject;
                    DontDestroyOnLoad(GO);
                    prefabName = GO.name = PrefabListing.Instance.Prefabs[(int)race].Prefab[(int)gender].name;
                }
                else
                {
                    bool foundAtleastOne = false;
                    //We can't find any prefab of that gender, so look at all the genders of that entry and see if any are set
                    for (int i = 0; i < PrefabListing.Instance.Prefabs[(int)race].Prefab.Length; i++)
                    {
                        if (PrefabListing.Instance.Prefabs[(int)race].Prefab[i] != null)
                        {
                            try
                            {
                            GO = Instantiate(PrefabListing.Instance.Prefabs[(int)race].Prefab[i]) as GameObject;
                            DontDestroyOnLoad(GO);
                            prefabName = GO.name = PrefabListing.Instance.Prefabs[(int)race].Prefab[i].name;
                            foundAtleastOne = true;
                            }
                            catch
                            {
                                Debug.LogError("PROBLEM");
                            }

                            break;
                        }
                    }

                    //Nothing listed at all, so get a default male
                    if (!foundAtleastOne)
                    {
                        _pool.TryGetValue(EQBrowser.Race.Default, out storage);

                        storage.GetGOFromStorage(EQBrowser.Gender.Male, out GO);
                    }
                }
            }
        }
        else
        {
            //If we're here, no storage for that race is present OR there's no prefab for that race/gender combo
            //So we'll just use a default race (0) and male (0)
            _pool.TryGetValue(EQBrowser.Race.Default, out storage);

            storage.GetGOFromStorage(EQBrowser.Gender.Male, out GO);
        }

        prefabName = GO.name;
        
        SetValues(GO, prefabName, race, gender, x, y, z, deltaX, deltaY, deltaZ, deltaH, spawnID, name, heading, deity, size, npcType, curHP, maxHP, level);

        return GO;    
    }

    protected void SetValues(GameObject GO, string prefabName,
        EQBrowser.Race race, EQBrowser.Gender gender,
        float x, float y, float z,
        float deltaX, float deltaY, float deltaZ, float deltaH,
        int spawnID, string name, float heading, int deity,
        float size, NPCController.NPCType npcType,
        byte curHP, byte maxHP, byte level)
    {
        GO.transform.parent = null;
        GO.SetActive(true);

        Vector3 pos = new Vector3(x, y, z);
        GO.transform.position = pos;

        float h = Mathf.Lerp(360f, 0, heading / 255f);

        GO.transform.localEulerAngles = new Vector3(0, h, 0);

        if (npcType == NPCController.NPCType.Player)
        {
            size = 1.4f;
            GO.transform.localScale = new Vector3(size, size, size);
        }
        else if (npcType == NPCController.NPCType.NPC)
        {
            GO.transform.localScale = new Vector3(size / 4f, size / 4f, size / 4f);
        }

        GO.name = spawnID.ToString();

        NPCController controller = GO.GetComponent<NPCController>();

        controller.RaceID = race;
        controller.spawnId = spawnID;
        controller.name = name;// Player's Name
        controller.prefabName = prefabName; //Don't think this is really needed - CasualSimpleton
        controller.deity = deity;// Player's Deity
        controller.size = size;// Model size
        controller.NPC = npcType;// 0=player,1=npc,2=pc corpse,3=npc corpse,a
        controller.curHp = curHP;// Current hp %%% wrong
        controller.maxHp = maxHP;// Current hp %%% wrong
        controller.level = level;// Spawn Level
        controller.gender = gender;// Gender (0=male, 1=female)

        //Set this first otherwise they won't appear in the correct place until they start pathing
        controller.SetMoveXYZ(x, y, z);
        controller.SetXYZRotDeltaXYZR(x, y, z, h, 0, deltaX, deltaY, deltaZ, deltaH);
        
        _spawnList.Add(spawnID, controller);
    }

    public bool GetSpawn(int spawnID, out NPCController controller)
    {
        bool present = _spawnList.TryGetValue(spawnID, out controller);

        return present;
    }

    /// <summary>
    /// Returns a single NPCController to the pool. Useful for things like deleting a single mob
    /// </summary>
    /// <param name="npcController"></param>
    public void ReturnSpawnToPool(NPCController npcController)
    {
        //TODO - If we're geting defaults returned in the place of other race/gender combos, that could be a problem to pay attention to
        _spawnList.Remove(npcController.spawnId);
        AddToPool(npcController.RaceID, npcController.gender, npcController.gameObject);
    }

    /// <summary>
    /// Returns ALL currently spawned NPCControllers back into the pool for later reuse
    /// </summary>
    public void ReturnSpawnsToPool()
    {
        //Spawn list is empty, skip the rest
        if (_spawnList.Count < 1)
            return;

        List<int> keys = new List<int>(_spawnList.Keys);

        int count = keys.Count;
        for (int i = 0; i < count; i++)
        {
            NPCController npcC = _spawnList[keys[i]];

            npcC.updateHeading = false;
            npcC.name = "";
            npcC.targetName = "";
            npcC.updateDeltas = false;
            npcC.clientUpdate = false;
            npcC.isTarget = false;
            npcC._anim_isDead = 0;
            npcC.name = npcC.prefabName;
            npcC.playerRespawn = false;
            npcC.movetoX = 0;
            npcC.movetoY = 0;
            npcC.movetoZ = 0;
            npcC.movetoH = 0;
            npcC.deltaX = 0;
            npcC.deltaY = 0;
            npcC.deltaZ = 0;
            npcC.curHp = 0;
            npcC.maxHp = 0;
            npcC.NPC = NPCController.NPCType.Player;
            npcC.animationspeed = 0;
            npcC.animationState = 0;
            npcC.deltaF = new Vector3(0, 0, 0);
            npcC.targetPosition = new Vector3(0, 0, 0);

            AddToPool(npcC.RaceID, npcC.gender, npcC.gameObject);
        }

        _spawnList.Clear();
    }
}


