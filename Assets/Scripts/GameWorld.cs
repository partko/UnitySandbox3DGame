using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// генерация чанков
public class GameWorld : MonoBehaviour
{
    // словарь для соответствия координат чанка и данных этого чанка
    // Vector2Int - координаты чанка
    public Dictionary<Vector2Int, ChunkData> ChunkDatas = new Dictionary<Vector2Int, ChunkData>();

    // количество загруженных чанков от игрока
    //private const int ViewRadius = 10; //default = 5
    public int ViewRadius = 10;

    public ChunkRenderer ChunkPrefab;
    public TerrainGenerator Generator;

    private Camera mainCamera;
    private Vector2Int currentPlayerChunk;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        //Generate();
        //StartCoroutine(Generate(true));

        // при старте загружаем чанки одновременно
        StartCoroutine(Generate(false));
    }

    //private void Generate()
    //{
    //    for (int x = currentPlayerChunk.x - ViewRadius; x < currentPlayerChunk.x + ViewRadius; x++)
    //    {
    //        for (int y = currentPlayerChunk.y - ViewRadius; y < currentPlayerChunk.y + ViewRadius; y++)
    //        {
    //            Vector2Int chunkPosition = new Vector2Int(x, y);

    //            // пропускаем уже загруженные чанки
    //            if (ChunkDatas.ContainsKey(chunkPosition)) continue;

    //            LoadChunkAt(chunkPosition);
    //        }
    //    }
    //}

    private IEnumerator Generate(bool wait)
    {
        for (int x = currentPlayerChunk.x - ViewRadius; x < currentPlayerChunk.x + ViewRadius; x++)
        {
            for (int y = currentPlayerChunk.y - ViewRadius; y < currentPlayerChunk.y + ViewRadius; y++)
            {
                Vector2Int chunkPosition = new Vector2Int(x, y);

                // пропускаем уже загруженные чанки
                if (ChunkDatas.ContainsKey(chunkPosition)) continue;

                LoadChunkAt(chunkPosition);

                // задержка генерации каждого чанка
                if (wait) yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }

    private void LoadChunkAt(Vector2Int chunkPosition)
    {
        float xPos = chunkPosition.x * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;
        float zPos = chunkPosition.y * ChunkRenderer.ChunkWidth * ChunkRenderer.BlockScale;

        ChunkData chunkData = new ChunkData();
        chunkData.ChunkPosition = new Vector2Int(chunkPosition.x, chunkPosition.y);
        chunkData.Blocks = Generator.GenerateTerrain((int)xPos, (int)zPos);
        ChunkDatas.Add(new Vector2Int(chunkPosition.x, chunkPosition.y), chunkData);

        var chunk = Instantiate(ChunkPrefab, new Vector3(xPos, 0, zPos), Quaternion.identity, transform);
        chunk.ChunkData = chunkData;
        chunk.ParentWorld = this;
        chunkData.Renderer = chunk;
    }

    // Update is called once per frame
    void Update()
    {
        // мировые целочисленные координаты игрока
        Vector3Int playerWorldPos = Vector3Int.FloorToInt(mainCamera.transform.position / ChunkRenderer.BlockScale);
        
        // чанк, в котором находится игрок
        Vector2Int playerChunk = GetChunkContainingBlock(playerWorldPos);

        if (playerChunk != currentPlayerChunk)
        {
            currentPlayerChunk = playerChunk;
            //Generate();
            // прогружаются чанки с задержкой
            StartCoroutine(Generate(true));

        }

        CheckInput();
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            bool isDestroing = Input.GetMouseButtonDown(0);

            // reycast из камеры в центр экрана
            // центр экрана 0.5f 0.5f
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

            // если попало в коллайдер, hitInfo - координаты попадания
            if (Physics.Raycast(ray, out var hitInfo))
            {
                // мировые юнити координаты блока
                Vector3 blockCenter;
                if (isDestroing)
                {
                    blockCenter = hitInfo.point - hitInfo.normal * ChunkRenderer.BlockScale / 2;
                }
                else
                {
                    blockCenter = hitInfo.point + hitInfo.normal * ChunkRenderer.BlockScale / 2;
                }

                // мировые целочисленные координаты блока
                Vector3Int blockWorldPos = Vector3Int.FloorToInt(blockCenter / ChunkRenderer.BlockScale);

                //if (blockWorldPos.x < 0) blockWorldPos.x--;
                //if (blockWorldPos.z < 0) blockWorldPos.z--;

                // позиция чанка, в котором лежит этот блок
                Vector2Int chunkPos = GetChunkContainingBlock(blockWorldPos);

                // пробуем получить чанк по позиции
                if (ChunkDatas.TryGetValue(chunkPos, out ChunkData chunkData))
                {
                    // координаты начала чанка, в котором он находится
                    Vector3Int chunkOrigin = new Vector3Int(chunkPos.x, 0, chunkPos.y) * ChunkRenderer.ChunkWidth;

                    // для получения локальной позиции блока, из мировой позиции блока вычитаем координаты начала чанка
                    if (isDestroing)
                    {
                        chunkData.Renderer.DestroyBlock(blockWorldPos - chunkOrigin);

                        //для перегенерации мешей соседних чанков
                        var blockPos = blockWorldPos - chunkOrigin;
                        if (blockPos.x == 0)
                        {
                            Vector2Int neighborСhunkPos = chunkPos + new Vector2Int(-1, 0);
                            if (ChunkDatas.TryGetValue(neighborСhunkPos, out ChunkData neighborСhunkData))
                            {
                                neighborСhunkData.Renderer.RegenerateMesh();
                            } 
                        }
                        if (blockPos.x == ChunkRenderer.ChunkWidth - 1)
                        {
                            Vector2Int neighborСhunkPos = chunkPos + new Vector2Int(1, 0);
                            if (ChunkDatas.TryGetValue(neighborСhunkPos, out ChunkData neighborСhunkData))
                            {
                                neighborСhunkData.Renderer.RegenerateMesh();
                            }
                        }
                        if (blockPos.z == 0)
                        {
                            Vector2Int neighborСhunkPos = chunkPos + new Vector2Int(0, -1);
                            if (ChunkDatas.TryGetValue(neighborСhunkPos, out ChunkData neighborСhunkData))
                            {
                                neighborСhunkData.Renderer.RegenerateMesh();
                            }
                        }
                        if (blockPos.z == ChunkRenderer.ChunkWidth - 1)
                        {
                            Vector2Int neighborСhunkPos = chunkPos + new Vector2Int(0, 1);
                            if (ChunkDatas.TryGetValue(neighborСhunkPos, out ChunkData neighborСhunkData))
                            {
                                neighborСhunkData.Renderer.RegenerateMesh();
                            }
                        }
                    }
                    else
                    {
                        //chunkData.Renderer.SpawnBlock(blockWorldPos - chunkOrigin, (BlockType)currentBlock);
                        chunkData.Renderer.SpawnBlock(blockWorldPos - chunkOrigin, BlockType.Wood);
                    }

                }
            }
        }
    }

    // возвращает позицию чанка, в котором находится блок
    public Vector2Int GetChunkContainingBlock(Vector3Int blockWorldPos)
    {
        //ошибка на границах минусовых чанков, считает блоки от 1 до 10, а не 0 до 9
        //Vector2Int chunkPosition = new Vector2Int(blockWorldPos.x / ChunkRenderer.ChunkWidth, blockWorldPos.z / ChunkRenderer.ChunkWidth);
        //if (blockWorldPos.x < 0) chunkPosition.x--;
        //if (blockWorldPos.z < 0) chunkPosition.y--;
        //return chunkPosition;

        return Vector2Int.FloorToInt(new Vector2(blockWorldPos.x * 1f / ChunkRenderer.ChunkWidth, blockWorldPos.z * 1f / ChunkRenderer.ChunkWidth));
    }

    [ContextMenu("Regenerate World")]
    public void Regenerate()
    {
        Generator.init();

        foreach (var chunkData in ChunkDatas)
        {
            Destroy(chunkData.Value.Renderer.gameObject);
        }

        ChunkDatas.Clear();

        // загружаем чанки одновременно
        StartCoroutine(Generate(false));
    }

}
