using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class GamePlayScipt : MonoBehaviour
{
    [SerializeField] private PlayerScriptBetter player1, player2;
    private Animator player1Animator, player2Animator, winnerTextAnimator; 
    //10-fireballReady, 11-block

    [SerializeField] private Text whoCountersWho, winnerText, scoreText, winnerTextInMenu;

    [SerializeField] private GameObject menuCanvas, gamePlayCanvas;
    [SerializeField] private Image[] roundsToWinButton;
    [SerializeField] private Animator mainCameraAnimator;
    private int roundsToWin = 5;

    private void Start()
    {
        player1Animator = player1.GetComponent<Animator>();
        player2Animator = player2.GetComponent<Animator>();
        winnerTextAnimator = winnerText.GetComponent<Animator>();
    }

    private Color32 defaultButtonColor = new Color32(248, 219, 0, 255);
    private Color32 pressedButtonColor = new Color32(108, 96, 0, 255);
    public void OnPlayButton()
    {
        menuCanvas.SetActive(false);
        gamePlayCanvas.SetActive(true);
        mainCameraAnimator.Play("GameStartAnim");
        StartCoroutine(NewRound());
    }

    public void OnFiveRoundsButton()
    {
        roundsToWin = 5;
        roundsToWinButton[0].color = pressedButtonColor;
        roundsToWinButton[1].color = defaultButtonColor;
        roundsToWinButton[2].color = defaultButtonColor;
    }
    public void OnTenRoundsButton() 
    {
        roundsToWin = 10;
        roundsToWinButton[0].color = defaultButtonColor;
        roundsToWinButton[1].color = pressedButtonColor;
        roundsToWinButton[2].color = defaultButtonColor;
    }
    public void OnTwentyRoundsButton()
    {
        roundsToWin = 20;
        roundsToWinButton[0].color = defaultButtonColor;
        roundsToWinButton[1].color = defaultButtonColor;
        roundsToWinButton[2].color = pressedButtonColor;
    }

    private IEnumerator NewRound()
    {
        Advertisement.Load("Interstitial_Android");
        player1.score = 0;
        player2.score = 0;
        scoreText.text = "0-0";

        player1.ableToSwitch = false;
        player2.ableToSwitch = false;
        ableToAttack = false;

        yield return new WaitForSeconds(1); //camera animation time

        ResetFight();

        player1.justSwitched = false;
        player2.justSwitched = false;

        player1.ResetFireBall();
        player2.ResetFireBall();

        player1Animator.SetInteger("AnimN", (player1.currentPose = 1) + 3);
        player2Animator.SetInteger("AnimN", (player2.currentPose = 1) + 3);
        SwitchedPose();
    }
    [SerializeField] private Animator redirectAnimation1, redirectAnimation2;
    public IEnumerator NewRound(PlayerScriptBetter winnerPlayer)
    {
        winnerPlayer.score++;
        if(winnerPlayer.score == roundsToWin)  //END GAME
        {
            adsScript.ShowAdsVideo();
            if(redirectAnimation1.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f || redirectAnimation2.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
                yield return new WaitForSeconds(0.75f); //redirectAnimation time
            winnerTextInMenu.text = "Player " + winnerPlayer.ID + " is a winner!";
            menuCanvas.SetActive(true);
            gamePlayCanvas.SetActive(false);
            mainCameraAnimator.Play("defaultCameraPosition"); //Default for menu
            yield break;
        }

        player1.ableToSwitch = false;
        player2.ableToSwitch = false;
        winnerText.text = "Player " + winnerPlayer.ID + " wins!";
        winnerTextAnimator.Play("showWinner");
        scoreText.text = player2.score +"-"+player1.score;
        yield return new WaitForSeconds(1.3f);
        ResetFight();

        player1.justSwitched = false;
        player2.justSwitched = false;

        player1.ResetFireBall();
        player2.ResetFireBall();

        player1Animator.SetInteger("AnimN", (player1.currentPose = 1) + 3);
        player2Animator.SetInteger("AnimN", (player2.currentPose = 1) + 3);
        SwitchedPose();
    }
    public void ResetFight()
    {
        if (player1Animator.GetInteger("AnimN")!=11) player1Animator.SetInteger("AnimN", player1.currentPose + 3);
        if (player2Animator.GetInteger("AnimN") != 11)  player2Animator.SetInteger("AnimN", player2.currentPose + 3);

        player1.ableToSwitch = true;
        player2.ableToSwitch = true;

        player1.redirected = false;
        player2.redirected = false;

        player1.redirected = false;
        player2.redirected = false;

        player1.pressedRedirect = false;
        player2.pressedRedirect = false;

        player1.avoidedFireBall = false;
        player2.avoidedFireBall = false;

        ableToAttack = true;
        fireballGame = false;
        timeToRedirect = false;
    }

    private bool ableToAttack = true, fireballGame = false;
    public void OnAttackButtonPlayer1()
    {
        if (fireballGame) { RedirectFireBall(player1, player2); return; }
        if (!ableToAttack) return;
        ableToAttack = false;

        player1.ableToSwitch = false;
        StartCoroutine(player1.OnAttack(0.5f, player2, player2Animator));
    }

    public void OnAttackButtonPlayer2()
    {
        if (fireballGame) { RedirectFireBall(player2, player1); return; }
        if (!ableToAttack) return;
        ableToAttack = false;

        player2.ableToSwitch = false;
        StartCoroutine(player2.OnAttack(0.5f, player1, player1Animator));
    }



    private Animator playerLaunchedAnimator, playerDefendingAnimator;
    private PlayerScriptBetter playerLaunchedScript, playerDefendingScript;
    public void OnFireBallButtonPlayer1()
    {
        if (fireballGame) return;

        player1.ResetFireBall();

        playerLaunchedScript = player1;
        playerDefendingScript = player2;

        playerLaunchedAnimator = player1Animator;
        playerDefendingAnimator = player2Animator;

        StartCoroutine(FireBallGame());
    }

    public void OnFireBallButtonPlayer2()
    {
        if (fireballGame) return;

        player2.ResetFireBall();

        playerLaunchedScript = player2;
        playerDefendingScript = player1;

        playerLaunchedAnimator = player2Animator;
        playerDefendingAnimator = player1Animator;

        StartCoroutine(FireBallGame());
    }

    private bool timeToRedirect = false;
    private IEnumerator FireBallGame()
    {
        fireballGame = true;
        ableToAttack = false;
        playerLaunchedAnimator.Play("FireBallLaunch"); //íåëüçÿ ïîìåíÿòü ïîçó ïîêà àíèìàöèÿ èãðàåò... ?
        playerDefendingScript.fireBallSprite.color = playerLaunchedScript.fireBallSprite.color;
        yield return new WaitForSeconds(1.5f); //first animation time is 1.5f
        timeToRedirect = true;
        player1.AttackAnimations(); //!!ÌÁ ÏÎÌÅÍßÒÜ ÏÎÑËÅ ÎÆÈÄÀÍÈß ÈÃÐÎÊÀ //no matter what player

        yield return new WaitForSeconds(0.5f); // time of camera animation and player to react
        timeToRedirect = false;

        if (playerDefendingScript.redirected)
            playerDefendingAnimator.Play("FireBallRedirectAnim");
        else
        {
            playerDefendingScript.PunchByFireBallAnimation();
            StartCoroutine(NewRound(playerLaunchedScript));
            yield break;
        }

        yield return new WaitForSeconds(0.75f); //time of redirect animation
        timeToRedirect = true;
        player1.AttackAnimations();

        yield return new WaitForSeconds(0.25f);
        timeToRedirect = false;

        if (playerLaunchedScript.redirected)
            playerLaunchedAnimator.Play("SecondRedirectAnim");
        else
        {
            playerLaunchedScript.PunchByFireBallAnimation();
            StartCoroutine(NewRound(playerDefendingScript));
            yield break;
        }

        yield return new WaitForSeconds(1.16f); //time of second redirect anim
        timeToRedirect = true;
        playerDefendingScript.pressedRedirect = false;
        player1.AttackAnimations();

        yield return new WaitForSeconds(0.25f);
        timeToRedirect = false;

        if (playerDefendingScript.avoidedFireBall)
            playerDefendingAnimator.Play("avoidedFireBallAnim");
        else
        {
            playerDefendingScript.PunchByFireBallAnimation();
            StartCoroutine(NewRound(playerLaunchedScript));
            yield break;
        }

        yield return new WaitForSeconds(0.5f); //avoid animation time 
        ResetFight();
    }



    public void RedirectFireBall(PlayerScriptBetter player, PlayerScriptBetter anotherPlayer)
    {
        if (player.pressedRedirect)
            return;
        player.PressedRedirect();

        if (!timeToRedirect)
            return;

        if(player.Equals(playerDefendingScript) && player.redirected && anotherPlayer.redirected)
        {
            player.AvoidedFireBall();
            return;
        }



        if( !(player.Equals(playerLaunchedScript)) && player.currentPose == anotherPlayer.currentPose)
        {
            player.redirected = true;
            player.currentPose = Random.Range(1, 4);
            switch(player.currentPose)
            {
                case 1: player.fireBallSprite.color = Color.blue; break;
                case 2: player.fireBallSprite.color = Color.yellow; break;
                case 3: player.fireBallSprite.color = Color.red; break;
            }
            anotherPlayer.fireBallSprite.color = player.fireBallSprite.color;
        }
        else if(player.Equals(playerLaunchedScript) && playerDefendingScript.redirected && player.currentPose == anotherPlayer.currentPose) 
            player.redirected = true;
    }

    private int player1Pose, player2Pose;
    public void SwitchedPose()
    {
        player1Pose = player1.currentPose;
        player2Pose = player2.currentPose;

        if (player1Pose == player2Pose)
            whoCountersWho.text = "=";
        else if(player1Pose > player2Pose)
        {
            if (player1Pose == 3 && player2Pose == 1)
            {
                whoCountersWho.text = ">";
                return;
            }
            else whoCountersWho.text = "<";
        }
        else
        {
            if (player2Pose == 3 && player1Pose == 1)
            {
                whoCountersWho.text = "<";
                return;
            }
            else whoCountersWho.text = ">";
        }
    }
}
