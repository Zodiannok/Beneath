using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public int RoomSeed;
    public int RoomLevel;

    // Has this room been found by the player?
    public bool RoomDiscovered
    {
        get { return _RoomDiscovered; }
    }
    private bool _RoomDiscovered = false;

    // Has the detail of the room been spawned? (If so a new component will be created for details.)
    public bool RoomDetailSpawned
    {
        get { return _RoomDetailSpawned; }
    }
    private bool _RoomDetailSpawned = false;

    // Has the room been cleared? (If so a new component will be created for details.)
    public bool RoomCleared
    {
        get { return _RoomCleared; }
    }
    private bool _RoomCleared = false;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // Discover this room.
    public void Discover()
    {
        if (!_RoomDiscovered)
        {
            _RoomDiscovered = true;
        }
    }

    // Spawn room details. (Including dungeon encounters and status.)
    public void SpawnDetails()
    {
        if (!_RoomDetailSpawned)
        {
            _RoomDetailSpawned = true;

            // Create room detail component.
        }
    }

    // Clear room.
    public void ClearRoom()
    {
        if (!_RoomCleared)
        {
            if (_RoomDetailSpawned)
            {
                // Delete room detail component.
            }

            _RoomCleared = true;

            // Create cleared room detail component.
        }
    }
}
