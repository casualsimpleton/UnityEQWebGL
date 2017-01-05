#if UNITY_EDITOR
//Class to contain a bunch of positions since we're trying to figure out a way to get our ping without explicitly asking for a ping via the network
//So we're going to keep track of the out going positions we send to the server, and then listen for it to come back and figure out the round trip time.
//Then we can tell the approximate ping time to the server and we can now use that for network latency issues
//CasualSimpleton

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using EQBrowser;

namespace EQBrowser
{
    public static class DebugPositionTracker
    {
        public struct PositionData
        {
            public short _entityID;
            public float _posX;
            public float _posY;
            public float _posZ;

            public float _deltaX;
            public float _deltaY;
            public float _deltaZ;
            public float _deltaHeading;

            public int _animationSpeed;

            public float _rotation;

            public float _timeSent;
        }

        static List<PositionData> _cachedPositions = new List<PositionData>(20);
        static float _lastCheckedTimer;
        static bool _lookingForUpdate = false;

        public static bool IsLookingForUpdate()
        {
            if (Time.time > _lastCheckedTimer && _cachedPositions.Count < 1000)
            {
                return true;
            }

            return false;
        }

        public static void AddData(short entityID, float posX, float posY, float posZ, float deltaX, float deltaY, float deltaZ,
            float deltaHeading, int animationSpeed, float rotation)
        {
            //Don't keep caching them if it seems like its never ending
            if (_cachedPositions.Count > 1000)
            {
                return;
            }

            PositionData newData = new PositionData();
            newData._entityID = entityID;
            newData._posX = posX;
            newData._posY = posY;
            newData._posZ = posZ;
            newData._deltaX = deltaX;
            newData._deltaY = deltaY;
            newData._deltaZ = deltaZ;
            newData._deltaHeading = deltaHeading;
            newData._animationSpeed = animationSpeed;
            newData._rotation = rotation;
            newData._timeSent = Time.time;

            _cachedPositions.Add(newData);
        }

        public static void CheckForMatches(short entityID, float posX, float posY, float posZ, float deltaX, float deltaY, float deltaZ,
            float deltaHeading, int animationSpeed, float rotation)
        {
            if (_cachedPositions.Count > 1000)
            {
                _cachedPositions.Clear();
            }

            //Not looking for an update at the moment
            if (!_lookingForUpdate)
                return;

            bool foundMatch = false;
            for (int i = 0; i < _cachedPositions.Count; i++)
            {
                PositionData testPos = _cachedPositions[i];

                if (entityID != testPos._entityID)
                    continue;

                if(posX != testPos._posX || posY != testPos._posY || posZ != testPos._posZ)
                    continue;

                if(deltaX != testPos._deltaX || deltaY != testPos._deltaY || deltaZ != testPos._deltaZ)
                    continue;

                if (rotation != testPos._rotation)
                    continue;

                foundMatch = true;

                Debug.LogFormat("Pos Match. Index: {0}. TimeDelta: {1}", i, (Time.time - testPos._timeSent));
            }

            if (foundMatch)
            {
                _lastCheckedTimer = Time.time + 2f;
                _cachedPositions.Clear();
            }
        }
    }
}
#endif