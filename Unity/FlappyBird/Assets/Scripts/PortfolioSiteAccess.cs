/* Created by musta
 * 2/8/2019 12:06:17 PM
 */

using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace MattRGeorge
{
    /// <summary>
    /// Used to access various functions on the portfolio site at MattRGeorge.com such as score and logging in/out (no logging in/out).
    /// </summary>
    public class PortfolioSiteAccess : MonoBehaviour
    {
        #region variables
        #region Public
        [Tooltip("The ID of the game in the site's database.")]
        [SerializeField] private int serverGameID = 0;
        #endregion

        #region Properties
        public List<string> SupportedScoreTypes
        {
            get
            {
                return supportedScoreTypes;
            }
        }
        #endregion

        #region private
        private const string ROOT_DOMAIN_URL = "http://localhost:56300/";
        private const string SCORES_CONTROLLER = "Scores/";
        private const string SAVE_SCORE_URL = "SaveScore";
        private const string GET_SCORES_URL = "GetScores";
        private const string GET_SCORE_TYPES_URL = "GetScoreTypes";

        private List<string> supportedScoreTypes = new List<string>();
        #endregion
        #endregion

        #region Functions
        #region Public
        /// <summary>
        /// Start uploading the given score to the site.
        /// </summary>
        /// <param name="user">The username for the score.</param>
        /// <param name="type">The type of score (Points, Seconds, etc...).</param>
        /// <param name="score">The score value.</param>
        public void StartUploadScore(string user, string type, int score)
        {
            if (supportedScoreTypes == null || supportedScoreTypes.Count == 0 || user == null || user == "" || type == null || type == "") return;
            if (!supportedScoreTypes.Contains(type))
            {
                PrintErrorDebugMsg_PortfolioSiteAccess("\"" + type + "\" score type is not supported by site!");
                return;
            }

            ScoreData data = new ScoreData()
            {
                username = user,
                scoreType = type,
                amount = score,
            };
            StartCoroutine("UploadScore", data);
        }
        public void StartGettingScores(string user = null)
        {

        }
        #endregion

        #region Private
        /// <summary>
        /// Gets the list of supported score types from the site.
        /// </summary>
        private IEnumerator GetSupportedScoreTypes()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(ROOT_DOMAIN_URL + SCORES_CONTROLLER + GET_SCORE_TYPES_URL))
            {
                //www.certificateHandler = new AcceptAllSelfSignedCerts();
                PrintDebugMsg_PortfolioSiteAccess("Getting score types...");
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) PrintErrorDebugMsg_PortfolioSiteAccess("Error while getting score types: " + www.error + "\n\tURL: " + www.url);
                else
                {
                    PrintDebugMsg_PortfolioSiteAccess("Received score types! JSON:\n" + www.downloadHandler.text);

                    supportedScoreTypes = new List<string>();
                    supportedScoreTypes = ParseJSONArray(www.downloadHandler.text);
                }
            }

            if (supportedScoreTypes == null || supportedScoreTypes.Count == 0) PrintErrorDebugMsg_PortfolioSiteAccess("No supported score types found!");
            else
            {
                string debugStr = "Supported score types received (" + supportedScoreTypes.Count + "):";
                foreach (string type in supportedScoreTypes) debugStr += "\n\t- " + type;
                PrintDebugMsg_PortfolioSiteAccess(debugStr);
            }
        }

        /// <summary>
        /// Uploads the given score to the site.
        /// </summary>
        /// <param name="data">The score information to be uploaded.</param>
        private IEnumerator UploadScore(ScoreData data)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("projectID", serverGameID.ToString()));
            formData.Add(new MultipartFormDataSection("playerName", data.username));
            formData.Add(new MultipartFormDataSection("scoreType", data.scoreType));
            formData.Add(new MultipartFormDataSection("score", data.amount.ToString()));

            using (UnityWebRequest www = UnityWebRequest.Post(ROOT_DOMAIN_URL + SCORES_CONTROLLER + SAVE_SCORE_URL, formData))
            {
                //www.certificateHandler = new AcceptAllSelfSignedCerts();
                PrintDebugMsg_PortfolioSiteAccess("Uploading score...\n\tUsername: " + data.username + "\n\tScore Type: " + data.scoreType + "\n\tScore: " + data.amount);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) PrintErrorDebugMsg_PortfolioSiteAccess("Error while uploading score: " + www.error + "\n\tURL: " + www.url);
                else PrintDebugMsg_PortfolioSiteAccess("Uploaded score!");
            }
        }
        /*private IEnumerator GetScores(string user = null)
        {

        }*/
        
        /// <summary>
        /// Parses a section of a JSON object that represents one array of strings.
        /// </summary>
        /// <param name="jsonArrayText">Section of JSON that is an array of strings.</param>
        /// <returns>A list containing the parsed array.</returns>
        private List<string> ParseJSONArray(string jsonArrayText)
        {
            List<string> parsedArray = new List<string>();
            bool read = false;
            string currIndex = "";
            foreach(char c in jsonArrayText)
            {
                if (c == '"' && read)
                {
                    parsedArray.Add(currIndex);
                    currIndex = "";
                    read = false;
                }
                else if (c == '"') read = true;
                else if(read) currIndex += c;
            }

            return parsedArray;
        }
        #endregion
        #endregion

        #region Unity Functions
        private void Awake()
        {
            PrintDebugMsg_PortfolioSiteAccess("Debugging enabled.");

            if (serverGameID <= 0) PrintErrorDebugMsg_PortfolioSiteAccess("Server game ID is not set!");

            StartCoroutine("GetSupportedScoreTypes");
        }
        #endregion

        #region Template
        #region Variables
        [SerializeField] private bool isDebug_PortfolioSiteAccess = false;
        private string isDebugScriptName_PortfolioSiteAccess = "PortfolioSiteAccess";
        #endregion

        #region Functions
        /// <summary>
        /// Uses Debug.Log() to post a message with the name of this script and the object its attached to as the prefix only if the debugging flag is true under GlobalVariables->DefaultVariables.
        /// </summary>
        /// <param name="msg">The message.</param>
        private void PrintDebugMsg_PortfolioSiteAccess(string msg)
        {
            if (isDebug_PortfolioSiteAccess) Debug.Log(isDebugScriptName_PortfolioSiteAccess + " (" + this.gameObject.name + "): " + msg);
        }
        /// <summary>
        /// Uses Debug.LogWarning() to post a message with the name of this script and the object its attached to as the prefix.
        /// </summary>
        /// <param name="msg">The message.</param>
        private void PrintWarningDebugMsg_PortfolioSiteAccess(string msg)
        {
            Debug.LogWarning(isDebugScriptName_PortfolioSiteAccess + " (" + this.gameObject.name + "): " + msg);
        }
        /// <summary>
        /// Uses Debug.LogError() to post a message with the name of this script and the object its attached to as the prefix.
        /// </summary>
        /// <param name="msg">The message.</param>
        private void PrintErrorDebugMsg_PortfolioSiteAccess(string msg)
        {
            Debug.LogError(isDebugScriptName_PortfolioSiteAccess + " (" + this.gameObject.name + "): " + msg);
        }
        #endregion
        #endregion

        private class ScoreData
        {
            public string username = "";
            public string scoreType = "";
            public int amount = 0;
        }
    }

    public class AcceptAllSelfSignedCerts : CertificateHandler
    {
        public static string PUBLIC_KEY = "PublicKey";

        protected override bool ValidateCertificate(byte[] certificateData)
        {
            X509Certificate2 cert = new X509Certificate2(certificateData);
            string pk = cert.GetPublicKeyString();
            if (pk.ToLower().Equals(PUBLIC_KEY.ToLower())) return true;

            return false;
        }
    }
}