using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public Node nodePrefab;

    private List<Node> nodeList = new List<Node>();

    public GameObject background;

	// Use this for initialization
	void Start () {

        for(int i = 0; i < 5; i++)
        {
            Node n = Instantiate(nodePrefab) as Node; ;
            n.Initialize(-6 + (i * 3), -4f, "A");
            n.gameObject.transform.localScale = new Vector3(0.35f, 0.35f, 1);
            byte[] rgb = { (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255) };
            n.setColor(rgb);
            nodeList.Add(n);
        }

        for(int j = 0; j < 4; j++)
        {
            List<Vector2> pos = new List<Vector2>();
            pos.Add(nodeList[j + 1].transform.position);
            List<string> name = new List<string>();
            name.Add("B");
            nodeList[j].createLines(pos, name, true);
        }

        SpriteRenderer s = background.GetComponent<SpriteRenderer>();
        s.color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);

    }
	
}
