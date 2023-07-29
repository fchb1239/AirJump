using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

using AirJump.Logging;

namespace AirJump.Behaviours
{
    class VersionVerifier : MonoBehaviour
    {
        public static VersionVerifier instance;
        public bool validVersion = false;
        public string newestVersion = "0.0.0";

        private static readonly string versionCheckUrl = "https://raw.githubusercontent.com/fchb1239/AirJump/main/NewestVersion";

        private void Awake()
        {
            instance = this;
            StartCoroutine(VerifyVersion());
        }

        private IEnumerator VerifyVersion()
        {
            AJLog.Log("Getting newest version");
            var request = new UnityWebRequest(versionCheckUrl, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();
            //Pretty sure github adds an \n after it for whatever reason
            validVersion = request.downloadHandler.text.Contains(PluginInfo.Version);
            newestVersion = request.downloadHandler.text;
            if (validVersion)
                AJLog.Log("Valid version :)");
            else
                AJLog.Log("Invalid version :(");
        }
    }
}
