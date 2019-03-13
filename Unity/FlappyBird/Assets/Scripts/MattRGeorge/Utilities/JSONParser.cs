using System.Collections.Generic;

namespace MattRGeorge.Utilities
{
    /// <summary>
    /// Parses a JSON string into a JSONObject with the values of the the JSON in dictionaries.
    /// </summary>
    public static class JSONParser
    {
        /// <summary>
        /// Read an array of JSON objects from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="startingI">The index to start at.</param>
        /// <param name="endingI">The index ReadObjectArray() finished at.</param>
        /// <returns>The parsed array of JSON objects.</returns>
        private static List<JSONObject> ReadObjectArray(string json, int startingI, out int endingI)
        {
            List<JSONObject> objs = new List<JSONObject>();

            for (endingI = startingI; endingI < json.Length; endingI++)
            {
                if (json[endingI] == '{') objs.Add(ReadObject(json, endingI, out endingI));
                else if (json[endingI] == ']') break;
            }

            return objs;
        }
        /// <summary>
        /// Read an array of values from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="startingI">The index to start at.</param>
        /// <param name="endingI">The index ReadValueArray() finished at.</param>
        /// <returns>The parsed array of values.</returns>
        private static List<string> ReadValueArray(string json, int startingI, out int endingI)
        {
            List<string> values = new List<string>();

            for (endingI = startingI; endingI < json.Length; endingI++)
            {
                if (json[endingI] == '"') values.Add(ReadString(json, endingI, out endingI));
                else if (json[endingI] == ']') break;
                else if (json[endingI] != ' ' && json[endingI] != ',') values.Add(ReadPrimitiveValue(json, endingI, out endingI));
            }

            return values;
        }
        /// <summary>
        /// Main entry point for reading an array of the given JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="startingI">The index to start at.</param>
        /// <param name="endingI">The index ReadArray() finished at.</param>
        /// <param name="parsingObj">Reference to the JSON object to store the read array in.</param>
        /// <param name="keyName">The name to assign to the dictionary key for the array.</param>
        /// <returns>The parsed array.</returns>
        private static void ReadArray(string json, int startingI, out int endingI, ref JSONObject parsingObj, string keyName)
        {
            for (endingI = startingI; endingI < json.Length; endingI++)
            {
                if (json[endingI] == '{')
                {
                    parsingObj.objArrays.Add(keyName, ReadObjectArray(json, endingI, out endingI));
                    break;
                }
                else if (json[endingI] == '"' || (json[endingI] != ' ' && json[endingI] != '['))
                {
                    parsingObj.valueArrays.Add(keyName, ReadValueArray(json, endingI, out endingI));
                    break;
                }
            }
        }

        /// <summary>
        /// Read a primitive value from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="startingI">The index to start at.</param>
        /// <param name="endingI">The index ReadPrimitiveValue() finished at.</param>
        /// <returns>The parsed string value.</returns>
        private static string ReadPrimitiveValue(string json, int startingI, out int endingI)
        {
            string value = "";

            bool isReading = false;
            for (endingI = startingI; endingI < json.Length; endingI++)
            {
                if (json[endingI] != ' ' && !isReading)
                {
                    isReading = true;
                    value += json[endingI];
                }
                else if ((json[endingI] == ' ' || json[endingI] == ',' || json[endingI] == '}' || json[endingI] == ']') && isReading)
                {
                    if (json[endingI] == '}') endingI--;
                    break;
                }
                else if (json[endingI] != ' ' && isReading) value += json[endingI];
            }

            return value;
        }
        /// <summary>
        /// Read a string key or value from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="startingI">The index to start at.</param>
        /// <param name="endingI">The index ReadString() finished at.</param>
        /// <returns>The parsed string.</returns>
        private static string ReadString(string json, int startingI, out int endingI)
        {
            string str = "";

            bool isReading = false;
            for (endingI = startingI; endingI < json.Length; endingI++)
            {
                if (json[endingI] == '"' && !isReading) isReading = true;
                else if (json[endingI] == '"' && isReading) break;
                else if (isReading) str += json[endingI];
            }

            return str;
        }
        /// <summary>
        /// Read a JSON object from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="startingI">The index to start at.</param>
        /// <param name="endingI">The index ReadObject() finished at.</param>
        /// <returns>The parsed JSON object.</returns>
        private static JSONObject ReadObject(string json, int startingI, out int endingI)
        {
            JSONObject parsedJSON = new JSONObject();

            string currKey = "";
            int curlyBracketCount = 0;
            for (endingI = startingI; endingI < json.Length; endingI++)
            {
                if (json[endingI] == '{') curlyBracketCount++;
                else if (json[endingI] == '}')
                {
                    curlyBracketCount--;
                    if (curlyBracketCount == 0) break;
                }
                else if (json[endingI] == '[')
                {
                    ReadArray(json, endingI, out endingI, ref parsedJSON, "ROOT_ARRAY");
                    continue;
                }

                if (json[endingI] == '"' && currKey.Length == 0)
                {
                    currKey = ReadString(json, endingI, out endingI);

                    for (int i = endingI + 1; i < json.Length; i++)
                    {
                        if (json[i] != ':' && json[i] != ' ')
                        {
                            if (json[i] == '{')
                            {
                                parsedJSON.jsonObjects.Add(currKey, ReadObject(json, i, out endingI));
                                break;
                            }
                            else if (json[i] == '[')
                            {
                                ReadArray(json, i, out endingI, ref parsedJSON, currKey);
                                break;
                            }
                            else if (json[i] == '"')
                            {
                                parsedJSON.values.Add(currKey, ReadString(json, i, out endingI));
                                break;
                            }
                            else
                            {
                                parsedJSON.values.Add(currKey, ReadPrimitiveValue(json, i, out endingI));
                                break;
                            }
                        }
                    }

                    currKey = "";
                }
            }

            return parsedJSON;
        }
        /// <summary>
        /// Parses a JSON string that only has one value and one value alone.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The parsed value.</returns>
        private static JSONObject ReadSingleValue(string json)
        {
            JSONObject parsedObj = new JSONObject();

            int tempI = 0;
            for(int i = 0; i < json.Length; i++)
            {
                if (json[i] == '"')
                {
                    parsedObj.values.Add("SINGLE_VALUE", ReadString(json, i, out tempI));
                    break;
                }
                else if(json[i] != ' ' && json[i] != '{' && json[i] != '[')
                {
                    parsedObj.values.Add("SINGLE_VALUE", ReadPrimitiveValue(json, i, out tempI));
                    break;
                }
            }

            return parsedObj;
        }

        /// <summary>
        /// Parse a JSON string.
        /// NOTE: Doesn't support JSON strings with mixed types.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The parsed JSON object.</returns>
        public static JSONObject ParseJSON(string json)
        {
            int tempI = 0;
            if (!json.Contains(":") && !json.Contains("[")) return ReadSingleValue(json);
            else return ReadObject(json, 0, out tempI);
        }
    }

    /// <summary>
    /// A parsed JSON object with the values in dictionaries.
    /// </summary>
    public class JSONObject
    {
        public Dictionary<string, string> values = new Dictionary<string, string>();
        public Dictionary<string, JSONObject> jsonObjects = new Dictionary<string, JSONObject>();
        public Dictionary<string, List<JSONObject>> objArrays = new Dictionary<string, List<JSONObject>>();
        public Dictionary<string, List<string>> valueArrays = new Dictionary<string, List<string>>();
    }
}