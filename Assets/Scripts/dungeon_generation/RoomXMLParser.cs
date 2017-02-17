using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class RoomXMLParser {

	private string path;
	private XmlDocument xmlDoc;
	private TextAsset textXml;
	private List<RoomTemplate> roomTemplates;

	private List<Tuple<int, int>> doorAnchorsNorth;
	private List<Tuple<int, int>> doorAnchorsEast;
	private List<Tuple<int, int>> doorAnchorsSouth;
	private List<Tuple<int, int>> doorAnchorsWest;

	public List<RoomTemplate> getRoomTemplatesDictFrom(string filename) {
		if (filename != null && filename != "") {
			loadXMLFromAssest(filename);
			readXml();
			return roomTemplates;
		}
		return null;
	}

	private void loadXMLFromAssest(string filename) {
		xmlDoc = new XmlDocument();
		if(System.IO.File.Exists(getPath(filename))) {
			xmlDoc.LoadXml(System.IO.File.ReadAllText(getPath(filename)));
		} else {
			textXml = (TextAsset)Resources.Load(filename, typeof(TextAsset));
			xmlDoc.LoadXml(textXml.text);
		}
	}

	private void readXml() {
		roomTemplates = new List<RoomTemplate>();
		foreach(XmlElement node in xmlDoc.SelectNodes("rooms/room")) {
			int width = int.Parse(node.GetAttribute("width"));
			int height = int.Parse(node.GetAttribute("height"));
			string type = node.SelectSingleNode("type").InnerText;
			string template = node.SelectSingleNode("template").InnerText;

			TileType[,] tileTypeMap = getTileMapFrom(template, width, height);

			RoomTemplate roomTp = new RoomTemplate(width, height, type, tileTypeMap, doorAnchorsNorth, doorAnchorsEast, doorAnchorsSouth, doorAnchorsWest);
			roomTemplates.Add(roomTp);

//			Debug.Log(roomTp.type + 
//				" North: "+doorAnchorsNorth.Count+
//				" East: "+doorAnchorsEast.Count+
//				" South: "+doorAnchorsSouth.Count+
//				" West: "+doorAnchorsWest.Count);
		}
	}

	private TileType[,] getTileMapFrom(string template, int width, int height) {
		doorAnchorsNorth = new List<Tuple<int, int>>();
		doorAnchorsEast = new List<Tuple<int, int>>();
		doorAnchorsSouth = new List<Tuple<int, int>>();
		doorAnchorsWest = new List<Tuple<int, int>>();

		TileType[,] tileTypeMap = new TileType[width,height];
		string[] linesTemp = template.Split(new char[]{});
		List<string> lines = new List<string>();
		// Remove empty lines
		foreach (var line in linesTemp) {
			if (line == "")
				continue;
			lines.Add(line);
		}

		for (int y = height-1; y >= 0; y--) {
			int trueY = height-1-y;
			char[] chars = lines[trueY].ToCharArray();
			for (int x = 0; x < width; x++) {
				tileTypeMap[x,y] = getTileTypeFor(chars[x]);
				if (chars[x].ToString() == "D") {
					if(x < 2) {
						doorAnchorsWest.Add(new Tuple<int, int>(x, y));
					} else if (x < width-2) {
						if (y < 2) {
							doorAnchorsSouth.Add(new Tuple<int, int>(x, y));
						} else if (y >= height-2) {
							doorAnchorsNorth.Add(new Tuple<int, int>(x, y));
						}
					} else {
						doorAnchorsEast.Add(new Tuple<int, int>(x, y));
					}
				}
			}
		}

		return tileTypeMap;
	}

	private TileType getTileTypeFor(char symbol) {
		TileType type;
		switch (symbol.ToString()) {
		case "D": // Door anchor flag
		case "d": // door flag
		case "#": // Wall
			type = TileType.wallBottom;
			break;
		case "-": // Floor
			type = TileType.floor;
			break;
		case "~": // Empty
			type = TileType.empty;
			break;
		default:
			type = TileType.empty;
			break;
		}
		return type;
	}

	private string getPath(string filename) {
		#if UNITY_EDITOR
		return Application.dataPath +"/Resources/"+filename;
		#else
		return Application.dataPath +"/"+ fileName;
		#endif
	}
}
