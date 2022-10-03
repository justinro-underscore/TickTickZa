﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : MonoBehaviour
{
    public static PlayController instance = null;

    public ConveyorBelt conveyorBelt;
    public Pizza displayPizza;
    public ToppingsDisplay toppingsDisplay;

    [SerializeField] private List<PizzaOrder> pizzaOrders = null;

    private int pizzaIndex;
    private int score;
    private const int MAX_SCORE = 99;
    private const int MAX_LIFE = 3;
    private int numLives;

    private Pizza currPizza;
    private PizzaOrder currPizzaOrder;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        InitalizeGame();
    }

    private List<int> ValidatePizzaOrders()
    {
        List<int> invalidLevels = new List<int>();
        for (int i = 0; i < pizzaOrders.Count; i++)
        {
            PizzaOrder order = pizzaOrders[i];
            if (order != null)
            {
                int numIngredients = order.meats.Count + order.peppers.Count +
                    order.vegetables.Count + order.genericToppings.Count + (order.hasPineapple ? 1 : 0);
                if (numIngredients > Constants.maxIngredients)
                {
                    invalidLevels.Add(i);
                }
            }
            else
            {
                invalidLevels.Add(i);
            }
        }
        return invalidLevels;
    }

    public void Restart()
    {
        InitalizeGame();
    }

    private void InitalizeGame()
    {
        List<int> invalidLevels = ValidatePizzaOrders();
        if (invalidLevels.Count > 0)
        {
            string invalidLevelsStr = "";
            foreach (int level in invalidLevels)
            {
                invalidLevelsStr += (invalidLevelsStr != "" ? ", " : "") + level;
            }
            Debug.LogError("Invalid levels! " + invalidLevelsStr);
            return;
        }

        // Initialize the game
        pizzaIndex = 0;
        score = 0;
        numLives = MAX_LIFE;

        // Update score
        UIController.uicInstance.SetScore(score);

        // Initialize hearts
        for ( int i = 0; i < MAX_LIFE; i++ ) {
            UIController.uicInstance.SetHearts( false );
        }

        StartLevel();
    }

    private void StartLevel()
    {
        currPizza = conveyorBelt.CreatePizza();
        currPizzaOrder = pizzaOrders[pizzaIndex];

        displayPizza.gameObject.SetActive(true);
        displayPizza.ResetPizza();
        displayPizza.SetPizza(currPizzaOrder);

        toppingsDisplay.SetPizzaOrder(currPizzaOrder);

        // Ship meat
        DeliveryManager.dmInstance.DeliverMeat(currPizzaOrder.meats, currPizzaOrder.numMeatToShip);
    
        // Init is done for level, so let's start the timer! (that doesn't really mean anything haha)
        TimerController.tcInstance.StartTimer();
    }

    public void EndLevel()
    {
        // TODO Check if complete and do other stuff
        bool pizzaCorrect = currPizza.IsOrderCorrect(currPizzaOrder);
        if (pizzaCorrect)
        {
            score += 4;
            if (score > MAX_SCORE)
            {
                score = MAX_SCORE;
            }
            // Update score
            UIController.uicInstance.SetScore(score);

            SoundController.scInstance.PlaySingle("pizzaCorrect");

            Debug.Log("Pizza correct!");
        }
        else
        {
            UIController.uicInstance.SetHearts( true );
            numLives--;

            SoundController.scInstance.PlaySingle("pizzaWrong");

            Debug.Log("Pizza incorrect");
        }
        displayPizza.gameObject.SetActive(false);
        toppingsDisplay.ResetPizza();

        pizzaIndex++; // TODO For now I just want to test with pizza 1
        GameObject.Destroy(currPizza.gameObject);

        // DESTROY THE PLANTS
        PlantManager.pmInstance.WipePlants();

        // Wipe the meat!
        DeliveryTable.dtInstance.WipeBoxes();

        if (numLives == 0)
        {
            // You lost :(
            GameController.instance.GameOver();
            GameOverController.gocInstance.UpdateFinalScore(score);
            ResetScene();
        }
        else 
        if (pizzaIndex >= pizzaOrders.Count - 1)
        {
            // You won!
            GameController.instance.GameWon();
            ResetScene();
        }
        else
        {
            // Increase belt speed!
            BeltController.bcInstance.UpdateSpeed();

            // Increase veggie wheel speed/or change direction
            WheelController.wcInstance.UpdateSpeedAndDirection();

            Invoke("StartLevel", 1); // TODO Does using Invoke work with pause?
        }
    }

    private void ResetScene()
    {
        // TODO: there are 100% things I have missed to reset -> must be a better way to do this
        BeltController.bcInstance.Reset();
        WheelController.wcInstance.Reset();
        DrawerController.dcInstance.Reset();
        PlantManager.pmInstance.ResetWateringCan();
        DeliveryTable.dtInstance.Reset(); // Might catch lingering boxes in some weird state -> why did I do it this way god why
    }

    public bool SetSauce(Constants.Sauces sauce)
    {
        if (currPizzaOrder.sauce != sauce)
        {
            return false;
        }
        bool success = currPizza.SetSauce(sauce);
        if (success)
        {
            toppingsDisplay.SetSauceComplete();
        }
        return success;
    }

    public bool SetCheese(Constants.CheeseTypes cheese)
    {
        if (currPizzaOrder.cheese != cheese)
        {
            return false;
        }
        bool success = currPizza.SetCheese(cheese);
        if (success)
        {
            toppingsDisplay.SetCheeseComplete();
        }
        return success;
    }

    public bool AddMeat(Constants.Meats meat)
    {
        if (!currPizzaOrder.meats.Contains(meat))
        {
            return false;
        }
        bool success = currPizza.AddMeat(meat);
        if (success)
        {
            toppingsDisplay.SetMeatComplete(meat);
        }
        return success;
    }

    public bool AddPepper(Constants.Peppers pepper)
    {
        if (!currPizzaOrder.peppers.Contains(pepper))
        {
            return false;
        }
        bool success = currPizza.AddPepper(pepper);
        if (success)
        {
            toppingsDisplay.SetPepperComplete(pepper);
        }
        return success;
    }

    public bool AddVegetable(Constants.Vegetables vegetable)
    {
        if (!currPizzaOrder.vegetables.Contains(vegetable))
        {
            return false;
        }
        bool success = currPizza.AddVegetable(vegetable);
        if (success)
        {
            toppingsDisplay.SetVegetableComplete(vegetable);
        }
        return success;
    }

    public bool AddGenericTopping(Constants.GenericToppings topping)
    {
        if (!currPizzaOrder.genericToppings.Contains(topping))
        {
            return false;
        }
        bool success = currPizza.AddGenericTopping(topping);
        if (success)
        {
            toppingsDisplay.SetGenericToppingComplete(topping);
        }
        return success;
    }

    public bool AddPineapple()
    {
        if (!currPizzaOrder.hasPineapple)
        {
            return false;
        }
        bool success = currPizza.AddPineapple();
        if (success)
        {
            toppingsDisplay.SetPineappleComplete();
        }
        return success;
    }
}
