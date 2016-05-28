using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Constructs a string table (string -> string map)
public class StringTable
{
    private Dictionary<string, string> _StringMap;

    public StringTable()
    {
        _StringMap = new Dictionary<string, string>();
    }

    public void Add(string key, string value)
    {
        _StringMap[key] = value;
    }

    public bool Has(string key)
    {
        return _StringMap.ContainsKey(key);
    }

    public string Get(string key)
    {
        return _StringMap[key];
    }

    public static string FormatSubstitute(string format, IDictionary<string, string> keyValues)
    {
        string resultString = "";

        // Where is the last recorded "plain text".
        int lastTextIndex = 0;
        int leftIndex = format.IndexOf('{');
        int rightIndex = leftIndex >= 0 ? format.IndexOf('}', leftIndex) : -1;

        while (leftIndex >= 0 && rightIndex >= 0)
        {
            // Append the last segment of plain text to result.
            resultString += format.Substring(lastTextIndex, leftIndex - lastTextIndex);

            string key = format.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
            string value;
            if (keyValues.TryGetValue(key, out value))
            {
                // If the key-value pair is found, append the value string to the result string and move the plain text pointer to after the brackets.
                resultString += value;
                lastTextIndex = rightIndex + 1;
            }
            else
            {
                // Otherwise move the plain text pointer to the left bracket - it's now considered "plain text" too.
                lastTextIndex = leftIndex;
            }

            leftIndex = format.IndexOf('{', rightIndex);
            rightIndex = leftIndex >= 0 ? format.IndexOf('}', leftIndex) : -1;
        }

        // Append the last of plain text.
        resultString += format.Substring(lastTextIndex);

        return resultString;
    }

    public static StringTable GlobalStringTable = new StringTable();
}
