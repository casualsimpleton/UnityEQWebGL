﻿using UnityEngine;
using UnityEngine.Profiling;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using System.Xml;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using EQBrowser;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCController : MonoBehaviour 
{
    public enum NPCType : byte
    {
        Player = 0,
        NPC = 1,
        PC_Corpse = 2,
        NPC_Corpse = 3
    }

	public EQBrowser.Race RaceID = Race.Human;
	public int spawnId = 0;
	public int corpseId = 0;
	public string name = "";// Player's Name
	public string prefabName;// Player's prefab Name
	public float x;// x coord
	public float y;// y coord
	public float z;// z coord
	public float heading;// heading

	public float step;
	public Vector3 targetPosition;
	public Vector3 deltaF;

	//-x,z,y 
	public float movetoX;// x coord
	public float movetoY;// y coord
	public float movetoZ;// z coord
	public float movetoH;// z coord
	//-x,z,y 
	public float deltaX;// x coord
	public float deltaY;// y coord
	public float deltaZ;// z coord
	public float deltaH;// z coord

	private float gravity = 1.0f;
	public bool isGrounded = false;

	public bool isTarget = false;
	public bool colorActivate = false;
	public int _anim_isWalk;
	public int _anim_isIdle;
	public int _anim_isDead;
	public int _anim_isPunch;
	public int _anim_isHurt;
	public bool clientUpdate;

	public int animationspeed;
	public int deity = 0;// Player's Deity
	public float size = 0;// Model size
    public NPCType NPC = 0;// 0=player,1=npc,2=pc corpse,3=npc corpse,a
	public byte curHp = 0;// Current hp %%% wrong
	public int maxHp = 0;// Current hp %%% wrong
	public byte level = 0;// Spawn Level
	public EQBrowser.Gender gender = Gender.Male;// Gender (0=male, 1=female)
	public int animationState;//animation
	public CharacterController controller;
    public Transform _transform;

	public float fixnum;
	public float ycalc;
	public float offset;
	public Renderer rend;
	
	public GameObject NameObject;
    TextMesh namePlateText;
	public bool updateHeading;
	public bool updateDeltas;
	public bool playerRespawn = false;
	public string targetName = "";

#if UNITY_EDITOR
    protected float _lastUpdateTime;
#endif

    void Awake()
    {
        _transform = this.transform;

        TerrainMask = (1 << LayerMask.NameToLayer("Terrain"));
    }
	
	void Start()
	{
        namePlateText = NameObject.GetComponent<TextMesh>();

		UpdateNamePlate();

		//define character controller
		controller = this.GetComponent<CharacterController>();
		//place NPCs via ZoneSpawns packet var
//		_transform.position = new Vector3(x, y + 5f, z);
		//define overhead name object
		rend = this.NameObject.GetComponent<Renderer>();
		rend.material.color = Color.red;
	}

    public void UpdateNamePlate()
    {
        //clean name for overhead name
        targetName = name;
        string targetClean = Regex.Replace(targetName, "[0-9\0]", "");
        targetClean = Regex.Replace(targetClean, "[_]", " ");
        //string targetName2 = Regex.Replace(targetClean, "[_]", " ");
        //string targetName3 = Regex.Replace(targetName2, "[\0]", "");
        
        //generate name above head				
        namePlateText.text = targetClean;
    }

	void Update () 
	{
		//gravity

        Profiler.BeginSample("NPC NamePlates");
        
		if(string.IsNullOrEmpty(targetName))
		{
            UpdateNamePlate();
		}

		//if player respawned, reset NPC name Vars in Start
		if(playerRespawn == true)
        {
            Start();
            playerRespawn = false;
        }
		//overhead names face player
		
        //if(Camera.main.velocity != new Vector3(0,0,0) || this.controller.velocity != new Vector3(0,0,0) || updateHeading == true)
        //{
        //    NameObject.transform.LookAt(2 * NameObject.transform.position - Camera.main.transform.position);
        //}
        if (WorldConnect.MainCamera.velocity != new Vector3(0, 0, 0) || this.controller.velocity != new Vector3(0, 0, 0) || updateHeading == true)
        {
            NameObject.transform.LookAt(2 * NameObject.transform.position - WorldConnect.MainCameraTransform.position);
        }

		//green name when targetted
		if(isTarget == true && colorActivate == false)
		{
			rend.material.color = Color.green;
			colorActivate = true;
		}
		//red name when targetted
		if(isTarget == false && colorActivate == true)
		{
			rend.material.color = Color.red;
			colorActivate = false;
		}

        Profiler.EndSample();
        Profiler.BeginSample("NPC Heading");

		//update heading from clientupdate packet
		if(updateHeading == true)
		{
			float h = Mathf.Lerp(360,0,movetoH/255f);
			transform.localEulerAngles = new Vector3(0,h,0);
			updateHeading = false;
		}

		//update deltas from clientupdate packet
		if(updateDeltas == true)
		{
			deltaF = new Vector3 (deltaX,deltaY,deltaZ);
			updateDeltas = false;
		}

        Profiler.EndSample();
		
		// TODO-performance: Only do this when the NPC hp changes (specifically it decreases)
		if(NPC == NPCType.PC_Corpse || _anim_isDead == 1)
        {
            if (_anim_isDead != 1)
            {
                AnimDeadNow();
            }
        }
		else
		{
			//wandering
			// TODO-performance: When a update arrives store whether or not the NPC is moving. Calculating magnitude involves sqrt 
			//if (deltaF.magnitude != 0)
//            {
				
//                //step = delta time x speed. The server is calculating the speed which is represented as the magnitude of vector x y z. Translate the game object by those deltas multiplied by delta time
//                if(NPC == NPCType.NPC)
//                {
//                    step = deltaF.magnitude * 10f;
//                    //sets clientupdate flag to false when an npc is autorunning, waiting for another clientupdate packet
//                    if (_transform.position.x == movetoX && _transform.position.z == movetoZ) { clientUpdate = false; }
//                    //if new update from server, move there
//                    if(clientUpdate == true)
//                    {
//                        //initial movement
//                        ycalc = _transform.position.y;
//                        targetPosition.x = movetoX;
//                        targetPosition.z = movetoZ;
//                    }
//                    //if waiting on update from server, move along the delta positions
//                    if(clientUpdate == false)
//                    {
//                        //continuing to move in between updates
//                        // TODO-performance: Store this when the pos packet arrives
//                        targetPosition.x += deltaX;
//                        targetPosition.z += deltaZ;
//                    }

//                    //move now
//                    raycastY();
//                    targetPosition.y = _transform.position.y;

////#if UNITY_EDITOR
////                    if (Selection.activeGameObject == this.gameObject)
////                    {
////                        Debug.Log(string.Format("Moving NPC ID: {0} Time: {1} CurPos: {2} TarPos: {3} Step: {4} dT: {5}",
////                            spawnId, Time.time, _transform.position, targetPosition, step, Time.deltaTime));
////                    }
////#endif

//                    //float distFromTargetSqr = Vector2.SqrMagnitude(new Vector2(_transform.position.x, _transform.position.z) - new Vector2(targetPosition.x, targetPosition.z));
                    
//                    Vector3 newPos = _transform.position + (deltaF * step * Time.deltaTime);
//                    float dotProd = Vector3.Dot(targetPosition, newPos);

//                    if (dotProd > 0)
//                    {
//                        _transform.position = newPos;
//                    }

//                    //if (distFromTargetSqr > 0.1f)
//                    {
//                        //_transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step * Time.deltaTime);
//                        //_transform.position = Vector3.SmoothDamp(_transform.position, targetPosition, ref _velocity, _dampSpeed);

//                        //_transform.position = _transform.position + (deltaF * step * Time.deltaTime);
//                    }

//                    if (deltaX == 0 && deltaY == 0 && deltaZ == 0 && movetoX != 0 && movetoY != 0 && movetoZ != 0)
//                    {
//                        if (isIdle == 0)// && distFromTargetSqr < 0.5f)
//                        {
//                            _transform.position = new Vector3(movetoX, _transform.position.y, movetoZ);
//                            idleNow();
//                        }
//                    }
//                    else
//                    {
//                        if (isWalk == 0) 
//                        { 
//                            walkNow(); 
//                        }
//                    }
////#if UNITY_EDITOR
////                    else
////                    {
////                        if (Selection.activeGameObject == this.gameObject)
////                        {
////                            Debug.Log(string.Format("Inside epsilon step: {0} deltaF: {1:0.000}", step, deltaF));
////                        }
////                    }
////#endif

//#if UNITY_EDITOR
//                    if (Selection.activeGameObject == this.gameObject)
//                    {
//                        DebugMovementHelper();
//                    }
//#endif
//                }
//                //if this a player not an npc
//                else
//                {
//#if UNITY_EDITOR
//                    if (Selection.activeGameObject == this.gameObject)
//                    {
//                        Debug.Log(string.Format("Moving PC ID: {0} Time: {1} CurPos: {2} TarPos: {3} Step: {4} dT: {5}",
//                            spawnId, Time.time, _transform.position, targetPosition, step, Time.deltaTime));
//                    }
//#endif
//                    targetPosition = new Vector3 (movetoX,movetoY,movetoZ);
//                    _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, 1);
//                }
			
//            //draw pretty lines of pathing for scene editor
//            //Debug.DrawRay (_transform.position, (_transform.position - targetPosition), Color.green);
//            //Debug.DrawRay (_transform.position, (targetPosition - _transform.position), Color.green);
//                Debug.DrawLine(_transform.position, targetPosition, Color.green);
//                Debug.DrawLine(_transform.position, new Vector3(movetoX, movetoY, movetoZ), Color.blue);

//            }

            Profiler.BeginSample("NPC Movement");
            //Calculate delta magnitude just once since we may use it in multiple areas
            #region Option1
            //float deltaMag = deltaF.magnitude;

            //if (deltaMag != 0f)
            //{
            //    //Check animation state - should be walking if it's not
            //    if (_anim_isWalk == 0)
            //    {
            //        AnimWalkNow();
            //    }

            //    //Just a guess at the speed - 10 seems to work well - since we'll be using SimpleMove() we don't want to add Time.deltaTime as it does it internally, along with gravity
            //    step = deltaMag * 10f;

            //    //If it's an NPC and not a player, we're going to path it between its current position and it's goal position
            //    if (NPC == NPCType.NPC)
            //    {
            //        //NPC is not where we want them to be, so keep moving them forward - we're only here because we already checked deltas were not 0, otherwise we want to teleport them probably
            //        //if (_transform.position.x != movetoX || _transform.position.z != movetoZ)
            //        if(!EQMath.Overlap_TwoCircles(_transform.position.x, _transform.position.z, 1f, movetoX, movetoZ, 1f))
            //        {
            //            //controller.Move(deltaF * step);
            //            bool isGrounded = controller.SimpleMove(deltaF * step);

            //            //TODO - If not grounded, probably want to show them falling? unless levitation is on
            //        }

            //    }
            //    //It's a player, we just use dead reckoning - point them in the direction they were last known, and move them forward at their last known speed
            //    else
            //    {
            //        bool isGrounded = controller.SimpleMove(deltaF * step);

            //        //TODO - If not grounded, probably want to show them falling? unless levitation is on
            //    }
            //}
            //else
            //{
            //    if (!EQMath.Overlap_TwoCircles(_transform.position.x, _transform.position.z, 1f, movetoX, movetoZ, 1f))
            //    {
            //        _transform.position = new Vector3(movetoX, movetoY, movetoZ);
            //        CheckYHeight(true);
            //    }

            //    if (_anim_isIdle == 0)
            //    {
            //        AnimIdleNow();
            //    }
            //}
            #endregion

            #region Option 2
            if (NPC == NPCType.Player)
            {
                float deltaMag = deltaF.magnitude;

                //They're moving
                if (deltaMag != 0)
                {
                    if (_anim_isWalk == 0)
                    {
                        AnimWalkNow();
                    }

                    //Just a guess at the speed - 10 seems to work well - since we'll be using SimpleMove() we don't want to add Time.deltaTime as it does it internally, along with gravity
                    step = deltaMag * 10f;

                    //This a player, we're just going to shoot them off on dead reckoning
                    bool isGrounded = controller.SimpleMove(deltaF * step);

                    //TODO - If not grounded, probably want to show them falling? unless levitation is on
                }
                else
                {
                    if (_anim_isIdle == 0)
                    {
                        CheckYHeight(true);
                        AnimIdleNow();
                    }
                }

                Debug.DrawLine(_transform.position, deltaF * Time.deltaTime, Color.blue);
            }
            else
            {
                //They aren't where they're suppose to be, so keep moving
                if (!EQMath.Overlap_TwoCircles(_transform.position.x, _transform.position.z, 1f, movetoX, movetoZ, 1f))
                {
                    if (_anim_isWalk == 0)
                    {
                        AnimWalkNow();
                    }

                    float deltaMag = deltaF.magnitude;

                    //We know they're coming to a stand still at the end, however they still aren't on their target position, so we'll use the last known delta pos
                    //to get us there
                    if (deltaMag == 0f)
                    {
                        deltaF.x = _prevDeltaX;
                        deltaF.y = _prevDeltaY;
                        deltaF.z = _prevDeltaZ;
                        deltaMag = deltaF.magnitude;
                    }

                    //Just a guess at the speed - 10 seems to work well - since we'll be using SimpleMove() we don't want to add Time.deltaTime as it does it internally, along with gravity
                    step = deltaMag * 10f;

                    bool isGrounded = controller.SimpleMove(deltaF * step);

                    Debug.DrawLine(_transform.position, _transform.position + deltaF, Color.gray);
                    Debug.DrawLine(_transform.position, new Vector3(movetoX, movetoY, movetoZ), Color.red);

                    //TODO - If not grounded, probably want to show them falling? unless levitation is on
                }
                else
                {
                    if (_anim_isIdle == 0)
                    {
                        CheckYHeight(true);
                        AnimIdleNow();
                    }

                    Debug.DrawLine(_transform.position, new Vector3(movetoX, movetoY, movetoZ), Color.green);
                }

                
            }

            
            #endregion
            Profiler.EndSample();

            //CheckYHeight(updateDeltas);

            








			//idle npc after reaching a target destination.
            //else
            //{
            //    if (deltaX == 0 && deltaY == 0 && deltaZ == 0 && movetoX != 0 && movetoY != 0 && movetoZ != 0)
            //    {
            //        if(isIdle == 0)
            //        {
            //            idleNow();
            //        }

            //        _transform.position = new Vector3(movetoX, _transform.position.y, movetoZ);
            //        raycastY();
            //    }
            //    else
            //    {
            //        raycastY();
            //        //FOR  Y ADJUSTMENTS IF UNDER OR BENEATH WORLD WHEN NOT MOVING AND NO POSITION UPDATES FROM SERVER
            //    }
            //}
		}
	}

    RaycastHit[] _yRayHits = new RaycastHit[2];
    public static int TerrainMask; 
    protected int _rayFrameCount;
    public void CheckYHeight(bool forceCheck = false)
    {
        //Do this every 2 frames to offset some of the cost, or skip if we're not moving
        //_rayFrameCount++;
        //if (_rayFrameCount < 2 || forceCheck)
        //{
        //    return;
        //}

        //_rayFrameCount = 0;

        //Use non alloc raycast to reduce GC impact.
        //We're going to offset the ray origin up a bit since there is a small chance we could have clipped through the ground a smidge
        //TOOD - Consider using half the physical size so the ray comes from the mid point (waist-ish) area
        int hitCount = Physics.RaycastNonAlloc(_transform.position + (Vector3.up * 5f), Vector3.down, _yRayHits, 10f, TerrainMask);

        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                //Don't need to check the tag, since we're assuming the walk-able terrain is on the correct layer
                //Bump them down a smidge
                //if (_yRayHits[i].distance > 3.1f)
                //{
                //    //_transform.position -= new Vector3(0f,20f * Time.deltaTime,0f);
                //    _transform.position -= new Vector3(0, _yRayHits[i].distance, 0f);
                //}

                ////Bump them up a bit... why 3?
                //if (_yRayHits[i].distance < 3)
                //{
                //    //_transform.position += new Vector3(0f,20f * Time.deltaTime,0f);
                //    _transform.position += new Vector3(0, _yRayHits[i].distance, 0f);
                //}

                _transform.position = _yRayHits[i].point;

            }
        }
        else
        {
            //We must be way up in the air or something, keep moving
            //TODO - We probably want them to fall, not move up
            _transform.position += new Vector3(0, 20f * Time.deltaTime * 2, 0f);
        }
    }
	
	public void raycastY()
	{
		RaycastHit[] hitsDown;
		hitsDown = Physics.RaycastAll(_transform.position, Vector3.down, 200.0F);
		for (int i = 0; i < hitsDown.Length; i++) 
		{
			RaycastHit hitDown = hitsDown[i];
//			Debug.Log("HITS" + hitsDown.Length);
			if(hitsDown.Length > 1)
			{
				if(hitDown.distance > 3.1f && hitDown.collider.tag == "Terrain")
                {
                    _transform.position -= new Vector3(0f,20f * Time.deltaTime,0f);
                }
				
                if(hitDown.distance < 3f && hitDown.collider.tag == "Terrain")
                {
                    _transform.position += new Vector3(0f,20f * Time.deltaTime,0f);
                }
			}
			else
			{
				_transform.position += new Vector3(0f,20f * Time.deltaTime,0f);
			}
		}
		if(hitsDown.Length == 0)
        {
            _transform.position += new Vector3(0f,20f * Time.deltaTime,0f);
        }
	}
	
	public void AnimWalkNow()
	{
		_anim_isWalk = 1;
		_anim_isIdle = 0;
		_anim_isPunch = 0;
		_anim_isDead = 0;
		_anim_isHurt = 0;
		GetComponent<Animator>().Play("Walk");
	}

	public void AnimIdleNow()
	{
		_anim_isWalk = 0;
		_anim_isIdle = 1;
		_anim_isPunch = 0;
		_anim_isDead = 0;
		_anim_isHurt = 0;
		GetComponent<Animator>().Play("Idle");
	}

	public void AnimPunchNow()
	{
		_anim_isWalk = 0;
		_anim_isIdle = 0;
		_anim_isPunch = 1;
		_anim_isDead = 0;
		_anim_isHurt = 0;
		GetComponent<Animator>().Play("Punch");
	}

	public void AnimHurtNow()
	{
		_anim_isWalk = 0;
		_anim_isIdle = 0;
		_anim_isPunch = 0;
		_anim_isDead = 0;
		_anim_isHurt = 1;
		GetComponent<Animator>().Play("Hurt");
	}

	public void AnimDeadNow()
	{
		_anim_isWalk = 0;
		_anim_isIdle = 0;
		_anim_isPunch = 0;
		_anim_isDead = 1;
		_anim_isHurt = 0;
		GetComponent<Animator>().Play("Dead");
        name = namePlateText.text + "'s corpse";
	}

    public void SetMoveXYZ(float X, float Y, float Z)
    {
        movetoX = X;
        movetoY = Y;
        movetoZ = Z;
    }

    public void SetHeading(float H)
    {
        movetoH = H;
        updateHeading = true;
    }

    public void SetAnimSpeed(int speed)
    {
        animationspeed = speed;
    }

    public void SetDeltaXYZ(float dX, float dY, float dZ)
    {
        deltaX = dX;
        deltaY = dY;
        deltaZ = dZ;
        updateDeltas = true;
    }

    public void SetDeltaHeading(float dH)
    {
        deltaH = dH;
    }

#if UNITY_EDITOR
    private Vector3 _curPos = new Vector3(0,0,0);
    private Vector3 _prevMovePos = new Vector3(0, 0, 0);
    private Vector3 _newMovePos = new Vector3(0, 0, 0);

    
#endif

    float _prevMoveX;
    float _prevMoveY;
    float _prevMoveZ;
    float _prevMoveH;
    float _prevDeltaX;
    float _prevDeltaY;
    float _prevDeltaZ;

    public void SetXYZRotDeltaXYZR(float newX, float newY, float newZ, float newH, int animSpeed, float dX, float dY, float dZ, float dH)
    {
#if UNITY_EDITOR
        //If we've got the entity selected in the scene, we'll print detailed shit here to log for easier tracking via console - Casual
        //Printing name last, since for some reason it truncates the rest of the message, too lazy to figure out why
        if (Selection.activeGameObject == this.gameObject)
        {
            Debug.Log(string.Format("ID: {0} Time: {1} curX: {21} curY: {22} curZ: {23} mtX: {2} mtY: {3} mtZ: {4} mtH: {5} animSpeed: {6} dX: {7} dY: {8} dZ: {9} dH: {10}\n In X: {11} Y: {12} Z: {13} H: {14} animSpeed: {15} DX: {16} DY: {17} DZ: {18} DH: {19}\nName: {20}",
                spawnId, Time.time, movetoX, movetoY, movetoZ, movetoH, animationspeed, deltaX, deltaY, deltaZ, deltaH,
                newX, newY, newZ, newH, animSpeed, dX, dY, dZ, dH, name, _transform.position.x, _transform.position.y, _transform.position.z));

            DebugMovementHelper();
        }
#endif

        _prevMoveX = movetoX;
        _prevMoveY = movetoY;
        _prevMoveZ = movetoZ;
        _prevMoveH = movetoH;

        movetoX = newX;
        movetoY = newY;
        movetoZ = newZ;

        _lastUpdateTime = Time.time;
        
        if (movetoH != newH)
        {
            movetoH = newH;
            updateHeading = true;
        }

        animationspeed = animSpeed;

        if (deltaX != dX || deltaY != dY || deltaZ != dZ)
        {
            _prevDeltaX = deltaX;
            _prevDeltaY = deltaY;
            _prevDeltaZ = deltaZ;

            deltaX = dX;
            deltaY = dY;
            deltaZ = dZ;
            updateDeltas = true;
        }

        Vector3 deltaPos = new Vector3(deltaX, deltaY, deltaZ);
        //Move us to where the server says we started
        //We're going to assume there's some latency and shift them forward that much
        _transform.position = (new Vector3(_prevMoveX, _prevMoveY, _prevMoveZ) + (deltaPos * 10f * (Time.fixedDeltaTime * 20)));
        CheckYHeight(true);

        deltaH = dH;
        clientUpdate = true;
    }

#if UNITY_EDITOR
    void DebugMovementHelper()
    {
        _curPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        _prevMovePos = new Vector3(_prevMoveX, _prevMoveY, _prevMoveZ);
        _newMovePos = new Vector3(movetoX, movetoY, movetoZ);
        
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_curPos, 1f);
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(_prevMovePos, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_newMovePos, 1f);
    }
#endif
}