using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ShowTimingWindowScale
{

    public class TextBehaviour : MonoBehaviour
    {
	    public GameObject TextObject;
	    public Text text;
	    public Shadow shadowText;
	    public RectTransform rectTransform;
	    public void setSize(int size)
		{
			text.fontSize = size;
			text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
		}

		public void setText(string text)
		{
			this.text.text = text;
			this.text.color = new Color(0.99f, 0.99f, 0.99f);
			//this.text.rectTransform.sizeDelta = new Vector2(this.text.preferredWidth, this.text.preferredHeight);
		}

		public void setPosition(float x, float y)
		{
			Vector2 pos = new Vector2(x, y);
			rectTransform.anchorMin = pos;
			rectTransform.anchorMax = pos;
			rectTransform.pivot = pos;
		}

		public void Awake()
		{
			
			Canvas mainCanvas = gameObject.AddComponent<Canvas>();
			mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			mainCanvas.sortingOrder = 10001;
			CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
			scaler.referenceResolution = new Vector2(1920, 1080);
			
			
			TextObject = new GameObject();
			TextObject.transform.SetParent(transform);
			TextObject.AddComponent<Canvas>();
			rectTransform = TextObject.GetComponent<RectTransform>();
			
			GameObject textObject = new GameObject();
			textObject.transform.SetParent(TextObject.transform);

			text = textObject.AddComponent<Text>();
			text.font = RDString.GetFontDataForLanguage(RDString.language).font;
			text.alignment = toAlign(Main.setting.align);
			text.fontSize = Main.setting.size;
			text.color = Color.white;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;

			shadowText = textObject.AddComponent<Shadow>();
			shadowText.effectColor = new Color(0f, 0f, 0f, 0.4f);
			shadowText.effectDistance = new Vector2(4f, -4f);
			
			Vector2 pos = new Vector2(Main.setting.x, Main.setting.y);
			rectTransform.anchorMin = pos;
			rectTransform.anchorMax = pos;
			rectTransform.pivot = pos;
			
			rectTransform.anchoredPosition = Vector2.zero;
		}
		

		public TextAnchor toAlign(int _align)
        {
			if (_align == 0) return TextAnchor.UpperLeft;
			if (_align == 1) return TextAnchor.UpperCenter;
			return TextAnchor.UpperRight;
		}
		

	}
}
