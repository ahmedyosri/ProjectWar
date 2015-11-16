using UnityEngine;
using System.Collections;

public class GridVisualizer : MonoBehaviour {

    Color32[] colors;
    Texture2D mTexture;

	// Use this for initialization
	void Start () {
        mTexture = new Texture2D(60, 60);
        GetComponent<Renderer>().material.SetTexture(0, mTexture);
        colors = new Color32[IMapsMgr.inflMapTiles];
        mTexture.SetPixels32(colors);
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void SetColor(Color32[] newColors)
    {
        colors = newColors;
        mTexture.SetPixels32(colors);
        mTexture.Apply();
    }
}
