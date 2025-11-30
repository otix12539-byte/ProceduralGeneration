using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class Generator : MonoBehaviour
{
    public enum Algo { Perlin, Cellular }
    public Algo algorithm = Algo.Perlin;

    [Header("Карта")]
    public int width = 100;
    public int height = 100;

    [Header("Perlin шум")]
    public float scale = 20f;
    public float threshold = 0.5f; 

    [Header("Клітинний автомат")]
    [Range(0f, 1f)] public float fillProbability = 0.45f;
    public int caSteps = 4;
    public int birthLimit = 4;
    public int deathLimit = 3;

    [Header("Seed / Детермінованість")]
    public int seed = 0;
    public bool randomSeed = true; 
    public bool saveSeedToPlayerPrefs = true;

    [Header("Візуалізація")]
    public Tilemap tilemap;
    public TileBase wallTile;
    public TileBase floorTile;

    int[,] map; // 1 = стіна, 0 = підлога
    System.Random prng;

    void Start()
    {
        GenerateOnStart();
    }

    public void GenerateOnStart()
    {
        if (randomSeed) seed = Environment.TickCount;
        prng = new System.Random(seed);
        if (saveSeedToPlayerPrefs) PlayerPrefs.SetInt("LastSeed", seed);
        GenerateMap();
        DrawMap();
    }

    public void GenerateMap()
    {
        map = new int[width, height];
        if (algorithm == Algo.Perlin) GeneratePerlin();
        else GenerateCellular();
    }

    void GeneratePerlin()
    {
        float xo = (float)prng.NextDouble() * 1000f;
        float yo = (float)prng.NextDouble() * 1000f;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float sample = Mathf.PerlinNoise((x + xo) / scale, (y + yo) / scale); 
                map[x, y] = sample > threshold ? 1 : 0;
            }
    }

    void GenerateCellular()
    {
        // ініціалізація
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                map[x, y] = (prng.NextDouble() < fillProbability) ? 1 : 0;
            }

        // ітерації
        for (int i = 0; i < caSteps; i++)
        {
            int[,] newMap = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int neighWalls = CountNeighbors(x, y);
                    if (map[x, y] == 1)
                        newMap[x, y] = (neighWalls < deathLimit) ? 0 : 1;
                    else
                        newMap[x, y] = (neighWalls > birthLimit) ? 1 : 0;
                }
            map = newMap;
        }
    }

    int CountNeighbors(int sx, int sy)
    {
        int count = 0;
        for (int x = sx - 1; x <= sx + 1; x++)
            for (int y = sy - 1; y <= sy + 1; y++)
            {
                if (x == sx && y == sy) continue;
                if (x < 0 || y < 0 || x >= width || y >= height)
                    count++;
                else if (map[x, y] == 1) count++;
            }
        return count;
    }

    public void DrawMap()
    {
        if (tilemap == null) return;
        tilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x - width / 2, y - height / 2, 0);
                tilemap.SetTile(pos, map[x, y] == 1 ? wallTile : floorTile);
            }
    }
}
