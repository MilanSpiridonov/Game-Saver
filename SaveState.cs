using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SaveGame : MonoBehaviour
{    
    //public GameObject[] toSave;
    public List<GameObject> toSave = new List<GameObject>();
    public static string SceneName;
    public static string Path;
    public bool Auto_save;
    private void Start()
    {        
        SceneName = SceneManager.GetActiveScene().name;
        Path = Application.dataPath + "/" + SceneName + ".txt";        
        string saved = "";
        foreach (var f in toSave)
        {
            saved += f.name + " - ";
        }
        //print(saved);
        if (Auto_save)
            try
            {
                LoadUp(SceneName);
            }
            catch
            {
                Debug.LogError("There has been an error with the save file, please make sure it exists and is not corrupted!");
            }
    }

    private void Update()
    {
       
    }

    public void LoadUp(string SceneNameTemp)
    {
        //print(Application.dataPath + "/" + SceneNameTemp + ".txt");
        StreamReader reader = new StreamReader(Application.dataPath + "/" + SceneName + ".txt", true);
        string[] dataTemp = reader.ReadToEnd().Split('\n');
        List<string> saveData = new List<string>();
        foreach (string s in dataTemp)
        {
            if (s != "")
                saveData.Add(s);
        }
        foreach (string s in saveData)
        {
            foreach (var GameObj in toSave)
            {
                if (GameObj.name == s.Split('~')[1])
                {
                    Component[] comps = GameObj.GetComponents(typeof(Component));
                    foreach (Component comp in comps)
                    {
                        foreach (var v in comp.GetType().GetFields())
                        {
                            if (v.Name == s.Split('~')[3])
                            {
                                switch (s.Split('~')[5].Split('.')[1])
                                {
                                    case "String":
                                        v.SetValue(comp, s.Split('~')[7]);
                                        break;
                                    case "Int32":
                                        v.SetValue(comp, int.Parse(s.Split('~')[7]));
                                        break;
                                    case "Single":
                                        v.SetValue(comp, float.Parse(s.Split('~')[7]));
                                        break;
                                    case "Vector3":
                                        Vector3 vec;
                                        string[] vectr = s.Split('~')[7].Split('(')[1].Split(')')[0].Split(',');
                                        vec = new Vector3(float.Parse(vectr[0]), float.Parse(vectr[1]), float.Parse(vectr[2]));
                                        v.SetValue(comp, vec);
                                        if (v.Name == "S_Position")
                                            GameObj.transform.position = vec;
                                        break;
                                    case "Vector2":
                                        Vector2 vec2;
                                        string[] vectr2 = s.Split('~')[7].Split('(')[1].Split(')')[0].Split(',');
                                        vec2 = new Vector2(float.Parse(vectr2[0]), float.Parse(vectr2[1]));
                                        v.SetValue(comp, vec2);
                                        if (v.Name == "S_Position")
                                            GameObj.transform.position = vec2;
                                        break;
                                    case "Boolean":
                                        v.SetValue(comp, bool.Parse(s.Split('~')[7]));
                                        break;
                                }

                            }
                        }
                    }
                }
            }
        }
        reader.Close();

    }

    public void SaveState()
    {
        StreamWriter writerClean = new StreamWriter(Path, false);
        writerClean.Write("");
        writerClean.Close();        
        foreach (var f in toSave)
        {            
            Component[] comps = f.GetComponents(typeof(Component));
            foreach (Component s in comps)
            {                
                foreach (var g in s.GetType().GetFields())
                {
                    print("here");
                    StreamWriter writer = new StreamWriter(Path, true);
                    try
                    {
                        if (g.Name.Split('_')[0] == "S") //Saveble vars need to start with '_S'!!!
                        {
                            string data = "OBJECT NAME~" + f.name + "~VAR NAME~" + g.Name + "~VAR TYPE~" + g.FieldType + "~VAR VALUE~" + g.GetValue(s);
                            print(data);
                            writer.WriteLine(data);
                        }
                    }
                    catch { }
                    writer.Close();
                }
            }
        }
    }

}
