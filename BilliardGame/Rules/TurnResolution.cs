namespace BilliardGame.Rules;

public readonly record struct TurnResolution(bool SwitchPlayer, bool CueBallInHand, bool GameWon, bool WinnerIsPlayerOne, string Message);
