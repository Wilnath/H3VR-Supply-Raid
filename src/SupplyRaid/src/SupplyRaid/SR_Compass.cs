﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using H3MP;
using H3MP.Networking;
using FistVR;

namespace SupplyRaid
{

    public class SR_Compass : MonoBehaviour
    {
        [Header("Directions")]
        public Transform supplyPointDirection;
        public Transform lastSupplyDirection;

        [Header("Setup")]
        public Transform face;  //Faces players head
        public Text directionText;
        public Text pointsText;
        public float distance = .25f;
        public Slider healthBar;
        public Text levelText;


        [Header("Networking")]
        public GameObject playerArrowPrefab;
        public int playerCount = 0;
        public List<Transform> playerArrows = new List<Transform>();

        void Update()
        {
            //If There is a manager and we're in gameplay
            if (SR_Manager.instance == null || !SR_Manager.instance.running)
                return;

            //GM.CurrentPlayerBody.transform.position;
            if (!SR_Manager.instance.optionHand)
                transform.position = GM.CurrentPlayerBody.LeftHand.position - GM.CurrentPlayerBody.LeftHand.forward * distance;
            else
                transform.position = GM.CurrentPlayerBody.RightHand.position - GM.CurrentPlayerBody.RightHand.forward * distance;
            transform.rotation = Quaternion.identity;

            face.transform.LookAt(GM.CurrentPlayerBody.Head.position);

            //Direction
            directionText.text = Mathf.FloorToInt(GM.CurrentPlayerBody.Head.rotation.eulerAngles.y).ToString();

            //Last SupplyPoint
            Vector3 pos = SR_Manager.instance.GetLastSupplyPoint().position;
            pos.y = transform.position.y;
            lastSupplyDirection.LookAt(pos);

            //H3MP Setup NETWORKING player points

            healthBar.value = GM.GetPlayerHealth();
            //GM.CurrentSceneSettings.IsSpawnLockingEnabled

            Transform marker = SR_Manager.instance.GetCompassMarker();
            if (!marker)
                return;
            pos = marker.position;
            pos.y = transform.position.y;
            supplyPointDirection.LookAt(pos);

            pointsText.text = SR_Manager.instance.Points.ToString();
            levelText.text = SR_Manager.instance.statCaptures.ToString();

            NetworkUpdate();
        }

        void NetworkUpdate()
        {
            if (Networking.ServerRunning())
                return;

            //Player count updated, update arrows
            if (playerCount != GameManager.players.Count)
            {
                CreatePlayerArrows();
                playerCount = GameManager.players.Count;
            }

            //Update Arrows to look at players
            for (int i = 0; i < GameManager.players.Count; i++)
            {
                Vector3 pos = GameManager.players[i].head.position;
                pos.y = transform.position.y;
                playerArrows[i].transform.LookAt(pos);
            }
        }

        void CreatePlayerArrows()
        {
            //Clear Old Arrows
            for (int i = 0; i < playerArrows.Count; i++)
            {
                Destroy(playerArrows[i].gameObject);
            }

            playerArrows.Clear();

            //DO nothing if single player
            //if (GameManager.players.Count <= 1)
            //    return;

            for (int i = 0; i < GameManager.players.Count; i++)
            {
                GameObject arrow =  Instantiate(playerArrowPrefab, playerArrowPrefab.transform.parent);
                arrow.SetActive(true);
                playerArrows.Add(arrow.transform);
            }

        }
    }
}