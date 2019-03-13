/* Created by musta
 * 2/8/2019 12:06:17 PM
 */

using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using MattRGeorge.Utilities;

namespace MattRGeorge.PortfolioSiteAccess
{
    /// <summary>
    /// Used to access various functions on the portfolio site at MattRGeorge.com such as score and logging in/out (no logging in/out).
    /// </summary>
    public class ScoreAccess : MonoBehaviour
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
        private const string SAVE_SCORE_CONTROLLER = "SaveScore";
        private const string GET_SCORES_ACTION = "GetScores";
        private const string GET_USER_SCORES_ACTION = "GetUserScores";
        private const string GET_TOP_SCORES_ACTION = "GetTopScores";
        private const string GET_USER_TOP_SCORES_ACTION = "GetUserTopScores";
        private const string GET_SCORE_TYPES_ACTION = "GetScoreTypes";

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

            ScoreValue data = new ScoreValue()
            {
                playerName = user,
                scoreType = type,
                scoreAmount = score.ToString(),
            };
            StartCoroutine("UploadScore", data);
        }

        /// <summary>
        /// Start getting all scores for this game.
        /// </summary>
        /// <param name="callbackMethod">The callback method that will be called to send the resulting scores.</param>
        public void StartGettingScores(Func<List<ScoreValue>, bool> callbackMethod)
        {
            if (callbackMethod == null) return;

            GetScoreData data = new GetScoreData()
            {
                URLAction = GET_SCORES_ACTION,
                OnGotScores = callbackMethod,
            };
            StartCoroutine("GetScores", data);
        }
        /// <summary>
        /// Start getting all scores for this game for the given user.
        /// </summary>
        /// <param name="user">The user that the desired scores belong to.</param>
        /// <param name="callbackMethod">The callback method that will be called to send the resulting scores.</param>
        public void StartGettingScores(string user, Func<List<ScoreValue>, bool> callbackMethod)
        {
            if (user == null || user == "" || callbackMethod == null) return;

            GetScoreData data = new GetScoreData()
            {
                URLAction = GET_USER_SCORES_ACTION,
                User = user,
                OnGotScores = callbackMethod,
            };
            StartCoroutine("GetScores", data);
        }
        /// <summary>
        /// Start getting the top x scores for this game.
        /// </summary>
        /// <param name="topX">The amount of scores to get.</param>
        /// <param name="callbackMethod">The callback method that will be called to send the resulting scores.</param>
        public void StartGettingTopScores(int topX, Func<List<ScoreValue>, bool> callbackMethod)
        {
            if (topX <= 0 || callbackMethod == null) return;

            GetScoreData data = new GetScoreData()
            {
                URLAction = GET_TOP_SCORES_ACTION,
                TopX = topX,
                OnGotScores = callbackMethod,
            };
            StartCoroutine("GetScores", data);
        }
        /// <summary>
        /// Start getting the top x scores for this game for the given user.
        /// </summary>
        /// <param name="user">The user that the desired scores belong to.</param>
        /// <param name="topX">The amount of scores to get.</param>
        /// <param name="callbackMethod">The callback method that will be called to send the resulting scores.</param>
        public void StartGettingTopScores(string user, int topX, Func<List<ScoreValue>, bool> callbackMethod)
        {
            if (user == null || user == "" || topX <= 0 || callbackMethod == null) return;

            GetScoreData data = new GetScoreData()
            {
                URLAction = GET_USER_TOP_SCORES_ACTION,
                User = user,
                TopX = topX,
                OnGotScores = callbackMethod,
            };
            StartCoroutine("GetScores", data);
        }
        #endregion

        #region Private
        /// <summary>
        /// Gets the list of supported score types from the site.
        /// </summary>
        private IEnumerator GetSupportedScoreTypes()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(ROOT_DOMAIN_URL + SCORES_CONTROLLER + GET_SCORE_TYPES_ACTION))
            {
                //www.certificateHandler = new AcceptAllSelfSignedCerts();
                PrintDebugMsg_PortfolioSiteAccess("Getting score types...");
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) PrintErrorDebugMsg_PortfolioSiteAccess("Error while getting score types: " + www.error + "\n\tURL: " + www.url);
                else
                {
                    PrintDebugMsg_PortfolioSiteAccess("Received score types! JSON:\n" + www.downloadHandler.text);
                    
                    supportedScoreTypes = JSONParser.ParseJSON(www.downloadHandler.text).valueArrays["ROOT_ARRAY"];
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
        private IEnumerator UploadScore(ScoreValue data)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("projectID", serverGameID.ToString()));
            formData.Add(new MultipartFormDataSection("playerName", data.playerName));
            formData.Add(new MultipartFormDataSection("scoreType", data.scoreType));
            formData.Add(new MultipartFormDataSection("score", data.scoreAmount.ToString()));

            using (UnityWebRequest www = UnityWebRequest.Post(ROOT_DOMAIN_URL + SCORES_CONTROLLER + SAVE_SCORE_CONTROLLER, formData))
            {
                //www.certificateHandler = new AcceptAllSelfSignedCerts();
                PrintDebugMsg_PortfolioSiteAccess("Uploading score...\n\tUsername: " + data.playerName + "\n\tScore Type: " + data.scoreType + "\n\tScore: " + data.scoreAmount);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError) PrintErrorDebugMsg_PortfolioSiteAccess("Error while uploading score: " + www.error + "\n\tURL: " + www.url);
                else PrintDebugMsg_PortfolioSiteAccess("Uploaded score!");
            }
        }

        /// <summary>
        /// Gets the scores using the data given. Uses a provided callback method to send results.
        /// </summary>
        /// <param name="data">The data to be used when getting results.</param>
        private IEnumerator GetScores(GetScoreData data)
        {
            if ((data != null || data.OnGotScores != null) && (data.URLAction != null || data.URLAction != ""))
            {
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormDataSection("projectID", serverGameID.ToString()));
                if (data.User != null && data.User != "") formData.Add(new MultipartFormDataSection("username", data.User));
                if (data.TopX > 0) formData.Add(new MultipartFormDataSection("topX", data.TopX.ToString()));

                using (UnityWebRequest www = UnityWebRequest.Post(ROOT_DOMAIN_URL + SCORES_CONTROLLER + data.URLAction, formData))
                {
                    //www.certificateHandler = new AcceptAllSelfSignedCerts();
                    PrintDebugMsg_PortfolioSiteAccess("Getting" + ((data.TopX > 0) ? " Top " + data.TopX : "") + " scores" + ((data.User != null && data.User != "") ? " for " + data.User : "") + "...");
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError) PrintErrorDebugMsg_PortfolioSiteAccess("Error while getting scores: " + www.error + "\n\tURL: " + www.url);
                    else
                    {
                        PrintDebugMsg_PortfolioSiteAccess("Received scores! JSON:\n" + www.downloadHandler.text);

                        List<ScoreValue> scores = new List<ScoreValue>();
                        JSONObject parsedJSON = JSONParser.ParseJSON(www.downloadHandler.text);
                        foreach (JSONObject obj in parsedJSON.objArrays["ROOT_ARRAY"]) scores.Add(new ScoreValue(obj));
                        if(!data.OnGotScores(scores)) PrintWarningDebugMsg_PortfolioSiteAccess("OnGetScores callback failed for some reason!");
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Unity Functions
        private void Awake()
        {
            PrintDebugMsg_PortfolioSiteAccess("Debugging enabled.");

            if (serverGameID <= 0) PrintErrorDebugMsg_PortfolioSiteAccess("Server game ID must be 1 or greater!");

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
        
        /// <summary>
        /// The options that are passed the single parameter IEnumerators getting scores from server.
        /// </summary>
        private class GetScoreData
        {
            public string URLAction { get; set; }
            public string User { get; set; }
            public int TopX { get; set; }
            public Func<List<ScoreValue>, bool> OnGotScores { get; set; }
        }
    }

    /// <summary>
    /// The values of a score object from the server.
    /// </summary>
    public class ScoreValue
    {
        public int id = 0;
        public int projectID = 0;
        public string playerName = "";
        public string scoreType = "";
        public string scoreAmount = "";

        public ScoreValue()
        {

        }
        public ScoreValue(JSONObject parsedJSON)
        {
            int.TryParse(parsedJSON.values["id"], out id);
            int.TryParse(parsedJSON.values["projectID"], out projectID);
            playerName = parsedJSON.values["playerName"];
            scoreType = parsedJSON.values["scoreType"];
            scoreAmount = parsedJSON.values["scoreAmount"];
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