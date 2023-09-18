﻿using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SupplyRaid
{
    [System.Serializable]
    public class SR_CharacterPreset
    {
        [Tooltip("(REQUIRED) Name of this character")]
        public string name = "Character Name";
        [Tooltip("Short explanation of this character"), Multiline(6)]
        public string description = "Put a brief explination of the character here";
        [Tooltip("(REQUIRED) The menu category this character will go in, Recommend mod creator names etc")]
        public string category = "Mod";
        [Tooltip("Points player receives per capture, endless reuses the last array position")]
        public int[] pointsLevel = new int[1];

        [Tooltip("Cost of a new Magazine")]
        public int newMagazineCost = 1;
        [Tooltip("Cost of upgrading a magazine")]
        public int upgradeMagazineCost = 2;
        [Tooltip("Cost of Duplicating a magazine")]
        public int duplicateMagazineCost = 1;
        [Tooltip("How many points the players get for recycling weapons")]
        public int recyclerPoints = 1;

        //JSON string refference
        public string factionName = "";
        public List<string> startGearCategories = new List<string>();
        [Tooltip("(REQUIRED) What buy categories are available to this character")]
        public List<SR_PurchaseCategory> purchaseCategories = new List<SR_PurchaseCategory>();


        //PRIVATE after we create the functions
        [Tooltip("Preview image of the character when selected")]
        private Sprite thumbnail;
        private int factionIndex = -1;
        private int[] startGearIndex;

        private string thumbnailPath = "";


        public void ExportJson()
        {
            using (StreamWriter streamWriter = new StreamWriter(Application.dataPath + "/SR_Character_" + name + ".json"))
            {
                string json = JsonUtility.ToJson(this, true);
                streamWriter.Write(json);
            }
        }

        public void SetupCharacterPreset(List<SR_ItemCategory> items, List<SR_SosigFaction> factions)
        {
            //Debug.Log("Setting up Character Gear for " + name);
            startGearIndex = new int[startGearCategories.Count];

            //Setup Start Gear
            for (int x = 0; x < startGearCategories.Count; x++)
            {
                for (int y = 0; y < items.Count; y++)
                {
                    if (items[y] != null && startGearCategories[x] == items[y].name)
                    {
                        startGearIndex[x] = y;
                        break;
                    }
                }
            }

            //Setup Purchase Categories Indexes
            List<SR_PurchaseCategory> removeCategory = new List<SR_PurchaseCategory>();

            for (int i = 0; i < purchaseCategories.Count; i++)
            {
                for (int y = 0; y < items.Count; y++)
                {
                    if (purchaseCategories[i].itemCategory == items[y].name)
                    {
                        purchaseCategories[i].SetIndex(y);
                        break;
                    }
                }

                if (purchaseCategories[i].GetIndex() == -1)
                {
                    Debug.Log("Supply Raid - Purchase Category " + purchaseCategories[i].name + " could not set item Category");
                    removeCategory.Add(purchaseCategories[i]);
                }
            }

            //Remove any non-functional Purchase category
            for (int i = 0; i < removeCategory.Count; i++)
            {
                purchaseCategories.Remove(removeCategory[i]);
            }

            //Debug.Log("Setting up Character Faction for " + name);
            //Assign the Faction
            for (int i = 0; i < factions.Count; i++)
            {
                if (factions[i] != null && factionName == factions[i].name)
                {
                    factionIndex = i;
                    break;
                }
            }

            //Debug.Log("Finished setup for " + name);
        }

        public void SetupThumbnailPath(string thumbPath)
        {
            thumbnailPath = thumbPath;
        }
        public Sprite Thumbnail()
        {
            if (thumbnailPath == "")
            {
                Debug.LogError("Supply Raid - Thumbnail not defined for character : " + category + "/" + name);
                return null;
            }

            if (thumbnail == null)
                thumbnail = SR_Global.LoadSprite(thumbnailPath);

            if (thumbnail == null)
                return SR_Manager.instance.fallbackThumbnail;

            return thumbnail;
        }

        public SR_SosigFaction Faction()
        {
            if (factionIndex != -1)
                return SR_Manager.instance.factions[factionIndex];

            return null;
        }

        public int StartGearLength()
        {
            return startGearCategories.Count;
        }

        public SR_ItemCategory StartGear(int i)
        {
            if (i != -1)
                return SR_Manager.instance.itemCategories[startGearIndex[i]];

            return null;
        }


        public bool HasRequirement()
        {
            if (name == "")
                return false;
            if (category == "")
                return false;

            return true;
        }

	}

    [System.Serializable]
    public class SR_PurchaseCategory
    {
        private int index = -1;
        public string name;
        public string itemCategory;
        public int cost = 1;

        public void SetIndex(int i)
        {
            index = i;
        }
        public int GetIndex()
        {
            return index;
        }

        public SR_ItemCategory ItemCategory()
        {
            //Debug.Log("Supply Raid - Item Category Check " + index);

            if (index != -1)
            {
                return SR_Manager.instance.itemCategories[index];
            }

            return null;
        }
    }
}