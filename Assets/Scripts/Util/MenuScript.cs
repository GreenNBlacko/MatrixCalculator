using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
	[Header("Values")]
	public string TitleText;
	public List<Menu> Menus = new List<Menu>();

	[Header("Assets")]
	public TMP_Text Title;
	public RectTransform ItemList;

	public void Start () {
		Title.text = TitleText;

		foreach(RectTransform child in ItemList) {
			Destroy(child.gameObject);
		}

		for(int i = 0; i < Menus.Count; i++) {
			var menu = Menus[i];

			var menuButton = new GameObject(menu.name);

			var tr = menuButton.AddComponent<RectTransform>();
			var im = menuButton.AddComponent<Image>();
			var le = menuButton.AddComponent<LayoutElement>();
			var bu = menuButton.AddComponent<Button>();

			tr.SetParent(ItemList);
			tr.localScale = Vector3.one;
			im.color = new Color(1, 1, 1, 0.07f);
			le.minHeight = 95;
			bu.onClick.AddListener(delegate { LoadMenu(Menus.IndexOf(menu)); });

			var text = new GameObject("Text");

			var trt = text.AddComponent<RectTransform>();
			var tet = text.AddComponent<TextMeshProUGUI>();

			trt.SetParent(tr);
			trt.localScale = Vector3.one;
			trt.anchorMin = Vector2.zero;
			trt.anchorMax = Vector2.one;
			trt.pivot = Vector3.one / 2;
			trt.sizeDelta = Vector2.one;
			tet.text = menu.name;
			tet.raycastTarget = false;
			tet.alignment = TextAlignmentOptions.Center;
			tet.fontSize = 36;
		}
	}

	public void LoadMenu(int i) {
		foreach(Menu menu in Menus) {
			menu.asset.SetActive(false);
		}

		Menus[i].asset.SetActive(true);
	}
}

[System.Serializable]
public class Menu {
	public string name;
	public GameObject asset;

	public Menu() { }

	public Menu(string name) { 
		this.name = name;
	}

	public Menu(GameObject asset) { 
		this.name = asset.name;
		this.asset = asset;
	}
}

