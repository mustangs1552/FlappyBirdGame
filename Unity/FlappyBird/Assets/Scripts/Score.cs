using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class Score : MonoBehaviour
    {
        public string uploadScoreURL = "";
        public uint serverGameID = 0;

        private int amount = 0;

        public int Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
                if (amount < 0) amount = 0;
            }
        }

        public void StartUploadScore(string username)
        {
            if(uploadScoreURL == null || uploadScoreURL == "")
            {
                Debug.LogError("No high score URL set!");
            }
            else
            {
                StartCoroutine("UploadScore", username);
            }
        }

        private IEnumerator UploadScore(string username)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("username=" + username));
            formData.Add(new MultipartFormDataSection("score=" + amount));

            UnityWebRequest www = UnityWebRequest.Post(uploadScoreURL, formData);
            Debug.Log("Uploading score...");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) Debug.LogError("Error while uploading score: " + www.error);
            else Debug.Log("Uploaded score!");
        }
    }
}
