using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    //game settings
    public static int NODE_NUMBER = 4;
    public static int LAYERS = 4;

    public string startingLetter = "A";
    public string endingLetter = "Z";

    public static int MAX_HEALTH = 30;

    //Prefabs
    public Node nodePrefab;

    private List<Node> nodeList = new List<Node>();

    public Text scoreText;
    public Slider healthBar;
    public GameObject overpanel;
    public Text highscoreText;


    //background/camera
    public Camera camera;
    public GameObject background;

    //related to the game
    private bool gameOver = false;
    private bool roundOver = true;
    private bool inBetween = false;
    private int inBetweenTime = 0;
    private int score = 0; //score
    private double health = MAX_HEALTH;
    private List<string> roundPath; //correct path
    private List<string> playerPath = new List<string>(); //path of the player
 

    // Use this for initialization
    void Start()
    {
        if(!PlayerPrefs.HasKey("highscore"))
        {
            PlayerPrefs.SetInt("highscore", 0);
        }

        highscoreText.text = "Highscore: " + PlayerPrefs.GetInt("highscore");

        healthBar.maxValue = MAX_HEALTH;
        healthBar.value = MAX_HEALTH;
        newGraph();
    }

    //main function, creates all

    private void newGraph()
    {
        NODE_NUMBER = Random.Range(3, 8);
        LAYERS = Random.Range(3, 8);
        playerPath = new List<string>();
        playerPath.Add(startingLetter);
        roundOver = false;
        randomBackgroundColor();
        createGraph();

    }

    private void createGraph()
    {
        //destroys any
        if (nodeList.Count != 0)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                Destroy(nodeList[i].gameObject);
            }
        }

        nodeList = new List<Node>();

        //creates the nodes
        Node start = Instantiate(nodePrefab) as Node;
        start.Initialize(-2, (1 + (2 - NODE_NUMBER)) / (float)2, getChar(65));
        start.name = startingLetter;
        byte[] startColor = { 0, 255, 0 };
        start.setColor(startColor);

        nodeList.Add(start);

        //middle

        string[,] grid = new string[LAYERS, NODE_NUMBER]; //grid of the nodes letters

        for (int i = 0; i < LAYERS; i++)
        {
            byte[] layerRGB = { (byte)Random.RandomRange(0, 255), (byte)Random.RandomRange(0, 255), (byte)Random.RandomRange(0, 255) };
            for (int j = 0; j < NODE_NUMBER; j++)
            {
                Node m = Instantiate(nodePrefab) as Node;

                string firstLetter = getChar(66 + i);
                string secondLetter = getChar(65 + j);

                //getChar(66 + (i * NODE_NUMBER + j))

                m.Initialize(i - 1, 1 - j, firstLetter + secondLetter);
                m.name = firstLetter + secondLetter;
                m.setColor(layerRGB);
                nodeList.Add(m);
                grid[i, j] = firstLetter + secondLetter;
            }
        }

        //end

        Node end = Instantiate(nodePrefab) as Node;
        end.Initialize(-1 + LAYERS, (1 + (2 - NODE_NUMBER)) / (float)2, getChar(90));
        end.name = endingLetter;
        byte[] endColor = { 255, 0, 0 };
        end.setColor(endColor);
        nodeList.Add(end);


        //now creates all the lines

        //start
        List<Vector2> allPositions = new List<Vector2>();
        List<string> stringPositions = new List<string>();
        for (int i = 0; i < NODE_NUMBER; i++)
        {
            stringPositions.Add(nodeList[1+i].getLetter()); //adds the nodes in first layer after start
        }

        for (int i = 0; i < stringPositions.Count; i++)
        {
            allPositions.Add(getPosFromLetter(stringPositions[i]));
        }

        nodeList[0].createLines(allPositions, stringPositions, true);


        //middle
        for (int i = 0; i < LAYERS; i++)
        {
            for (int j = 0; j < NODE_NUMBER; j++)
            {
                allPositions = new List<Vector2>(); //positions on screen of where line ends
                stringPositions = new List<string>(); //letter values


                if (i != LAYERS - 1) //makes sure it isn't the last layer
                {
                    allPositions.Add(getPosFromLetter(grid[i + 1, j])); //adds right
                    stringPositions.Add(grid[i + 1, j]);
                }
                else //last layer, all connects to final node
                {
                    allPositions.Add(getPosFromLetter(endingLetter));
                    stringPositions.Add(endingLetter);
                }


                if (j != NODE_NUMBER - 1) //adds the bottom
                {
                    allPositions.Add(getPosFromLetter(grid[i, j + 1]));
                    stringPositions.Add(grid[i, j + 1]);
                }

                nodeList[(i * NODE_NUMBER + j) + 1].createLines(allPositions, stringPositions, true); //creates the lines
            }
        }

        Dictionary<string, Dictionary<string, int>> graph = new Dictionary<string, Dictionary<string, int>>(); //graph of connectors

        //now gets the list of connecting
        for (int i = 0; i < nodeList.Count; i++)
        {
            List<string> letters = nodeList[i].getConnecting(); //letters of those it is connecting to
            List<int> lineWeights = nodeList[i].getLineWeights(); //weights of the lines
            Dictionary<string, int> connectors = new Dictionary<string, int>(); //connectors dictionary

            for (int z = 0; z < letters.Count; z++)
            {
                connectors.Add(letters[z], lineWeights[z]);
            }

            graph.Add(nodeList[i].getLetter(), connectors); //adds to main
        }

        Dictionary<string, string> path = dijkstra(startingLetter, endingLetter, graph);

        //formats path
        List<string> actualPath = new List<string>();
        actualPath.Add(endingLetter);
        string current = endingLetter;

        while (current != startingLetter)
        {
            Debug.Log(current);
            actualPath.Add(path[current]);
            current = path[current];
        }

        actualPath.Reverse();

        roundPath = actualPath;

        //highlightLines(actualPath);

        //setst he camera
        setCamera();
    }

    private void setCamera()
    {
        float x = ((nodeList[0].transform.position.x + nodeList[nodeList.Count - 1].transform.position.x) / 2) - 1.0f;
        float y = ((nodeList[1].transform.position.y + nodeList[NODE_NUMBER].transform.position.y) / 2);
        camera.transform.position = new Vector3(x, y, -10);

        float size = (NODE_NUMBER * LAYERS) / 5.5f;
        
        if(NODE_NUMBER > LAYERS)
        {
            size = 0.45f * NODE_NUMBER + 0.15f;
        } else
        {
            size = 0.45f * LAYERS + 0.15f;
        }

        camera.orthographicSize = size;
    }


    //supporting functions
    private Vector2 getPosFromLetter(string letter) //returns the position on screen based on letter
    {
        Vector2 pos = new Vector2(0, 0);
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].getLetter() == letter)
            {
                pos = nodeList[i].transform.position;
            }
        }
        return pos;
    }


    private string getChar(int i) //returns ascii based on value
    {
        return (char)i + "";
    }

    private Dictionary<string, string> dijkstra(string start, string end, Dictionary<string, Dictionary<string, int>> graph) //solves the problem
    {
        Dictionary<string, int> distance = new Dictionary<string, int>(); //distance from all nodes to start
        List<string> known = new List<string>(); //list of those already visited
        Dictionary<string, string> path = new Dictionary<string, string>(); //closest node from any node

        //sets up dummy values
        foreach (var item in graph)
        {
            string key = item.Key;

            if (key.Equals(start))
            {
                distance.Add(key, 0);
            }
            else
            {
                distance.Add(key, 100000000);
            }
        }

        while (!checkIn(known, end))
        { //as long as the end hasn't been visited yet
            string vertex = minimumDistance(distance, known); //gets the closest vertex not yet visited
            known.Add(vertex); //now visited

            Dictionary<string, int> connectingNodes = graph[vertex]; //all the nodes connecting

            foreach (var item in connectingNodes)
            {
                int current_distance = getDistance(distance, item.Key); //current distance from starting to node
                int vertex_distance = getDistance(distance, vertex); //current distance of the node being checked
                int connectingDistance = graph[vertex][item.Key]; //distance between two nodes

                if (current_distance > vertex_distance + connectingDistance) //if the current distance to the node is greater than the distance to the node being checked plus the gap in between
                {
                    distance[item.Key] = vertex_distance + connectingDistance; //new distance
                    path[item.Key] = vertex;  //new connector
                }
            }

        }
        return path;

    }

    private int getDistance(Dictionary<string, int> distance, string letter) //returns the distance of any node from the starting
    {
        foreach (var item in distance)
        {
            if (item.Key.Equals(letter))
            {
                return item.Value;
            }
        }
        return -1;
    }


    private string minimumDistance(Dictionary<string, int> distance, List<string> known)
    {
        int min = -1; //minimum distance
        string letter = ""; //letter
        int starting = -1; //value

        //lists of the keys and values
        List<string> keys = new List<string>();
        List<int> values = new List<int>();
        foreach (var item in distance)
        {
            keys.Add(item.Key);
            values.Add(item.Value);
        }

        foreach (var item in distance) //gets the first letter to start with
        {
            starting++;
            if (!checkIn(known, item.Key)) //makes sure it hasn't been checked yet
            {
                min = item.Value;
                letter = item.Key;
                break;
            }
        }

        for (int i = starting; i < distance.Count; i++) //finds the minimum
        {
            if (values[i] < min && !checkIn(known, keys[i]))
            {
                min = values[i];
                letter = keys[i];
            }
        }

        return letter;

    }

    private bool checkIn(List<string> arr, string letter)
    {
        foreach (string c in arr)
        {
            if (c.Equals(letter))
            {
                return true;
            }
        }
        return false;
    }

    private void highlightLines(List<string> goodP)
    {
        for (int i = 0; i < goodP.Count - 1; i++)
        {
            string letter = goodP[i];
            for (int j = 0; j < nodeList.Count; j++)
            {
                if (nodeList[j].getLetter().Equals(letter))
                {
                    nodeList[j].highlightLine(goodP[i + 1], "red");
                }
            }

        }
    }

    private void randomBackgroundColor()
    {
        List<int> values = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            values.Add(i);
        }

        byte[] rgb = new byte[3];

        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, values.Count);
            int spot = values[index];

            values.RemoveAt(index);
            if (i == 0)
            {
                rgb[spot] = 255;
            }
            else if (i == 1)
            {
                rgb[spot] = 174;
            }
            else
            {
                rgb[spot] = (byte)Random.Range(174, 255);
            }
        }

        SpriteRenderer color = background.GetComponent<SpriteRenderer>();

        color.color = new Color32(rgb[0], rgb[1], rgb[2], 255);

    }

    private bool isConnected(string letter, string letter2)
    {
        bool found = false;
        for (int i = 0; i < nodeList.Count; i++)
        {
            if(nodeList[i].getLetter().Equals(letter))
            {
                List<string> connecting = nodeList[i].getConnecting();
               
                for(int j = 0; j < connecting.Count; j++)
                {
                    if(connecting[j].Equals(letter2))
                    {
                        found = true;
                        break;
                    }
                }
                break;
            }
        }
        return found;
    }

    private int calculateScore(List<string> path)
    {
        int score = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            score += getLineWeight(path[i], path[i + 1]);
        }
        return score;
    }

    private int getLineWeight(string letter1, string letter2) //returns the weight of the line connecting two letters
    {
        for(int i = 0; i < nodeList.Count; i++)
        {
            if(nodeList[i].getLetter().Equals(letter1)) //correct node
            {
                return nodeList[i].getValueOfLine(letter2); //returns value
            }
        }
        return -10000000; //defualt
    }

    private void selectedLine(string A, string B) //makes a line green
    {
        for(int i = 0; i < nodeList.Count; i++)
        {
            if(nodeList[i].getLetter().Equals(A))
            {
                nodeList[i].highlightLine(B, "green");
            }
        }
    }


    public void Update()
    {

        if (!gameOver) //makes sure the game isn't over
        {

            if (!roundOver && !inBetween) //check for user input
            {
                //user input
                if (Input.GetMouseButtonDown(0))
                {

                    string letter = "";

                    for (int i = 0; i < nodeList.Count; i++) //checks for being selected
                    {
                        if(nodeList[i].getSelected())
                        {
                            letter = nodeList[i].getLetter();
                            break;
                        }

                    }


                    //deselects all
                    for (int i = 0; i < nodeList.Count; i++) //checks for being selected
                    {
                        nodeList[i].deselect();
                    }

                    if (!letter.Equals("")) //a node was selected
                    {
                        if (letter.Equals(startingLetter)) //first letter
                        {
                            if (playerPath.Count == 0)
                            {
                                playerPath.Add(startingLetter);
                            }

                        }
                        else if (playerPath.Count != 0)//everything else
                        {
                            string lastLetter = playerPath[playerPath.Count - 1]; //gets previous letter
                            if (isConnected(lastLetter, letter)) //is connected
                            {
                                playerPath.Add(letter);
                                selectedLine(lastLetter, letter);
                                if (letter.Equals(endingLetter))
                                { //round is over if it was the last letter
                                    roundOver = true;
                                    inBetween = true;
                                }

                            }
                        }
                    }

                }
            }
            else if (inBetween)//round over
            {

                if (inBetweenTime == 0)
                {
                    //calculate the score
                    int optimalScore = calculateScore(roundPath);
                    int playerScore = calculateScore(playerPath);

                    score += 1;

                    //update highscore

                    if(score > PlayerPrefs.GetInt("highscore"))
                    {
                        PlayerPrefs.SetInt("highscore", score);
                    }

                    Debug.Log(score);

                    highscoreText.text = "Highscore: " + PlayerPrefs.GetInt("highscore");

                    health = health - (playerScore - optimalScore);
                    if (health <= 0)
                    {
                        gameOver = true;
                    }
                    scoreText.text = "Score: " + score;
                    healthBar.value = (int)health;
                    Debug.Log((int)health);

                }
                else if (inBetweenTime == 100) //over
                {

                    inBetweenTime = -1;
                    inBetween = false;

                }
                else //in between 1-99
                {
                    //show actual path
                    highlightLines(roundPath);
                }
                inBetweenTime++;
            }
            else
            {
                //shows a new graph
                newGraph();
            }
        } else //game over
        {
            overpanel.SetActive(true);

            if(Input.GetMouseButtonDown(0)) //play again
            {
                inBetweenTime = -1;
                inBetween = false;
                gameOver = false;
                overpanel.SetActive(false);

                roundOver = false;

                score = 0; //score
                health = MAX_HEALTH;

                healthBar.value = (float)health;
                scoreText.text = "Score: " + 0;

                newGraph();

            }
        
        }
    } 
    
}






