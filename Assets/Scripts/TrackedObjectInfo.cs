using System.Collections;
using System.Collections.Generic;
using SocketIO;
using UnityEngine;

/**
 * Tracks the position of a Vive tracker and sends 
 * proximity events to a WAMS app. 
 * 
 * Assumes 2 physical devices with hardcoded spacial positions
 * and WAMS view indices of 0 and 1, and a 3rd device with view
 * index 2, that can be moved around and close to one of the screens.
 *
**/
public class TrackedObjectInfo : MonoBehaviour {

    float PROXIMITY_DISTANCE = 0.25f;
    int TRACKED_VIEW_INDEX   = 2;

    int wasNearDisplay = -1;
    Vector3 trackerPosition;
    List<Vector3> displayPositions;
    private SocketIOComponent socket;
    
	void Start () {
        displayPositions = new List<Vector3>();
        displayPositions.Add(new Vector3(-1.5f, 0.9f, -0.3f));
        displayPositions.Add(new Vector3(-1.55f, 0.95f, 0.28f));
        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();
	}
	
	void Update () {
        trackerPosition = this.GetComponent<Transform>().position;

        int index = 0;
        foreach(Vector3 displayPosition in displayPositions)
        {
            float distance = Vector3.Distance(trackerPosition, displayPosition);
            if (IsNearDisplay(distance))
            {
                if (wasNearDisplay != index)
                {
                    NotifyServer("deviceNearScreen", index);
                    wasNearDisplay = index;
                }

            }
            else
            {
                if (wasNearDisplay == index)
                {
                    NotifyServer("deviceFarFromScreens", -1);
                    wasNearDisplay = -1;
                }
            }
            index++;
        }
    }

    void NotifyServer(string ev, int nearIndex)
    {
        Debug.Log(ev);
        JSONObject payload = new JSONObject(JSONObject.Type.OBJECT);
        payload.AddField("deviceIndex", TRACKED_VIEW_INDEX);

        if (ev == "deviceNearScreen")
        {
            payload.AddField("nearIndex", nearIndex);
        }

        JSONObject dreport = DReport(ev, payload);

        socket.Emit("dispatch", dreport);
    }

    bool IsNearDisplay(float distance)
    {
        return distance < PROXIMITY_DISTANCE;
    }

    JSONObject DReport(string action, JSONObject payload)
    {
        JSONObject data = new JSONObject(JSONObject.Type.OBJECT);
        data.AddField("action", action);
        data.AddField("payload", payload);

        JSONObject dreport = new JSONObject(JSONObject.Type.OBJECT);
        dreport.AddField("data", data);

        return dreport;
    }
}
