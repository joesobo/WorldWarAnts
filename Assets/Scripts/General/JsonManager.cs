using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class JsonManager {
    public static void Write<T>(string path, T collection) {
        using var w = new StreamWriter(path, false);
        var json = JsonUtility.ToJson(collection);
        w.Write(json);
        w.Flush();
        w.Close();
    }

    public static void Remove<T>(string path, List<T> collection, int index) {
        var tempCollection = new List<T>();
        if (index == collection.Count - 1) {
            tempCollection = collection.GetRange(0, index);
            tempCollection.AddRange(collection.GetRange(index + 1, collection.Count));
        } else {
            tempCollection = collection.GetRange(0, index);
        }
        Write(path, tempCollection);
    }

    public static T Read<T>(string path) {
        T collection = default;

        try {
            using var r = new StreamReader(path);
            var json = r.ReadToEnd();
            collection = JsonUtility.FromJson<T>(json);
            r.Close();
        } catch (Exception e) {
            Debug.Log("ERROR: No file found.");
            Debug.Log(e.Message);
        }
        
        return collection;
    }
}
