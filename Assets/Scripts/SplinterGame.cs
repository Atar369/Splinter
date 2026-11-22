using System;
using System.Collections.Generic;
using UnityEngine;

public class SplinterGame : MonoBehaviour
{
    private enum GameScreen
    {
        Opening,
        ChapterBrief,
        Puzzle,
        Meeting,
        Complete,
        Credits
    }

    private struct Chapter
    {
        public string Name;
        public string Role;
        public string Brief;
        public string Cost;
        public string Strength;
        public string Meeting;
        public string Objective;
        public Color Accent;
    }

    private static readonly Chapter[] Chapters =
    {
        new Chapter
        {
            Name = "Lantern Market",
            Role = "Joey · red-green colour vision deficiency",
            Brief = "Red and green merge into a single grey-green tone. The hidden doorway stops looking hidden.",
            Cost = "Traffic-light meanings become unreliable.",
            Strength = "Patterns that camouflage a doorway become visible.",
            Meeting = "“My vision is different, not empty. Tell me what a sign means instead of assuming I guessed.”",
            Objective = "Choose the door revealed by collapsing red and green.",
            Accent = new Color(0.96f, 0.72f, 0.28f)
        },
        new Chapter
        {
            Name = "Silent Harbour",
            Role = "Sam · hearing loss and tinnitus",
            Brief = "The harbour gives up sound and becomes a map of light, movement and floor vibration.",
            Cost = "Audio warnings cannot guide you.",
            Strength = "Visual attention remains clear during the storm.",
            Meeting = "“Being quiet does not make a person absent. Face me, use a clear cue, and let me contribute.”",
            Objective = "Read the harbour, not the missing alarm.",
            Accent = new Color(0.35f, 0.74f, 0.82f)
        },
        new Chapter
        {
            Name = "Tilting Library",
            Role = "Lin · vestibular disorder and vertigo",
            Brief = "The world sways. Hurry is dangerous; measured movement reveals a stable route.",
            Cost = "Rapid movement and unstable views cause disorientation.",
            Strength = "Careful observation exposes places others rush past.",
            Meeting = "“Slowness is not a lack of ability. Give me a handhold and time to orient.”",
            Objective = "Anchor yourself until the library route becomes stable.",
            Accent = new Color(0.65f, 0.48f, 0.82f)
        },
        new Chapter
        {
            Name = "Shifting Archive",
            Role = "Ray · dyslexia",
            Brief = "Moving text is unreliable. Shapes, sequence and spatial memory carry the meaning.",
            Cost = "Written instructions rotate and swap positions.",
            Strength = "Alternative representations reveal the structure of a puzzle.",
            Meeting = "“I work through ideas differently. A picture or a shape can make the whole page clear.”",
            Objective = "Use the picture that remains constant beneath shifting text.",
            Accent = new Color(0.42f, 0.78f, 0.54f)
        }
    };

    private GameScreen screen = GameScreen.Opening;
    private int chapterIndex;
    private int bossStage;
    private float holdProgress;
    private bool holdComplete;
    private float worldTime;
    private bool wrongAnswer;
    private bool firstRun = true;

    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle buttonStyle;
    private GUIStyle mutedStyle;
    private GUIStyle accentStyle;
    private Texture2D pixel;
    private Texture2D panelTexture;
    private Texture2D accentTexture;
    private Texture2D dollTexture;
    private static SplinterGame instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateGame()
    {
        if (FindObjectOfType<SplinterGame>() != null)
        {
            return;
        }

        var root = new GameObject("Splinter");
        root.AddComponent<SplinterGame>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (Camera.main == null)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.065f);
        }

        pixel = MakeTexture(1, 1, Color.white);
        panelTexture = MakeTexture(1, 1, new Color(0.075f, 0.09f, 0.12f, 0.96f));
        accentTexture = MakeTexture(1, 1, Chapters[0].Accent);
        dollTexture = MakeTexture(1, 1, new Color(0.82f, 0.72f, 0.55f));
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        DestroyCreatedTextures();
    }

    private void DestroyCreatedTextures()
    {
        if (pixel != null) Destroy(pixel);
        if (panelTexture != null) Destroy(panelTexture);
        if (accentTexture != null) Destroy(accentTexture);
        if (dollTexture != null) Destroy(dollTexture);

        pixel = null;
        panelTexture = null;
        accentTexture = null;
        dollTexture = null;
    }

    private void BuildStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 34;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.white;
        titleStyle.wordWrap = true;

        bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 17;
        bodyStyle.normal.textColor = new Color(0.86f, 0.88f, 0.91f);
        bodyStyle.wordWrap = true;

        smallStyle = new GUIStyle(GUI.skin.label);
        smallStyle.fontSize = 13;
        smallStyle.normal.textColor = new Color(0.68f, 0.72f, 0.78f);
        smallStyle.wordWrap = true;

        mutedStyle = new GUIStyle(GUI.skin.label);
        mutedStyle.fontSize = 15;
        mutedStyle.normal.textColor = new Color(0.62f, 0.66f, 0.72f);
        mutedStyle.wordWrap = true;

        accentStyle = new GUIStyle(GUI.skin.label);
        accentStyle.fontSize = 13;
        accentStyle.fontStyle = FontStyle.Bold;
        accentStyle.normal.textColor = CurrentAccent();
        accentStyle.wordWrap = true;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 16;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;
        buttonStyle.normal.background = panelTexture;
        buttonStyle.hover.background = panelTexture;
        buttonStyle.active.background = panelTexture;
        buttonStyle.padding = new RectOffset(14, 14, 10, 10);
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        var texture = new Texture2D(width, height);
        var pixels = new Color[width * height];
        for (var i = 0; i < pixels.Length; i++) pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private Color CurrentAccent()
    {
        return screen == GameScreen.Complete && chapterIndex >= Chapters.Length
            ? new Color(0.92f, 0.57f, 0.42f)
            : chapterIndex < Chapters.Length ? Chapters[Mathf.Clamp(chapterIndex, 0, Chapters.Length - 1)].Accent : Color.white;
    }

    private void Update()
    {
        worldTime += Time.unscaledDeltaTime;

        if (screen == GameScreen.Puzzle && chapterIndex == 2)
        {
            var anchor = Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.UpArrow);
            if (anchor)
            {
                holdProgress += Time.deltaTime;
            }
            else
            {
                holdProgress = Mathf.Max(0f, holdProgress - Time.deltaTime * 0.45f);
            }

            if (holdProgress >= 3f)
            {
                holdComplete = true;
            }
        }

        var sway = Camera.main;
        if (chapterIndex == 2 && screen == GameScreen.Puzzle && holdComplete)
        {
            if (sway != null)
                sway.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(worldTime * 0.8f) * 0.3f);
        }
        else if (sway != null && sway.transform.localRotation != Quaternion.identity)
        {
            sway.transform.localRotation = Quaternion.identity;
        }
        else if (holdComplete)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void OnGUI()
    {
        if (titleStyle == null)
        {
            BuildStyles();
        }

        GUI.color = new Color(0.035f, 0.045f, 0.065f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pixel, ScaleMode.StretchToFill, false);
        GUI.color = Color.white;

        DrawBackground();

        var panel = new Rect(34, 34, Screen.width - 68, Screen.height - 68);
        GUI.DrawTexture(panel, panelTexture);
        GUI.DrawTexture(new Rect(panel.x, panel.y, 5, panel.height), pixel);

        switch (screen)
        {
            case GameScreen.Opening:
                DrawOpening(panel);
                break;
            case GameScreen.ChapterBrief:
                DrawChapterBrief(panel);
                break;
            case GameScreen.Puzzle:
                DrawPuzzle(panel);
                break;
            case GameScreen.Meeting:
                DrawMeeting(panel);
                break;
            case GameScreen.Complete:
                DrawComplete(panel);
                break;
            case GameScreen.Credits:
                DrawCredits(panel);
                break;
        }
    }

    private void DrawBackground()
    {
        var accent = CurrentAccent();
        var glow = Mathf.Sin(worldTime * 0.45f) * 0.04f + 0.08f;
        GUI.color = new Color(accent.r, accent.g, accent.b, glow);
        for (var i = 0; i < 8; i++)
        {
            var x = ((i * 181 + worldTime * (8 + i)) % (Screen.width + 180)) - 90f;
            var y = 90 + i * 87 + Mathf.Sin(worldTime * 0.3f + i) * 24;
            GUI.DrawTexture(new Rect(x, y, 120 + i * 8, 2), pixel);
        }
        GUI.color = Color.white;
    }

    private void DrawOpening(Rect panel)
    {
        GUI.DrawTexture(new Rect(panel.x + 70, panel.y + 80, 190, 260), dollTexture);
        DrawDollFace(panel.x + 112, panel.y + 155);

        var contentX = panel.x + 310;
        GUI.Label(new Rect(contentX, panel.y + 70, panel.width - 350, 60),
            "SPLINTER", titleStyle);

        accentStyle.normal.textColor = new Color(0.88f, 0.67f, 0.41f);
        GUI.Label(new Rect(contentX, panel.y + 125, panel.width - 350, 30),
            "A PERSPECTIVE-SWAP PUZZLE", accentStyle);

        GUI.Label(new Rect(contentX, panel.y + 185, panel.width - 350, 110),
            "Kai wakes as a blank wooden doll.\nBehind every failure stands a Mask that knows his worst habits.",
            bodyStyle);

        if (GUI.Button(new Rect(contentX, panel.y + 310, 220, 48),
            firstRun ? "ENTER THE WORKSHOP" : "CONTINUE", buttonStyle))
        {
            screen = GameScreen.ChapterBrief;
        }

        GUI.Label(new Rect(contentX, panel.y + 375, panel.width - 350, 80),
            "Content note: This is a fictional, symbolic experience—not a complete simulation of any condition.",
            smallStyle);
    }

    private void DrawDollFace(float x, float y)
    {
        GUI.color = new Color(0.12f, 0.12f, 0.13f);
        GUI.DrawTexture(new Rect(x, y, 18, 18), pixel);
        GUI.DrawTexture(new Rect(x + 54, y, 18, 18), pixel);
        GUI.color = Color.white;
    }

    private void DrawChapterBrief(Rect panel)
    {
        if (chapterIndex >= Chapters.Length)
        {
            DrawWorkshopBrief(panel);
            return;
        }

        var chapter = Chapters[chapterIndex];
        accentStyle.normal.textColor = chapter.Accent;
        GUI.Label(new Rect(panel.x + 50, panel.y + 48, panel.width - 100, 28),
            $"CHAPTER {chapterIndex + 1} · {chapter.Role}", accentStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 82, panel.width - 100, 62),
            chapter.Name, titleStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 155, panel.width - 100, 70),
            chapter.Brief, bodyStyle);

        var left = panel.x + 50;
        var right = panel.x + panel.width * 0.52f;
        GUI.Label(new Rect(left, panel.y + 245, panel.width * 0.42f, 20), "THE COST", accentStyle);
        GUI.Label(new Rect(left, panel.y + 270, panel.width * 0.42f, 55), chapter.Cost, bodyStyle);
        GUI.Label(new Rect(right, panel.y + 245, panel.width * 0.42f, 20), "THE DIFFERENT WAY", accentStyle);
        GUI.Label(new Rect(right, panel.y + 270, panel.width * 0.42f, 55), chapter.Strength, bodyStyle);

        if (GUI.Button(new Rect(panel.x + 50, panel.y + panel.height - 85, 210, 48),
            "BEGIN MEMORY", buttonStyle))
        {
            wrongAnswer = false;
            holdProgress = 0f;
            holdComplete = false;
            screen = GameScreen.Puzzle;
        }
    }

    private void DrawWorkshopBrief(Rect panel)
    {
        GUI.Label(new Rect(panel.x + 50, panel.y + 48, panel.width - 100, 28),
            "FINAL CHAPTER · KAI", accentStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 82, panel.width - 100, 62),
            "The Workshop", titleStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 155, panel.width - 100, 100),
            "The Mask has gathered every rule, every taunt and every memory. It can only be faced with what Kai learned.",
            bodyStyle);

        if (GUI.Button(new Rect(panel.x + 50, panel.y + 310, 210, 48),
            "FACE THE MASK", buttonStyle))
        {
            bossStage = 0;
            wrongAnswer = false;
            screen = GameScreen.Puzzle;
        }
    }

    private void DrawPuzzle(Rect panel)
    {
        if (chapterIndex >= Chapters.Length)
        {
            DrawBossPuzzle(panel);
            return;
        }

        var chapter = Chapters[chapterIndex];
        GUI.Label(new Rect(panel.x + 50, panel.y + 38, panel.width - 100, 28),
            chapter.Objective, accentStyle);

        switch (chapterIndex)
        {
            case 0:
                DrawLanternPuzzle(panel);
                break;
            case 1:
                DrawHarbourPuzzle(panel);
                break;
            case 2:
                DrawLibraryPuzzle(panel);
                break;
            case 3:
                DrawArchivePuzzle(panel);
                break;
        }

        if (wrongAnswer)
        {
            GUI.Label(new Rect(panel.x + 50, panel.y + panel.height - 58, panel.width - 100, 34),
                "The Mask: “Try again. This is how every classmate felt explaining themselves to you.”",
                mutedStyle);
        }
    }

    private void DrawLanternPuzzle(Rect panel)
    {
        var y = panel.y + 115;
        var totalWidth = panel.width - 100;
        var cardWidth = (totalWidth - 40) / 3f;

        var cards = new[]
        {
            new { Label = "RED", Color = new Color(0.85f, 0.18f, 0.18f), Correct = false },
            new { Label = "BLUE", Color = new Color(0.18f, 0.43f, 0.95f), Correct = true },
            new { Label = "GREEN", Color = new Color(0.32f, 0.78f, 0.38f), Correct = false }
        };

        for (var i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            var x = panel.x + 50 + i * (cardWidth + 20);
            var old = GUI.color;
            GUI.color = card.Color;
            GUI.DrawTexture(new Rect(x, y, cardWidth, 210), pixel);
            GUI.color = old;

            GUI.Label(new Rect(x + 14, y + 14, cardWidth - 28, 32), card.Label, smallStyle);
            DrawDoor(x + cardWidth * 0.5f - 45, y + 62, i == 1);

            if (GUI.Button(new Rect(x + 18, y + 225, cardWidth - 36, 46),
                i == 1 ? "CHOOSE THE BLUE DOOR" : "CHOOSE THIS DOOR", buttonStyle))
            {
                if (card.Correct) FinishPuzzle();
                else wrongAnswer = true;
            }
        }

        GUI.Label(new Rect(panel.x + 50, panel.y + panel.height - 92, panel.width - 100, 24),
            "In deuteranopia the red and green signals merge; the blue doorway remains distinct.",
            smallStyle);
    }

    private void DrawDoor(float x, float y, bool hidden)
    {
        GUI.color = hidden ? new Color(0.12f, 0.16f, 0.23f) : new Color(0.2f, 0.14f, 0.1f);
        GUI.DrawTexture(new Rect(x, y, 90, 130), pixel);
        GUI.color = hidden ? new Color(0.95f, 0.78f, 0.32f) : new Color(0.05f, 0.05f, 0.06f);
        GUI.DrawTexture(new Rect(x + 12, y + 12, 66, 118), pixel);
        GUI.color = Color.white;
    }

    private void DrawHarbourPuzzle(Rect panel)
    {
        GUI.Label(new Rect(panel.x + 50, panel.y + 96, panel.width - 100, 50),
            "A storm silences the warning horn.", bodyStyle);

        var options = new[]
        {
            "FOLLOW THE FADING HORN",
            "WATCH THE LIGHTHOUSE SEQUENCE",
            "READ THE FLOOR VIBRATION"
        };

        for (var i = 0; i < options.Length; i++)
        {
            var y = panel.y + 170 + i * 62;
            var pulse = Mathf.Sin(worldTime * 4f + i) > 0.2f;
            if (i == 2 && pulse)
            {
                GUI.DrawTexture(new Rect(panel.x + 54, y + 16, 18, 18), pixel);
            }
            if (GUI.Button(new Rect(panel.x + 90, y, panel.width - 140, 46), options[i], buttonStyle))
            {
                if (i == 2) FinishPuzzle();
                else wrongAnswer = true;
            }
        }

        GUI.Label(new Rect(panel.x + 50, panel.y + panel.height - 90, panel.width - 100, 44),
            "Sam cannot hear the storm. A visual path and safe vibration guide him through.",
            smallStyle);
    }

    private void DrawLibraryPuzzle(Rect panel)
    {
        var amount = holdComplete ? 0f : Mathf.Sin(worldTime * 1.8f) * 18f;
        GUI.Label(new Rect(panel.x + 50 + amount, panel.y + 110, panel.width - 100, 60),
            "“The exit is near the oldest shelf.”", bodyStyle);

        GUI.Label(new Rect(panel.x + 50, panel.y + 190, panel.width - 100, 28),
            holdComplete ? "HANDHOLD FOUND · ROUTE STABLE" : "HOLD R / ↑ TO ANCHOR",
            accentStyle);

        GUI.DrawTexture(new Rect(panel.x + 50, panel.y + 225, panel.width - 100, 18), panelTexture);
        GUI.DrawTexture(new Rect(panel.x + 50, panel.y + 225, (panel.width - 100) * Mathf.Clamp01(holdProgress / 3f), 18), pixel);

        if (holdComplete)
        {
            if (GUI.Button(new Rect(panel.x + 50, panel.y + 285, 210, 48),
                "FOLLOW THE STABLE ROUTE", buttonStyle))
            {
                FinishPuzzle();
            }
        }

        GUI.Label(new Rect(panel.x + 50, panel.y + panel.height - 78, panel.width - 100, 32),
            "Pacing and orientation are adaptations, not weakness.",
            smallStyle);
    }

    private void DrawArchivePuzzle(Rect panel)
    {
        GUI.Label(new Rect(panel.x + 50, panel.y + 96, panel.width - 100, 50),
            "The instruction is moving. Choose the image that stays consistent.", bodyStyle);

        var symbols = new[] { "A", "◆", "3", "?" };
        var drift = Mathf.Sin(worldTime * 2.2f) * 7f;
        var total = panel.width - 100;
        var cardWidth = (total - 60) / 4f;

        for (var i = 0; i < symbols.Length; i++)
        {
            var x = panel.x + 50 + i * (cardWidth + 20);
            var y = panel.y + 180 + (i % 2 == 0 ? drift : -drift);
            GUI.Label(new Rect(x, y, cardWidth, 90), symbols[i], titleStyle);

            if (GUI.Button(new Rect(x, y + 100, cardWidth, 46),
                i == 1 ? "PICTURE" : "TEXT", buttonStyle))
            {
                if (i == 1) FinishPuzzle();
                else wrongAnswer = true;
            }
        }
    }

    private void DrawBossPuzzle(Rect panel)
    {
        GUI.Label(new Rect(panel.x + 50, panel.y + 42, panel.width - 100, 28),
            $"THE MASK · SEAL {bossStage + 1} OF 3", accentStyle);

        if (bossStage == 0)
        {
            GUI.Label(new Rect(panel.x + 50, panel.y + 100, panel.width - 100, 58),
                "“Which way is clear?”", titleStyle);
            DrawThreeBossChoices(panel, "DISTINCT BLUE", "LOUDER TAUNT", "FORCE THE DOOR");
        }
        else if (bossStage == 1)
        {
            GUI.Label(new Rect(panel.x + 50, panel.y + 100, panel.width - 100, 58),
                "“The warning is gone. What remains?”", titleStyle);
            DrawThreeBossChoices(panel, "VISIBLE CUES", "MORE NOISE", "ATTACK THE MASK");
        }
        else
        {
            GUI.Label(new Rect(panel.x + 50, panel.y + 100, panel.width - 100, 58),
                "“Who can help make this route fair?”", titleStyle);
            DrawThreeBossChoices(panel, "COOPERATE", "KEEP RUNNING", "BREAK THE RULES");
        }
    }

    private void DrawThreeBossChoices(Rect panel, string first, string second, string third)
    {
        var choices = new[] { first, second, third };
        for (var i = 0; i < choices.Length; i++)
        {
            var y = panel.y + 190 + i * 62;
            if (GUI.Button(new Rect(panel.x + 50, y, panel.width - 100, 46), choices[i], buttonStyle))
            {
                if (i == 0)
                {
                    bossStage++;
                    wrongAnswer = false;
                    if (bossStage >= 3) screen = GameScreen.Meeting;
                }
                else
                {
                    wrongAnswer = true;
                }
            }
        }

        if (wrongAnswer)
        {
            GUI.Label(new Rect(panel.x + 50, panel.y + panel.height - 70, panel.width - 100, 34),
                "The Mask feeds on Kai’s old reflex: isolate, dominate, ignore.",
                mutedStyle);
        }
    }

    private void FinishPuzzle()
    {
        wrongAnswer = false;
        screen = GameScreen.Meeting;
    }

    private void DrawMeeting(Rect panel)
    {
        if (chapterIndex >= Chapters.Length)
        {
            GUI.Label(new Rect(panel.x + 50, panel.y + 45, panel.width - 100, 28),
                "THE MASK COMES OFF", accentStyle);
            GUI.Label(new Rect(panel.x + 50, panel.y + 85, panel.width - 100, 62),
                "It was Kai.", titleStyle);
            GUI.Label(new Rect(panel.x + 50, panel.y + 165, panel.width - 100, 110),
                "The taunts were Kai’s own words. Every role he mocked was waiting to be understood from the other side.",
                bodyStyle);

            if (GUI.Button(new Rect(panel.x + 50, panel.y + 310, 210, 48),
                "BECOME HUMAN", buttonStyle))
            {
                screen = GameScreen.Complete;
            }
            return;
        }

        var chapter = Chapters[chapterIndex];
        GUI.Label(new Rect(panel.x + 50, panel.y + 45, panel.width - 100, 28),
            "A VOICE IN THE MEMORY", accentStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 82, panel.width - 100, 62),
            chapter.Role, titleStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 165, panel.width - 100, 100),
            chapter.Meeting, bodyStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 280, panel.width - 100, 48),
            "A new carving appears on the doll.", mutedStyle);

        if (GUI.Button(new Rect(panel.x + 50, panel.y + panel.height - 85, 210, 48),
            "NEXT MEMORY", buttonStyle))
        {
            chapterIndex++;
            firstRun = false;
            if (chapterIndex >= Chapters.Length)
            {
                screen = GameScreen.ChapterBrief;
            }
            else
            {
                screen = GameScreen.ChapterBrief;
            }
        }
    }

    private void DrawComplete(Rect panel)
    {
        GUI.Label(new Rect(panel.x + 50, panel.y + 42, panel.width - 100, 28),
            "IDENTITY RESTORED", accentStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 82, panel.width - 100, 62),
            "Kai becomes whole.", titleStyle);

        GUI.DrawTexture(new Rect(panel.x + 50, panel.y + 160, 220, 220), dollTexture);
        GUI.DrawTexture(new Rect(panel.x + 126, panel.y + 200, 18, 18), pixel);
        GUI.DrawTexture(new Rect(panel.x + 176, panel.y + 200, 18, 18), pixel);

        GUI.Label(new Rect(panel.x + 310, panel.y + 165, panel.width - 360, 140),
            "The body is not a list of obstacles overcome.\nIt is the shape of four people Kai had refused to understand.",
            bodyStyle);

        if (GUI.Button(new Rect(panel.x + 310, panel.y + 320, 210, 48),
            "CONTINUE", buttonStyle))
        {
            screen = GameScreen.Credits;
        }
    }

    private void DrawCredits(Rect panel)
    {
        GUI.Label(new Rect(panel.x + 50, panel.y + 42, panel.width - 100, 28),
            "SPLINTER", accentStyle);
        GUI.Label(new Rect(panel.x + 50, panel.y + 82, panel.width - 100, 62),
            "A playable design prototype", titleStyle);

        GUI.Label(new Rect(panel.x + 50, panel.y + 165, panel.width - 100, 150),
            "Four conditions. One continuous moral perspective.\n\n" +
            "This prototype uses simplified symbolic mechanics and must not replace lived experience, " +
            "medical guidance or consultation with people who have these conditions.",
            bodyStyle);

        if (GUI.Button(new Rect(panel.x + 50, panel.y + 345, 210, 48),
            "RETURN TO START", buttonStyle))
        {
            chapterIndex = 0;
            bossStage = 0;
            firstRun = true;
            screen = GameScreen.Opening;
        }
    }
}
