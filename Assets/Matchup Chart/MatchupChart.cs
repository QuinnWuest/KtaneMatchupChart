using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class MatchupChart : MonoBehaviour {
    // Module info
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;

    public KMSelectable[] TypeDisplays;
    public KMSelectable[] Arrows;

    public MeshRenderer[] TypeDisplayScreens;
    public MeshRenderer InputDisplayScreen;
    public MeshRenderer[] MatchupTiles;
    public TextMesh[] MatchupTileTexts;

    public Material[] TypeMaterials;
    public Material EmptyMaterial;
    public Material[] MatchupMaterials;


    // Solving info
    private readonly string[] TYPE_NAMES = { "Normal", "Fighting", "Flying", "Poison", "Ground", "Rock", "Bug", "Ghost", "Steel",
        "Fire", "Water", "Grass", "Electric", "Psychic", "Ice", "Dragon", "Dark", "Fairy", "Stellar" };
    private readonly string[] MATCHUP_TEXTS = { ".", "✓", "✗", "Ø" };

    private readonly int NO_TYPE = 18;

    private int[][] TYPE_SELECTION = new int[3][];
    private int[][] TYPE_CHART = new int[19][];

    private int[] selectedTypes = new int[8];
    private int[] solutionTypes = new int[8];
    private bool[] screenLocked = new bool[8];
    private int selectedIndex = 0;
    private int[] matchupValues = new int[16];

    private int lastDigit = 0;
    private int typeTable = 0;

    private bool canPress = false;
    private int typesEntered = 0;

    // Logging info
    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved = false;


    // Ran as bomb loads
    private void Awake() {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < TypeDisplays.Length; i++) {
            int j = i;
            TypeDisplays[i].OnInteract += delegate () { TypeDisplayPressed(j); return false; };
        }
        
        for (int i = 0; i < Arrows.Length; i++) {
            int j = i;
            Arrows[i].OnInteract += delegate () { ArrowButtonPressed(j); return false; };
        }
	}

    // Gets information
    private void Start() {
        lastDigit = Bomb.GetSerialNumberNumbers().Last();
        InitTypes();

        selectedIndex = UnityEngine.Random.Range(0, TYPE_SELECTION[typeTable].Length);
        UpdateInputDisplay();

        for (int i = 0; i < selectedTypes.Length; i++) {
            selectedTypes[i] = NO_TYPE;
            solutionTypes[i] = NO_TYPE;
            screenLocked[i] = false;
        }

        CreateGrid();

        canPress = true;
    }

    // Initiates module information
    private void InitTypes() {
        TYPE_SELECTION[0] = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15 };
        TYPE_SELECTION[1] = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        TYPE_SELECTION[2] = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };

        /* 0 = Neutral
         * 1 = Super effective
         * 2 = Not very effective
         * 3 = No effect
         */

        TYPE_CHART[0] = new int[] { 0, 0, 0, 0, 0, 2, 0, 3, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1 }; // Normal
        TYPE_CHART[1] = new int[] { 1, 0, 2, 2, 0, 1, 2, 3, 1, 0, 0, 0, 0, 2, 1, 0, 1, 2, -1 }; // Fighting
        TYPE_CHART[2] = new int[] { 0, 1, 0, 0, 0, 2, 1, 0, 2, 0, 0, 1, 2, 0, 0, 0, 0, 0, -1 }; // Flying
        TYPE_CHART[4] = new int[] { 0, 0, 3, 1, 0, 1, 2, 0, 1, 1, 0, 2, 1, 0, 0, 0, 0, 0, -1 }; // Ground
        TYPE_CHART[5] = new int[] { 0, 2, 1, 0, 2, 0, 1, 0, 2, 1, 0, 0, 0, 0, 1, 0, 0, 0, -1 }; // Rock
        TYPE_CHART[8] = new int[] { 0, 0, 0, 0, 0, 1, 0, 0, 2, 2, 2, 0, 2, 0, 1, 0, 0, 1, -1 }; // Steel
        TYPE_CHART[9] = new int[] { 0, 0, 0, 0, 0, 2, 1, 0, 1, 2, 2, 1, 0, 0, 1, 2, 0, 0, -1 }; // Fire
        TYPE_CHART[10] = new int[] { 0, 0, 0, 0, 1, 1, 0, 0, 0, 1, 2, 2, 0, 0, 0, 2, 0, 0, -1 }; // Water
        TYPE_CHART[11] = new int[] { 0, 0, 2, 2, 1, 1, 2, 0, 2, 2, 1, 2, 0, 0, 0, 2, 0, 0, -1 }; // Grass
        TYPE_CHART[12] = new int[] { 0, 0, 1, 0, 3, 0, 0, 0, 0, 0, 1, 2, 2, 0, 0, 2, 0, 0, -1 }; // Electric
        TYPE_CHART[13] = new int[] { 0, 1, 0, 1, 0, 0, 0, 0, 2, 0, 0, 0, 0, 2, 0, 0, 3, 0, -1 }; // Psychic
        TYPE_CHART[15] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 1, 0, 3, -1 }; // Dragon
        TYPE_CHART[17] = new int[] { 0, 1, 0, 2, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 1, 1, 0, -1 }; // Fairy
        TYPE_CHART[18] = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }; // Stellar

        if (lastDigit == 1) { // Gen 1
            typeTable = 0;
            Debug.LogFormat("[Matchup Chart #{0}] Last digit of the serial number is 1. Using table 1 in the manual.", moduleId);

            TYPE_CHART[3] = new int[] { 0, 0, 0, 2, 2, 2, 1, 2, 3, 0, 0, 1, 0, 0, 0, 0, 0, 1, -1 }; // Poison
            TYPE_CHART[6] = new int[] { 0, 2, 2, 1, 0, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 1, 2, -1 }; // Bug
            TYPE_CHART[7] = new int[] { 3, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 3, 0, 0, 2, 0, -1 }; // Ghost
            TYPE_CHART[14] = new int[] { 0, 0, 1, 0, 1, 0, 0, 0, 2, 0, 2, 1, 0, 0, 2, 1, 0, 0, -1 }; // Ice
            TYPE_CHART[16] = new int[] { 0, 2, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 1, 0, 0, 2, 2, -1 }; // Dark
        }

        else if (lastDigit >= 2 && lastDigit <= 5) { // Gen 2-5
            typeTable = 1;
            Debug.LogFormat("[Matchup Chart #{0}] Last digit of the serial number is between 2-5. Using table 2 in the manual.", moduleId);

            TYPE_CHART[3] = new int[] { 0, 0, 0, 2, 2, 2, 0, 2, 3, 0, 0, 1, 0, 0, 0, 0, 0, 1, -1 }; // Poison
            TYPE_CHART[6] = new int[] { 0, 2, 2, 2, 0, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 1, 2, -1 }; // Bug
            TYPE_CHART[7] = new int[] { 3, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 1, 0, 0, 2, 0, -1 }; // Ghost
            TYPE_CHART[14] = new int[] { 0, 0, 1, 0, 1, 0, 0, 0, 2, 2, 2, 1, 0, 0, 2, 1, 0, 0, -1 }; // Ice
            TYPE_CHART[16] = new int[] { 0, 2, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 1, 0, 0, 2, 2, -1 }; // Dark
        }

        else { // Gen 6+
            typeTable = 2;
            Debug.LogFormat("[Matchup Chart #{0}] Last digit of the serial number doesn't meet either rule. Using table 3 in the manual.", moduleId);

            TYPE_CHART[3] = new int[] { 0, 0, 0, 2, 2, 2, 0, 2, 3, 0, 0, 1, 0, 0, 0, 0, 0, 1, -1 }; // Poison
            TYPE_CHART[6] = new int[] { 0, 2, 2, 2, 0, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 0, 1, 2, -1 }; // Bug
            TYPE_CHART[7] = new int[] { 3, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 2, 0, -1 }; // Ghost
            TYPE_CHART[14] = new int[] { 0, 0, 1, 0, 1, 0, 0, 0, 2, 2, 2, 1, 0, 0, 2, 1, 0, 0, -1 }; // Ice
            TYPE_CHART[16] = new int[] { 0, 2, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 2, 2, -1 }; // Dark
        }
    }

    // Creates the grid
    private void CreateGrid() {
        // Creates an intended solution
        var typeCount = 0;
        var typeToAdd = 0;
        var valid = true;

        do {
            valid = true;
            typeToAdd = UnityEngine.Random.Range(0, TYPE_SELECTION[typeTable].Length);

            for (int i = 0; i < typeCount; i++) {
                if (solutionTypes[i] == typeToAdd) {
                    valid = false;
                    break;
                }
            }

            if (valid) {
                solutionTypes[typeCount] = typeToAdd;
                typeCount++;
            }
        } while (typeCount < 4);

        do {
            valid = true;
            typeToAdd = UnityEngine.Random.Range(0, TYPE_SELECTION[typeTable].Length);

            for (int i = 4; i < typeCount; i++) {
                if (solutionTypes[i] == typeToAdd) {
                    valid = false;
                    break;
                }
            }

            if (valid) {
                solutionTypes[typeCount] = typeToAdd;
                typeCount++;
            }
        } while (typeCount < 8);


        // Indicates the matchups on the grid
        for (int i = 0; i < MatchupTiles.Length; i++) {
            var matchup = TYPE_CHART[solutionTypes[4 + i / 4]][solutionTypes[i % 4]];

            matchupValues[i] = matchup;
            MatchupTiles[i].material = MatchupMaterials[matchup];

            if (matchup != 0)
                MatchupTileTexts[i].text = MATCHUP_TEXTS[matchup];
        }

        // Logs the grid
        Debug.LogFormat("[Matchup Chart #{0}] The module generated as such:", moduleId);
        
        for (int i = 0; i < MatchupTiles.Length; i += 4) {
            Debug.LogFormat("[Matchup Chart #{0}] {1} {2} {3} {4}", moduleId,
                MATCHUP_TEXTS[matchupValues[i]], MATCHUP_TEXTS[matchupValues[i + 1]], MATCHUP_TEXTS[matchupValues[i + 2]], MATCHUP_TEXTS[matchupValues[i + 3]]);
        }


        // Chooses a random tile to place in the grid
        var revealed = UnityEngine.Random.Range(0, 8);
        selectedTypes[revealed] = solutionTypes[revealed];
        TypeDisplayScreens[revealed].material = TypeMaterials[solutionTypes[revealed]];
        screenLocked[revealed] = true;
        typesEntered++;

        Debug.LogFormat("[Matchup Chart #{0}] The {1} type was automatically placed into screen {2} for you.",
            moduleId, TYPE_NAMES[solutionTypes[revealed]], NameDisplay(revealed));

        // Logs the intended solution
        Debug.LogFormat("[Matchup Chart #{0}] One possible solution is: {1}, {2}, {3}, {4} across the top, and {5}, {6}, {7}, {8} on the left.",
            moduleId, TYPE_NAMES[solutionTypes[0]], TYPE_NAMES[solutionTypes[1]], TYPE_NAMES[solutionTypes[2]], TYPE_NAMES[solutionTypes[3]],
            TYPE_NAMES[solutionTypes[4]], TYPE_NAMES[solutionTypes[5]], TYPE_NAMES[solutionTypes[6]], TYPE_NAMES[solutionTypes[7]]);
    }


    // Arrow button pressed
    private void ArrowButtonPressed(int i) {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, gameObject.transform);
        Arrows[i].AddInteractionPunch(0.25f);

        if (canPress) {
            if (i == 0) { // Left
                selectedIndex--;
                if (selectedIndex < 0)
                    selectedIndex += TYPE_SELECTION[typeTable].Length;

                UpdateInputDisplay();
            }

            else { // Right
                selectedIndex++;
                selectedIndex %= TYPE_SELECTION[typeTable].Length;

                UpdateInputDisplay();
            }
        }
    }

    // Type screen pressed
    private void TypeDisplayPressed(int i) {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, gameObject.transform);
        TypeDisplays[i].AddInteractionPunch(0.5f);

        if (canPress && !moduleSolved && !screenLocked[i]) {
            // Removes a type from the screen
            if (selectedTypes[i] != NO_TYPE) {
                typesEntered--;
                Debug.LogFormat("[Matchup Chart #{0}] Removed the {1} type from screen {2}. Currenly entered types: {3}",
                    moduleId, TYPE_NAMES[selectedTypes[i]], NameDisplay(i), typesEntered);

                selectedTypes[i] = NO_TYPE;
                TypeDisplayScreens[i].material = EmptyMaterial;
            }

            // Tries to add a type to the screen
            else {
                var typeToAdd = TYPE_SELECTION[typeTable][selectedIndex];
                var valid = true;

                // Has type been used in the row/column already?
                if (i > 3) {
                    for (int j = 4; j < 8; j++) {
                        if (selectedTypes[j] == typeToAdd) {
                            valid = false;
                            break;
                        }
                    }
                }

                else {
                    for (int j = 0; j < 4; j++) {
                        if (selectedTypes[j] == typeToAdd) {
                            valid = false;
                            break;
                        }
                    }
                }

                // Can the type fit into the grid?
                if (i > 3) {
                    for (int j = 0; j < 4; j++) {
                        var foundMatchup = TYPE_CHART[typeToAdd][selectedTypes[j]];
                        if (foundMatchup != -1 && foundMatchup != matchupValues[(i - 4) * 4 + j]) {
                            valid = false;
                            break;
                        }
                    }
                }

                else {
                    for (int j = 4; j < 8; j++) {
                        var foundMatchup = TYPE_CHART[selectedTypes[j]][typeToAdd];
                        if (foundMatchup != -1 && foundMatchup != matchupValues[(j - 4) * 4 + i]) {
                            valid = false;
                            break;
                        }
                    }
                }

                // Success
                if (valid) {
                    typesEntered++;
                    Debug.LogFormat("[Matchup Chart #{0}] Successfully added the {1} type to screen {2}. Currenly entered types: {3}",
                        moduleId, TYPE_NAMES[typeToAdd], NameDisplay(i), typesEntered);

                    selectedTypes[i] = typeToAdd;
                    TypeDisplayScreens[i].material = TypeMaterials[typeToAdd];

                    // All screens have been entered
                    if (typesEntered >= 8) {
                        Debug.LogFormat("[Matchup Chart #{0}] All types have been added successfully! Module solved!", moduleId);
                        Audio.PlaySoundAtTransform("pokeball_catch", transform);
                        moduleSolved = true;
                        GetComponent<KMBombModule>().HandlePass();
                    }
                }

                // Failure
                else {
                    Debug.LogFormat("[Matchup Chart #{0}] Could not add the {1} type to screen {2}. Strike!",
                        moduleId, TYPE_NAMES[typeToAdd], NameDisplay(i));

                    GetComponent<KMBombModule>().HandleStrike();
                }
            }
        }
    }


    // Updates the screen on the input display
    private void UpdateInputDisplay() {
        InputDisplayScreen.material = TypeMaterials[TYPE_SELECTION[typeTable][selectedIndex]];
    }

    // Returns a screen name for logging
    private string NameDisplay(int i) {
        switch (i) {
            case 1: return "Column-2";
            case 2: return "Column-3";
            case 3: return "Column-4";
            case 4: return "Row-1";
            case 5: return "Row-2";
            case 6: return "Row-3";
            case 7: return "Row-4";
            default: return "Column-1";
        }
    }
}