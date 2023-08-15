using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Core.IManager
{
    public static GameManager instance;

    public Transform pyramidTransform;
    
    public Transform drawPileTransform;
    CardEntity drawPilePlaceholder;
    
    public Transform pairPileTransform;
    CardEntity pairPilePlaceholder;

    public Transform discardPileTransform;
    public CardEntity discardTop;

    public List<CardData> gameDeck = new List<CardData>();
    public List<CardEntity> pyramidCards = new List<CardEntity>();
    public List<CardData> discardPile = new List<CardData>();
    public List<CardData[]> pairPile = new List<CardData[]>();

    public CardEntity selectionA;
    public CardEntity selectionB;


    public override void Awake() {
        base.Awake();

        if (instance == null) {
            instance = this;
        }
        else {
            DuplicateManagerError();
        }

        StartNewGame();
    }

    public void SendToPair(List<CardEntity> cardsToPair) {
        List<CardData> tempData = new List<CardData>();
        for (int x = 0; x < cardsToPair.Count; x++) {
            if (cardsToPair[x] == null)
                continue;

            tempData.Add(cardsToPair[x].getCardData());

            if(cardsToPair[x].cardType == CardEntity.CardType.Pyramid)
                cardsToPair[x].HideCard();
        }



        pairPile.Add(tempData.ToArray());
    }

    public void SelectCard(CardEntity cardToSelect) {
        if (cardToSelect.isSelected) {
            ClearSelection();
            return;
        }
            


        if (selectionA == null) {
            selectionA = cardToSelect;
            selectionA.SetSelected(true);
        }
        else if(selectionB == null) {
            selectionB = cardToSelect;
            selectionB.SetSelected(true);
        }

        int value = CalculateSelection();
        print("Combo Value: " + value);

        if(selectionA && selectionB && value != 13) {
            ClearSelection();
            return;
        }

        if (value == 13) {
            if((selectionA && selectionA.cardType == CardEntity.CardType.Draw) || (selectionB && selectionB.cardType == CardEntity.CardType.Draw)) {
                RemoveTopDiscardCard();
            }

            SendToPair(new List<CardEntity>() { selectionA, selectionB });
            ClearSelection();
        }
    }

    public void ClearSelection() {
        if(selectionA != null)
            selectionA.SetSelected(false);  
        if (selectionB != null)
            selectionB.SetSelected(false);
        selectionA = null;
        selectionB = null;
    }

    public int CalculateSelection() {
        int value = 0;
        if (selectionA != null)
            value += selectionA.cardValue;

        if (selectionB != null)
            value += selectionB.cardValue;

        return value;
    }

    public void StartNewGame() {
        ClearPyramid();
        gameDeck.Clear();
        gameDeck.AddRange(Core.Cards.CardManager.instance.DeckData.Values);
        SetupGamePyramid();
        SetupPairPile();
        SetupDrawPile();
    }

    public void ClearPyramid() {
        int childCount = pyramidTransform.childCount;

        for (int x = 0; x < childCount; x++) {
            int x_childCount = pyramidTransform.GetChild(x).childCount;
            for (int y = 0; y < x_childCount; y++) {
                Destroy(pyramidTransform.GetChild(x).GetChild(y).gameObject);
            }
        }
    }

    public void SetupGamePyramid() {
        int pyramidCardCount = 28;

        int cardPerRow = 1;
        int rowCount = 0;
        for (int x = 0; x < pyramidCardCount; x++) {
            CardData card = DrawCard(false);
            Transform currentRow = getPyramidRow(cardPerRow - 1);


            CardEntity newCard = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Pyramid, card.cardSlug, currentRow, true);
            pyramidCards.Add(newCard);
            newCard.frontButton.onClick.AddListener(delegate { OnPyramidCardClicked(newCard); });

            rowCount++;

            if (rowCount == cardPerRow) {
                cardPerRow++;
                rowCount = 0;
            }
                

        }

        StartCoroutine(PyramidIntro());
    }

    IEnumerator PyramidIntro() {
        yield return new WaitForSeconds(1f);

        int cardPerRow = 1;
        int rowCount = 0;
        for (int x = 0; x < pyramidCards.Count; x++) {
            pyramidCards[x].ToggleFaceUp(true);
            rowCount++;

            if(rowCount == cardPerRow) {
                cardPerRow++;
                rowCount = 0;
                yield return new WaitForSeconds(.142f);

            }

        }
    }


    public void OnDrawPileClicked() {

        if(gameDeck.Count > 0) {
            SetDiscardPileTop(DrawCard());
        }
    }

    public void OnPyramidCardClicked(CardEntity cardClicked) {
        if (cardClicked.isFaceUp) {
            SelectCard(cardClicked);
        }
    }

    public void OnDiscardPileClicked() {
        if(discardPile.Count > 0 && discardTop != null) {
            SelectCard(discardTop);
        }
    }


    #region Piles
    #region Draw Pile
    public void SetupDrawPile() {
        if (drawPilePlaceholder == null) {
            drawPilePlaceholder = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Placeholder, null, drawPileTransform);
            drawPilePlaceholder.backButton.onClick.AddListener(delegate { OnDrawPileClicked(); });
        }

        ToggleDrawPlaceholder(true);
    }
    public void ToggleDrawPlaceholder(bool state) {
        if (drawPilePlaceholder != null)
            drawPilePlaceholder.gameObject.SetActive(state);
    }


    #endregion

    #region Pair Pile
    public void SetupPairPile() {
        if (pairPilePlaceholder == null)
            pairPilePlaceholder = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Placeholder, null, pairPileTransform);

        pairPilePlaceholder.gameObject.SetActive(false);
    }

    public void TogglePairPlaceholder(bool state) {
        if (pairPilePlaceholder != null)
            pairPilePlaceholder.gameObject.SetActive(state);
    }


    #endregion

    #region Discard Pile
    public void SetDiscardPileTop(CardData cardToSet) {
        if(discardTop == null) {
            discardTop = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Draw, cardToSet.cardSlug,  discardPileTransform);
            discardTop.frontButton.onClick.AddListener(delegate { OnDiscardPileClicked(); });
        }
        else {
            discardTop.InitCard(cardToSet.cardSlug, CardEntity.CardType.Draw);
        }

        ToggleDiscardPile(true);
    }
 
    public void RemoveTopDiscardCard() {
        print("removing top card");

        discardPile.RemoveAt(discardPile.Count - 1);
        CardData nextCard = getLastDiscard();
        if(nextCard == null) {
            ToggleDiscardPile(false);
            print("next card null");
        }
        else {
            print("next card!");
            SetDiscardPileTop(nextCard);
        }
    }

    public CardData getLastDiscard() {
        if(discardPile.Count > 0) {
            return discardPile[discardPile.Count - 1];
        }

        return null;
    }

    public void ToggleDiscardPile(bool state) {
        discardTop.gameObject.SetActive(state);
    }

    #endregion

    #endregion




    public CardData DrawCard(bool discard = true) {
        int random = Random.Range(0, gameDeck.Count);
        CardData cache = gameDeck[random];
        gameDeck.Remove(cache);

        if(gameDeck.Count <= 0) {
            ToggleDrawPlaceholder(false);
        }

        if (discard) {
            discardPile.Add(cache);
        }

        return cache;
    }

    public Transform getPyramidRow(int rowIndex) {
        return pyramidTransform.GetChild(rowIndex);
    }

}
