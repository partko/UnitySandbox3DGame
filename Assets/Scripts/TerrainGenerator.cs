using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainGenerator : MonoBehaviour
{
    public float BaseHeight = 64;
    public NoiseOctavSettings[] Octaves;

    [Serializable]
    public class NoiseOctavSettings
    {
        public FastNoiseLite.NoiseType NoiseType;
        public float Frequency = 0.2f;
        public float Amplitude = 1;

    }

    private FastNoiseLite[] octaveNoises;
    public void Awake()
    {
        octaveNoises = new FastNoiseLite[Octaves.Length];
        for (int i = 0; i < Octaves.Length; i++)
        {
            octaveNoises[i] = new FastNoiseLite();
            octaveNoises[i].SetNoiseType(Octaves[i].NoiseType);
            octaveNoises[i].SetFrequency(Octaves[i].Frequency);

        }
    }

    public BlockType[,,] GenerateTerrain(int xOffset, int zOffset)
    {
        // FastNoiseLite noise = new FastNoiseLite();
        // noise.setNoiseType(FastNoiseLite.NoiseType.Rerlin);
        // float f = noise.getNoise(1, 2);


        var result = new BlockType[ChunkRenderer.ChunkWidth, ChunkRenderer.ChunkHeight, ChunkRenderer.ChunkWidth];
        for (int x = 0; x < ChunkRenderer.ChunkWidth; x++)
        {
            for (int z = 0; z < ChunkRenderer.ChunkWidth; z++)
            {
                float height = GetHeight((x * ChunkRenderer.BlockScale + xOffset), (z * ChunkRenderer.BlockScale + zOffset));
                float grassLayerHeight = 1;
                float dirtLayerHeight = 5;

                for (int y = 0; y < height; y++)
                {
                    if (height - y*ChunkRenderer.BlockScale < grassLayerHeight)
                    {
                        result[x, y, z] = BlockType.Grass;
                    }
                    else if (height - y*ChunkRenderer.BlockScale < dirtLayerHeight)
                    {
                        result[x, y, z] = BlockType.Dirt;
                    }
                    else
                    {
                        result[x, y, z] = BlockType.Stone;
                    }
                }
                for (int y = 0; y < 32; y++)
                {
                    if (result[x, y, z] == BlockType.Air)
                    result[x, y, z] = BlockType.Water;

                }

                // //float height = Mathf.PerlinNoise((x/4f+xOffset)*.2f, (z/4f+zOffset)*.2f) *25 + 10; //принимает 2 координаты и возвращает высоту шума в точке
                // float height = Mathf.PerlinNoise((x+xOffset)*.02f, (z+zOffset)*.02f) *25 + 10;

                // //for (int y = 0; y < height; y++) result[x, y, z] = BlockType.Grass;
                // if (height < 16 )
                // {
                //     for (int y = 0; y < 8; y++) result[x, y, z] = BlockType.Stone;
                //     for (int y = 8; y < 12; y++) result[x, y, z] = BlockType.Dirt;
                //     for (int y = 12; y < height; y++) result[x, y, z] = BlockType.Water;
                // }
                // else
                // {
                //     for (int y = (int) height; y > height-1; y--) result[x, y, z] = BlockType.Grass;
                //     for (int y = (int) height-1; y > height-4; y--) result[x, y, z] = BlockType.Dirt;
                //     for (int y = (int) height-4; y > 0; y--) result[x, y, z] = BlockType.Stone;
                // }

            }
        }
        return result;
    }

    private float GetHeight(float x, float y)
    {
        float result = BaseHeight;
        for (int i =0; i < Octaves.Length; i++)
        {
            // float noise = 0;
            // if (i==0) {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            // else if (i==1) {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            // else if (i==2) {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            // else {noise = octaveNoises[i].GetNoise(x/8, y/8);}
            float noise = octaveNoises[i].GetNoise(x/8, y/8);
            result += noise * Octaves[i].Amplitude / 2;
        }
        return result;
    }
}
