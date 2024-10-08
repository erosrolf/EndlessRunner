using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EndlessRunner
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int _tileStartCount = 10;
        [SerializeField] private int _minimumStraightTiles = 3;
        [SerializeField] private int _maximumStraightTiles = 15;
        [SerializeField] private GameObject _startingTile;
        [SerializeField] private List<GameObject> _turnTiles;
        [SerializeField] private List<GameObject> _obstacles;

        private Vector3 _currentTileLocation = Vector3.zero;
        private Vector3 _currentTileDirection = Vector3.forward;
        private GameObject _previousTile;

        private List<GameObject> _currentTiles;
        private List<GameObject> _currentObstacles;

        private void Start()
        {
            _currentTiles = new List<GameObject>();
            _currentObstacles = new List<GameObject>();
            
            Random.InitState(System.DateTime.Now.Millisecond);

            for (int i = 0; i < _tileStartCount; ++i)
            {
                SpawnTile(_startingTile.GetComponent<Tile>(), false);
            }
            
            SpawnTile(SelectRandomGemaObjectFromList(_turnTiles).GetComponent<Tile>());
        }

        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(_currentTileDirection, Vector3.up);
            
            _previousTile = GameObject.Instantiate(tile.gameObject, _currentTileLocation, newTileRotation);
            _currentTiles.Add(_previousTile);

            if (spawnObstacle)
            {
                SpawnObstacle();
            }

            if (tile.type == TileType.STRAIGHT)
            {
                _currentTileLocation +=
                    Vector3.Scale(_previousTile.GetComponent<Renderer>().bounds.size, _currentTileDirection);
            }
        }

        public void AddNewDirection(Vector3 direction)
        {
            _currentTileDirection = direction;
            DeletePreviousTiles();
            DeletePreviousObstacles();

            Vector3 tilePlacementScale;
            if (_previousTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
            {
                tilePlacementScale = Vector3.Scale(_previousTile.GetComponent<Renderer>().bounds.size / 2 + (Vector3.one * _startingTile.GetComponent<BoxCollider>().size.z / 2), _currentTileDirection);
            }
            else
            {
                tilePlacementScale = Vector3.Scale((_previousTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2)) + (Vector3.one * _startingTile.GetComponent<BoxCollider>().size.z / 2), _currentTileDirection);
            }

            _currentTileLocation += tilePlacementScale;

            int currentPathLength = Random.Range(_minimumStraightTiles, _maximumStraightTiles);
            for (int i = 0; i < currentPathLength; ++i)
            {
                SpawnTile(_startingTile.GetComponent<Tile>(), (i == 0) ? false : true);
            }

            SpawnTile(SelectRandomGemaObjectFromList(_turnTiles).GetComponent<Tile>(), false);
            SpawnTile(_turnTiles[0].GetComponent<Tile>());
            AddNewDirection(Vector3.left);
        }

        private GameObject SelectRandomGemaObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }

        private void SpawnObstacle()
        {
            if (Random.value > 0.2f) return;

            GameObject obstaclePrefab = SelectRandomGemaObjectFromList(_obstacles);
            Quaternion newObjectRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(_currentTileDirection, Vector3.up);
            GameObject obstacle = Instantiate(obstaclePrefab, _currentTileLocation, newObjectRotation);
            _currentObstacles.Add(obstacle);
        }

        private void DeletePreviousTiles()
        {
            while (_currentTiles.Count > 1)
            {
                GameObject tile = _currentTiles[0];
                _currentTiles.RemoveAt(0);
                Destroy(tile);
            }
        }

        private void DeletePreviousObstacles()
        {
            while (_currentObstacles.Count > 0)
            {
                GameObject obstacle = _currentObstacles[0];
                _currentTiles.RemoveAt(0);
                Destroy(obstacle);
            }
        }
    }
}