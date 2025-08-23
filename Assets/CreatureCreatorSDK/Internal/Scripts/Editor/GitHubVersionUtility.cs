using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class GitHubVersionUtility
{
    /// <summary>
    /// Gets the latest release tag from a public GitHub repo.
    /// </summary>
    /// <param name="owner">Repo owner (username or org)</param>
    /// <param name="repo">Repository name</param>
    /// <returns>The tag_name of the latest release, or null if failed.</returns>
    public static async Task<string> GetLatestReleaseAsync(string owner, string repo)
    {
        string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("User-Agent", "Unity-Version-Checker");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"GitHub request failed: {request.error}");
                return null;
            }

            try
            {
                var json = request.downloadHandler.text;
                var tagKey = "\"tag_name\":\"";
                int start = json.IndexOf(tagKey, StringComparison.Ordinal);
                if (start == -1) return null;

                start += tagKey.Length;
                int end = json.IndexOf("\"", start, StringComparison.Ordinal);

                return json.Substring(start, end - start);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse GitHub response: {e.Message}");
                return null;
            }
        }
    }
}