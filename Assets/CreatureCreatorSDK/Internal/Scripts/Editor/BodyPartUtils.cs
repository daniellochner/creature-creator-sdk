using System.IO;
using DanielLochner.Assets.CreatureCreator;
using UnityEditor;

public static class BodyPartUtils
{
    public static void NewBodyPart()
    {
        if (ModdingUtils.TryCreateNewItem<BodyPartConfig>(out string bodyPartName, out string bodyPartPath))
        {
            string dstPath = Path.Combine(bodyPartPath, $"{bodyPartName}.prefab");
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/BodyPart.prefab", dstPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    public static bool BuildBodyPart(BodyPartConfig config, bool buildAll)
    {
        return ModdingUtils.TryBuildItem<BodyPartConfig, BodyPartConfigData>(BodyPartConfig.GetCurrent(), buildAll, delegate
        {

        });
    }

    public static void TestBodyPart(BodyPartConfig config)
    {

    }

    public static void UploadBodyPart(BodyPartConfig config)
    {

    }
}
