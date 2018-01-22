using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class IndiexpoAPI_WebGL {


    [DllImport("__Internal")]
    private static extern void ShowMessage(string str);

    [DllImport("__Internal")]
    private static extern void UploadScore(int s);

    //Use this method to send score to the Indiexpo server
    public static void SendScore(int s)
    {

        UploadScore(s);
    }

    //Use this method if you want to show a messagge to the user in the browser window
    public static void SendMessage(string str)
    {

        ShowMessage(str);
    }



}
