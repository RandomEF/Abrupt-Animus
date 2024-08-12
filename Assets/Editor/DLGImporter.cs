using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(1, "dlg")]
public class DLGImporter: ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext context){
        TextAsset asset = new TextAsset(File.ReadAllText(context.assetPath));
        context.AddObjectToAsset("dlg", asset, Resources.Load("dlg icon.png") as Texture2D);
        context.SetMainObject(asset);
    }
}
