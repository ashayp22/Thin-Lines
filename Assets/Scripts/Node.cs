using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    //relating to where it connects
    private List<string> connecting = new List<string>(); //letters of what it connects to

    //letter
    private string letter;

    //actual line
    public Line linePrefab;
    private List<Line> LineList = new List<Line>();

    private bool selected = false;

    public void Initialize(float xPos, float yPos, string name)
    {
        transform.position = new Vector2(xPos, yPos);
        letter = name;

    }
	
    //creates the lines
    public void createLines(List<Vector2> pos, List<string> names, bool random) //parameter is the positions of the lines
    {
        connecting = names;
        for(int i = 0; i < pos.Count; i++) //creates lines with new weights
        {
            Line line = Instantiate(linePrefab) as Line;

            if(random)
            {
                line.Initialize(Random.Range(1, 10), transform.position, pos[i]);

            } else
            {
                line.Initialize(10, transform.position, pos[i]);
            }

            line.transform.parent = this.transform;
            LineList.Add(line);
        }
    }

    public string getLetter() //returns its letter
    {
        return letter;
    }

    public List<string> getConnecting()
    {
        return connecting;
    }

    public List<int> getLineWeights()
    {
        List<int> vals = new List<int>();
        for(int i = 0; i < LineList.Count; i++)
        {
            vals.Add(LineList[i].getWeight());
        }
        return vals;
    }
	

    public void highlightLine(string letter, string color)
    {
        int index = -1;
        for(int i = 0; i < connecting.Count; i++)
        {
            if(connecting[i] == letter)
            {
                index = i;
                break;
            }
        }
        byte[] rgb = new byte[3];

        if (color.Equals("red"))
        {
            rgb[0] = 255;
            rgb[1] = 0;
            rgb[2] = 0;
        } else if (color.Equals("green"))
        {
            rgb[0] = 0;
            rgb[1] = 255;
            rgb[2] = 0;
        }

        LineList[index].changeColor(rgb);
    }

    public void setColor(byte[] rgb)
    {
        SpriteRenderer color = GetComponent<SpriteRenderer>();
        color.color = new Color32(rgb[0], rgb[1], rgb[2], 255);
    }

    public int getValueOfLine(string letter) //returns the weight of a line
    {
        for(int i = 0; i < connecting.Count; i++)
        {
            if(letter.Equals(connecting[i])) //correct letter
            {
                return LineList[i].getWeight();//returns weight
            }
        }
        return -1000000; //default
    }

    //for selecting
    public void OnMouseDown()
    {
        selected = true;
    }

    public bool getSelected()
    {
        return selected;
    }

    public void deselect()
    {
        selected = false;
    }

}
