using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

public class TerrainGenerator : MonoBehaviour
{
    //public static int[,,] GenerateTerrain(int xOffset, int zOffset)

    public float BaseHeight = 64;
    public NoiseOctavSettings[] Octaves;
    public NoiseOctavSettings DomainWarp;

    public NoiseOctavSettings[] GraniteOctaves;
    public NoiseOctavSettings[] CoalOctaves;
    public NoiseOctavSettings[] CaveOctaves;

    public NoiseOctavSettings[] TreeOctaves;


    [Serializable]
    public class NoiseOctavSettings
    {
        public FastNoiseLite.NoiseType NoiseType;
        public float Frequency = 0.2f;
        public float Amplitude = 1;

        public float stretchX = 1;
        public float stretchY = 1;
        public float stretchZ = 1;

        public float threshold = 0;

    }

    private FastNoiseLite[] octaveNoises;

    //private FastNoiseLite warpNoiseX;
    //private FastNoiseLite warpNoiseY;

    private FastNoiseLite warpNoise;

    private FastNoiseLite[] graniteNoises;
    private FastNoiseLite[] coalNoises;
    private FastNoiseLite[] caveNoises;

    private FastNoiseLite[] treeNoises;



    public void Awake()
    {
        init();
    }

    public void init()
    {
        octaveNoises = new FastNoiseLite[Octaves.Length];
        for (int i = 0; i < Octaves.Length; i++)
        {
            octaveNoises[i] = new FastNoiseLite();
            octaveNoises[i].SetNoiseType(Octaves[i].NoiseType);
            octaveNoises[i].SetFrequency(Octaves[i].Frequency);
        }

        warpNoise = new FastNoiseLite();
        warpNoise.SetNoiseType(DomainWarp.NoiseType);
        warpNoise.SetFrequency(DomainWarp.Frequency);
        warpNoise.SetDomainWarpAmp(DomainWarp.Amplitude);

        graniteNoises = new FastNoiseLite[GraniteOctaves.Length];
        for (int i = 0; i < GraniteOctaves.Length; i++)
        {
            graniteNoises[i] = new FastNoiseLite();
            graniteNoises[i].SetNoiseType(GraniteOctaves[i].NoiseType);
            graniteNoises[i].SetFrequency(GraniteOctaves[i].Frequency);
        }

        coalNoises = new FastNoiseLite[CoalOctaves.Length];
        for (int i = 0; i < CoalOctaves.Length; i++)
        {
            coalNoises[i] = new FastNoiseLite();
            coalNoises[i].SetNoiseType(CoalOctaves[i].NoiseType);
            coalNoises[i].SetFrequency(CoalOctaves[i].Frequency);
        }

        caveNoises = new FastNoiseLite[CaveOctaves.Length];
        for (int i = 0; i < CaveOctaves.Length; i++)
        {
            caveNoises[i] = new FastNoiseLite();
            caveNoises[i].SetNoiseType(CaveOctaves[i].NoiseType);
            caveNoises[i].SetFrequency(CaveOctaves[i].Frequency);
        }

        treeNoises = new FastNoiseLite[TreeOctaves.Length];
        for (int i = 0; i < TreeOctaves.Length; i++)
        {
            treeNoises[i] = new FastNoiseLite();
            treeNoises[i].SetNoiseType(TreeOctaves[i].NoiseType);
            treeNoises[i].SetFrequency(TreeOctaves[i].Frequency);
        }
    }

    public BlockType[,,] GenerateTerrain(int xOffset, int zOffset)
    {
        //FastNoiseLite noise = new FastNoiseLite();
        //noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        //float f = noise.GetNoise(1, 2);


        //var result = new int[ChunkRenderer.ChunkWidth, ChunkRenderer.ChunkHeight, ChunkRenderer.ChunkWidth];
        var result = new BlockType[ChunkRenderer.ChunkWidth, ChunkRenderer.ChunkHeight, ChunkRenderer.ChunkWidth];

        for (int x = 0; x < ChunkRenderer.ChunkWidth; x++)
        {
            for (int z = 0; z < ChunkRenderer.ChunkWidth; z++)
            {
                //float height = Mathf.PerlinNoise((x + xOffset) * .2f, (z + zOffset)*.2f) * 5 + 10;
                float height = GetHeight((x * ChunkRenderer.BlockScale + xOffset), (z * ChunkRenderer.BlockScale + zOffset));
                float grassLayerHeight = 1;
                float dirtLayerHeight = 3;

                for (int y = 0; y < height; y++)
                {
                    //result[x, y, z] = 1;
                    //result[x, y, z] = BlockType.Stone;

                    if (height - y * ChunkRenderer.BlockScale < grassLayerHeight)
                    {
                        result[x, y, z] = BlockType.Grass;
                    }
                    else if (height - y * ChunkRenderer.BlockScale < dirtLayerHeight)
                    {
                        result[x, y, z] = BlockType.Dirt;
                    }
                    else
                    {
                        result[x, y, z] = BlockType.Stone;
                    }
                }

                // вода
                for (int y = 0; y < 32; y++)
                {
                    if (result[x, y, z] == BlockType.Air)
                        result[x, y, z] = BlockType.Water;

                }

                // гранит
                for (int y = 0; y < 64; y++)
                {
                    if (result[x, y, z] == BlockType.Stone)
                    {
                        if (GetGranite((x * ChunkRenderer.BlockScale + xOffset), y, (z * ChunkRenderer.BlockScale + zOffset)))
                        {
                            result[x, y, z] = BlockType.Granite;
                        }
                    }
                }

                // уголь
                for (int y = 0; y < 64; y++)
                {
                    if (result[x, y, z] == BlockType.Stone)
                    {
                        if (GetCoal((x * ChunkRenderer.BlockScale + xOffset), y, (z * ChunkRenderer.BlockScale + zOffset)))
                        {
                            result[x, y, z] = BlockType.StoneCoal;
                        }
                    }
                }

                // пещеры
                for (int y = 0; y < 64; y++)
                {
                    if (result[x, y, z] != BlockType.Air && result[x, y, z] != BlockType.Water)
                    {
                        //Debug.Log(GetCave(x, y, z));
                        //if (GetCave(x, y, z) > 1)
                        //Debug.Log(GetCave((x * ChunkRenderer.BlockScale + xOffset), y, (z * ChunkRenderer.BlockScale + zOffset)));
                        if (GetCave((x * ChunkRenderer.BlockScale + xOffset), y, (z * ChunkRenderer.BlockScale + zOffset)))
                        {
                            result[x, y, z] = BlockType.Air;
                        }
                    }
                }


                
            }
        }

        // деревья
        var middle = ChunkRenderer.ChunkWidth / 2;
        Random rnd = new Random();
        
        for (int y = 20; y < 64; y++)
        {
            if (result[middle, y, middle] == BlockType.Grass && result[middle, y+1, middle] == BlockType.Air)
            {
                if (GetTree((middle * ChunkRenderer.BlockScale + xOffset), y, (middle * ChunkRenderer.BlockScale + zOffset)))
                {
                    for (int i = 0; i < y % 12; i++)
                    {
                        result[middle, y + i, middle] = BlockType.Wood;

                        if (i > 4)
                        for (int x = ChunkRenderer.ChunkWidth/ rnd.Next(3, i); x < ChunkRenderer.ChunkWidth - ChunkRenderer.ChunkWidth/ rnd.Next(3, i); x++)
                        {
                            for (int z = ChunkRenderer.ChunkWidth/ rnd.Next(3, i); z < ChunkRenderer.ChunkWidth - ChunkRenderer.ChunkWidth/ rnd.Next(3, i); z++)
                            {
                                if (GetTree((x * ChunkRenderer.BlockScale + xOffset), y+i, (z * ChunkRenderer.BlockScale + zOffset)))
                                {
                                    result[x, y + i, z] = BlockType.Leaves;
                                }
                            }
                        }
                    }
                }
            }
        }


        return result;
    }

    private float GetHeight(float x, float y)
    {
        float result = BaseHeight;
        for (int i = 0; i < Octaves.Length; i++)
        {
            //float newX = x + warpNoiseX.GetNoise(x, y);
            //float newY = y + warpNoiseY.GetNoise(x, y);
            warpNoise.DomainWarp(ref x, ref y);
            // float noise = 0;
            // if (i==0) {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            // else if (i==1) {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            // else if (i==2) {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            // else {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            float noise = octaveNoises[i].GetNoise(x / 8, y / 8);
            result += noise * Octaves[i].Amplitude / 2;
        }
        return result;
    }


    private bool GetGranite(float x, float y, float z)
    {
        bool result = false;
        for (int i = 0; i < GraniteOctaves.Length; i++)
        {
            float noise = graniteNoises[i].GetNoise(x / GraniteOctaves[i].stretchX, y / GraniteOctaves[i].stretchY, z / GraniteOctaves[i].stretchZ);
            if (noise > GraniteOctaves[i].threshold) result = true;
            //if (i == 2) Debug.Log(noise);
        }
        return result;
    }

    private bool GetCoal(float x, float y, float z)
    {
        bool result = false;
        for (int i = 0; i < CoalOctaves.Length; i++)
        {
            float noise = coalNoises[i].GetNoise(x / CoalOctaves[i].stretchX, y / CoalOctaves[i].stretchY, z / CoalOctaves[i].stretchZ);
            if (noise > CoalOctaves[i].threshold) result = true;
        }
        return result;
    }

    private bool GetCave(float x, float y, float z)
    {
        //float result = 0;
        bool result = false;
        for (int i = 0; i < CaveOctaves.Length; i++)
        {
            //float noise = caveNoises[i].GetNoise(x, y, z);
            //float noise = caveNoises[i].GetNoise(x / 8, y, z / 8);
            float noise = caveNoises[i].GetNoise(x / CaveOctaves[i].stretchX, y / CaveOctaves[i].stretchY, z / CaveOctaves[i].stretchZ);

            //result += noise * CaveOctaves[i].Amplitude;
            if (noise > CaveOctaves[i].threshold) result = true;
            //if (i == 2) Debug.Log(noise);
        }
        return result;
    }

    private bool GetTree(float x, float y, float z)
    {
        bool result = false;
        for (int i = 0; i < TreeOctaves.Length; i++)
        {
            float noise = treeNoises[i].GetNoise(x / TreeOctaves[i].stretchX, y / TreeOctaves[i].stretchY, z / TreeOctaves[i].stretchZ);
            if (noise > TreeOctaves[i].threshold) result = true;
        }
        return result;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
