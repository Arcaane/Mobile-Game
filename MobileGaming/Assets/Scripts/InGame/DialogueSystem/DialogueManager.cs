using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LogicUI.FancyTextRendering;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private MarkdownRenderer dialogueMarkdownRenderer;
    [SerializeField] private MarkdownRenderer displayNameMarkdownRenderer;

    [Header("TextCoroutine")]
    [SerializeField] private float typingSpeed = 0.04f;
    private Coroutine displayLineCoroutine;

    private List<string> openTags = new ();
    private List<string> closeTags = new ();
    private string sourceText;
    private string completeText;

    private bool isDialoguePlaying;
    private Queue<string> textQueue = new Queue<string>();

    private static DialogueManager instance;

    [SerializeField, TextArea(10, 10)] private string testText;

    [ContextMenu("test")]
    private void Test()
    {
        EnterDialogueMode(testText);
    }

    public static void SetInstance(DialogueManager dialogueManager)
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        
        instance = dialogueManager;
        
        dialogueManager.ExitDialogueMode();
        
        DontDestroyOnLoad(instance.gameObject);
    }

    private void Start()
    {
        isDialoguePlaying = false;

        dialogueCanvas.SetActive(false);
    }

    public static void EnterDialogueMode(string text)
    {
        var texts = text.Split("\n");
        foreach (var str in texts)
        {
            instance.textQueue.Enqueue(str);
        }
        
        instance.isDialoguePlaying = true;

        instance.dialogueCanvas.SetActive(true);

        InputService.OnPress += instance.TryContinueStory;
        
        instance.ContinueStory();
    }
    
    private void ExitDialogueMode()
    {
        textQueue.Clear();
        
        isDialoguePlaying = false;
        dialogueCanvas.SetActive(false);
        dialogueMarkdownRenderer.Source = string.Empty;
        
        InputService.OnPress -= TryContinueStory;
    }

    private void TryContinueStory(Vector2 _)
    {
        ContinueStory();
    }

    private void ContinueStory()
    {
        if (displayLineCoroutine !=null)
        {
            StopCoroutine(displayLineCoroutine);
            displayLineCoroutine = null;
            dialogueMarkdownRenderer.Source = completeText;
            return;
        }

        if (textQueue.Count <= 0)
        {
            ExitDialogueMode();
            return;
        }
        
        displayLineCoroutine = StartCoroutine(DisplayLineRoutine(textQueue.Dequeue()));
    }
    
    private IEnumerator DisplayLineRoutine(string line)
    {
        openTags.Clear();
        closeTags.Clear();
        sourceText = string.Empty;
        dialogueMarkdownRenderer.Source = string.Empty;
        var settings = dialogueMarkdownRenderer.RenderSettings;
        completeText = line;
        
        for (var index = 0; index < line.Length; index++)
        {
            sourceText += CharacterToAppend();

            var suffixesText = closeTags.Aggregate(string.Empty, (current, s) => current + s);

            dialogueMarkdownRenderer.Source = $"{sourceText}{suffixesText}";
            
            yield return new WaitForSeconds(typingSpeed);
            
            string CharacterToAppend()
            {
                var letter = line[index];
                var characterToAppend = letter.ToString();
                var tempTag = characterToAppend;
                
                if (letter == ' ')
                {
                    index++;
                    characterToAppend += CharacterToAppend();
                    return characterToAppend;
                }
                
                if (characterToAppend.IsInTag(settings))
                {
                    if (CheckForMultiCharTag(out characterToAppend))
                    {
                        index += characterToAppend.Length - 1;
                    }
                }
                
                if (characterToAppend.IsOpenTag(settings, out var closeTag) && !openTags.Contains(characterToAppend))
                {
                    index++;
                    openTags.Add(characterToAppend);
                    closeTags.Add(closeTag);
                    characterToAppend += CharacterToAppend();
                    return characterToAppend;
                }
                
                if (characterToAppend.IsCloseTag(settings, out var openTag))
                {
                    openTags.Remove(openTag);
                    closeTags.Remove(characterToAppend);
                    return characterToAppend;
                }
                
                return characterToAppend;

                bool CheckForMultiCharTag(out string markdownTag)
                {
                    markdownTag = string.Empty;
                    while(tempTag.IsInTag(settings))
                    {
                        if (index + tempTag.Length >= line.Length)
                        {
                            return false;
                        }
                        
                        tempTag += line[index + tempTag.Length];
                    }

                    markdownTag = tempTag.Remove(tempTag.Length - 1, 1);
                    return markdownTag.IsInTag(settings);
                }
            }
        }

        displayLineCoroutine = null;
        dialogueMarkdownRenderer.Source = completeText;
    }
}