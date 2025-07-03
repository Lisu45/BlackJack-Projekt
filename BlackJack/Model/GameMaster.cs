using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace BlackJack.Model;

class GameMaster : ObservableObject
{
    int initialBudget;
    readonly ChartData chartData;
    public event Action? OnPlayerLost;
    public event Action? DealerDone;
    public string Result { get; set; } = string.Empty;

    public Player player;
    public Dealer dealer;
    public CardSheet cardSheet;

    Result result;

    bool canMakeMove;
    public bool CanMakeMove
    {
        get => canMakeMove;
        set => SetProperty(ref canMakeMove, value);
    }
    double winIndicator;
    public GameMaster(ChartData chartData, int playerBudget, Result result)
    {
        this.result = result;
        this.chartData = chartData;
        initialBudget = playerBudget;
        winIndicator = 5.0;
        cardSheet = new CardSheet();
        player = new Player();
        player.Budget = initialBudget;
        dealer = new Dealer();
        CanMakeMove = true;
    }

    public void SetBet(int bet)
    {
        player.Bet = bet;
        player.Budget -= player.Bet;
    }

    public void StartGame()
    {
        player.Hit(cardSheet.PickCard());
        player.Hit(cardSheet.PickCard());
        PlayerTooManyCards();
        dealer.Hit(cardSheet.PickCard());
    }

    public void PlayerHit()
    {
        player.Hit(cardSheet.PickCard());
        if (PlayerTooManyCards())
        {
            OnPlayerLost?.Invoke();
        }
    }

    public bool PlayerTooManyCards()
    {
        if (player.Points > 21)
        {
            foreach (Card card in player.Sheet)
            {
                if (card.Point == 11)
                {
                    player.Points -= 10;
                    card.Point = 1;
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    public async void DealerMakeMove()
    {
        bool moveMade = true;
        while (moveMade && !DealerTooManyCards())
        {
            moveMade = dealer.Hit(cardSheet.PickCard());
            await Task.Delay(500);
        }
        FindWinner();
        DealerDone?.Invoke();
    }

    public bool DealerTooManyCards()
    {
        if (dealer.Points > 21)
        {
            foreach (Card card in dealer.Sheet)
            {
                if (card.Point == 11)
                {
                    dealer.Points -= 10;
                    card.Point = 1;
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    public void FindWinner()
    {
        GetPlayerWins();
        GetBlackJack();
        if(winIndicator == 5.0)
        {
            throw new Exception("Brak zwyciezcy");
        } else if (winIndicator == 0 && player.Budget == 0)
        {
            Result += "\nSplukales sie";
        }
    }

    public void GetBlackJack()
    {
        if (player.Sheet.Count == 2 && player.Points == 21 || dealer.Sheet.Count == 2 && dealer.Points == 21)
        {
            Result = "BlackJack!";
            winIndicator = 2.5;
            if (dealer.Sheet.Count == 2 && dealer.Points == 21)
            {
                Result = "Wygrywa dealer";
                winIndicator = 0;
            }
            if (player.Sheet.Count == 2 && player.Points == 21 && dealer.Sheet.Count == 2 && dealer.Points == 21)
            {
                Result = "Remis!";
                winIndicator = 1;
            }
        }
    }

    public void GetPlayerWins()
    {
        if (player.Points > 21 || dealer.Points > 21)
        {
            if (player.Points > 21 && dealer.Points > 21)
            {
                Result = "Bust!";
                winIndicator = 1;
            }
            else if (player.Points > 21)
            {
                Result = "Wygrywa dealer! Gracz Bust!";
                winIndicator = 0;
            }
            else if (dealer.Points > 21)
            {
                Result = "Gracz Wygrywa! Dealer Bust";
                winIndicator = 2;
            }
        }
        else
        {
            if (player.Points > dealer.Points)
            {
                Result = "Wygrywa Gracz!";
                winIndicator = 2;
            }
            else if (dealer.Points > player.Points)
            {
                Result = "Wygrywa Dealer";
                winIndicator = 0;
            }
            else
            {
                Result = "Remis!";
                winIndicator = 1;
            }
        }
    }
    public void PayOut()
    {
        player.Budget += Convert.ToInt32(player.Bet * winIndicator);
        FinalValues();
    }

    public void FinalValues()
    {
        int profit = player.Budget - initialBudget;
        chartData.AddDataPoint(player.Budget, player.Bet, profit);
        result.Budget = player.Budget;
        result.Bet = player.Bet;
        result.WinIndicator = Convert.ToInt32(winIndicator);
        player.Bet = 0;

    }

    public void ClearCards()
    {
        player.Sheet.Clear();
        dealer.Sheet.Clear();
    }
}