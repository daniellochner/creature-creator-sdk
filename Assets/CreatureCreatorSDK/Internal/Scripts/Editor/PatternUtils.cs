using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class PatternUtils
{
    public static void NewPattern()
    {

    }

    public static bool BuildPattern(PatternConfig config, bool buildAll)
    {
        return true;
    }

    public static void TestPattern(PatternConfig config)
    {

    }

    public static void UploadPattern(PatternConfig config)
    {

    }

    public static void BuildAndTestPattern(PatternConfig config)
    {
        if (BuildPattern(config, true))
        {
            TestPattern(config);
        }
    }

    public static void BuildAndUploadPattern(PatternConfig config)
    {
        if (BuildPattern(config, true))
        {
            UploadPattern(config);
        }
    }
}
