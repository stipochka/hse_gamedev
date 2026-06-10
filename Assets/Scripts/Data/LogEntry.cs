using UnityEngine;

[CreateAssetMenu(fileName = "LogEntry", menuName = "Lore/Log Entry")]
public class LogEntry : ScriptableObject
{
    public string title;
    [TextArea(3, 10)]
    public string content;
    public Sprite icon;

    // Рантайм-состояние: не сериализуется в SO, сбрасывается при каждом запуске.
    [System.NonSerialized]
    public bool isFound;
}
