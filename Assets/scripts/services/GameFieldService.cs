﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class GameFieldService : MonoBehaviour
{
    private Main mainComponent;
    private GameObjects _gameObjects;
    private int safeFieldsIndex;
    private int dangerousFieldsIndex;

    public GameFieldService(ref GameObjects gameObjects)
    {
        _gameObjects = gameObjects;
        mainComponent = _gameObjects.main.GetComponent<Main>();
    }
    
    /*
    * <summary>Основной метод - заполняет все поля на игровом поле</summary>
    */
    public void fillGameFields()
    {
        List<Card> safeFields = getSafeFields();
        List<Card> dangerousFields = getDangerousFields();

        setGoalAndStartFields();
        setDangerousFields(dangerousFields);
        SetSafetyCardsInField(safeFields);
    }

    private void setGoalAndStartFields()
    {
        Card goal = new Card() { isWin = true, position = mainComponent.goalPosition };
        mainComponent.gameFields[(int)mainComponent.goalPosition.x-1, (int)mainComponent.goalPosition.y-1] = goal;

        Card start = new Card() { isStart = true, position = mainComponent.heroPosition };
        mainComponent.gameFields[(int)mainComponent.heroPosition.x-1, (int)mainComponent.heroPosition.y-1] = start;
    }

    public void SetSafetyCardsInField(List<Card> safeCards)
    {
        safeFieldsIndex = 0;

        System.Random rnd = new System.Random(DateTime.Now.Millisecond);

        Stack<Card> way = new Stack<Card>();

        Card current = mainComponent.gameFields[(int)mainComponent.goalPosition.x - 1, (int) mainComponent.goalPosition.y - 1];

        way.Push(current);

        do {
            List<Card> availableCards = GetAvailableCards(current);

            if (availableCards.Count == 0) 
            {
                current = way.Pop();
                continue;
             }
           
            int rndIndex = rnd.Next(0, availableCards.Count);

            Card directionCard = availableCards[rndIndex];

            if(directionCard.position.x == Constants.startPosition.x && directionCard.position.y == Constants.startPosition.y)
            {
                break;
            }

            mainComponent.gameFields[(int)directionCard.position.x - 1, (int)directionCard.position.y - 1] = safeCards[safeFieldsIndex];
            mainComponent.gameFields[(int)directionCard.position.x - 1, (int)directionCard.position.y - 1].position = directionCard.position;
            
            safeFieldsIndex++;

            current = mainComponent.gameFields[(int)directionCard.position.x - 1, (int)directionCard.position.y - 1];
            way.Push(current);

        } while (way.Count > 0);
    }

    private List<Card> GetAvailableCards(Card current)
    {
        List<Card> availableCards = new List<Card>();

        List<Card> surroundingCards = new List<Card> { 
            GetCard(Constants.leftPosition, current.position),
            GetCard(Constants.rightPosition, current.position),
            GetCard(Constants.upPosition, current.position),
            GetCard(Constants.downPosition, current.position)
        };

        foreach(Card surroundingCard in surroundingCards)
        {
            if (surroundingCard != null && surroundingCard.isSafe == false)
            {
                bool isСontact = IssetContactCard(surroundingCard.position, current.position);

                if (!isСontact) availableCards.Add(surroundingCard);
            }
        }

        return availableCards;
    }

    //если касается то true
    private bool IssetContactCard(Position surrounding, Position current)
    {
        foreach(Position check in Constants.adjacentPositions)
        {
            float x = surrounding.x + check.x;
            float y = surrounding.y + check.y;

            if ((current.x == x && current.y == y) || x > 10 || x < 1 || y >10 || y < 1) continue;

            Card checkingCard = mainComponent.gameFields[(int)x - 1, (int)y - 1];

            if (checkingCard.isStart || checkingCard.isWin || checkingCard.isSafe) return true;
        }

        return false;
    }

    private Card GetCard(Position distanse, Position current)
    {
        if (current.x + distanse.x > 0
            && current.x + distanse.x < 11
            && current.y + distanse.y < 11
            && current.y + distanse.y > 0
            ) {
            return mainComponent.gameFields[(int)current.x + (int)distanse.x - 1, (int) current.y + (int)distanse.y - 1];
        }

        return null;
    }

    public void setOpenField(Position openedField)
    {
         GameObject instance = Instantiate(_gameObjects.openField);
         instance.transform.position = new Vector3(
            instance.transform.position.x + (Constants.step / 2) + (Constants.step * openedField.x ),
            instance.transform.position.y + (Constants.step / 2) + (Constants.step * openedField.y),
            Constants.openedFieldZ
            );
    }
  
    /*
    * <summary>Получает опасные поля из БД</summary>
    */
    private List<Card> getDangerousFields()
    {
        CardRepository repository = new CardRepository();

        return repository.getCardsBySafety(false);
    }

    /*
    * <summary>Получает безопасные поля из БД</summary>
    */
    private List<Card> getSafeFields()
    {
        CardRepository repository = new CardRepository();

        return repository.getCardsBySafety(true);
    }

    /*
     * <summary>Создает опасные поля? пропуская начало и конец </summary>
     */
    private void setDangerousFields(List<Card> dangerousFields)
    {
        dangerousFields = Shuffler.listShuffler(dangerousFields);

        int dangerousFieldsIndex = 0;

        for (int i = 0; i < Constants.fieldSize; i++)
        {
            for (int j = 0; j < Constants.fieldSize; j++)
            {
                if (mainComponent.gameFields[i, j] == null)
                {
                    if (mainComponent.gameFields[i, j] != null 
                        && (mainComponent.gameFields[i, j].isWin 
                        || mainComponent.gameFields[i, j].isStart)) continue;

                    mainComponent.gameFields[i, j] = dangerousFields[dangerousFieldsIndex];

                    mainComponent.gameFields[i, j].position = new Position() { x = i + 1, y = j + 1 };

                    dangerousFieldsIndex++;

                    dangerousFieldsIndex = dangerousFieldsIndex > dangerousFields.Count - 1 ? 0 : dangerousFieldsIndex;
                }
            }
        }
    }
}