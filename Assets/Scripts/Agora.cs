using System.Collections.Generic;
using System.IO;
using agora_gaming_rtc;
using UnityEngine;

public class Agora : MonoBehaviour
{
    public Transform remoteSurfaceParent;
    public GameObject remoteSurfacePrefab;
    public Texture2D texture2d;

    public string appId;
    public string token;
    public string channel = "test";

    private IRtcEngine rtcEngine;

    private int _ticks;
    private bool connected;

    private void Start()
    {
        Connect();
    }

    public void Connect()
    {
        rtcEngine = IRtcEngine.GetEngine(appId);
        rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
        rtcEngine.OnUserJoined += OnUserJoined;
        rtcEngine.OnUserEnableVideo += OnUserEnableVideo;

        rtcEngine.SetExternalVideoSource(true); // Disable local webcam
        // rtcEngine.MuteLocalVideoStream(true);
        // rtcEngine.MuteLocalAudioStream(true);

        
        rtcEngine.EnableVideo();
        rtcEngine.EnableVideoObserver();
        rtcEngine.JoinChannelByKey(token, channel);
    }

    private void OnJoinChannelSuccess(string channelname, uint uid, int elapsed)
    {
        Debug.Log($"Joined channel: {channelname} as {uid}");
        connected = true;
    }

    private void OnUserJoined(uint uid, int elapsed)
    {
        Debug.Log($"{uid} joined");
    }

    private void OnUserEnableVideo(uint uid, bool b)
    {
        Debug.Log($"{uid} enabled video");


        GameObject remoteSurface = Instantiate(remoteSurfacePrefab, remoteSurfaceParent);
        remoteSurface.name = $"{remoteSurfacePrefab.name}-{uid}";

        VideoSurface videoSurface = remoteSurface.GetComponentInChildren<VideoSurface>();
        videoSurface.EnableFilpTextureApply(false, true);
        videoSurface.SetForUser(uid);
        videoSurface.SetEnable(true);
    }
    
    private void Update()
    {
        if (!connected)
        {
            return;
        }

        ExternalVideoFrame frame = Texture2DToExternalVideoFrame(texture2d);
        rtcEngine.PushVideoFrame(frame);
    }

    private ExternalVideoFrame Texture2DToExternalVideoFrame(Texture2D tex)
    {
        byte[] bytes = tex.GetRawTextureData();

        ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
        
        externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
        externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;
        externalVideoFrame.buffer = bytes;
        externalVideoFrame.stride = tex.width; 
        externalVideoFrame.height = tex.height;
        externalVideoFrame.timestamp = _ticks++;
        return externalVideoFrame;
    }

    private void OnApplicationQuit()
    {
        IRtcEngine.Destroy();
    }
}