using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains
{
    public class ExampleGameManager : MonoBehaviour
    {
        [Header("Parameters")]
        public float playerSpeed = 3.0f;

        public float difficultTerrainWeight = 100.0f;

        [Header("Other")]
        public Camera camera;

        public Vector2Int startPlayerPosition;
        public Tilemap mazeTileMap;
        public Tilemap pathTilemap;
        public TileBase pathTile;
        public TileBase targetTile;

        public GameObject player;

        public TextMeshProUGUI distanceText;
        public TextMeshProUGUI weightedDistanceText;

        List<Vector2Int> path = new();
        Vector2Int currentTagetCell;
        bool isDestinationReached = true;
        Pathfinder2D pathfinder;

        void Start()
        {
            var weightedTilemap = GetWeightedTilemap();
            pathfinder = new Pathfinder2D(weightedTilemap);
        }

        void Update()
        {
            if ((Input.GetMouseButtonDown(0)) & (isDestinationReached))
            {
                OnMouseDown();
            }

            if (isDestinationReached)
            {
                var cursorPosition = camera.ScreenToWorldPoint(Input.mousePosition);
                var cursorCell = (Vector2Int)pathTilemap.WorldToCell(cursorPosition);
                var playerCell = (Vector2Int)pathTilemap.WorldToCell(player.gameObject.transform.position);
                pathfinder.FindPath(playerCell, cursorCell, ref path, out var result);
                pathTilemap.ClearAllTiles();
                ShowPath();
                UpdateDistance(result.distance);
                UpdateWeightedDistance(result.weightedDistance);
            }

            if (!isDestinationReached)
            {
                Vector3 targetPosition = mazeTileMap.CellToWorld((Vector3Int)currentTagetCell);
                player.gameObject.transform.position = Vector3.MoveTowards(player.gameObject.transform.position, targetPosition, playerSpeed * Time.deltaTime);

                if (Vector3.Distance(player.gameObject.transform.position, targetPosition) <= 0.1f)
                {
                    player.gameObject.transform.position = targetPosition;
                    NextPathPoint();
                }
            }
        }

        Dictionary<Vector2Int, float> GetWeightedTilemap()
        {
            var result = new Dictionary<Vector2Int, float>();
            for (int x = mazeTileMap.origin.x; x < mazeTileMap.origin.x + mazeTileMap.size.x; x++)
            {
                for (int y = mazeTileMap.origin.y; y < mazeTileMap.origin.y + mazeTileMap.size.y; y++)
                {
                    TileBase tile = mazeTileMap.GetTile(new Vector3Int(x, y, 0));
                    if (tile)
                    {
                        if ((tile.name == "Floor") | (tile.name == "Floor_HEX"))
                        {
                            result.Add(new(x, y), 1.0f);
                        }
                        else if ((tile.name == "Floor2") | (tile.name == "Floor2_HEX"))
                        {
                            result.Add(new(x, y), difficultTerrainWeight);
                        }
                    }
                }
            }

            return result;
        }

        void ShowPath()
        {
            foreach (var cell in path)
            {
                pathTilemap.SetTile((Vector3Int)cell, pathTile);
            }

            if (path.Count > 0)
            {
                pathTilemap.SetTile((Vector3Int)path[0], targetTile);
            }
        }

        void UpdateDistance(float newDistance)
        {
            distanceText.text = newDistance.ToString();
        }

        void UpdateWeightedDistance(float newDistance)
        {
            weightedDistanceText.text = newDistance.ToString();
        }


        // Start is called before the first frame update
        void OnMouseDown()
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var start = (Vector2Int)mazeTileMap.WorldToCell(player.gameObject.transform.position);
            var end = (Vector2Int)mazeTileMap.WorldToCell(mousePosition);
            pathfinder.FindPath(start, end, ref path, out var result);
            isDestinationReached = false;
            NextPathPoint();
        }

        void NextPathPoint()
        {
            if (path.Count > 0)
            {
                currentTagetCell = path[^1];
                path.RemoveAt(path.Count - 1);
            }
            else
            {
                isDestinationReached = true;
            }
        }
    }
}