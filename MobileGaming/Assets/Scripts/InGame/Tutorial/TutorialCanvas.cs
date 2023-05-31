using DG.Tweening;
using UnityEngine;

public class TutorialCanvas : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private GameObject cursorGo;
    [field:SerializeField] public RectTransform CursorTr { get; private set; }
    [SerializeField] private Animator cursorAnimator;
    [SerializeField] private RectTransform[] waypoints;
    
    private Vector3 generatorStartPosition => waypoints[0].position;
    private Vector3 redMachineStartPosition => waypoints[7].position;
    private Vector3 generatorPosition => waypoints[1].position;
    private Vector3 redMachinePosition => waypoints[2].position;
    private Vector3 blueMachinePosition => waypoints[3].position;
    private Vector3 clientPosition => waypoints[4].position;
    private Vector3 endPosition => waypoints[5].position;
    private Vector3 scissorsPosition => waypoints[6].position;
    private Vector3 linePosition => waypoints[8].position;

    private Sequence sequence;
        
    public void ShowCursor(bool value)
    {
        cursorGo.SetActive(value);
    }
    
    public void PlayClickAnimationHold()
    {
        cursorAnimator.Play("ANIM_Pointer_OnPress");
    }
    
    public void PlayReleaseAnimation()
    {
        cursorAnimator.Play("ANIM_Pointer_Release");
    }
    
    // start, generator, red (loops)
    // redStart ,red, client (loops)
    // start, generator, blue, client (loops)
    // scissors
    
    public void PlayFirstSequence()
    {
        StopSequence();
        
        sequence = DOTween.Sequence();
        sequence.AppendCallback(() => ShowCursor(true));
        sequence.AppendCallback(() => CursorTr.position = generatorStartPosition);
        sequence.AppendInterval(0.1f);
        sequence.Append(CursorTr.DOMove(generatorPosition, 1f));
        sequence.AppendCallback(PlayClickAnimationHold);
        sequence.AppendInterval(0.5f);
        sequence.Append(CursorTr.DOMove(redMachinePosition, 1f));
        sequence.AppendCallback(PlayReleaseAnimation);
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => ShowCursor(false));

        sequence.SetLoops(-1).SetDelay(1f);

        sequence.Play();
    }
    
    public void PlaySecondSequence()
    {
        StopSequence();
        
        sequence = DOTween.Sequence();
        sequence.AppendCallback(() => ShowCursor(true));
        sequence.AppendCallback(() => CursorTr.position = redMachineStartPosition);
        sequence.AppendInterval(0.1f);
        sequence.Append(CursorTr.DOMove(redMachinePosition, 1f));
        sequence.AppendCallback(PlayClickAnimationHold);
        sequence.AppendInterval(0.5f);
        sequence.Append(CursorTr.DOMove(clientPosition, 1f));
        sequence.AppendCallback(PlayReleaseAnimation);
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => ShowCursor(false));

        sequence.SetLoops(-1).SetDelay(1f);

        sequence.Play();
    }
    
    public void PlayThirdSequence()
    {
        StopSequence();
        
        sequence = DOTween.Sequence();
        sequence.AppendCallback(() => CursorTr.position = generatorStartPosition);
        sequence.AppendCallback(() => ShowCursor(true));
        sequence.AppendInterval(0.1f);
        sequence.Append(CursorTr.DOMove(generatorPosition, 1f));
        sequence.AppendCallback(PlayClickAnimationHold);
        sequence.AppendInterval(0.5f);
        sequence.Append(CursorTr.DOMove(blueMachinePosition, 1f));
        sequence.AppendInterval(0.1f);
        sequence.Append(CursorTr.DOMove(clientPosition, 1f));
        sequence.AppendCallback(PlayReleaseAnimation);
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => ShowCursor(false));

        sequence.SetLoops(-1).SetDelay(1f);

        sequence.Play();
    }
    
    public void PlayFourthSequence()
    {
        StopSequence();
        
        CursorTr.position = scissorsPosition;
        sequence = DOTween.Sequence();
        sequence.AppendCallback(() => CursorTr.position = scissorsPosition);
        sequence.AppendCallback(() => ShowCursor(true));
        sequence.AppendInterval(1f);
        sequence.AppendCallback(PlayClickAnimationHold);
        sequence.AppendInterval(0.5f);
        sequence.Append(CursorTr.DOMove(linePosition, 1f));
        sequence.AppendCallback(PlayReleaseAnimation);
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => ShowCursor(false));

        sequence.SetLoops(-1).SetDelay(1f);
        
        sequence.Play();
    }
    
    public void StopSequence()
    {
        ShowCursor(false);
        sequence.Kill();
    }
}
