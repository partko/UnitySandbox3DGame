using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public const int ChunkWidth = 16;
    public const int ChunkHeight = 128;
    public const float BlockScale = 1f;//.25f;

    //public BlockType[,,] Blocks = new BlockType[ChunkWidth, ChunkHeight, ChunkWidth];
    public ChunkData ChunkData;
    public GameWorld ParentWorld;

    private Mesh chunkMesh;

    private List<Vector3> verticies = new List<Vector3>(); //вершины
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> triangles = new List<int>(); //треугольники

    // Start is called before the first frame update
    void Start()
    {
        //Mesh chunkMesh = new Mesh();
        chunkMesh = new Mesh();

        // Blocks[0,0,0] = 1;
        // Blocks[0,0,1] = 1;
        //Blocks = TerrainGenerator.GenerateTerrain((int) transform.position.x, (int) transform.position.y);

        RegenerateMesh();

        GetComponent<MeshFilter>().mesh = chunkMesh;
        //GetComponent<MeshCollider>().sharedMesh = chunkMesh;
    }

    private void RegenerateMesh()
    {
        verticies.Clear();
        uvs.Clear();
        triangles.Clear();

        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    GenerateBlock(x, y, z);
                }
            }
        }

        chunkMesh.triangles = Array.Empty<int>();
        chunkMesh.vertices = verticies.ToArray();
        chunkMesh.uv = uvs.ToArray();
        chunkMesh.triangles = triangles.ToArray();

        chunkMesh.Optimize(); //оптимизирует расположение вертексов

        chunkMesh.RecalculateNormals(); //для правильного взаимодействия с освещением
        chunkMesh.RecalculateBounds(); //для колайдеров

        GetComponent<MeshCollider>().sharedMesh = chunkMesh;
    }

    public void SpawnBlock(Vector3Int blockPosition, BlockType bType)
    {
        ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z] = bType;
        RegenerateMesh();
    }

    public void DestroyBlock(Vector3Int blockPosition)
    {
        ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z] = BlockType.Air;
        RegenerateMesh();
    }

    private BlockType GetBlockAtPosition(Vector3Int blockPosition)
    {
        if (blockPosition.x >= 0 && blockPosition.x < ChunkWidth &&
            blockPosition.y >= 0 && blockPosition.y < ChunkHeight &&
            blockPosition.z >= 0 && blockPosition.z < ChunkWidth)
        {
            //return Blocks[blockPosition.x, blockPosition.y, blockPosition.z];
            return ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z];
        }
        else
        {
            if (blockPosition.y < 0 || blockPosition.y >= ChunkHeight) return BlockType.Air;
            
            Vector2Int adjacentChunkPosition = ChunkData.ChunkPosition;
            if (blockPosition.x < 0)
            {
                adjacentChunkPosition.x--;
                blockPosition.x += ChunkWidth;
            }
            else if (blockPosition.x >= ChunkWidth)
            {
                adjacentChunkPosition.x++;
                blockPosition.x -= ChunkWidth;
            }
            if (blockPosition.z < 0)
            {
                adjacentChunkPosition.y--;
                blockPosition.z += ChunkWidth;
            }
            else if (blockPosition.z >= ChunkWidth)
            {
                adjacentChunkPosition.y++;
                blockPosition.z -= ChunkWidth;
            }

            if (ParentWorld.ChunkDatas.TryGetValue(adjacentChunkPosition, out ChunkData adjacentChunk))
            {
                return adjacentChunk.Blocks[blockPosition.x, blockPosition.y, blockPosition.z];
            }
            else
            {
                return BlockType.Air; //блоки за пределами чанка считаются воздухом, у чанка ресуются стены
                //return BlockType.Grass; //блоки за пределами чанка считаются блоком, у чанка не ресуются стены
            }
            
        }
    }

    private void GenerateBlock(int x, int y, int z)
    {
        var blockPosition = new Vector3Int(x, y, z);

        var blockType = GetBlockAtPosition(blockPosition);

        if (blockType == BlockType.Air) return;

        if (GetBlockAtPosition(blockPosition + Vector3Int.right) == 0)
        {
            GenerateRightSide(blockPosition);
            AdddUvs(blockType);
        }
        if (GetBlockAtPosition(blockPosition + Vector3Int.left) == 0)
        {
            GenerateLeftSide(blockPosition);
            AdddUvs(blockType);
        }
        if (GetBlockAtPosition(blockPosition + Vector3Int.forward) == 0)
        {
            GenerateFrontSide(blockPosition);
            AdddUvs(blockType);
        }
        if (GetBlockAtPosition(blockPosition + Vector3Int.back) == 0)
        {
            GenerateBackSide(blockPosition);
            AdddUvs(blockType);
        }
        if (GetBlockAtPosition(blockPosition + Vector3Int.up) == 0)
        {
            GenerateTopSide(blockPosition);
            AdddUvs(blockType);
        }
        if (GetBlockAtPosition(blockPosition + Vector3Int.down) == 0)
        {
            GenerateBottomSide(blockPosition);
            AdddUvs(blockType);
        }
    }

    private void GenerateRightSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(1,0,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,1,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,0,1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,1,1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateLeftSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0,0,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0,0,1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0,1,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0,1,1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateFrontSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0,0,1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,0,1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0,1,1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,1,1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateBackSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0,0,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0,1,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,0,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,1,0) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateTopSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0,1,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0,1,1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,1,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,1,1) + blockPosition) * BlockScale);
        
        AddLastVerticiesSqare();
    }

    private void GenerateBottomSide(Vector3Int blockPosition)
    {

        verticies.Add((new Vector3(0,0,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,0,0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0,0,1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1,0,1) + blockPosition) * BlockScale);
        
        AddLastVerticiesSqare();
    }

    private void AddLastVerticiesSqare()
    {
        triangles.Add(verticies.Count - 4);
        triangles.Add(verticies.Count - 3);
        triangles.Add(verticies.Count - 2);

        triangles.Add(verticies.Count - 3);
        triangles.Add(verticies.Count - 1);
        triangles.Add(verticies.Count - 2);
    }

    private void AdddUvs(BlockType blockType)
    {
        // uvs.Add(new Vector2(0, 0));
        // uvs.Add(new Vector2(0, 1));
        // uvs.Add(new Vector2(1, 0));
        // uvs.Add(new Vector2(1, 1));

        // uvs.Add(new Vector2(0.9375f, 0.25f));
        // uvs.Add(new Vector2(0.9375f, 0.3125f));
        // uvs.Add(new Vector2(1, 0.25f));
        // uvs.Add(new Vector2(1, 0.3125f));

        if (blockType == BlockType.Grass)
        {
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0.25f, 0));
            uvs.Add(new Vector2(0.25f, 1));
        }
        else if (blockType == BlockType.Dirt)
        {
            uvs.Add(new Vector2(0.25f, 0));
            uvs.Add(new Vector2(0.25f, 1));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0.5f, 1));
        }
        else if (blockType == BlockType.Stone)
        {
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(0.75f, 0));
            uvs.Add(new Vector2(0.75f, 1));
        }
        else if (blockType == BlockType.Water)
        {
            uvs.Add(new Vector2(0.75f, 0));
            uvs.Add(new Vector2(0.75f, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
        }
        else
        {
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(0.75f, 0));
            uvs.Add(new Vector2(0.75f, 1));
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
