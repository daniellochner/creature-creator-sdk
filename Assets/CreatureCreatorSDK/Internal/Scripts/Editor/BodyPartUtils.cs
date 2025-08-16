using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class BodyPartUtils
{
    public static void NewBodyPart()
    {

    }

    public static bool BuildBodyPart(BodyPartConfig config, bool buildAll)
    {
        return true;
    }

    public static void TestBodyPart(BodyPartConfig config)
    {

    }

    public static void UploadBodyPart(BodyPartConfig config)
    {

    }

    public static void BuildAndTestBodyPart(BodyPartConfig config)
    {
        if (BuildBodyPart(config, true))
        {
            TestBodyPart(config);
        }
    }

    public static void BuildAndUploadBodyPart(BodyPartConfig config)
    {
        if (BuildBodyPart(config, true))
        {
            UploadBodyPart(config);
        }
    }
}
