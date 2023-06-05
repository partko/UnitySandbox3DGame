using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public const int ChunkWidth = 10;
    public const int ChunkHeight = 128;
    public const float BlockScale = 1f; //.5f;//.25f;

    //public int[,,] Blocks = new int[ChunkWidth, ChunkHeight, ChunkWidth];
    //public BlockType[,,] Blocks = new BlockType[ChunkWidth, ChunkHeight, ChunkWidth];
    public ChunkData ChunkData;
    public GameWorld ParentWorld;

    private Mesh chunkMesh;

    private List<Vector3> verticies = new List<Vector3>(); // набор позиций вершин
    private List<Vector2> uvs = new List<Vector2>(); // координаты для текстурной развертки
    private List<int> triangles = new List<int>(); // набор индексов вершин

    // Start is called before the first frame update
    void Start()
    {
        //Mesh chunkMesh = new Mesh();
        chunkMesh = new Mesh();
        RegenerateMesh();

        //Blocks[0, 0, 0] = 1;
        //Blocks[0, 0, 1] = 1;
        //Blocks = TerrainGenerator.GenerateTerrain((int)transform.position.x, (int)transform.position.y);


/*        for (int y = 0; y < ChunkHeight; y++)
        {
            for (int x = 0; x < ChunkWidth; x++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    GenerateBlock(x, y, z);
                }
            }
        }



        chunkMesh.vertices = verticies.ToArray();
        chunkMesh.triangles = triangles.ToArray();

        chunkMesh.Optimize(); //оптимизирует расположение вертексов
        chunkMesh.RecalculateNormals(); //для правильного взаимодействия с освещением
        chunkMesh.RecalculateBounds(); //для колайдеров*/


        GetComponent<MeshFilter>().mesh = chunkMesh;
        //GetComponent<MeshCollider>().sharedMesh = chunkMesh;


    }

    public void RegenerateMesh()
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
        Debug.ClearDeveloperConsole();
        Debug.Log(blockPosition.x + " " + blockPosition.y + " " + blockPosition.z);
        ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z] = BlockType.Air;
        RegenerateMesh();
    }

    private void GenerateBlock(int x, int y, int z)
    {
        var blockPosition = new Vector3Int(x, y, z);

        var blockType = GetBlockAtPosition(blockPosition);

        //if (Blocks[x, y, z] == 0) return;
        if (GetBlockAtPosition(blockPosition) == 0) return;

        // проверяем соседние стороны на наличие блока, генерируем сторону только если в соседнем воздух
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

    private BlockType GetBlockAtPosition(Vector3Int blockPosition)
    {
        // проверка на выход из массива
        if (blockPosition.x >= 0 && blockPosition.x < ChunkWidth &&
            blockPosition.y >= 0 && blockPosition.y < ChunkHeight &&
            blockPosition.z >= 0 && blockPosition.z < ChunkWidth)
        {
            //return Blocks[blockPosition.x, blockPosition.y, blockPosition.z];
            return ChunkData.Blocks[blockPosition.x, blockPosition.y, blockPosition.z];
        }
        else
        {
            // блоки за пределом чанка считаются воздухом
            //return 0;
            //return BlockType.Air;

            // блоки за пределом чанка считаются твердым блоком
            //return 1;
            //return BlockType.Grass;

            // проверка на выход за границы по вертикали
            if (blockPosition.y < 0 || blockPosition.y >= ChunkHeight) return BlockType.Air;

            //координаты нужного соседнего чанка
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
                return BlockType.Air; //блоки за пределами чанка считаются воздухом, у чанка рисуются стены
                //return BlockType.Grass; //блоки за пределами чанка считаются блоком, у чанка не рисуются стены
            }
        }
    }

    private void GenerateRightSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(1, 0, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 1, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 0, 1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 1, 1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateLeftSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0, 0, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0, 0, 1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0, 1, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0, 1, 1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateFrontSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0, 0, 1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 0, 1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0, 1, 1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 1, 1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateBackSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0, 0, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0, 1, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 0, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 1, 0) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateTopSide(Vector3Int blockPosition)
    {
        verticies.Add((new Vector3(0, 1, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0, 1, 1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 1, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 1, 1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void GenerateBottomSide(Vector3Int blockPosition)
    {

        verticies.Add((new Vector3(0, 0, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 0, 0) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(0, 0, 1) + blockPosition) * BlockScale);
        verticies.Add((new Vector3(1, 0, 1) + blockPosition) * BlockScale);

        AddLastVerticiesSqare();
    }

    private void AddLastVerticiesSqare()
    {
        //uvs.Add(new Vector2(0, 0));
        //uvs.Add(new Vector2(0, 1));
        //uvs.Add(new Vector2(1, 0));
        //uvs.Add(new Vector2(1, 1));

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
            uvs.Add(new Vector2(0.125f, 0));
            uvs.Add(new Vector2(0.125f, 1));
        }
        else if (blockType == BlockType.Dirt)
        {
            uvs.Add(new Vector2(0.125f, 0));
            uvs.Add(new Vector2(0.125f, 1));
            uvs.Add(new Vector2(0.25f, 0));
            uvs.Add(new Vector2(0.25f, 1));
        }
        else if (blockType == BlockType.Stone)
        {
            uvs.Add(new Vector2(0.25f, 0));
            uvs.Add(new Vector2(0.25f, 1));
            uvs.Add(new Vector2(0.375f, 0));
            uvs.Add(new Vector2(0.375f, 1));
        }
        else if (blockType == BlockType.Water)
        {
            uvs.Add(new Vector2(0.375f, 0));
            uvs.Add(new Vector2(0.375f, 1));
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0.5f, 1));
        }
        else if (blockType == BlockType.Wood)
        {
            uvs.Add(new Vector2(0.5f, 0));
            uvs.Add(new Vector2(0.5f, 1));
            uvs.Add(new Vector2(0.625f, 0));
            uvs.Add(new Vector2(0.625f, 1));
        }
        else if (blockType == BlockType.Granite)
        {
            uvs.Add(new Vector2(0.625f, 0));
            uvs.Add(new Vector2(0.625f, 1));
            uvs.Add(new Vector2(0.75f, 0));
            uvs.Add(new Vector2(0.75f, 1));
        }
        else if (blockType == BlockType.StoneCoal)
        {
            uvs.Add(new Vector2(0.75f, 0));
            uvs.Add(new Vector2(0.75f, 1));
            uvs.Add(new Vector2(0.875f, 0));
            uvs.Add(new Vector2(0.875f, 1));
        }
        else if (blockType == BlockType.Leaves)
        {
            uvs.Add(new Vector2(0.875f, 0));
            uvs.Add(new Vector2(0.875f, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
        }
        else
        {
            uvs.Add(new Vector2(0.25f, 0));
            uvs.Add(new Vector2(0.25f, 1));
            uvs.Add(new Vector2(0.375f, 0));
            uvs.Add(new Vector2(0.375f, 1));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
