using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardEntity : MonoBehaviour
{

    public enum CardType
    {
        Placeholder,
        Pyramid,
        Draw
    }

    public Image cardBack;
    public Image cardFill;
    public Image cardFace;
    public Image cardOverlay;
    public Image cardOutline;

    public CardType cardType;
    public int cardValue;
    CardData cardData;

    public Button cardButton;

    public Button backButton;
    public Button frontButton;
    public bool isFaceUp = true;

    public bool isSelected = false;


    public void InitCard(string cardSlug, CardType cardType , bool faceDown = false) {
        bool isPlaceholder = string.IsNullOrEmpty(cardSlug);
        this.cardType = cardType;


        if (isPlaceholder) {
            this.cardData = null;
            this.cardValue = -1;
        }
        else { 
            this.cardData = Core.Cards.CardManager.instance.DeckData[cardSlug];
            this.cardValue = cardData.cardValue;
            cardFace.sprite = cardData.cardSprite;
        }

        ToggleFaceUp(!(faceDown || isPlaceholder), true);


    }

    public void ToggleFaceUp(bool state, bool _skip = false) {
        isFaceUp = state;
        

        cardOverlay.enabled = state;

        frontButton.enabled = state;
        backButton.enabled = !state;

        if (state) {
            cardButton = frontButton;
        }
        else {
            cardButton = backButton;
        }

        if(!_skip)
            StartCoroutine(flipCoroutine());
        else {
            VisualFlip(isFaceUp);
        }
    }

    public void VisualFlip(bool state, bool _over = false) {
        if(_over)
            transform.localScale = new Vector3(transform.localScale.x * - 1, 1, 1);

        cardFill.enabled = state;
        cardFace.enabled = state;
    }

    public void SetSelected(bool state) {
        isSelected = state;
        cardOutline.gameObject.SetActive(state);
    }

    public CardData getCardData() {
        return cardData;
    }

    public void HideCard() {
        cardFace.enabled = false;
        cardBack.enabled = false;
        cardOverlay.enabled = false;
        cardFill.enabled = false;
        cardOutline.enabled = false;
    }

    IEnumerator flipCoroutine() {
        float ogRot = transform.localEulerAngles.y;
        float goalRot = ogRot + 180f;
        float maxTime = 1f;
        float t = 1f;

        bool flipped = false;

        while(t > 0) {
            

            

            t -= Time.deltaTime;

            float p = 1-(t / maxTime);

            float newY = Mathf.Lerp(ogRot, goalRot, p);

            if (newY - ogRot >= 90 && !flipped) {
                VisualFlip(isFaceUp, true);
                flipped = true;
            }

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, newY, transform.localEulerAngles.z);
            yield return new WaitForEndOfFrame();
        }

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, goalRot, transform.localEulerAngles.z);


    }

}
