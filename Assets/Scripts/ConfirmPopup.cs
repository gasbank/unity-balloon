using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Text = TMPro.TextMeshProUGUI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Subcanvas), typeof(RectTransform))]
public class ConfirmPopup : MonoBehaviour {
    public static ConfirmPopup instance;
    public Text title;
    public Text message;
    public Button button1;
    public Text button1Text;
    public Button button2;
    public Text button2Text;
    public Button claimButton;
    public Text claimButtonText;
    public Button closeButton;
    UnityAction onButton1;
    UnityAction onButton2;
    UnityAction onClaimButton;
    UnityAction onCloseButton;
    public Image claimImage;
    public GameObject decoratedHeader;
    public Text decoratedTitle;
    public Image popupImage;
    public LayoutElement popupImageLayoutElement;
    [SerializeField] Animator topAnimator = null;
    [SerializeField] InputField inputField = null;
    [SerializeField] Text inputFieldPlaceholderText = null;
    [SerializeField] Subcanvas subcanvas = null;
    [SerializeField] Image topImage = null;
    [SerializeField] RectTransform contentTopRect = null;
    [SerializeField] RectTransform popupImageParentRect = null;
    [SerializeField] Graphic skipRenderGraphic = null;
    [SerializeField] int autoCloseRemainSec = -1;
    [SerializeField] Text autoCloseText = null;

    public bool IsOpen { get { return subcanvas.IsOpen; } }

    public string InputFieldText { get { return inputField.text; } }
    int AutoCloseRemainSec {
        get => autoCloseRemainSec;
        set {
            autoCloseRemainSec = value;
            if (autoCloseText != null) {
                autoCloseText.gameObject.SetActive(autoCloseRemainSec > 0);
                if (autoCloseRemainSec > 0) {
                    autoCloseText.text = "\\{0}초후 자동으로 닫힙니다.".Localized(autoCloseRemainSec);
                }
            }
        }
    }

    public enum Header {
        Normal,
        Decorated,
    }

    public enum WidthType {
        Normal = 480,
        Narrow = 380,
    }

    public enum ShowPosition {
        Center,
        Bottom,
    }

    // void OnValidate() {
    //     subcanvas = GetComponent<Subcanvas>();
    // }

    IEnumerator Start() {
        while (true) {
            //SushiDebug.Log("ConfirmPopup.Start coro...");
            yield return new WaitForSeconds(1);
            if (AutoCloseRemainSec > 0 && IsOpen) {
                SushiDebug.Log("ConfirmPopup.Start autoCloseRemainSec - 1...");
                AutoCloseRemainSec--;
                if (AutoCloseRemainSec == 0) {
                    SushiDebug.Log("ConfirmPopup.Start autoCloseRemainSec == 0");
                    if (onCloseButton != null) {
                        SushiDebug.Log("ConfirmPopup.Start onClose");
                        onCloseButton();
                    } else if (onButton1 != null) {
                        SushiDebug.Log("ConfirmPopup.Start onButton1");
                        onButton1();
                    } else if (onButton2 != null) {
                        SushiDebug.Log("ConfirmPopup.Start onButton2");
                        onButton2();
                    } else if (onClaimButton != null) {
                        SushiDebug.Log("ConfirmPopup.Start onClaimButton");
                        onClaimButton();
                    }
                }
            }
        }
    }

    void OpenPopup() {
        topAnimator.SetTrigger("Appear");
    }

    void ClosePopup() {
        // should receive message even if there is nothing to do
    }

    void UpdateTitle(string titleText, Header header) {
        if (header == Header.Normal) {
            title.text = titleText;
            title.gameObject.SetActive(true);
            decoratedHeader.SetActive(false);
        } else {
            title.gameObject.SetActive(false);
            decoratedHeader.SetActive(true);
            decoratedTitle.text = titleText;
        }
    }

    public void OpenYesNoPopup(string msg, UnityAction onYes, UnityAction onNo) {
        OpenYesNoPopup(msg, onYes, onNo, "\\확인".Localized());
    }

    public void OpenPopup(string msg, UnityAction onButton1, UnityAction onButton2, UnityAction onButton3,
        string titleText, Header header, string button1Text, string button2Text, string button3Text,
        string inputFieldText = "", string inputFieldPlaceholder = "", bool showInputField = false,
        Sprite popupSprite = null, Sprite topImageSprite = null,
        WidthType widthType = WidthType.Normal,
        float popupImageTopOffset = 0,
        ShowPosition showPosition = ShowPosition.Center,
        UnityAction onCloseButton = null,
        bool allowTouchEventsOutsidePopup = false,
        int autoCloseSec = -1) {

        // 너비가 넓은 버전(기본 버전) 팝업인지, 좁은 버전인지
        contentTopRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)widthType);

        // 화면 가운데에 나오게 할지 아니면 하단에 나오게 할지
        if (showPosition == ShowPosition.Center) {
            contentTopRect.anchorMin = contentTopRect.anchorMax = contentTopRect.pivot = new Vector2(0.5f, 0.5f);
        } else {
            contentTopRect.anchorMin = contentTopRect.anchorMax = contentTopRect.pivot = new Vector2(0.5f, 0.0f);
        }

        // 팝업 오른쪽 위에 X 표시 버튼 표시할지 말지
        closeButton.gameObject.SetActive(onCloseButton != null);
        this.onCloseButton = onCloseButton;

        // 팝업 뒤에 있는 요소들로 터치 이벤트를 허용할지 말지 결정한다.
        // skipRenderGraphic이 켜져 있으면 이벤트를 허용하지 않는다.
        skipRenderGraphic.enabled = (allowTouchEventsOutsidePopup == false);

        // top image
        topImage.gameObject.SetActive(topImageSprite != null);
        topImage.sprite = topImageSprite;

        UpdateTitle(titleText, header);
        message.text = msg;
        message.gameObject.SetActive(string.IsNullOrEmpty(msg) == false);
        
        //image
        if (popupSprite != null) {
            var popupImageHeight = 80;
            ActivatePopupImage(popupSprite, popupImageTopOffset, popupImageHeight);
        } else {
            popupImageParentRect.gameObject.SetActive(false);
        }

        // Button 1
        if (onButton1 != null) {
            button1.gameObject.SetActive(true);
            this.button1Text.text = button1Text;
        } else {
            button1.gameObject.SetActive(false);
        }
        this.onButton1 = onButton1;
        // Button 2
        if (onButton2 != null) {
            button2.gameObject.SetActive(true);
            this.button2Text.text = button2Text;
        } else {
            button2.gameObject.SetActive(false);
        }
        this.onButton2 = onButton2;
        // Button 3
        if (onButton3 != null) {
            claimButton.gameObject.SetActive(true);
            this.claimButtonText.text = button3Text;
        } else {
            claimButton.gameObject.SetActive(false);
        }
        this.onClaimButton = onButton3;

        if (inputField != null) {
            inputField.gameObject.SetActive(showInputField);
            inputField.text = inputFieldText;
            inputFieldPlaceholderText.text = inputFieldPlaceholder;
        }

        AutoCloseRemainSec = autoCloseSec;

        //gameObject.SetActive(true);
        subcanvas.Open();
    }

    public void ActivatePopupImage(Sprite popupSprite, float popupImageTopOffset, int popupImageHeight) {
        popupImageParentRect.gameObject.SetActive(true);
        popupImage.sprite = popupSprite;
        //popupImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, popupImageTopOffset, popupImageParentRect.rect.height - popupImageTopOffset);
        popupImage.rectTransform.anchorMin = Vector2.zero;
        popupImage.rectTransform.anchorMax = Vector2.one;
        popupImage.rectTransform.anchoredPosition = Vector2.zero;//popupImageParentRect.sizeDelta / 2;
        popupImage.rectTransform.pivot = new Vector2(0.5f, 0.0f); // lower middle
        popupImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, popupImageParentRect.rect.size.x);// .sizeDelta;
        popupImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, popupImageParentRect.rect.size.y + popupImageTopOffset);// .sizeDelta;
        SetPopupImageHeight(popupImageHeight);
    }

    private void SetPopupImageHeight(int popupImageHeight) {
        popupImageLayoutElement.flexibleHeight = popupImageLayoutElement.minHeight = popupImageLayoutElement.preferredHeight = popupImageHeight;
    }

    public void OpenGeneralPopup(string msg, UnityAction onButton1, UnityAction onButton2, UnityAction onButton3, string titleText, Header header, string button1Text, string button2Text, string button3Text) {
        OpenPopup(msg, onButton1, onButton2, onButton3, titleText, header, button1Text, button2Text, button3Text);
    }

    public void OpenTwoButtonPopup(string msg, UnityAction onButton1, UnityAction onButton2, string titleText, string button1Text, string button2Text, Header header = Header.Normal, ShowPosition showPosition = ShowPosition.Center, UnityAction onCloseButton = null, bool allowTouchEventsOutsidePopup = false) {
        OpenPopup(msg, onButton1, onButton2, null, titleText, header, button1Text, button2Text, "", "", "", false, null, null, WidthType.Normal, 0, showPosition, onCloseButton, allowTouchEventsOutsidePopup);
    }

    public void OpenYesNoPopup(string msg, UnityAction onYes, UnityAction onNo, string titleText, Header header = Header.Normal) {
        OpenTwoButtonPopup(msg, onYes, onNo, titleText, "\\예".Localized(), "\\아니요".Localized(), header);
    }

    public void OpenInputFieldPopup(string msg, UnityAction onYes, UnityAction onNo, string titleText, Header header, string inputFieldText, string inputFieldPlaceholder) {
        OpenPopup(msg, onYes, onNo, null, titleText, header, "\\확인".Localized(), "\\취소".Localized(), "", inputFieldText, inputFieldPlaceholder, true);
    }

    public void OpenClaimPopup(string msg, string claimButtonText, UnityAction onClaim, string titleText, Header header = Header.Normal, int autoCloseSec = -1) {
        OpenPopup(msg, null, null, onClaim, titleText, header, "", "", claimButtonText, "", "", false, null, null, WidthType.Normal, 0, ShowPosition.Center, null, false, autoCloseSec);
    }

    public void Open(string msg, UnityAction onYes = null) {
        Open(msg, onYes ?? Close, "\\확인".Localized());
    }

    public void Open(string msg, UnityAction onYes, string titleText, Header header = Header.Normal) {
        OpenPopup(msg, onYes, null, null, titleText, header, "\\확인".Localized(), "", "");
    }
    public void OpenOneButtonPopup(string msg, UnityAction onYes, string titleText,string confirmText, Header header = Header.Normal){
        OpenPopup(msg, onYes, null, null, titleText, header, confirmText, "", "");
    }

    public void OpenYesImagePopup(string titleText, string msg, UnityAction onYes, Sprite sprite) {
        OpenYesImagePopup(titleText, sprite, msg, "\\확인".Localized(), onYes);
    }

    public void OpenYesImagePopup(string titleText, Sprite sprite, string msg, string button1Text, UnityAction onButton1) {
        OpenPopup(msg, onButton1, null, null, titleText, Header.Normal, button1Text, "", "", "", "", false, sprite);
    }
    public void OpenYesNoImagePopup(string textTitle, string msg, UnityAction onYes, Sprite sprite) {
        OpenYesNoImagePopup(textTitle, sprite, msg, "\\받기".Localized(), "\\취소".Localized(), onYes, Close);
    }
     public void OpenYesImagePopup(string titleText, Sprite sprite, string msg, string button1Text, UnityAction onButton1,Header header = Header.Normal) {
        OpenPopup(msg, onButton1, null, null, titleText, header, button1Text, "", "", "", "", false, sprite);
    }

    public void OpenYesNoImagePopup(string titleText, Sprite sprite, string msg, string button1Text, string button2Text, UnityAction onButton1, UnityAction onButton2) {
        OpenPopup(msg, onButton1, onButton2, null, titleText, Header.Normal, button1Text, button2Text, "", "", "", false, sprite);
    }

    public void Close() {
        //gameObject.SetActive(false);
        subcanvas.Close();
    }

    public void OnButton1() {
        onButton1();
    }

    public void OnButton2() {
        onButton2();
    }

    public void OnClaimButton() {
        onClaimButton();
    }

    public void OnCloseButton() {
        onCloseButton();
    }

    public void ActivateTopImage(Sprite sprite) {
        topImage.gameObject.SetActive(true);
        topImage.sprite = sprite;
    }
}
