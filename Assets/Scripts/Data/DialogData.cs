using UnityEngine;

[CreateAssetMenu(fileName = "NewDialog", menuName = "Dialog/Dialog Data")]
public class DialogData : ScriptableObject
{
    [TextArea(2, 5)]
    public string[] lines;
}