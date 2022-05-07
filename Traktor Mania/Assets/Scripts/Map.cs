using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private int sizeX = 30;
    [SerializeField] private int sizeY = 30;
    [SerializeField] private GameObject tilePref;
    [SerializeField] public Material[] materials;
    
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Score")]
    [SerializeField] private Score score;
    private Tile[,] tiles;

    private void Awake()
    {
        tiles = new Tile[sizeX, sizeY];
        score.ChangeMaxValue(sizeX * sizeY);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var newObj = Instantiate(tilePref);
                newObj.name = "TIle " + x +" " + y;
                newObj.transform.parent = transform;
                newObj.transform.position = new Vector3(x, transform.position.y, y);
                
                tiles[x, y] = newObj.GetComponent<Tile>();
                tiles[x, y].renderer.material = materials[0];
            }
        }
    }

    private void Update()
    {
        tiles[Mathf.RoundToInt(player.position.x), Mathf.RoundToInt(player.position.z)].renderer.material = materials[1];
    }

    public void FillArea()
    {
        //for (int x = 0; x < sizeX; x++)
        //{
        //    List<int> startsLine = new List<int>(); //список начальных точек линнии заливки по Y
        //    List<int> endsLine = new List<int>(); //список конечных точек линнии заливки по Y
        //    bool start = false;

        //    for (int y = 0; y < sizeY; y++)
        //    {
        //        if (y == 0 && tiles[x, y].isHighilited && !tiles[x, y + 1].isHighilited)
        //        {
        //            startsLine.Add(y + 1);
        //            start = true;
        //        }
        //        else if (y > 0 && y < sizeY - 1 && tiles[x, y].isHighilited && !tiles[x, y + 1].isHighilited)
        //        {
        //            if (!start)
        //            {
        //                startsLine.Add(y + 1);
        //                start = true;
        //            }
        //            else
        //            {
        //                endsLine.Add(y);
        //                start = false;
        //            }
        //        }
        //        else if (y == sizeY - 1 && startsLine.Count > endsLine.Count && tiles[x, y].isHighilited)
        //            endsLine.Add(y);
        //    }

        //    if (endsLine.Count > 0)
        //        for (int i = 0; i < startsLine.Count; i++)
        //         FillLineY(x, startsLine[i], endsLine[i]);
        //}
        //Debug.Log("Area is fiiled!");
        ////ClearHighilatedTiles();
    }

    private void FillLineY(int x, int start, int end)
    {
        for (int i = start; i < end; i++)
        {
            tiles[x, i].renderer.material = materials[1];
            tiles[x, i].collider.enabled = false;
            score.AddPoint();
        }
    }

    public void HighliteTile(int x, int y)
    {
        tiles[x, y].isHighilited = true;
    }

    private void ClearHighilatedTiles()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if(tiles[x, y].isHighilited)
                {
                    tiles[x, y].isHighilited = false;
                }
            }
        }
    }
}
