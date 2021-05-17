using System;

[Serializable]
public class WorldData {
    public string name;
    public int seed;
    public string last_played;
    public bool isCreative;

    public WorldData(string name, int seed, string last_played, bool isCreative) {
        this.name = name;
        this.seed = seed;
        this.last_played = last_played;
        this.isCreative = isCreative;
    }
}