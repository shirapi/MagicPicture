﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class FilmManager : MonoBehaviour {

    [SerializeField] private int   kMaxFilm;
    [SerializeField] private int   kMaxPhantom;
    [SerializeField] private float kCoordinateUnit;
    [SerializeField] private float kPhantomDistance;
    [SerializeField] private float kScaleThreshold_StoM_per;
    [SerializeField] private float kScaleThreshold_MtoL_per;
    [SerializeField] private float kScaleRatio_S;
    [SerializeField] private float kScaleRatio_M;
    [SerializeField] private float kScaleRatio_L;
    [SerializeField] private float kFilmSpace_z;

    const int kFilmSpace_x = 100;
    const int kFilmSpace_y = 5000;

    public struct Film
    {
        public GameObject obj;
        public RawImage image;
        public float offset_y;
        public float rot_y;
        public float scale;
        //撮影時のプレイや―の姿勢
        public float playerRot_y { get; set; }

        public void ResetPos(int filmIndex)
        {
            obj.transform.position = new Vector3(
                kFilmSpace_x * filmIndex,
                kFilmSpace_y,
                0.0f);
        }
    }

    //Magicameにセットしてもらう
    private float maxDistance = 0;
    public  float MaxDistance
    {
        set { maxDistance = value; }
    }

    private Film[]       films       = null;
    private GameObject[] phantoms    = null;
    private GameObject   silhouette  = null;
    private GameObject   player      = null;
    private GameObject   magicame    = null;
    private Camera       photoUICamera = null;
    private int currentFilmNum    = 0;
    private int prevFilmNum       = 0;
    private bool isPhantomMode    = false;

    void Start()
    {
        //RawImage img = new RawImage();
        this.films    = new Film[kMaxFilm];
        this.phantoms = new GameObject[kMaxPhantom];

        player        = GameObject.Find("Player");
        magicame      = player.transform.Find("FPSCamera").gameObject;
        photoUICamera = GameObject.Find("PhotoUICamera").GetComponent<Camera>();

        //ほかに書き方あるかも
        RawImage[] images;
        images = this.transform.Find("Canvas/photo").gameObject.GetComponentsInChildren<RawImage>();
        for (int i = 0; i < images.Length; i++)
        {
            films[i].image = images[i];
        }
    }

    void Update()
    {
        //フィルム選択の更新
        UpdateCurrentFilmNum();

        //PhantomModeの変更
        if (Input.GetButtonDown("ForSilhouetteMode"))
        {
            this.isPhantomMode = !this.isPhantomMode;
            if (!this.isPhantomMode)
            {
                films[currentFilmNum].ResetPos(currentFilmNum);
            }
            else
            {
                ChangeSilhouette(films[currentFilmNum].obj);
            }
        }

        if (this.isPhantomMode)
        {
           if(prevFilmNum != currentFilmNum)
            {
                ChangeSilhouette(films[currentFilmNum].obj);
            }

            UpdateSilhouette();

            prevFilmNum = currentFilmNum;
        }

        //現像
        if (Input.GetButtonDown("ForDevelopPhantom") && this.isPhantomMode)
        {
            if(silhouette.GetComponent<ObjectAttribute>().CanPhantom) DevelopPhantom();
        }

        //写真、オブジェクトの回転
        if (Input.GetButtonDown("ForRotatePicture"))
        {
            
        }
    }

    private void DevelopPhantom()
    {
        AddPhantom(this.films[this.currentFilmNum].obj);

        //追加時の各設定
        this.phantoms[0].GetComponent<ObjectAttribute>().Taken();
        this.phantoms[0].transform.parent = null;
    }

    //@param 撮影のray
    //@param 撮影時のrayのスタート位置
    public void Take(RaycastHit filmingObj ,Vector3 rayOrigin)
    {
        if (this.films[this.currentFilmNum].obj != null)
        {
            Destroy(this.films[this.currentFilmNum].obj);
        }

        //offset,scale,rotの設定
        {
            //offset
            this.films[this.currentFilmNum].offset_y = filmingObj.point.y - filmingObj.collider.gameObject.transform.position.y;

            //scale
            {
                float distance = filmingObj.distance;
                if (distance <= maxDistance * kScaleThreshold_StoM_per * 0.01)
                {
                    this.films[this.currentFilmNum].scale = kScaleRatio_L;
                }
                else if (distance <= maxDistance * kScaleThreshold_MtoL_per * 0.01)
                {
                    this.films[this.currentFilmNum].scale = kScaleRatio_M;
                }
                else
                {
                    this.films[this.currentFilmNum].scale = kScaleRatio_S;
                }
            }

            //rotation
            {
                //もう少しいい書き方あるかも
                float dx = rayOrigin.x - filmingObj.collider.gameObject.transform.position.x;
                float dz = rayOrigin.z - filmingObj.collider.gameObject.transform.position.z;
                float deg = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
                if (deg <= 22.5f || 338.5f < deg)
                {
                    this.films[this.currentFilmNum].rot_y = 0.0f;
                }
                else if (deg <= 67.5f)
                {
                    this.films[this.currentFilmNum].rot_y = 45.0f;
                }
                else if (deg <= 112.5)
                {
                    this.films[this.currentFilmNum].rot_y = 90.0f;
                }
                else if (deg <= 157.5)
                {
                    this.films[this.currentFilmNum].rot_y = 135.0f;
                }
                else if (deg <= 202.5f)
                {
                    this.films[this.currentFilmNum].rot_y = 180.0f;
                }
                else if (deg <= 247.5f)
                {
                    this.films[this.currentFilmNum].rot_y = 225.0f;
                }
                else if (deg <= 292.5f)
                {
                    this.films[this.currentFilmNum].rot_y = 270.0f;
                }
                else if (deg <= 337.5f)
                {
                    this.films[this.currentFilmNum].rot_y = 315.0f;
                }
            }
        }

        //生成
        {
            this.films[this.currentFilmNum].obj = Instantiate(filmingObj.collider.gameObject);

            this.films[this.currentFilmNum].ResetPos(currentFilmNum);

            //this.films[this.currentFilmNum].obj.transform.SetParent(player.transform, true);

            this.films[this.currentFilmNum].obj.transform.localScale *= this.films[this.currentFilmNum].scale;

            //this.films[this.currentFilmNum].obj.transform.localRotation *= Quaternion.Euler(new Vector3(
            //    0.0f,
            //    this.films[this.currentFilmNum].rot_y,
            //    0.0f)) *
            //    Quaternion.Inverse(player.transform.localRotation);
        }

        //スクショ
        {
            //todo 別スクリプトへ
            //カメラの移動
            Vector3 pos;
            pos.x = kFilmSpace_x * currentFilmNum;
            pos.y = kFilmSpace_y;
            pos.z = kFilmSpace_z;
            photoUICamera.transform.position = pos;
            
            StartCoroutine("CreatePhoto");
        }
    }

    IEnumerator CreatePhoto()
    {
        yield return new WaitForEndOfFrame();

        int width = (int)films[this.currentFilmNum].image.rectTransform.rect.width;
        int height = (int)films[this.currentFilmNum].image.rectTransform.rect.height;

        // アクティブなレンダーテクスチャをキャッシュしておく
        RenderTexture currentRT = RenderTexture.active;

        RenderTexture renderTexture = new RenderTexture(width, height, 24);

        // アクティブなレンダーテクスチャを一時的にTargetに変更する
        RenderTexture.active = renderTexture;

        photoUICamera.targetTexture = renderTexture;

        photoUICamera.Render();

        Texture2D photo = new Texture2D(width, height);
        photo.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        photo.Apply();

        films[this.currentFilmNum].image.texture = photo;
    }

    //@param 変更するオブジェクト
    private void ChangeSilhouette(GameObject obj)
    {
        if (silhouette != null)
        {
            //todo シルエットをリセットできるように書く
            films[currentFilmNum].ResetPos(currentFilmNum);
            silhouette.transform.parent = null;
            silhouette.transform.localRotation *= Quaternion.Inverse(Quaternion.Euler(new Vector3(
                0.0f,
                this.films[this.currentFilmNum].rot_y,
                0.0f)));
            silhouette.GetComponent<Collider>().isTrigger = false;
        }

        //変更
        silhouette = obj;

        if (silhouette != null)
        {
            silhouette.transform.SetParent(player.transform, true);
            this.films[this.currentFilmNum].obj.transform.localRotation = this.films[this.currentFilmNum].obj.transform.localRotation * Quaternion.Euler(new Vector3(
                0.0f,
                this.films[this.currentFilmNum].rot_y,
                0.0f));
            //this.films[this.currentFilmNum].obj.transform.localRotation = this.films[this.currentFilmNum].obj.transform.localRotation * player.transform.localRotation;
            //silhouette.transform.parent = player.transform;
            silhouette.GetComponent<Collider>().isTrigger = true;
        }
    }

    private void UpdateSilhouette()
    {
        //各定点に移動
        Vector3 pos =
        magicame.transform.position + (magicame.transform.forward.normalized * kPhantomDistance);
        pos.x = ((int)(pos.x / kCoordinateUnit)) * kCoordinateUnit;
        pos.y -= this.films[this.currentFilmNum].offset_y;
        pos.z = ((int)(pos.z / kCoordinateUnit)) * kCoordinateUnit;

        silhouette.transform.position = pos;
    }

    private void UpdateCurrentFilmNum()
    {
        Func<int, int> standardizationFilmNum = nextFilmNum => (nextFilmNum + this.kMaxFilm) % this.kMaxFilm;

        if (Input.GetButtonDown("ForRotatePicture"))
        {
            if (Input.GetAxis("ForRotatePicture") < 0.0f)
            {
                this.currentFilmNum = standardizationFilmNum(--currentFilmNum);
            }
            else if (Input.GetAxis("ForRotatePicture") > 0.0f)
            {
                this.currentFilmNum = standardizationFilmNum(++currentFilmNum);
            }
        }
        Debug.Log(this.currentFilmNum);
    }

    //phantomの要素番号0番に追加
    //@param film
    private void AddPhantom(GameObject film)
    {
        GameObject phantom = Instantiate(
            film,
            film.transform.position,
            film.transform.localRotation * player.transform.localRotation);

        phantom.GetComponent<Collider>().isTrigger = false;

        if (this.phantoms[0] != null)
        {
            GameObject next = phantom; //次に代入するやつ
            GameObject tmp; //一時保管

            int count;
            for (count = 0; count < kMaxPhantom; ++count)
            {
                tmp = this.phantoms[count];
                this.phantoms[count] = next;
                next = tmp;
                if (tmp == null)
                {
                    break;
                }
            }

            Destroy(next);//内部でnullチェックが行われているためnullでもエラー起きない
        }
        else
        {
            this.phantoms[0] = phantom;
        }
    }
}
