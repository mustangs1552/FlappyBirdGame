using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class Score : MonoBehaviour
    {
        private int amount = 0;
        private const string URL_UPLOAD_SCORE = "";

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
            StartCoroutine("UploadScore", username);
        }

        private IEnumerator UploadScore(string username)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("username=" + username));
            formData.Add(new MultipartFormDataSection("score=" + amount));

            UnityWebRequest www = UnityWebRequest.Post(URL_UPLOAD_SCORE, formData);
            Debug.Log("Uploading score...");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) Debug.LogError("Error while uploading score: " + www.error);
            else Debug.Log("Uploaded score!");
        }
    }
}
