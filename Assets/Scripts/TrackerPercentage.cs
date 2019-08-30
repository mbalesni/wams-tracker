using System.Collections;
using System.Collections.Generic;
using SocketIO;
using UnityEngine;



class Space
{
    public Vector3 topLeft;
    public Vector3 topRight;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
    public float width;
    public float height;

    public Space()
    {
        topLeft     = new Vector3(-1.6f, 1.85f,-1f);
        topRight    = new Vector3(-1.6f, 1.85f, -0.4f);
        bottomLeft  = new Vector3(-1.6f, 1f, -1f);
        bottomRight = new Vector3(-1.6f, 1f,  -0.4f);
        width = Vector3.Distance(topLeft, topRight);
        height = Vector3.Distance(topLeft, bottomLeft);
    }
}

/**
 * Tracks the position of a Vive tracker and sends 
 * relative position in space to a WAMS app. 
 *
**/
public class TrackerPercentage : MonoBehaviour
{
    int TRACKED_VIEW_INDEX = 2;
    
    Vector3 trackerPosition;
    Space space;
    private SocketIOComponent socket;

    void Start()
    {
        space = new Space();
        GameObject go = GameObject.Find("SocketIO");
        socket = go.GetComponent<SocketIOComponent>();
    }

    void Update()
    {
        trackerPosition = this.GetComponent<Transform>().position;
        float relativePosX = (trackerPosition.z - space.topLeft.z) / space.width;
        float relativePosY = (space.topLeft.y - trackerPosition.y) / space.height;
        float relativePosXLimited = limitPosition(relativePosX);
        float relativePosYLimited = limitPosition(relativePosY);
        Debug.Log("X: " + relativePosXLimited * 100 + "%, Y: " + relativePosYLimited * 100 + "%");
        NotifyServer("position", relativePosXLimited, relativePosYLimited);
    }

    float limitPosition(float relativeCoordinate)
    {
        if (relativeCoordinate < 0) return 0;
        if (relativeCoordinate > 1) return 1;
        return relativeCoordinate;
    }

    void NotifyServer(string ev, float relPosX, float relPosY)
    {
        JSONObject position = new JSONObject(JSONObject.Type.OBJECT);
        position.AddField("x", relPosX);
        position.AddField("y", relPosY);

        JSONObject payload = new JSONObject(JSONObject.Type.OBJECT);
        payload.AddField("deviceIndex", TRACKED_VIEW_INDEX);
        payload.AddField("position", position);

        JSONObject dreport = DReport(ev, payload);

        socket.Emit("dispatch", dreport);
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
