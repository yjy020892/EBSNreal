using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class MyBuildPostprocessor
{
    const string json_path = @"/SPVR/Resources/Configuration";
    const string json_file = "/Setting.json";

#if !UNITY_ANDROID
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
    {
        string jsonFullPath = json_path + json_file;

        string buildDirectory = System.IO.Path.GetDirectoryName(pathToBuildProject);
        string buildDataDirectory = buildDirectory + "/" + Application.productName + "_Data";
        System.IO.Directory.CreateDirectory(buildDataDirectory + json_path);
        FileUtil.CopyFileOrDirectory(Application.dataPath + jsonFullPath, buildDataDirectory + jsonFullPath);
    }
#endif
}
