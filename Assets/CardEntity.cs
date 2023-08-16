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
        Draw,
        Showcase
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

    public bool isRemoved = false;
    public int rowIndex;
    public CardEntity parentA;
    public CardEntity parentB;

    private void Awake() {
        if(cardType == CardType.Showcase && gameObject.activeSelf) {
            ShowcaseLogic();
        }
    }

    public void ShowcaseLogic() {
        frontButton.enabled = false;
        backButton.enabled = false;
        cardOverlay.gameObject.SetActive(false);
        InitCard(GameManager.instance.getRandomCard().cardSlug, CardType.Showcase, false);
        StartCoroutine(showcaseCoroutine());
    }

    bool bgSet = false;

    public void InitCard(string cardSlug, CardType cardType , bool faceDown = false) {
        bool isPlaceholder = string.IsNullOrEmpty(cardSlug);
        this.cardType = cardType;

        if(cardType != CardType.Showcase || !bgSet)
            this.cardBack.sprite = GameManager.instance.getBackground();

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
        bgSet = true;

    }

    private void OnDrawGizmosSelected() {
        if (parentA != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(parentA.transform.position, .1f);
        }
        if (parentB != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(parentB.transform.position, .1f);
        }

        
    }

    public void CheckInteractable() {
        bool state = CanBePaired();
        cardButton.interactable = state;
    }

    public void SetParents(CardEntity a, CardEntity b) {
        parentA = a;
        parentB = b;
    }

    public bool CanBePaired() {
        bool hasParents = parentA != null && parentB != null;
        if(hasParents)
            return (parentA.isRemoved && parentB.isRemoved);
        else {
            return true;
        }
    }

    public void ToggleFaceUp(bool state, bool _skip = false, float flipTime = 1f) {
        isFaceUp = state;
        
        if(cardType != CardType.Showcase) {
            cardOverlay.enabled = state;

            frontButton.enabled = state;
            backButton.enabled = !state;
        }

        

        if (state) {
            cardButton = frontButton;
        }
        else {
            cardButton = backButton;
        }

        if(!_skip)
            StartCoroutine(flipCoroutine(flipTime));
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
        ToggleOutline(state);
    }

    public void CheatOutline(bool state) {
        if (state && cardOutline.color != GameManager.instance.cheatOutline) {
            cardOutline.color = GameManager.instance.cheatOutline;
        }

        if(!state && cardOutline.color != GameManager.instance.cardOutline) {
            cardOutline.color = GameManager.instance.cardOutline;
        }

        ToggleOutline(state);
    }


    public void ToggleOutline(bool state) {
        cardOutline.gameObject.SetActive(state);
    }

    public CardData getCardData() {
        return cardData;
    }

    public void HideCard(bool sendToPair = false, bool removeSource = false) {
        cardFace.enabled = false;
        cardBack.enabled = false;
        cardOverlay.enabled = false;
        cardFill.enabled = false;
        cardOutline.enabled = false;
        isRemoved = true;

        if (sendToPair) {
            CloneCardToPair();
        }

    }

    public void CloneCardToPair() {
        CardEntity cloneCard = Core.Cards.CardManager.instance.CreateCard(CardType.Placeholder, getCardData().cardSlug, GameManager.instance.pyramidTransform.parent, false);
        cloneCard.transform.position = transform.position;

        cloneCard.StartMoveCoroutine(GameManager.instance.pairPileTransform);
    }

    public void StartMoveCoroutine(Transform target) {
        StartCoroutine(moveCoroutine(target));
    }

    IEnumerator flipCoroutine(float time = 1f) {
        float ogRot = transform.localEulerAngles.y;
        float goalRot = ogRot + 180f;
        float maxTime = time;
        float t = time;

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

    IEnumerator moveCoroutine(Transform target) {
        ToggleFaceUp(false, false, .5f);

        Vector2 ogPos = transform.position;
        Vector2 goalPos = target.position;
        float maxTime = 1f;
        float t = 1f;

        bool flipped = false;

        while (t > 0) {




            t -= Time.deltaTime;

            float p = 1 - (t / maxTime);

            Vector3 newPos = Vector3.Lerp(ogPos, goalPos, p);

            transform.position = newPos;
            yield return new WaitForEndOfFrame();
        }

        transform.position = goalPos;
        GameManager.instance.TogglePairPlaceholder(true);
        print("ding");
        Destroy(gameObject);
    }

    IEnumerator showcaseCoroutine() {
        
        InitCard(GameManager.instance.getRandomCard().cardSlug, CardType.Showcase, true);


        float randomTime = Random.Range(1f, 3f);
        yield return new WaitForSeconds(randomTime);

        ToggleFaceUp(true, false, 1f);
        yield return new WaitForSeconds(1f);
        this.cardBack.sprite = GameManager.instance.getBackground();

        randomTime = Random.Range(3f, 7f);
        yield return new WaitForSeconds(randomTime);
        ToggleFaceUp(false, false, 1f);
        yield return new WaitForSeconds(1f);
        
        StartCoroutine(showcaseCoroutine());
    }

}
