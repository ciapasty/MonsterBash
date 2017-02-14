using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class RoomXMLParser {

	private string path;
	private XmlDocument xmlDoc;
	private TextAsset textXml;
	private List<RoomTemplate> roomTemplates;

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

			RoomTemplate roomTp = new RoomTemplate(width, height, type, tileTypeMap);
			roomTemplates.Add(roomTp);
		}
	}

	private TileType[,] getTileMapFrom(string template, int width, int height) {
		TileType[,] tileTypeMap = new TileType[width,height];
		string[] linesTemp = template.Split(new char[]{});
		List<string> lines = new List<string>();
		// Remove empty lines
		foreach (var line in linesTemp) {
			if (line == "")
				continue;
			lines.Add(line);
		}

		for (int y = 0; y < height; y++) {
			char[] chars = lines[y].ToCharArray();
			for (int x = 0; x < width; x++) {
				tileTypeMap[x,y] = getTileTypeFor(chars[x]);
			}
		}

		return tileTypeMap;
	}

	private TileType getTileTypeFor(char symbol) {
		TileType type;
		switch (symbol.ToString()) {
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
