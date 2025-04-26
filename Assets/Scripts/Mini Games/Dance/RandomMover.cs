using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMover : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemies = 5;

    [Header("Movement Settings")]
    [SerializeField] private float moveInterval = 1f;
    [SerializeField] private float spacing = 1.4f;

    [Header("Overlap Behavior")]
    [SerializeField] private bool allowSoftOverlap = true;
    [SerializeField] private float overlapOffsetAmount = 0.2f;

    [Header("References")]
    [SerializeField] private ShowPath showPath;

    private int gridRows;
    private int gridCols;
    private Vector3 startPos;

    private Coroutine movementCoroutine;
    private List<GameObject> objectsToMove = new List<GameObject>();
    private Dictionary<Vector2Int, List<GameObject>> gridOccupants = new Dictionary<Vector2Int, List<GameObject>>();

    private void Start()
    {
        SpawnEnemies();
    }

    public void InitializeGridSettings(int rows, int cols, float spacingAmount)
    {
        gridRows = rows;
        gridCols = cols;
        spacing = spacingAmount;
        startPos = new Vector3(-(gridCols - 1) * spacing / 2f, (gridRows - 1) * spacing / 2f, 0f);
    }

    private void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not assigned!");
            return;
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, this.transform);
            objectsToMove.Add(enemy);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        int randomRow = Random.Range(0, gridRows);
        int randomCol = Random.Range(0, gridCols);
        return startPos + new Vector3(randomCol * spacing, -randomRow * spacing, 0f);
    }

    public void StartMoving()
    {
        if (movementCoroutine == null)
            movementCoroutine = StartCoroutine(MoveLoop());
    }

    public void StopMoving()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            MoveObjects();
            yield return new WaitForSeconds(moveInterval);
        }
    }

    private void MoveObjects()
    {
        foreach (var obj in objectsToMove)
        {
            if (obj == null) continue;

            Vector3 pos = obj.transform.position;
            int dir = Random.Range(0, 4);
            Vector3 moveDelta = Vector3.zero;

            switch (dir)
            {
                case 0: moveDelta = new Vector3(0, spacing, 0); break;
                case 1: moveDelta = new Vector3(0, -spacing, 0); break;
                case 2: moveDelta = new Vector3(-spacing, 0, 0); break;
                case 3: moveDelta = new Vector3(spacing, 0, 0); break;
            }

            Vector3 newPos = pos + moveDelta;

            Vector2Int cell = GetGridCell(newPos);
            if (cell.x < 0 || cell.x >= gridCols || cell.y < 0 || cell.y >= gridRows)
                continue;

            if (!showPath.IsSquareOccupied(newPos))
            {
                StartCoroutine(SmoothMove(obj, pos, newPos, 0.3f));
            }
        }
    }

    private IEnumerator SmoothMove(GameObject obj, Vector3 startPos, Vector3 targetPos, float totalDuration)
    {
        if (obj == null) yield break;

        float halfDuration = totalDuration * 0.5f;
        Vector3 halfwayPos = Vector3.Lerp(startPos, targetPos, 0.5f);

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(startPos, halfwayPos, elapsed / halfDuration);
            yield return null;
        }

        Vector3 jitterOffset = new Vector3(
            Random.Range(-spacing * 0.1f, spacing * 0.1f),
            Random.Range(-spacing * 0.1f, spacing * 0.1f),
            0
        );
        Vector3 jitterPos = halfwayPos + jitterOffset;

        elapsed = 0f;
        float jitterDuration = 0.1f;
        while (elapsed < jitterDuration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(halfwayPos, jitterPos, elapsed / jitterDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(jitterPos, targetPos, elapsed / halfDuration);
            yield return null;
        }

        obj.transform.position = targetPos;

        Vector2Int cell = GetGridCell(targetPos);
        if (!gridOccupants.ContainsKey(cell))
            gridOccupants[cell] = new List<GameObject>();
        gridOccupants[cell].Add(obj);

        if (allowSoftOverlap)
        {
            int count = gridOccupants[cell].Count;
            Vector3 offset = Vector3.zero;

            switch (count)
            {
                case 2: offset = new Vector3(overlapOffsetAmount, 0f, 0f); break;
                case 3: offset = new Vector3(0f, overlapOffsetAmount, 0f); break;
                case 4: offset = new Vector3(-overlapOffsetAmount, 0f, 0f); break;
                case 5: offset = new Vector3(0f, -overlapOffsetAmount, 0f); break;
            }

            obj.transform.position += offset;
        }
        else
        {
            if (gridOccupants[cell].Count > 1)
            {
                gridOccupants[cell].Remove(obj);
                obj.transform.position = startPos;
            }
        }
    }

    private Vector2Int GetGridCell(Vector3 position)
    {
        int col = Mathf.RoundToInt((position.x - startPos.x) / spacing);
        int row = Mathf.RoundToInt((startPos.y - position.y) / spacing);
        return new Vector2Int(col, row);
    }
}