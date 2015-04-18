using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TileTex
{
	//This class enables us to define various textures for tiles
	public string       str;
	public Texture2D    tex;
}

[System.Serializable]
public class EnemyDef{
	//this class enables us to define various enemies
	public string			str;
	public GameObject		go;
}

public class LayoutTiles : MonoBehaviour
{
	static public LayoutTiles S;
	public TextAsset        roomsText;//The Rooms.xml file
	public string           roomNumber = "0";//Current room # as a string
	//^roomNumber as string allows encoding in the XML & rooms 0-F
	public GameObject       tilePrefab;//Prefab for all Tiles
	public TileTex[]        tileTextures;//A list of named textures for Tiles
	public GameObject		portalPrefab;//prefab for the portals between rooms
	public EnemyDef[]		enemyDefinitions;//prefab for enemies

	public bool _____________;

	private bool			firstRoom = true;//is this the first room built?
	public PT_XMLReader     roomsXMLR;
	public PT_XMLHashList   roomsXML;
	public Tile[,]          tiles;
	public Transform        tileAnchor;

	void Awake ()
	{
		S = this;//Set the Singleton for LayoutTiles

		//Make a new GameObject to be the TileAnchor (the parent transform of all
		//tiles.) This keeps Tiles tidy in the Hierarchy pane.
		GameObject tAnc = new GameObject ("TileAnchor");
		tileAnchor = tAnc.transform;

		//Read the XMl
		roomsXMLR = new PT_XMLReader ();//Create a PT_XMLReader
		roomsXMLR.Parse (roomsText.text);//Parse the rooms.xml file
		roomsXML = roomsXMLR.xml ["xml"] [0] ["room"];//Pull all the <room>s

		//Build the 0th Room
		BuildRoom (roomNumber);
	}

	//This is the GetTileTex() method that Tile uses
	public Texture2D GetTileTex (string tStr)
	{
		//Search through all the tileTextures for the proper string
		foreach (TileTex tTex in tileTextures) {
			if (tTex.str == tStr) {
				return (tTex.tex);
			}
		}
		//Return null if nothing was found
		return (null);
	}

	//Build a room from an XML <room> entry
	public void BuildRoom (PT_XMLHashtable room)
	{
		//destroy any old tiles
		foreach (Transform t in tileAnchor) {//clear out old tiles
			//you can iterate over a transform to get its children
			Destroy (t.gameObject);
		}

		//Move the PC out of the way
		PC.S.pos = Vector3.left * 1000;
		//this keeps the mage from accidentally triggering OnTriggerExit() on
		//a Portal to keep OnTriggerExit from being called at strange times
		//PC.S.ClearInput ();//Cancel any active mouse inputs

		string rNumStr = room.att ("num");

		//Get the texture names for the floors and walls from <room? attributes
		string floorTexStr = room.att ("floor");
		string wallTexStr = room.att ("wall");
		//Split the room into rows of tiles based on carriage returns in the 
		//Rooms.xml file
		string[] roomRows = room.text.Split ('\n');
		//Trim tabs from the beginnings of lines. However, we're leaving spaces
		//and underscores to allow for non-rectangular rooms
		for (int i = 0; i < roomRows.Length; i++) {
			roomRows[i] = roomRows[i].Trim ('\t');
		}
		//Clear the tiles Array
		tiles = new Tile[100, 100];//Arbitrary max room size is 100x100

		//Declare a number of local fields that we'll use later
		Tile ti;
		string type, rawType, tileTexStr;
		GameObject go;
		int height;
		float maxY = roomRows.Length - 1;
		List<Portal> portals = new List<Portal> ();

		//These loops scan through each tile of each row of the room
		for (int y = 0; y < roomRows.Length; y ++) {
			for (int x = 0; x < roomRows[y].Length; x ++) {
				//Set defaults
				height = 0;
				tileTexStr = floorTexStr;

				//Get the character representing the tile
				type = rawType = roomRows[y][x].ToString ();
				switch (rawType) {
				case " "://empty case
				case "_": //empty space
					//just skip over empty space
					continue;
				case "."://default floor
					//keep type = "."
					break;
				case "|"://default wall
					height = 1;
					break;
				default://Anything else will be interpreted as a floor tile
					type = ".";
					break;
				}
				//set the texture for floor or wall based on <room> attributes
				if (type == ".") {
					tileTexStr = floorTexStr;
				} else if (type == "|") {
					tileTexStr = wallTexStr;
				}
				go = Instantiate (tilePrefab) as GameObject;
				ti = go.GetComponent<Tile> ();
				//Set the parent Transform to tileAnchor
				ti.transform.parent = tileAnchor;
				//Set the position of the tile
				ti.pos = new Vector3 (x, maxY - y, 0);
				tiles [x, y] = ti;//Add ti to the tiles 2d array

				//Set the type, height, and texture of the Tile
				ti.type = type;
				ti.height = height;
				ti.tex = tileTexStr;

				//If the type is still rawType, continue to the next iteration
				if(rawType == type) continue;

				//Check for specific entities in the room
				switch (rawType){
				case "X"://starting position for the mage
					//PC.S.pos = ti.pos;//uses the mage singleton //Obsolete
					if(firstRoom){
						PC.S.pos = ti.pos;//uses the mage singleton
						roomNumber = rNumStr;
						//setting the roomNumber now keeps any portals from
						//moving the mage to them in this first room
						firstRoom = false;
					}
					break;
				case "0"://Numbers are room portals (up to F in hex)
				case "1":
				case "2":
				case "3":
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
				case "A":
				case "B":
				case "C":
				case "D":
				case "E":
				case "F":
					//instantiate a portal
					GameObject pGO = Instantiate (portalPrefab) as GameObject;
					Portal p = pGO.GetComponent<Portal>();
					if (pGO.GetComponent<Portal>() == null)
						print ("HOLY SHIT BRUH!!!11");
					p.pos = ti.pos;
					p.transform.parent = tileAnchor;
					//attaching this to the tileAnchor means that the portal will
					//be destroyed when a new room is built
					p.toRoom = rawType;
					portals.Add(p);
					break;
				default: 
					//Try to see if there's an enemy for that letter
					Enemy en = EnemyFactory(rawType);
					if(en == null) break;
					//set up the new enemy
					en.pos = ti.pos;
					//make en a child of tileAnchor so it's deleted when the next
					//room is loaded
					en.transform.parent = tileAnchor;
					en.typeString = rawType;
					break;
				}
				//More to come here
			}
		}

		//Position the PC
		foreach(Portal p in portals){
			/*if p.toRoom is the same as the room number the PC just exited,
			 * Then the mage should enter this room through this portal
			 * alternatively, if firstRoom == true and there was no X in the 
			 * room (as a default mage starting point, move the mage to this
			 * portal as a backup measure (if, for instance, you want to just 
			 * load room number "5")
			 */
			if(p.toRoom == roomNumber || firstRoom){
				//if there's an X in the room, firstRoom will be set to false
				//by the time the code gets here
				PC.S.StopWalking();
				PC.S.pos = p.pos;//move PC to this portal location
				//mage maintains the facing from the previous room, so there is
				//no need to rotate 
				p.justArrived = true;
				firstRoom = false;//stops a 2nd portal in this room from moving
								  //the mage to it
			}
		}
		//finally assign the room number
		roomNumber = rNumStr;
	}
	// Build a room based on room number. This is an alternative version of
	//  BuildRoom that grabs roomXML based on  num.
	public void BuildRoom (string rNumStr)
	{
		PT_XMLHashtable roomHT = null;
		for (int i=0; i<roomsXML.Count; i++) {
			PT_XMLHashtable ht = roomsXML [i];
			if (ht.att ("num") == rNumStr) {
				roomHT = ht;
				break;
			}
		}
		if (roomHT == null) {
			Utils.tr ("ERROR", "LayoutTiles.BuildRoom()",
						         "Room not found: " + rNumStr);
			return;
		}
		BuildRoom (roomHT);
	}

	public Enemy EnemyFactory (string sType){
		//see if there's an EnemyDef with that sType
		GameObject prefab = null;
		foreach (EnemyDef ed in enemyDefinitions) {
			if (ed.str == sType) {
				prefab = ed.go;
				break;
			}
		}
		if (prefab == null) {
			Utils.tr ("LayoutTiles.EnemyFactory()", "No EnemyDef for: " + sType);
			return (null);
		}
		GameObject go = Instantiate (prefab) as GameObject;

		//the generic form of getcompoenent (with the <>) won't work for 
		//interfaces like Enemy, so we must use this form instead
		Enemy en = (Enemy)go.GetComponent (typeof(Enemy));
		return (en);
	}
}
