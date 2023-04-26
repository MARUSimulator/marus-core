using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadImage : MonoBehaviour
{
    public bool flag = false;
    // Use this for initialization
    void Start () {
        StartCoroutine("load_image");
    }
   
    // Update is called once per frame
    void Update () {
   
    }
       
    IEnumerator load_image()
    {
        string[] filePaths = Directory.GetFiles(@"Dataset", "*.png");
        WWW www = new WWW("file://" + filePaths[0]);               
        yield return www;
             
        while (!www.isDone) { }

        if (www.error==null)
        { 
            flag = true;
            Texture2D new_texture = new Texture2D(4, 4, TextureFormat.DXT1, false);               
            www.LoadImageIntoTexture(new_texture);
            GameObject image = GameObject.Find ("RawImage");                                                             
		    image.GetComponent<RawImage>().texture = new_texture;
        }
    }
}
