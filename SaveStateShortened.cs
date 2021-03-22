using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveState : MonoBehaviour
{
    string SceneName;
    public List<GameObject> toSave = new List<GameObject>();
    private void Awake()
    {
        SceneName = SceneManager.GetActiveScene().name;
        try
        {
            StreamReader initR = new StreamReader(Application.dataPath + "/Saves/" + "data_" + SceneName + ".txt");
            foreach (string line in initR.ReadToEnd().Split('\n'))
            {
                if (line != "")
                {
                    if (line.Split('~')[0] == "Loaded_scene")
                    {
                        int ind = int.Parse(line.Split('~')[1]);
                        LoadGame(ind);
                    }
                }
            }
            initR.Close();
            //Dinamically check/modify a new file that says if we're to load game
            StreamWriter init = new StreamWriter(Application.dataPath + "/Saves/" + "data_" + SceneName + ".txt", false);
            init.Write("");
            init.Close();
        }
        catch
        {
            Debug.LogError("Save file not found!");
        }

    }

    /// <summary>
    /// Saves the current state of the game, with an index of your choice.
    /// </summary>
    /// <param name="Save_index">In which slot you want to save your state.</param>
    public void SaveGame(int Save_index)
    {
        string tempPath = Application.dataPath + "/Saves/" + Save_index.ToString() + "_" + SceneName + ".txt";
        StreamWriter Clear = new StreamWriter(tempPath, false);
        Clear.Write("");
        Clear.Close();

        foreach (GameObject Obj in toSave) // Parses through all objects, that we want to save
        {
            Component[] scripts = Obj.GetComponents(typeof(Component)); // Gets all the scripts in said object
            foreach (Component script in scripts)
            {
                foreach (var variable in script.GetType().GetFields()) // Parses through all the variables in the script
                {
                    StreamWriter write = new StreamWriter(tempPath, true);
                    try
                    {
                        if (variable.Name.Split('_')[0] == "S") // Saveble vars need to start with "S_", otherwise will be ignored!
                        {
                            /*OBJECT_NAME holds the name of the GameObject,
                             *VAR_NAME holds the name of the variable,
                             *VAR_TYPE holds the type of variable,
                             *VAR_VALUE holds the value of the variable
                             */

                            string data = "OBJECT_NAME~" + Obj.name + "~VAR_NAME~" + variable.Name + "~VAR_TYPE~" + variable.FieldType + "~VAR_VALUE~" + variable.GetValue(script);
                            print(data);
                            write.WriteLine(data);
                        }
                    }
                    catch { Debug.LogError("Error writing data to file"); }
                    write.Close();
                }
            }
        }
    }

    /// <summary>
    /// Loads a scene from a save file, used outside of said scene.
    ///<para>
    /// Looks for the default save file with index 0.
    /// </para>
    /// </summary>
    /// <param name="Scene"> Which scene you want to load. </param>
    public void LoadGame(int ind)
    {
        Debug.LogError("LOAD SCENE: " + ind);
        string tempPath = Application.dataPath + "/Saves/" + ind.ToString() + "_" + SceneName + ".txt";
        StreamReader read = new StreamReader(tempPath);
        List<string> saveData = new List<string>();
        foreach (string line in read.ReadToEnd().Split('\n')) //Gets all lines (or save points)
            if (line != "") // skip the blank lines and add those with value.
                saveData.Add(line);
        read.Close();
        foreach (string line in saveData)
        {
            foreach (GameObject Obj in toSave)
                if (Obj.name == line.Split('~')[1])
                {
                    Component[] components = Obj.GetComponents(typeof(Component));
                    foreach (Component component in components)
                    {
                        foreach (var variable in component.GetType().GetFields())
                        {
                            if (variable.Name == line.Split('~')[3])
                            {
                                switch (line.Split('~')[5].Split('.')[1])
                                {
                                    case "String":
                                        variable.SetValue(component, line.Split('~')[7]);
                                        break;
                                    case "Int32":
                                        variable.SetValue(component, int.Parse(line.Split('~')[7]));
                                        break;
                                    case "Single":
                                        variable.SetValue(component, float.Parse(line.Split('~')[7]));
                                        break;
                                    case "Vector2":
                                        string[] V2 = line.Split('~')[7].Split('(')[1].Split(')')[0].Split(',');
                                        Vector2 vector2 = new Vector2(float.Parse(V2[0]), float.Parse(V2[1]));
                                        variable.SetValue(component, vector2);
                                        if (variable.Name == "S_Position")
                                            Obj.transform.position = vector2;
                                        break;
                                    case "Vector3":
                                        string[] V3 = line.Split('~')[7].Split('(')[1].Split(')')[0].Split(',');
                                        Vector3 vector3 = new Vector3(float.Parse(V3[0]), float.Parse(V3[1]), float.Parse(V3[2]));
                                        variable.SetValue(component, vector3);
                                        if (variable.Name == "S_Position")
                                            Obj.transform.position = vector3;
                                        break;
                                    case "Boolean":
                                        variable.SetValue(component, bool.Parse(line.Split('~')[7]));
                                        break;
                                }
                            }
                        }
                    }
                }
        }
    }

    /// <summary>
    /// Loads a scene from a save file, used outside of said scene.
    /// Looks for the save file with the given index.    
    /// </summary>    
    /// <param name="Scene"> Which scene you want to load:
    /// <para>'SampleScene' will load scene named 'SampleScene' without loading save file.</para>
    /// <para>'SampleScene_0' will load scene named 'SampleScene' and load save file with index 0.</para>
    /// <para>'SampleScene_3' will load scene named 'SampleScene' and load save file with index 3.</para>
    /// </param>    
    public void Scene_LoadGame(string Scene)
    {
        string SceneTemp;
        int Save_index;
        try
        {
            SceneTemp = Scene.Split('_')[0];
            Save_index = int.Parse(Scene.Split('_')[1]);
            print(SceneTemp);
            print(Save_index);
        }
        catch
        {
            SceneTemp = Scene;
            Save_index = 42069;
        }

        string tempPath = Application.dataPath + "/Saves/" + "data_" + SceneTemp + ".txt";
        StreamWriter write = new StreamWriter(tempPath);
        if (Save_index != 42069)
            write.Write("Loaded_scene~" + Save_index.ToString());
        else write.Write("New_scene");
        write.Close();
        SceneManager.LoadScene(SceneTemp);
    }
}
