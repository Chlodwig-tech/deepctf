using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum biomType
{
    grass,
    accelerate,
    desert,
    lake
}

public class Biom
{
    public int counter = 0;
    public int chance;
    public GameObject prefab;
    public GameObject tiles;
    public biomType type;
    public Biom(int chance, GameObject prefab, GameObject tiles, biomType type)
    {
        this.chance = chance;
        this.prefab = prefab;
        this.tiles = tiles;
        this.type = type;
    }

    public int MaxTilesAmount(int mapSize)
    {
        return mapSize * chance / 100;
    }
    
}

public class Tile
{
    public int xCenter;
    public int yCenter;
    public int tilesMapListIndex;
    public biomType biom = biomType.grass;
    public int domain = -1;

    public Tile(int x, int y, int i)
    {
        xCenter = x;
        yCenter = y;
        tilesMapListIndex = i;
    }

    public void changeBiom(biomType b)
    {
        biom = b;
    }
}

public class Domain : IEnumerable<Tile>
{
    public float sizeInRelationToMapBesidesLakes;
    public int domain = -1;
    public List<Tile> tilesList = new List<Tile>();

    public Domain(int domainNumber)
    {
        domain = domainNumber;
    }

    public void Add(Tile t)
    {
        tilesList.Add(t);
    }
   
    public int Count
    {
        get { return tilesList.Count; }
    }

    public float Chance
    {
        get { return sizeInRelationToMapBesidesLakes * 100; }
    }

    // indeksator
    public Tile this[int i]
    {
        get { return tilesList[i]; }
        set { tilesList[i] = value; }
    }

    // enumerators
    // For IEnumerable<Tile>
    public IEnumerator<Tile> GetEnumerator()
    {
        return tilesList.GetEnumerator(); 
    }

    // For IEnumerable
    IEnumerator IEnumerable.GetEnumerator() 
    {
        return GetEnumerator();
    }

}

public class PerlinNoiseMapGeneration : MonoBehaviour
{
    public GameObject lakePrefab;
    public GameObject lakeTiles;
    public GameObject acceleratePrefab;
    public GameObject accelerateTiles;
    public GameObject desertPrefab;
    public GameObject desertTiles;
    public GameObject blueBase;
    public GameObject redBase;
    public GameObject bases;

    public int lakesPercentage = 20;
    public int accelerateSurfacePercentage = 10;
    public int desertsPercentage = 35;
    private int nextDomain = 0;
    private static GameObject blueBasePrefab;
    private static GameObject redBasePrefab;
    private static GameObject basesPrefab;
    private static GameObject newBlueBase;
    private static GameObject newRedBase;
    private static int height;
    private static int width;
    private int TileScale;
    private int mapSize;
    private static List<Tile> tiles;
    private static List<Domain> domains;
    private static List<Domain> noLakeDomains;
    private int noLakeTilesCounter = 0;

    public const int chanceEnhacerAdder = 5;

    private void Awake()
    {
        blueBasePrefab = blueBase;
        redBasePrefab = redBase;
        basesPrefab = bases;
        GameObject field = gameObject;
        TileScale = (int)lakePrefab.transform.localScale.x;
        height = (int)field.transform.localScale.x * 10; // 360
        width = (int)field.transform.localScale.z * 10; // 400
        tiles = new List<Tile>();
        domains = new List<Domain>();
        noLakeDomains = new List<Domain>();
        mapSize = (height * width) / (TileScale * TileScale);
        lakesPercentage = PlayerPrefs.GetInt("lakesPercent");
        accelerateSurfacePercentage = PlayerPrefs.GetInt("accSurfacesPercent");
        desertsPercentage = PlayerPrefs.GetInt("desertsPercent");
        newBlueBase = null;
        newRedBase = null;
    }


    // Start is called before the first frame update
    void Start()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        AiTrainer.SetPercentages(ref lakesPercentage, ref accelerateSurfacePercentage, ref desertsPercentage);

        GenerateMap();
        FindDomains();
        FindNoLakeDomains();
        PlaceBases();

        sw.Stop();
        UnityEngine.Debug.Log("Seconds = " + sw.Elapsed);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenerateMap()
    {
        FillTilesListWithGrassTiles();
        var tilesCopy = new List<Tile>(tiles);
        List<Biom> bioms = CreateBiomList();
        float scale = Random.Range(2f, 3f);
        foreach (Biom biom in bioms)
        {
            if (biom.chance == 0)
            {
                continue;
            }
            float offsetX = Random.Range(0f, 99999f);   // offsets for Perlin Noise Function
            float offsetY = Random.Range(0f, 99999f);

            int chanceEnhancer = -(biom.chance / 3);  // value added to chances for placing perticular biom tile

            bool notEnoughTiles = true;             // if more tiles should be placed
            while (notEnoughTiles)
            {
                // iterating in reverse, so I can remove a tile from list(tiles) in a run time
                for (int i = tilesCopy.Count - 1; i >= 0; i--)
                {
                    // PERLIN NOISE MAP GENERATION
                    float sample = GetPerlinNoiseSampleForTile(tilesCopy[i], scale, offsetX, offsetY) * 100;    // *100 since we need percents

                    if (sample < biom.chance + chanceEnhancer)
                    {
                        PutTileInGame(tilesCopy[i], biom);
                        tilesCopy.RemoveAt(i);  // delete Tile from List, so it won't be considered in next loop
                        biom.counter++;
                        if (biom.counter >= biom.MaxTilesAmount(mapSize))
                        {
                            notEnoughTiles = false;
                            break;
                        }
                    }
                }
                chanceEnhancer += chanceEnhacerAdder;
            }
        }
    }

    void FillTilesListWithGrassTiles()
    {
        for (int current_tile_height = (height / 2 - height) + (TileScale / 2), i = 0; current_tile_height < height / 2; current_tile_height += TileScale)
        {
            for (int current_tile_width = (width / 2 - width) + (TileScale / 2); current_tile_width < width / 2; current_tile_width += TileScale, i++)
            {
                tiles.Add(new Tile(current_tile_height, current_tile_width, i));
            }
        }
    }

    List<Biom> CreateBiomList()
    {
        Biom lakes = new(lakesPercentage, lakePrefab, lakeTiles, biomType.lake);
        Biom accelerate = new(accelerateSurfacePercentage, acceleratePrefab, accelerateTiles, biomType.accelerate);
        Biom desert = new(desertsPercentage, desertPrefab, desertTiles, biomType.desert);
        return new List<Biom> { accelerate, desert, lakes };
    }

    float GetPerlinNoiseSampleForTile(Tile tile, float scale, float offsetX, float offsetY)
    {
        float sample = Mathf.PerlinNoise(
                        (float)tile.xCenter / height * scale + offsetX,
                        (float)tile.yCenter / width * scale + offsetY);
        return sample;
    }

    void PutTileInGame(Tile tile, Biom biom)
    {
        tile.changeBiom(biom.type);
        GameObject newTile = Instantiate(biom.prefab, new Vector3(tile.xCenter, 0, tile.yCenter), Quaternion.identity);
        newTile.transform.SetParent(biom.tiles.transform);
    }

    // using FloodFill algorithm
    void FindDomains()
    {
        bool somethingAdded = false;
        Domain domain = new Domain(-1);        // never used object, just because below in for loop there is a need to make it
        Queue<int> queue = new Queue<int>();

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].domain == -1)
            {
                somethingAdded = true;
                domain = new Domain(nextDomain);
                addToDomain(queue, i, domain);
            }

            while (queue.Count > 0)
            {
                int next = queue.Dequeue();

                if (tiles[next].domain == -1)
                {
                    addToDomain(queue, next, domain);
                }
            }
            if (somethingAdded)
            {
                somethingAdded = false;
                domains.Add(domain);       // add list of tiles that have the same domain, to domains list
                nextDomain++;
            }
        }
    }

    void addToDomain(Queue<int> queue, int index, Domain domain)
    {
        tiles[index].domain = nextDomain;
        domain.Add(tiles[index]);
        addNeighborsToQueue(queue, index);
    }

    void addToQueueIfMatches(Queue<int> queue, int index, int indexChange)
    {
        if ((tiles[index].biom == biomType.lake && tiles[index + indexChange].biom == biomType.lake)
            || (tiles[index].biom != biomType.lake && tiles[index + indexChange].biom != biomType.lake)
            && (tiles[index + indexChange].domain == -1))
        {
            queue.Enqueue(index + indexChange);
        }
    }

    void addNeighborsToQueue(Queue<int> queue, int startingIndex)
    {
        // zast¹piæ wyliczonymi sta³ymi

        // for far left tiles
        if ((startingIndex - 99) % 100 == 0)
        {
            addToQueueIfMatches(queue, startingIndex, -1);
        }
        // for far right tiles
        else if (startingIndex % 100 == 0)
        {
            addToQueueIfMatches(queue, startingIndex, 1);
        }
        else
        {
            addToQueueIfMatches(queue, startingIndex, -1);
            addToQueueIfMatches(queue, startingIndex, 1);
        }

        // for far up tiles
        if (startingIndex >= 8900)
        {
            addToQueueIfMatches(queue, startingIndex, -100);
        }
        // for far down tiles
        else if (startingIndex <= 99)
        {
            addToQueueIfMatches(queue, startingIndex, 100);
        }
        else
        {
            addToQueueIfMatches(queue, startingIndex, -100);
            addToQueueIfMatches(queue, startingIndex, 100);
        }
    }

    void FindNoLakeDomains()
    {
        foreach (Domain domain in domains)
        {
            if (domain.tilesList[0].biom != biomType.lake)
            {
                noLakeDomains.Add(domain);
                noLakeTilesCounter += domain.Count;
            }
        }
        CheckForDomains();
    }

    // Checks if there are any domains with possibility to place flags left, if not, reloads the scene
    void CheckForDomains()
    {
        if (noLakeDomains.Count == 0)
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name); // RELOAD THE SCENE
            UnityEngine.Debug.Log("!! !!!  ! ! ! ! !!!     SCEENE RELOADED BUT GOES FURTHER!! ! !! !!!!   !! ! ! ! ! ! !!");
            //Start();
            return;
        }
    }

    public static List<Tile> GetPossibleBaseLocationsInDomain(Domain domain)
    {
        List<Tile> possibleBaseLocations = new List<Tile>();

        foreach (Tile t in domain)
        {
            // zast¹piæ wyliczonymi sta³ymi
            int i = t.tilesMapListIndex;

            if (((i - 99) % 100 == 0) || // for far left tiles
                    (i % 100 == 0) ||   // for far right tiles
                    (i >= 8900) ||      // for far up tiles 
                    (i <= 99))          // for far down tiles
            {
                continue;
            }
            else
            {
                // check if 3x3 has the same domain
                if ((tiles[i].domain == tiles[i + 1].domain) &&         // left
                        (tiles[i].domain == tiles[i - 1].domain) &&     // right
                        (tiles[i].domain == tiles[i + 100].domain) &&   // up
                        (tiles[i].domain == tiles[i - 100].domain) &&   // down
                        (tiles[i].domain == tiles[i - 101].domain) &&   // down right
                        (tiles[i].domain == tiles[i - 99].domain) &&    // down left
                        (tiles[i].domain == tiles[i + 101].domain) &&   // up right
                        (tiles[i].domain == tiles[i + 99].domain))      // up left
                {
                    possibleBaseLocations.Add(t);
                }
            }
        }

        return possibleBaseLocations;
    }


    private Domain IfNoDomainsGenerateNewMap()
    {
        CheckForDomains();

        foreach (Domain domain in noLakeDomains)
        {
            domain.sizeInRelationToMapBesidesLakes = (float)domain.Count / noLakeTilesCounter;
        }
        Domain domainNow = null;
        int choice = Random.Range(0, 100);
        float chanceSum = 0f;

        foreach (Domain domain in noLakeDomains)
        {
            chanceSum += domain.Chance;
            if (chanceSum >= choice)
            {
                domainNow = domain;
                break;
            }
        }
        // to make sure there are no problems with false float rounding
        if (domainNow == null)
        {
            domainNow = noLakeDomains[noLakeDomains.Count - 1];
        }

        return domainNow;
    }

    private int GetChoice2(Domain domainNow,List<Tile> possibleBaseLocations,int choice1)
    {
        int choice2 = Random.Range(0, possibleBaseLocations.Count);

        while ((choice1 == choice2) ||
                (Vector3.Distance(
                    new Vector3(possibleBaseLocations[choice1].xCenter, 0, possibleBaseLocations[choice1].yCenter),
                    new Vector3(possibleBaseLocations[choice2].xCenter, 0, possibleBaseLocations[choice2].yCenter)) < 4))       // minimum 4 distance between bases
        {
            choice2 = Random.Range(0, domainNow.Count);
        }
        return choice2;
    }

    void PlaceBases()
    {
        Domain domainNow;
        List<Tile> possibleBaseLocations;

        while (true)
        {
            // If no domains, generate new map
            domainNow = IfNoDomainsGenerateNewMap(); // other function name?

            // Check for 3x3 possible spaces in selected domain
            // Later, boundry tiles can be excluded from this search        -- PERFORMANCE THING
            possibleBaseLocations = GetPossibleBaseLocationsInDomain(domainNow);

            if(!(possibleBaseLocations.Count < 2))
                break;

            noLakeDomains.Remove(domainNow);
            noLakeTilesCounter -= domainNow.Count;

        }

        // random 3x3 space 
        // MAYBE PLACE FOR BETTER ALGORITHM

        int choice1 = Random.Range(0, possibleBaseLocations.Count);
        int choice2 = GetChoice2(domainNow, possibleBaseLocations, choice1);

        // place them
        SetBlueBaseOnTile(possibleBaseLocations[choice1]);
        SetRedBaseOnTile(possibleBaseLocations[choice2]);

    }

    void PlaceBases2()
    {
    // CHOOSE WHAT DOMAIN TO PLACE BASES INTO
    chooseDomain:                       // GOTO

        // If no domains, generate new map
        Domain domainNow = IfNoDomainsGenerateNewMap();

        // Check for 3x3 possible spaces in selected domain
        // Later, boundry tiles can be excluded from this search        -- PERFORMANCE THING
        List<Tile> possibleBaseLocations = GetPossibleBaseLocationsInDomain(domainNow);

        if (possibleBaseLocations.Count < 2)
        {
            noLakeDomains.Remove(domainNow);
            noLakeTilesCounter -= domainNow.Count;
            goto chooseDomain;
        }

        // random 3x3 space 
        // MAYBE PLACE FOR BETTER ALGORITHM

        int choice1 = Random.Range(0, possibleBaseLocations.Count);
        int choice2 = GetChoice2(domainNow, possibleBaseLocations, choice1);
                    
        // place them
        SetBlueBaseOnTile(possibleBaseLocations[choice1]);
        SetRedBaseOnTile(possibleBaseLocations[choice2]);
    }

    public static List<Tile> GetTilesList()
    {
        return tiles;
    }
    public static int GetWidth()
    {
        return width;
    }
    public static int GetHeight()
    {
        return height;
    }

    public static List<Domain> GetNoLakeDomains()
    {
        return noLakeDomains;
    }

    public static void SetBlueBaseOnTile(Tile centerTile)
    {
        Destroy(newBlueBase);
        newBlueBase = Instantiate(blueBasePrefab, new Vector3(centerTile.xCenter, 0, centerTile.yCenter), blueBasePrefab.transform.rotation);
        newBlueBase.GetComponent<BlueBaseScript>().CenterTile = centerTile;
        newBlueBase.transform.SetParent(basesPrefab.transform);
    }

    public static void SetRedBaseOnTile(Tile centerTile)
    {
        Destroy(newRedBase);
        newRedBase = Instantiate(redBasePrefab, new Vector3(centerTile.xCenter, 0, centerTile.yCenter), redBasePrefab.transform.rotation);
        newRedBase.GetComponent<RedBaseScript>().CenterTile = centerTile;
        newRedBase.transform.SetParent(basesPrefab.transform);
    }

}
