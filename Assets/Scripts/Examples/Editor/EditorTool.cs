using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorTool
{
    //将单个Sprite们合并为一个大图集
    [MenuItem("Assets/CustomTool/MergeSprite")]
    public static void MergeSprite()
    {
        string[] spriteGUIDs = Selection.assetGUIDs;
        if (spriteGUIDs == null || spriteGUIDs.Length <= 1)
            return;
        List<string> spritePathsList = new(spriteGUIDs.Length);
        for (int i = 0; i < spriteGUIDs.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(spriteGUIDs[i]);
            if (string.IsNullOrEmpty(assetPath))
                continue;
            spritePathsList.Add(assetPath);
        }
        spritePathsList.Sort();
        Texture2D firstTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathsList[0]);
        int unitHeight = firstTexture.height;
        int unitWidth = firstTexture.width;
        Texture2D outputTexture = new(unitWidth * spritePathsList.Count, unitHeight);
        for (int i = 0; i < spritePathsList.Count; i++)
        {
            Texture2D tempTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePathsList[i]);
            Color[] colors = tempTexture.GetPixels();
            outputTexture.SetPixels(i * unitWidth, 0, unitWidth, unitHeight, colors);
        }
        byte[] bytes = outputTexture.EncodeToPNG();
        File.WriteAllBytes(spritePathsList[0][..spritePathsList[0].LastIndexOf(firstTexture.name)] + "MergeSprite.png", bytes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}