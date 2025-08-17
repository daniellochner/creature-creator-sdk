using System.IO;
using DanielLochner.Assets.CreatureCreator;
using UnityEditor;

public static class BodyPartUtils
{
    public static void NewBodyPart()
    {
        if (ModdingUtils.TryCreateNewItem(out string bodyPartName, out string bodyPartPath, out BodyPartConfig config))
        {
            string dstPath = Path.Combine(bodyPartPath, $"{bodyPartName}.prefab");
            AssetDatabase.CopyAsset("Assets/CreatureCreatorSDK/Internal/Templates/BodyPart.prefab", dstPath);
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
        string path = ModdingUtils.GetBuildPath(config);
        ModdingUtils.StartGame(ModdingUtils.GetApplicationPath(), path, "loadbodypart");
    }

    public static void UploadBodyPart(BodyPartConfig config)
    {

    }
}
