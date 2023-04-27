using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class RoguelikeGame : MonoBehaviour {

	static int _moduleIdCounter = 1;
	int _moduleID = 0;
	bool solved = false;

	public KMBombModule Module;
	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMSelectable[] Buttons;
	public TextMesh[] ButtonTexts;
	public Material ScreenMat;


	private string[] items = { "Dagger", "GSword", "Axe", "Tablet", "PFlask", "Guide", "Tome", "Cloak", "Quiver", "Bow" };
	private List<string> itemsUsed = new List<string>();
	private List<string> itemsUsedOriginal = new List<string>();
	private List<string> itemsToSwap = new List<string>();
	private List<string> invItems = new List<string>();
	private List<string> invItemsOriginal = new List<string>();


	private int beginningDP;
	List<string> inputtedAnswers = new List<string>();
	private bool isDecline = false;

	private string Swap1, Swap2;

	public Texture[] _textures;

	public Renderer rendererShop1;
	public Renderer rendererShop2;
	public Renderer rendererInv1;
	public Renderer rendererInv2;
	public Renderer rendererInv3;
	public Renderer rendererInv4;

	public Color[] textColors;

	string[] ValidNames = { "dagger", "sword", "axe", "tablet", "flask", "guide", "tome", "cloak", "quiver", "bow" };
	List<string> ItemsAvailableRenamed = new List<string>();


	void Awake()
    {
		_moduleID = _moduleIdCounter++;
		itemsToSwap.Add("");
		itemsToSwap.Add("");
    }


	// Use this for initialization
	void Start () {
		
		for (int btn = 0; btn < Buttons.Length; btn++)
		{
			Buttons[btn].OnInteract = ButtonPressed(btn);
		}
        for (int btntext = 0; btntext < ButtonTexts.Length; btntext++)
        {
			int itemToUse = UnityEngine.Random.Range(0, 10);
			ButtonTexts[btntext].text = items[itemToUse];
			while (itemsUsed.Contains(items[itemToUse]))
            {
				itemToUse = UnityEngine.Random.Range(0, 10);
				ButtonTexts[btntext].text = items[itemToUse];
			}
			itemsUsed.Add(items[itemToUse]);
			ItemsAvailableRenamed.Add(ValidNames[itemToUse]);



		}

		rendererShop1.material.mainTexture = _textures.First(t => t.name == itemsUsed[0]);
		rendererShop2.material.mainTexture = _textures.First(t => t.name == itemsUsed[1]);
		rendererInv1.material.mainTexture = _textures.First(t => t.name == itemsUsed[2]);
		rendererInv2.material.mainTexture = _textures.First(t => t.name == itemsUsed[3]);
		rendererInv3.material.mainTexture = _textures.First(t => t.name == itemsUsed[4]);
		rendererInv4.material.mainTexture = _textures.First(t => t.name == itemsUsed[5]);


		for (int i = 0; i < 4; i++)
		{
			invItems.Add(itemsUsed[i + 2]);
		}
		beginningDP = DetermineInvDps();
		invItemsOriginal = new List<string>(invItems);
		itemsUsedOriginal = new List<string>(itemsUsed);
		Log("The shop items are {0} and {1}", itemsUsed[0], itemsUsed[1]);
		Log("Your inventory items are {0}, {1}, {2} and {3}", itemsUsed[2], itemsUsed[3], itemsUsed[4], itemsUsed[5]);
		Log("The beginning inventory DPS is {0}.", beginningDP); 

	}


	private int DetermineCorrectDps()
    {

		int currentHighest = 0;
        for (int shopItem = 0; shopItem < 2; shopItem++)
        {
            for (int invItem = 0; invItem < 4; invItem++)
            {
				SwapItems(shopItem, invItem + 2);
				GetInvItems();
				int current = DetermineInvDps();
				itemsUsed = new List<string>(itemsUsedOriginal);
				invItems = new List<string>(invItemsOriginal);
				if (current > currentHighest)
                {
					currentHighest = current;
					Swap1 = itemsUsed[shopItem];
					Swap2 = itemsUsed[invItem + 2];
					 

                }

			}
        }
		if (currentHighest <= beginningDP)
        {
			isDecline = true;
        }
		return currentHighest;
    }

	void Update()
    {
	
    }

	private int DetermineInvDps()
	{
		int _as = 1;
		int _ap = 1;
		int calculatedDps = 0;
		if (invItems.Contains("GSword"))
		{
			_as -= 2;
			_ap += 20;
			int gSwordPlace = invItems.IndexOf("GSword");
			if (gSwordPlace == 3)
            {
				invItems[0] = "X";
            } else
            {
				invItems[gSwordPlace + 1] = "X";
            }

		}
		if (invItems.Contains("Dagger"))
		{
			_as += 2;
			if (Bomb.GetOnIndicators().Count() == 0)
			{
				_ap += Bomb.GetIndicators().Count() * 4;
			}
		}
		if (invItems.Contains("Guide"))
        {
			_ap += (Bomb.GetSerialNumberLetters().Count(i => i == 'G') + 
				Bomb.GetSerialNumberLetters().Count(i => i == 'A') +
				Bomb.GetSerialNumberLetters().Count(i => i == 'M') +
				Bomb.GetSerialNumberLetters().Count(i => i == 'E')) * 5;

			_ap += (Bomb.GetModuleNames().Count(i => i.ToLower().Contains("game"))*20 - 20);

        }
		if (invItems.Contains("PFlask"))
        {
			calculatedDps += 25;
			if (invItems.Contains("Dagger"))
            {
				calculatedDps += 25;
            }
        }
		if (invItems.Contains("Tome"))
        {
			_ap += Convert.ToInt32(Math.Pow(2, Bomb.GetSerialNumberLetters().Count()));
			_as += Bomb.GetSerialNumberNumbers().Count();
        }
		if (invItems.Contains("Cloak"))
        {
			_ap += (int)Bomb.GetTime() / 60;
        }
		if (invItems.Contains("Quiver"))
        {
			_as += 4;
			_as -= Bomb.GetStrikes() * 2;
        }
		if (invItems.Contains("Bow"))
        {
			_as += Bomb.GetModuleNames().Count() % 7;
        }

		if (invItems.Contains("Axe"))
        {
			_ap += 10;
			string chars = "";
            foreach (string indInd in Bomb.GetIndicators())
            {
                foreach (char indLetter in indInd)
                {
					if (!(chars.Contains(indLetter)))
                    {
						chars += indLetter;
                    }
                }
            }
			_as = chars.Length;
			
        }
		if (invItems.Contains("Tablet"))
		{
			int prod = 1;
			foreach (int snNum in Bomb.GetSerialNumberNumbers())
			{
				prod *= snNum;
			}
			_ap = prod;
		}
		int _dps = calculatedDps + (_as * _ap);
		return _dps;
	}

    private KMSelectable.OnInteractHandler ButtonPressed(int btn)
    {
        return delegate
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[btn].transform);
            Buttons[btn].AddInteractionPunch();
			if (solved == true)
            {
				return false;
            }
			if (btn == 6)
			{
				int correctDps = DetermineCorrectDps();
				if (isDecline)
				{
					solved = true;
					Module.HandlePass();
					Log("You declined the offer, which was correct. Module Solved!");
					
				}
				else
				{
					Module.HandleStrike();
					inputtedAnswers.Clear();
					Log("You declined the offer, which was incorrect. Strike!");
				}
			} else
            {
				inputtedAnswers.Add(itemsUsed[btn]);
				ButtonTexts[btn].color = textColors[1];
				Buttons[btn].GetComponent<Renderer>().material.color = Color.green;
				if (inputtedAnswers.Count() == 2)
				{
					int correctDps = DetermineCorrectDps();
					if (inputtedAnswers[0] == inputtedAnswers[1])
					{
						for (int i = 0; i < ButtonTexts.Length; i++)
						{
							ButtonTexts[i].color = textColors[0];
							Buttons[i].GetComponent<Renderer>().material = ScreenMat;
						}
						inputtedAnswers.Clear();
						
						return false;
					}
					SwapItems(itemsUsed.IndexOf(inputtedAnswers[0]), itemsUsed.IndexOf(inputtedAnswers[1]));
					GetInvItems();
					int inputtedDps = DetermineInvDps();
					if (inputtedDps == correctDps && !(invItems.Contains(inputtedAnswers[0]) && invItems.Contains(inputtedAnswers[1])))
					{
						solved = true;
						Audio.PlaySoundAtTransform("correct", Buttons[btn].transform);
						Module.HandlePass();
						Log("Your ending shop items were {0} and {1} and your ending inventory items were {2}, {3}, {4} and {5} after buying. Module Solved!",
							itemsUsed[0], itemsUsed[1], itemsUsed[2], itemsUsed[3], itemsUsed[4], itemsUsed[5]);
						itemsUsed = new List<string>(itemsUsedOriginal);
						invItems = new List<string>(invItemsOriginal);
					}
					else
					{
						Audio.PlaySoundAtTransform("equip", Buttons[btn].transform);
						Module.HandleStrike();
						if (isDecline)
						{
							Log("You should have pressed decline because your starting DPS of {0} was equal to or greater than the highest swap which resulted in {1} DPS.",
								beginningDP, correctDps);
						}
						else
						{
							Log("Your ending shop items were {0} and {1} and your ending inventory items were {2}, {3}, {4} and {5} after buying. Which didn't result in the highest DPS. Strike!",
								itemsUsed[0], itemsUsed[1], itemsUsed[2], itemsUsed[3], itemsUsed[4], itemsUsed[5]);
							Log("Your answer had a DPS of {0} while you could have got the DPS of {1} by swapping {2} and {3}.", inputtedDps, correctDps, Swap1, Swap2);
							inputtedAnswers.Clear();
						}
						itemsUsed = new List<string>(itemsUsedOriginal);
						invItems = new List<string>(invItemsOriginal);
						for (int i = 0; i < ButtonTexts.Length; i++)
                        {
							ButtonTexts[i].color = textColors[0];
							Buttons[i].GetComponent<Renderer>().material = ScreenMat;
						}
						
					}
				}
				else
                {
					Audio.PlaySoundAtTransform("equip", Buttons[btn].transform);
				}

			}
			

			return false;


		};
    }

	private void SwapItems(int Swap1, int Swap2)
    {
		var tmp = itemsUsed[Swap1];
		itemsUsed[Swap1] = itemsUsed[Swap2];
		itemsUsed[Swap2] = tmp;
	}

	private void GetInvItems()
    {
        for (int i = 0; i < invItems.Count; i++)
        {
			invItems[i] = itemsUsed[i + 2];
        }
    }

	private void Log(string message, params object[] args)
	{
		Debug.LogFormat("[Roguelike Game #{0}] {1}", _moduleID, string.Format(message, args));

	}

	//twitch plays
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"To decline the trade, use !{0} decline | To swap any 2 items, use !{0} swap [Item 1] [Item 2] (Valid items are: axe, bow, cloak, sword, guide, flask, quiver, dagger, tablet, tome)";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*decline\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			Buttons[6].OnInteract();
		}

		if (Regex.IsMatch(parameters[0], @"^\s*swap\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (parameters.Length != 3)
			{
				yield return "sendtochaterror Parameter length invalid. Command ignored.";
				yield break;
			}

			if (!ValidNames.Contains(parameters[1].ToLower()) || !ValidNames.Contains(parameters[2].ToLower()))
			{
				yield return "sendtochaterror An item being swapped is invalid. Command ignored.";
				yield break;
			}

			Buttons[Array.IndexOf(itemsUsedOriginal.ToArray(), itemsUsedOriginal[Array.IndexOf(ItemsAvailableRenamed.ToArray(), parameters[1].ToLower())])].OnInteract();
			yield return new WaitForSecondsRealtime(0.1f);
			Buttons[Array.IndexOf(itemsUsedOriginal.ToArray(), itemsUsedOriginal[Array.IndexOf(ItemsAvailableRenamed.ToArray(), parameters[2].ToLower())])].OnInteract();
		}
	}
}
