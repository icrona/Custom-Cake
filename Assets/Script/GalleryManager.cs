﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GalleryManager : MonoBehaviour {
    private string filepath;
    private string connectionString;
    private int galleryNum;
    private Texture2D []tex;
    private int width;
    private int height;
    private int index;
    private string selectedCakeID;
    public Text galleryIndex;
    public Text emptyText;
    public Text nameText;
    public Text sizeText;
    public Text flavourText;
    public Text timeStamp;
    private Sprite[] cakes;
    public GameObject imagePanel;
    public GameObject []cakeImage;
    private bool cakeDetails;
    public GameObject galleryCanvas;
    public GameObject gridCanvas;
    public byte[] img;

    public GameObject cakeImagePrefab;
    public GameObject cakeImagePrefabContainer;
    private Texture2D []prefabTexture;
    private int cakeIndex;
    private List<CakeGallery> cakeData = new List<CakeGallery>();
    void Start()
    {
        galleryNum = 0;
        index = 0;
        cakeIndex = 0;

        filepath = Application.persistentDataPath + "/CakeDB.sqlite";
        connectionString = "URI=file:" + filepath;
        width = 5 * Screen.width / 7;
        height = Screen.height / 2;

        cakeImage = new GameObject[imagePanel.transform.childCount];
        tex = new Texture2D[imagePanel.transform.childCount];
        
        for (int i = 0; i < imagePanel.transform.childCount; i++)
        {
            cakeImage[i] = imagePanel.transform.GetChild(i).gameObject;
            tex[i] = new Texture2D(width, height, TextureFormat.RGB24, false);
        }
        cakeDetails = false;

        loadCakeData();
        showGalleryGrid();
    }
    void loadCakeData(){
        cakeData.Clear();
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT Count (*) From Cake";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        galleryNum = reader.GetInt32(0);
                    }
                }
                if (galleryNum == 0)
                {
                    emptyText.gameObject.SetActive(true);
                }
                
                string sqlQuery1 = "SELECT * FROM Cake";
                dbCmd.CommandText = sqlQuery1;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cakeData.Add(new CakeGallery(reader.GetInt32(0),reader.GetString(1), reader.GetString(2), 
                            reader.GetString(3), reader.GetString(8),(byte[])reader["Image1"], (byte[])reader["Image2"],(byte[])reader["Image3"],(byte[])reader["Image4"]));
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
            showCakeData();
        }
    }

    void showGalleryGrid()
    {
        cakes = new Sprite[galleryNum];
        prefabTexture = new Texture2D[galleryNum];
        for (int i=0;i<galleryNum;i++)
        {
            prefabTexture[i]= new Texture2D(width, height, TextureFormat.RGB24, false);
            prefabTexture[i].LoadImage(cakeData[i].img1);
            cakes[i] = Sprite.Create(prefabTexture[i], new Rect(0, 0, width, height), new Vector2(0f, 0f));

            GameObject container = Instantiate(cakeImagePrefab) as GameObject;
            container.GetComponent<Image>().sprite = cakes[i];
            container.transform.SetParent(cakeImagePrefabContainer.transform, false);

            container.transform.GetChild(1).GetComponent<Text>().text = cakeData[i].nameText;

            container.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => showCakeDetail(container.transform.GetSiblingIndex()));
        }
    }

    void showCakeDetail(int i)
    {
        cakeDetails = true;
        index = i;
        updateGalleryIndex();
        showCakeData();
        galleryCanvas.SetActive(true);
        gridCanvas.SetActive(false);
    }

    void showCakeData()
    {
        if (galleryNum == 0)
        {
            emptyText.gameObject.SetActive(true);
        }
        else if (index < 0)
        {
            index = 0;
        }
        else
        {
            selectedCakeID = cakeData[index].cakeID.ToString();
            nameText.text = cakeData[index].nameText;
            sizeText.text = cakeData[index].sizeText;
            flavourText.text = cakeData[index].flavourText;
            timeStamp.text = cakeData[index].timeStamp;
            tex[0].LoadImage(cakeData[index].img1);
            tex[1].LoadImage(cakeData[index].img2);
            tex[2].LoadImage(cakeData[index].img3);
            tex[3].LoadImage(cakeData[index].img4);
            for (int i = 0; i < imagePanel.transform.childCount; i++)
            {
                cakeImage[i].GetComponent<Image>().sprite = Sprite.Create(tex[i], new Rect(0, 0, width, height), new Vector2(0f, 0f));
                cakeImage[i].SetActive(false);
            }
            cakeImage[cakeIndex].SetActive(true);
        }
    }
    public void deleteCakeData()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();
            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = String.Format("DELETE FROM Cake WHERE ID=\"{0}\"",selectedCakeID);
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteScalar();
                dbConnection.Close();
            }
        }
        Destroy(cakeImagePrefabContainer.transform.GetChild(index).gameObject);
        index--;
        loadCakeData();
        showCakeData();
        backToStart(); 
    }
    public void toggleLeft()
    {
        index--;
        if (index<0)
        {
            index = galleryNum - 1;
        }
        cakeIndex = 0;
        showCakeData();
        updateGalleryIndex();
    }
    public void toggleRight()
    {
        index++;
        if (index==galleryNum)
        {
            index = 0;
        }
        cakeIndex = 0;
        showCakeData();
        updateGalleryIndex();
    }
    private void updateGalleryIndex()
    {
        galleryIndex.text = (index + 1).ToString() + "/" + galleryNum.ToString();
    }
    public void backToStart()
    {
        if (cakeDetails==false)
        {
            SceneManager.LoadScene("StartMenu");
        }
        else
        {
            gridCanvas.SetActive(true);
            galleryCanvas.SetActive(false);
            cakeDetails = false;
        }
       
    }
    public void leftCake()
    {
        cakeImage[cakeIndex].SetActive(false);
        cakeIndex--;
        if (cakeIndex < 0)
        {
            cakeIndex = 3;
        }
        cakeImage[cakeIndex].SetActive(true);
    }
    public void rightCake()
    {
        cakeImage[cakeIndex].SetActive(false);
        cakeIndex++;
        if (cakeIndex == 4)
        {
            cakeIndex = 0;
        }
        cakeImage[cakeIndex].SetActive(true);
    }

    public void getCakeId()
    {
        PlayerPrefs.SetString("selectedCakeID", selectedCakeID);
    }
}
