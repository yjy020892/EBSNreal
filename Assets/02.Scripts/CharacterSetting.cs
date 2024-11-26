using SPVR.Model;
using SPVR.Network;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 본인 아바타 스킨 설정 및 유저에게 공유
/// </summary>
public class CharacterSetting : SyncBehaviour
{
    [Header("Student(Boy)")]
    public GameObject[] boyHairStyle;
    public Texture2D[] boyHairColorsTexture;
    public GameObject[] boyHairAcc;
    public GameObject[] boyFaceAcc;
    public Texture2D[] boyTopTexture;
    public GameObject[] boyTopStyle;

    [Header("Student(Girl)")]
    public GameObject[] girlHairStyle;
    public Texture2D[] girlHairColorsTexture;
    public GameObject[] girlHairAcc;
    public GameObject[] girlFaceAcc;
    public Texture2D[] girlTopTexture;
    public GameObject[] girlTopStyle;

    private string nameStr;
    public Text nameText;

    void Start()
    {
        SetDirty();

        if (gameObject.name.Equals(SPVRNetwork.Instance.Me.Number.ToString()))
        {
            if (CharacterInfo.Job.Equals("Student"))
            {
                SetSkin();

                if (!string.IsNullOrEmpty(CharacterInfo.Name))
                {
                    nameStr = CharacterInfo.Name;
                }
                else
                {
                    nameStr = "학습자";
                }
            }
            else if(CharacterInfo.Job.Equals("Teacher"))
            {
                nameStr = "교육자";
            }

            nameText.text = nameStr;

            Transform[] tran = gameObject.GetComponentsInChildren<Transform>();
            foreach (Transform t in tran)
            {
                t.gameObject.layer = 11;
            }
        }
    }

    /// <summary>
    /// 본인 아바타 스킨 설정
    /// </summary>
    public void SetSkin()
    {
        int hairStyle = CharacterInfo.HairStyle;
        int hairColor = CharacterInfo.HairColor;
        int hairAcc = CharacterInfo.HairAcc;
        int faceAcc = CharacterInfo.FaceAcc;
        int topStyle = CharacterInfo.TopStyle;
        int topShape = CharacterInfo.TopShape;
        
        if (CharacterInfo.Sex.Equals("Man"))
        {
            boyHairStyle[hairStyle].SetActive(true);
            boyHairStyle[hairStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyHairColorsTexture[hairColor]);

            if(!hairAcc.Equals(-1))
            {
                boyHairStyle[hairStyle].SetActive(false);
                boyHairAcc[0].SetActive(true);
                boyHairAcc[0].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyHairColorsTexture[hairColor]);
                boyHairAcc[hairAcc].SetActive(true);
            }

            if(!faceAcc.Equals(-1))
            {
                boyFaceAcc[faceAcc].SetActive(true);
            }

            boyTopStyle[topStyle].SetActive(true);
            boyTopStyle[topStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[topShape]);
        }
        else if(CharacterInfo.Sex.Equals("Woman"))
        {
            girlHairStyle[hairStyle].SetActive(true);
            girlHairStyle[hairStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[hairColor]);

            if(!hairAcc.Equals(-1))
            {
                if(hairAcc < 7)
                {
                    girlHairStyle[hairStyle].SetActive(false);
                    girlHairAcc[hairStyle].SetActive(true);
                    girlHairAcc[hairStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[hairColor]);
                }

                girlHairAcc[hairAcc].SetActive(true);
            }

            if (!faceAcc.Equals(-1))
            {
                girlFaceAcc[faceAcc].SetActive(true);
            }

            girlTopStyle[topStyle].SetActive(true);
            girlTopStyle[topStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[topShape]);
        }
    }

    public override void SerializeView(BinaryWriter bw)
    {
        bw.Write(CharacterInfo.Job);

        if(CharacterInfo.Job.Equals("Student"))
        {
            bw.Write(gameObject.name);
            bw.Write(CharacterInfo.Sex);
            bw.Write(CharacterInfo.HairStyle);
            bw.Write(CharacterInfo.HairColor);
            bw.Write(CharacterInfo.HairAcc);
            bw.Write(CharacterInfo.FaceAcc);
            bw.Write(CharacterInfo.TopStyle);
            bw.Write(CharacterInfo.TopShape);
        }

        bw.Write(CharacterInfo.Name);
        //bw.Write(SPVRNetwork.Instance.Me.Number);
    }

    public override void DeserializeView(BinaryReader br)
    {
        string job = br.ReadString();

        if(job.Equals("Student"))
        {
            gameObject.name = br.ReadString();
            string sex = br.ReadString();
            int hairStyle = br.ReadInt32();
            int hairColor = br.ReadInt32();
            int hairAcc = br.ReadInt32();
            int faceAcc = br.ReadInt32();
            int topStyle = br.ReadInt32();
            int topShape = br.ReadInt32();

            if (sex.Equals("Man"))
            {
                for (int i = 0; i < boyHairStyle.Length; i++)
                {
                    boyHairStyle[i].SetActive(false);
                    boyTopStyle[i].SetActive(false);
                }
                for (int i = 0; i < boyHairAcc.Length; i++)
                {
                    boyHairAcc[i].SetActive(false);
                }

                boyHairStyle[hairStyle].SetActive(true);
                boyHairStyle[hairStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyHairColorsTexture[hairColor]);

                if (!hairAcc.Equals(-1))
                {
                    boyHairStyle[hairStyle].SetActive(false);
                    boyHairAcc[0].SetActive(true);
                    boyHairAcc[0].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyHairColorsTexture[hairColor]);
                    boyHairAcc[hairAcc].SetActive(true);
                }

                if (!faceAcc.Equals(-1))
                {
                    boyFaceAcc[faceAcc].SetActive(true);
                }

                boyTopStyle[topStyle].SetActive(true);
                boyTopStyle[topStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", boyTopTexture[topShape]);
            }
            else if (sex.Equals("Woman"))
            {
                for (int i = 0; i < girlHairStyle.Length; i++)
                {
                    girlHairStyle[i].SetActive(false);
                    girlTopStyle[i].SetActive(false);
                }
                for (int i = 0; i < girlHairAcc.Length; i++)
                {
                    girlHairAcc[i].SetActive(false);
                }

                girlHairStyle[hairStyle].SetActive(true);
                girlHairStyle[hairStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[hairColor]);

                if (!hairAcc.Equals(-1))
                {
                    if (hairAcc < 7)
                    {
                        girlHairStyle[hairStyle].SetActive(false);
                        girlHairAcc[hairStyle].SetActive(true);
                        girlHairAcc[hairStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlHairColorsTexture[hairColor]);
                    }

                    girlHairAcc[hairAcc].SetActive(true);
                }

                if (!faceAcc.Equals(-1))
                {
                    girlFaceAcc[faceAcc].SetActive(true);
                }

                girlTopStyle[topStyle].SetActive(true);
                girlTopStyle[topStyle].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", girlTopTexture[topShape]);
            }
        }

        string nameStr = br.ReadString();
        nameText.text = nameStr;
    }
}
