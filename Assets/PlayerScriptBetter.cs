using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScriptBetter : MonoBehaviour
{
    private Animator playerAnimator;

    public int currentPose = 1, ID, score = 0;
    public bool justSwitched = false, ableToSwitch = true, //toSwtich ÌÛÊÂÌ ‰Îˇ Ù‡Â·ÓÎ Ë„˚, ˜ÚÓ·˚ ÌÂ ÏÂÌˇÚ¸ ÔÓÍ‡ ¯‡ ÎÂÚËÚ
        redirected = false, pressedRedirect = false, avoidedFireBall = false; 
    private bool blueClicked = false, yellowClicked = false, redClicked = false;

    //R(pose 3)>Y(2)>B(1)>R
    char[,] result = new char[4, 4] { { 'n', 'n', 'n', 'n' }, //n for null
        {'n', 'T', 'L', 'W'},
        {'n', 'W', 'T', 'L'},
        {'n', 'L', 'W', 'T' }};

    [SerializeField] private GameObject fireBallButton;
    public SpriteRenderer fireBallSprite;

    [SerializeField] private Animator mainCamerAnimator, playersAnimator,
        timeSlider, fireText;

    [SerializeField] private GamePlayScipt gameManager;

    private void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerAnimator.SetInteger("AnimN", currentPose + 3);
    }

    public void OnBlueButton()
    {
        if (justSwitched || !ableToSwitch) return;

        blueClicked = true;
        fireBallSprite.color = Color.blue;
        currentPose = 1;

        StartCoroutine(OnButton(0.5f));
    }

    public void OnYellowButton()
    {
        if (justSwitched || !ableToSwitch) return;

        yellowClicked = true;
        fireBallSprite.color = Color.yellow;
        currentPose = 2;

        StartCoroutine(OnButton(0.5f));
    }

    public void OnRedButton()
    {
        if (justSwitched || !ableToSwitch) return;

        redClicked = true;
        fireBallSprite.color = Color.red;
        currentPose = 3;

        StartCoroutine(OnButton(0.5f));
    }

    private IEnumerator OnButton(float seconds)
    {
        justSwitched = true;
        if (blueClicked && yellowClicked && redClicked)
        {
            playerAnimator.SetInteger("AnimN", 10); //fireBallReady
            fireBallButton.SetActive(true);
        }
        else playerAnimator.SetInteger("AnimN", currentPose + 3);

        gameManager.SwitchedPose();


        timeSlider.Play("timerAnim");
        yield return new WaitForSeconds(seconds);
        justSwitched = false;
    }

    private bool switchedForCounterAttack;
    //10-fireballReady, 11-block +3 - poses, +6 - punched by
    public IEnumerator OnAttack(float time,
        PlayerScriptBetter anotherPlayer, Animator anotherPlayerAnimator)
    {
        AttackAnimations(ID);
        playerAnimator.SetInteger("AnimN", currentPose);
        yield return new WaitForSeconds(time);

        switch (result[currentPose, anotherPlayer.currentPose])
        {
            case 'T':
                //play tie animation
                //Ã¡ ƒŒ¡¿¬»“‹ ›‘‘≈ “ ¡ÀŒ ¿ » ¬ À”« “›∆
                anotherPlayerAnimator.SetInteger("AnimN", 11);
                playerAnimator.SetInteger("AnimN", currentPose + 3);
                gameManager.ResetFight();
                break;

            case 'W':
                //play win animation
                anotherPlayerAnimator.SetInteger("AnimN", currentPose + 6);
                //yield return new WaitForSeconds(0.5f); //death animation time
                //score++;
                StartCoroutine(gameManager.NewRound(this));
                break;

            case 'L':
                //play loose animation
                anotherPlayer.ableToSwitch = false;
                switchedForCounterAttack = anotherPlayer.justSwitched;
               
                anotherPlayerAnimator.SetInteger("AnimN", 11);

                yield return new WaitForSeconds(0.3f);

                AttackAnimations(anotherPlayer.ID);
                anotherPlayerAnimator.SetInteger("AnimN", anotherPlayer.currentPose);

                yield return new WaitForSeconds(time);

                if (switchedForCounterAttack) 
                {
                    playerAnimator.SetInteger("AnimN", 11);
                    anotherPlayerAnimator.SetInteger("AnimN", anotherPlayer.currentPose + 3);
                    gameManager.ResetFight();
                }
                else
                {
                    playerAnimator.SetInteger("AnimN", anotherPlayer.currentPose + 6);
                    //yield return new WaitForSeconds(0.5f); //death animation time
                    //anotherPlayer.score++;
                    StartCoroutine(gameManager.NewRound(anotherPlayer));
                }
                break;
        }
    }

    public void PunchByFireBallAnimation()
    {
        if (ID == 1)
        {
            playerAnimator.Play("PunchedByFireballFor1");
            playersAnimator.Play("Player1Fly");
        }
        else 
        {
            playerAnimator.Play("PunchedByFireballFor2");
            playersAnimator.Play("Player2Fly");
        }
    }

    public void AttackAnimations(int player)
    {
        mainCamerAnimator.Play("PlayerAttackAnim");
        playersAnimator.Play((player == 1) ? "Player1AttackAnim" : "Player2AttackAnim");
    }
    public void AttackAnimations()
    {
        mainCamerAnimator.Play("PlayerAttackAnim");
    }

    public void ResetFireBall()
    {
        fireBallButton.SetActive(false);
        redClicked = false;
        yellowClicked = false;
        blueClicked = false;
    }

    public void PressedRedirect()
    {
        pressedRedirect = true;
        fireText.GetComponent<Text>().text = "REDIRECT?";
        fireText.Play((ID == 1)? "Redirect1Anim" : "Redirect2Anim");
    }

    public void AvoidedFireBall()
    {
        avoidedFireBall = true;
        fireText.GetComponent<Text>().text = "DODGE!";
        fireText.Play((ID == 1) ? "Redirect1Anim" : "Redirect2Anim");
    }
}
