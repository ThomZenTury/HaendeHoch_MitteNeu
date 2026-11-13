using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;

public class SaveFolderReadout : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown _dropdown;
    private char[] delimiterChars = new char[] { '/', '\\' };
    private string path;
    private void Awake()
    {
        path = Application.persistentDataPath + "/Save/";
    }
    void Start()
    {
        GetFilesList();
    }

    public void GetFilesList()
    {
        DirectoryInfo dir = new DirectoryInfo(path);

        _dropdown.options.Clear();

        // FileInfo[] info = dir.GetFiles("*,*");
        // string[] files = Directory.GetFiles(path, "*.json");
        var orderedFiles = Directory.GetFiles(path, "*.json").Select(f => new FileInfo(f)).OrderByDescending(f => f.CreationTime);
        Debug.Log("Looking for files in: " + path);
        Debug.Log("Found files: " + orderedFiles.Count());

        foreach (FileInfo f in orderedFiles)
        {
            Debug.Log("Found file: " + f.Name);
            _dropdown.options.Add(new TMP_Dropdown.OptionData() { text = f.Name });
        }
    }
}
