using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework.Internal;

public class TemplateGraphicsCreator
{
    [MenuItem("Tools/Create Graphics")]
    public static void CreateGraphics() {
        //get icon
        var selectedIcon = Selection.activeObject as Texture2D;
        if (selectedIcon == null) {
            Debug.Log($"{Selection.assetGUIDs.Length}");
            Debug.LogError("Select an icon in the project folder and then click the 'Create Graphics'");
            return;
        }
        Debug.Log("Got an icon");
        //scale icon
        var icon = Resize(selectedIcon, 128, 128);
        Debug.Log("Resized icon copy to 128x128");
        //create background
        var background = new Texture2D(1, 1);
        background.SetPixel(0, 0, Color.black);
        background.Apply();
        Debug.Log("Created background with a black pixel");
        //remove old versions
        string pathLogo = Path.Combine(Application.dataPath, "WebGLTemplates", "PluginYG", "logo.png");
        string pathThumbnail = Path.Combine(Application.dataPath, "WebGLTemplates", "PluginYG", "thumbnail.png");
        string pathBackground = Path.Combine(Application.dataPath, "WebGLTemplates", "PluginYG", "background.png");
        FileUtil.DeleteFileOrDirectory(pathLogo);
        Debug.Log("Deleted old logo");
        FileUtil.DeleteFileOrDirectory(pathThumbnail);
        Debug.Log("Deleted old thumbnail");
        FileUtil.DeleteFileOrDirectory(pathBackground);
        Debug.Log("Deleted old background");
        AssetDatabase.Refresh();
        //same new versions
        File.WriteAllBytes(pathLogo, icon.EncodeToPNG());
        Debug.Log("Saved the new logo");
        File.WriteAllBytes(pathThumbnail, icon.EncodeToPNG());
        Debug.Log("Saved the new thumbnail");
        File.WriteAllBytes(pathBackground, background.EncodeToPNG());
        Debug.Log("Saved the new background");
        Debug.Log("Graphics creation for template is completed");
    }

    static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }
}
