using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using Nx;

public partial class MainGameModtroller : MonoBehaviour
{
	public enum PlayDirection : byte
	{
		UNDECIDED,
		UP,
		DOWN,
	}

						private	static	readonly	int							NUMBER_OF_CARDS_TO_LEAVE_IN_DECK_REFILL_PILE	= 1;
						public	static	readonly	float						MIN_TIMESCALE									= 0.5f;
						public	static	readonly	float						MAX_TIMESCALE									= 8.0f;

	[SerializeField]	private						bool						_demoMode;

	[SerializeField]	private						HumanPlayerModtroller		_humanPlayerPrefab;
	[SerializeField]	private						EasyAIPlayerModtroller		_easyAIPlayerPrefab;
	[SerializeField]	private						HardAIPlayerModtroller		_hardAIPlayerPrefab;

	[SerializeField]	private						CardAnimationData			_cardAnimationData;

	[SerializeField]	private						AudioSource					_cardFlipAudio;
						public						AudioSource					CardFlipAudio			{ get { return _cardFlipAudio; } }

	[SerializeField]	private						Deck						_deck;
	[SerializeField]	private						CardPile					_discardPile;
						public						int							DiscardPileLastValue	{ get { return _discardPile.ReadOnlyCards.Last().CardValue; } }
	[SerializeField]	private						CardPile					_wildcardPile;
						public						int							WildCardValue			{ get { return _wildcardPile.ReadOnlyCards.Last().CardValue; } }

	[SerializeField]	private						TweenHolder					_mainCameraAnchor;
	[SerializeField]	private						Camera						_mainCamera;
						public						Camera						MainCamera				{ get { return _mainCamera; } }
	[SerializeField]	private						Camera						_miniViewCamera;
	[SerializeField]	private						RawImage					_miniViewUIImage;
	[SerializeField]	private						RectTransform				_miniViewUIImageHolder;
	[SerializeField]	private						TweenableGraphics			_miniViewUIGraphics;
	[SerializeField]	private						NxSimpleButton				_playerReadyButton;
	[SerializeField]	private						GameObject					_submitCardsButton;
						public						GameObject					SubmitCardsButton		{ get { return _submitCardsButton; } }
	[SerializeField]	private						GameObject					_undoButton;
	[SerializeField]	private						GameObject					_redoButton;
	[SerializeField]	private						TweenableGraphics			_gameEndSymbol;
	[SerializeField]	private						TweenableGraphics			_gameEndText;
	[SerializeField]	private						Text						_gameEndSymbolText;
	[SerializeField]	private						NxKnobSlider				_timeScaleKnobSlider;
	[SerializeField]	private						Text						_timeScaleText;
	[SerializeField]	private						AdaptiveTutorialSystem		_tutorialSystem;
						public						AdaptiveTutorialSystem		TutorialSystem			{ get { return _tutorialSystem; } }

	[SerializeField]	private						TextMesh					_directionText;

	[SerializeField]	private						string[]					_playerIconCharacters;
	[SerializeField]	private						Transform[]					_playerRoots;
						private						AbstractPlayerModtroller[]	_players				= new AbstractPlayerModtroller[4];
						private						int							_currentPlayer;
						private						int							_indexOfLastPlayerToPlayACard;
						private						bool						_firstHumanHasPlayed	= false;

						private						GameSettings				_gameSettings;
						public						bool						SeeAICards				{ get { return _gameSettings.SeeAICards; } }
						public						bool						WildcardRule			{ get { return _gameSettings.WildCardRule; } }
						public						bool						OptionalPlayRule		{ get { return _gameSettings.OptionalPlayRule; } }
						public						bool						MaxDeviationRule		{ get { return _gameSettings.MaxDeviationRule; } }
						public						int							MaxDeviationThreshold	{ get { return _gameSettings.MaxDeviationThreshold; } }

	[SerializeField]	private						MeshMaterialSwapInfo[]		_qualityLoweringSwapInfos;
	[SerializeField]	private						GameObject					_shadowCamera;

	[SerializeField]	private						EasterEggListener			_easterEggListener;

						private						Commander					_commander;

	public AbstractPlayerModtroller[] Players { get { return _players; } }
	private int _numValidPlayers
	{
		get
		{
			int numValidPlayers = 0;
			_players.ForEach(p => p.IfIsNotNullThen(() => ++numValidPlayers));
			return numValidPlayers;
		}
	}
	private int _numHumanPlayers
	{
		get
		{
			int numHumanPlayers = 0;
			_players.ForEach(p => p.IfIsNotNullThen(() => numHumanPlayers += p.IsHuman ? 1 : 0));
			return numHumanPlayers;
		}
	}

	private int _nextPlayerIndex
	{
		get
		{
			int nextPlayerIndex = _currentPlayer;
			do
			{
				nextPlayerIndex = (nextPlayerIndex + 1) % _players.Length;
			} while (_players[nextPlayerIndex] == null || _players[nextPlayerIndex].Eliminated);
			return nextPlayerIndex;
		}
	}

	private PlayDirection _direction;
	private CardModViewtroller _cardWhenDirectionWasLastUpdated;
	public PlayDirection Direction
	{
		get
		{
			return _direction;
		}
		private set
		{
			_direction = value;

			string directionString;
			switch (_direction)
			{
			case PlayDirection.UP:
				directionString = "\u25B2";
				break;
			case PlayDirection.DOWN:
				directionString = "\u25BC";
				break;
			default:
				directionString = "?";
				break;
			}
			_directionText.text = directionString;
		}
	}


	// Startup functions
	private void Start()
	{
		_commander = new Commander(_cardAnimationData);
		Direction = PlayDirection.UNDECIDED;
		_cardWhenDirectionWasLastUpdated = null;

		if (_miniViewCamera != null && _miniViewUIImage != null && _miniViewUIImageHolder != null)
		{
			int miniViewSquareSide = Mathf.RoundToInt(Mathf.Min(Screen.width  * (_miniViewUIImageHolder.anchorMax.x - _miniViewUIImageHolder.anchorMin.x),
																Screen.height * (_miniViewUIImageHolder.anchorMax.y - _miniViewUIImageHolder.anchorMin.y)));
			_miniViewUIImage.texture = _miniViewCamera.targetTexture
				= new RenderTexture(miniViewSquareSide, miniViewSquareSide, 16, RenderTextureFormat.ARGB32);
		}

		if (_demoMode)
		{
			var demoData = new GameSettings();
			demoData.WildCardRule = false;
			demoData.EliminationRule = true;
			demoData.OptionalPlayRule = false;
			demoData.RefillHandRule = true;
			demoData.AllOrNothingRule = true;
			demoData.MaxDeviationRule = false;
			demoData.LoseBestCardRule = false;
			demoData.SeeAICards = true;
			demoData.Players = new GameSettings.PlayerType[4]
			{
				GameSettings.PlayerType.AI_HARD,
				GameSettings.PlayerType.AI_EASY,
				GameSettings.PlayerType.AI_HARD,
				GameSettings.PlayerType.AI_EASY,
			};
			demoData.NumberOfDecks = 2;
			demoData.NumberOfCardsPerPlayer = 5;
			demoData.NumberOfPointsToWin = 1;
			demoData.MaxDeviationThreshold = 3;

			SetupAndStartGame(demoData);
		}
		else
		{
			var mainMenuData = FindObjectOfType<MainMenuModtroller>();
			if (mainMenuData != null)
			{
				if (mainMenuData.ShouldDestroyShadowsOfNewCards)
				{
					RemoveCardShadows();
				}
				if (mainMenuData.ShouldReduceQualityOfNewCards)
				{
					ReduceCardQuality();
				}
				SetupAndStartGame(mainMenuData.GameSettings);
				Destroy(mainMenuData.gameObject);
			}
			else
			{
				SetupAndStartGame(new GameSettings());
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0, iMax = _players.Length; i < iMax; ++i)
		{
			if (_players[i] != null && _players[i].IsHuman)
			{
				((HumanPlayerModtroller)_players[i]).OnHumanTurnBegan -= ProcessCommandSystemOnHumanPlayerTurn;
			}
		}
	}

	private void SetupAndStartGame(GameSettings gameSettings)
	{
		_gameSettings = gameSettings;
		SetTimeScalePercentage(_gameSettings.TimeScalePercentage);
		if (_numHumanPlayers <= 0)
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
		for (int i = 0, iMax = _gameSettings.Players.Length; i < iMax; ++i)
		{
			switch (_gameSettings.Players[i])
			{
			case GameSettings.PlayerType.HUMAN:
				_players[i] = ((HumanPlayerModtroller)Instantiate(_humanPlayerPrefab)).Init(this);
				((HumanPlayerModtroller) _players[i]).OnHumanTurnBegan += ProcessCommandSystemOnHumanPlayerTurn;
				Screen.sleepTimeout = SleepTimeout.SystemSetting;
				break;
			case GameSettings.PlayerType.AI_EASY:
				_players[i] = ((EasyAIPlayerModtroller)Instantiate(_easyAIPlayerPrefab)).Init(this);
				break;
			case GameSettings.PlayerType.AI_HARD:
				_players[i] = ((HardAIPlayerModtroller)Instantiate(_hardAIPlayerPrefab)).Init(this);
				break;
			case GameSettings.PlayerType.NONE:
			default:
				_players[i] = null;
				break;
			}
			if (_players[i] != null)
			{
				_players[i].transform.parent = _playerRoots[i];
				_players[i].transform.ResetLocal();
				_players[i].PlayerSymbolText.text = _playerIconCharacters[i];
				_players[i].PlayerSymbolText.color = i >= 2 ? Color.red : Color.black;
				_players[i].Points = 0;
			}
		}

		while (_players[_currentPlayer] == null)
		{
			_currentPlayer = ++_currentPlayer % _players.Length;
		}

		_indexOfLastPlayerToPlayACard = _currentPlayer;
		do
		{
			_indexOfLastPlayerToPlayACard = (_indexOfLastPlayerToPlayACard + _players.Length - 1) % _players.Length;
		}
		while (_players[_indexOfLastPlayerToPlayACard] == null);

		if (!_demoMode)
		{
			if (_numHumanPlayers >= 1)
			{
				int firstHumanPlayer = 0;
				while (_players[firstHumanPlayer] == null || !_players[firstHumanPlayer].IsHuman)
				{
					++firstHumanPlayer;
				}
				_mainCameraAnchor.AddLocalQuaternionRotationTween(Quaternion.Euler(Vector3.up * (float) firstHumanPlayer / _players.Length * 360.0f));
			}
			else
			{
				_mainCameraAnchor.AddLocalQuaternionRotationTween(Quaternion.Euler(Vector3.up * (float) _currentPlayer / _players.Length * 360.0f));
			}
		}

		StartCoroutine(PopulateDeck());
	}

	private IEnumerator PopulateDeck()
	{
		if (!_demoMode)
		{
			yield return new WaitForSeconds(_cardAnimationData.TimeToWaitBeforePopulatingDeck);
		}
		int[] unShuffleData;
		var cardCreationTweenWaiter = new FinishableGroupWaiter(() => _deck.Shuffle(out unShuffleData, onFinished: () => StartCoroutine(DealCardsToPlayers())));
		var suits = (Card.CardSuit[])Enum.GetValues(typeof(Card.CardSuit));
		var values = (Card.CardValue[])Enum.GetValues(typeof(Card.CardValue));

		for (int i = 0, iMax = _gameSettings.NumberOfDecks; i < iMax; ++i)
		{
			for (int j = 0, jMax = values.Length; j < jMax; ++j)
			{
				TweenHolder createCardTweenHolder;
				for (int k = 0, kMax = suits.Length; k < kMax; ++k)
				{
					_deck.CreateCard(values[j], suits[k], _mainCamera, out createCardTweenHolder, fancyEntrance: true, angleOffsetForFancyEntrance:
						((float) (j + i * jMax) / (iMax * jMax * kMax / 2.0f) + k / 4.0f) * Mathf.PI * 2.0f);
					cardCreationTweenWaiter.AddFinishable(createCardTweenHolder);
				}
				yield return new WaitForSeconds(_cardAnimationData.CardCreationTotalDuration / (iMax * jMax));
			}
		}
		_easterEggListener.IfIsNotNullThen(e => e.enabled = true);
		cardCreationTweenWaiter.Ready = true;
	}


	// Main game functions
	private IEnumerator DealCardsToPlayers()
	{
		for (int i = 0, iMax = _players.Length; i < iMax; ++i)
		{
			if (_players[i] != null)
			{
				bool visible = _players[i].IsHuman ? i == _currentPlayer : _gameSettings.SeeAICards;
				_players[i].Hand.CardsTextVisibility = visible;
				_players[i].Hand.SetIntendedIncomingCardAnimState(visible ? CardModViewtroller.CardViewFSM.AnimState.VISIBLE
																		  : CardModViewtroller.CardViewFSM.AnimState.OBSCURED);
			}
		}

		var cardDealeryTweenWaiter = new FinishableGroupWaiter(() => StartCoroutine(BeginNewRound()));
		for (int i = 0, iMax = _gameSettings.NumberOfCardsPerPlayer; i < iMax; ++i)
		{
			for (int j = 0, jMax = _players.Length; j < jMax; ++j)
			{
				if (_players[j] != null)
				{
					if (_deck.CardCount <= 0)
					{
						bool isFilling = true;
						FillDeck(onFinished: () => isFilling = false);
						yield return new WaitUntil(() => isFilling == false);
					}

					var dealToCommand = new DealToCommand(_deck, _players[j]);
					_commander.ExecuteAndAddToCurrentTurnBundle(dealToCommand);
					cardDealeryTweenWaiter.AddFinishable(dealToCommand.OutTween);
					yield return new WaitForSeconds(_cardAnimationData.ConsecutiveCardDealDelay / _numValidPlayers);
				}
			}
		}
		cardDealeryTweenWaiter.Ready = true;
	}

	private IEnumerator BeginNewRound()
	{
		if (_deck.CardCount <= (_gameSettings.WildCardRule ? 1 : 0))
		{
			bool isFilling = true;
			FillDeck(onFinished: () => isFilling = false);
			yield return new WaitUntil(() => isFilling == false);
		}

		DealToCommand dealToCommand;
		if (_gameSettings.WildCardRule)
		{
			dealToCommand = new DealToCommand(_deck, _wildcardPile);
			_commander.ExecuteAndAddToCurrentTurnBundle(dealToCommand);
			dealToCommand.OutTween.AddLocalRotationTween(Vector3.one * 360.0f)
								  .AddOffsetHeightTween(_cardAnimationData.GeneralCardMoveHeight / 2.0f);
		}

		dealToCommand = new DealToCommand(_deck, _discardPile);
		_commander.ExecuteAndAddToCurrentTurnBundle(dealToCommand);
		dealToCommand.OutTween.AddLocalRotationTween(Vector3.one * 720.0f)
			.AddToOnFinishedOnce(() =>
			{
				UpdateDirection();
				StartCoroutine(BeginPlayerTurn());
			});
	}

	private IEnumerator BeginPlayerTurn()
	{
		if (_indexOfLastPlayerToPlayACard == _currentPlayer)
		{
			ResetDirection();

			_commander.ExecuteAndAddToCurrentTurnBundle(new SetPlayerScoreCommand(this, _currentPlayer,
				_players[_currentPlayer].Points + (_gameSettings.AllOrNothingRule ? _discardPile.CardCount : 1)));

			if (_players[_currentPlayer].Points >= _gameSettings.NumberOfPointsToWin && !_demoMode)
			{
				PlayerWin();
				return false;
			}

			if (_gameSettings.EliminationRule)
			{
				for (int i = 0, iMax = _players.Length; i < iMax; ++i)
				{
					if (_players[i] != null)
					{
						_commander.ExecuteAndAddToCurrentTurnBundle(new SetPlayerEliminatedCommand(this, i, false));
					}
				}
			}

			bool playerHandRefillDone = false,
				 wildcardPileRefillDone = false,
				 discardPileRefillDone = false;
			StartCoroutine(RefillDeckWithPlayerHands(_cardAnimationData.DeckRefillDelayPerCard, onFinished: () => playerHandRefillDone = true));

			if (_gameSettings.WildCardRule)
			{
				StartCoroutine(RefillDeckWithWildcardPile(_wildcardPile.CardCount != 0 ? _cardAnimationData.DeckRefillDelayPerCard * (_gameSettings.NumberOfCardsPerPlayer - 1) / _wildcardPile.CardCount
																					   : 0.0f, onFinished: () => wildcardPileRefillDone = true));
			}
			else
			{
				wildcardPileRefillDone = true;
			}

			StartCoroutine(RefillDeckWithDiscardPile(_discardPile.CardCount != 0 ? _cardAnimationData.DeckRefillDelayPerCard * (_gameSettings.NumberOfCardsPerPlayer - 1) / _discardPile.CardCount
																				   : 0.0f, onFinished: () => discardPileRefillDone = true));
			yield return new WaitUntil(() => playerHandRefillDone && wildcardPileRefillDone && discardPileRefillDone);

			var shuffleDeckCommand = new ShuffleCommand(_deck, onFinished: () => CycleCurrentPlayer(onFinished: () => StartCoroutine(_gameSettings.RefillHandRule ? DealCardsToPlayers() : BeginNewRound())));
			_commander.ExecuteAndAddToCurrentTurnBundle(shuffleDeckCommand);
		}
		else
		{
			_players[_currentPlayer].BeginCardSelection();
		}
	}

	private void ProcessCommandSystemOnHumanPlayerTurn()
	{
		_commander.FinishTurnBundle();
		_firstHumanHasPlayed = true;
		UpdateUndoAndRedoButtons();
	}

	private IEnumerator RefillDeckWithPlayerHands(float refillDelayPerCard, Action onFinished)
	{
		var deckRefillTweenWaiter = new FinishableGroupWaiter(onFinished);
		while (_gameSettings.RefillHandRule && _players.Exists(p => p != null && p.Hand.CardCount > 0))
		{
			for (int i = 0, iMax = _players.Length; i < iMax; ++i)
			{
				if (_players[i] != null && !_players[i].Hand.ReadOnlyCards.IsEmpty())
				{
					var moveCardCommand = new MoveCardCommand(_players[i].Hand, 0, _deck, visibleDuringTween: true);
					_commander.ExecuteAndAddToCurrentTurnBundle(moveCardCommand);
					moveCardCommand.OutTween.SetDuration(_cardAnimationData.DeckFillDurationPerCard);
					deckRefillTweenWaiter.AddFinishable(moveCardCommand.OutTween);
				}
			}
			yield return new WaitForSeconds(refillDelayPerCard);
		}
		deckRefillTweenWaiter.Ready = true;
	}

	private IEnumerator RefillDeckWithWildcardPile(float refillDelayPerCard, Action onFinished)
	{
		_wildcardPile.CardsTextVisibility = true;
		var deckRefillTweenWaiter = new FinishableGroupWaiter(onFinished);
		while (_gameSettings.WildCardRule && _wildcardPile.ReadOnlyCards.Last() != null)
		{
			var moveCardCommand = new MoveCardCommand(_wildcardPile, _wildcardPile.ReadOnlyCards.LastIndex(), _deck, visibleDuringTween: true);
			_commander.ExecuteAndAddToCurrentTurnBundle(moveCardCommand);
			moveCardCommand.OutTween.AddOffsetHeightTween(_cardAnimationData.GeneralCardMoveHeight / 2.0f)
									.SetDuration(_cardAnimationData.DeckFillDurationPerCard);
			deckRefillTweenWaiter.AddFinishable(moveCardCommand.OutTween);
			yield return new WaitForSeconds(refillDelayPerCard);
		}
		deckRefillTweenWaiter.Ready = true;
	}

	private IEnumerator RefillDeckWithDiscardPile(float refillDelayPerCard, Action onFinished)
	{
		_discardPile.CardsTextVisibility = true;
		var deckRefillTweenWaiter = new FinishableGroupWaiter(onFinished);
		while (_discardPile.ReadOnlyCards.Last() != null)
		{
			var moveCardCommand = new MoveCardCommand(_discardPile, _discardPile.ReadOnlyCards.LastIndex(), _deck, visibleDuringTween: true);
			_commander.ExecuteAndAddToCurrentTurnBundle(moveCardCommand);
			moveCardCommand.OutTween.SetDuration(_cardAnimationData.DeckFillDurationPerCard);
			deckRefillTweenWaiter.AddFinishable(moveCardCommand.OutTween);
			yield return new WaitForSeconds(refillDelayPerCard);
		}
		deckRefillTweenWaiter.Ready = true;
	}

	public void EndPlayerTurn(int[] cardIndexes, bool handCardsWereDragged = false)
	{
		StartCoroutine(EndPlayerTurnInternal(cardIndexes, handCardsWereDragged));
	}

	private IEnumerator EndPlayerTurnInternal(int[] cardIndexes, bool handCardsWereDragged)
	{
		HideUndoAndRedoButtons();
		if (cardIndexes.IsNullOrEmpty())
		{
			if (_gameSettings.EliminationRule)
			{
				_commander.ExecuteAndAddToCurrentTurnBundle(new SetPlayerEliminatedCommand(this, _currentPlayer, true));
			}
			CycleCurrentPlayer(onFinished: () => StartCoroutine(BeginPlayerTurn()));
		}
		else
		{
			_commander.ExecuteAndAddToCurrentTurnBundle(new SetIndexOfLastPlayerToPlayACardCommand(this, _currentPlayer));
			var cardMoveTweenWaiter = new FinishableGroupWaiter(() => CycleCurrentPlayer(onFinished: () => StartCoroutine(BeginPlayerTurn())));

			// Sort the indexes, then reverse iterate, cuz otherwise MoveCard() will shift the hand card indexes to the left like an asshole.
			Array.Sort(cardIndexes);
			for (int i = cardIndexes.Length - 1; i >= 0; --i)
			{
				var moveCardCommand = new MoveCardCommand(_players[_currentPlayer].Hand, cardIndexes[i], _discardPile, visibleDuringTween: true);
				_commander.ExecuteAndAddToCurrentTurnBundle(moveCardCommand);
				if (handCardsWereDragged)
				{
					moveCardCommand.OutTween.GetTweenOfType<OffsetHeightTween>().IfIsNotNullThen(t => t.Height = _cardAnimationData.CardDragSubmitTweenHeight);
				}
				cardMoveTweenWaiter.AddFinishable(moveCardCommand.OutTween);
				if (_deck.CardCount <= 0)
				{
					bool isFilling = true;
					FillDeck(onFinished: () => isFilling = false);
					yield return new WaitUntil(() => isFilling == false);
				}
				var dealToCommand = new DealToCommand(_deck, _players[_currentPlayer]);
				_commander.ExecuteAndAddToCurrentTurnBundle(dealToCommand);
				cardMoveTweenWaiter.AddFinishable(dealToCommand.OutTween);
				yield return new WaitForSeconds(_cardAnimationData.ConsecutiveCardSubmitDelay);
			}

			bool directionDidHardChange;
			UpdateDirection(out directionDidHardChange);
			if (_gameSettings.LoseBestCardRule && directionDidHardChange)
			{
				int mostAdvantageousCardIndex = _players[_currentPlayer].GetMostAdvantageousCardIndex();
				if (_direction == PlayDirection.DOWN ? _players[_currentPlayer].Hand.ReadOnlyCards[mostAdvantageousCardIndex].CardValue < DiscardPileLastValue
													 : _players[_currentPlayer].Hand.ReadOnlyCards[mostAdvantageousCardIndex].CardValue > DiscardPileLastValue)
				{
					var moveCardCommand = new MoveCardCommand(_players[_currentPlayer].Hand, mostAdvantageousCardIndex, _discardPile, 0);
					_commander.ExecuteAndAddToCurrentTurnBundle(moveCardCommand);
					cardMoveTweenWaiter.AddFinishable(moveCardCommand.OutTween);
					if (_deck.CardCount <= 0)
					{
						bool isFilling = true;
						FillDeck(onFinished: () => isFilling = false);
						yield return new WaitUntil(() => isFilling == false);
					}
					var dealToCommand = new DealToCommand(_deck, _players[_currentPlayer]);
					_commander.ExecuteAndAddToCurrentTurnBundle(dealToCommand);
					cardMoveTweenWaiter.AddFinishable(dealToCommand.OutTween);
				}
			}

			if (_gameSettings.WildCardRule && DiscardPileLastValue == WildCardValue)
			{
				if (_deck.CardCount <= 0)
				{
					bool isFilling = true;
					FillDeck(onFinished: () => isFilling = false);
					yield return new WaitUntil(() => isFilling == false);
				}
				var dealToCommand = new DealToCommand(_deck, _wildcardPile);
				_commander.ExecuteAndAddToCurrentTurnBundle(dealToCommand);
				dealToCommand.OutTween.AddOffsetHeightTween(_cardAnimationData.GeneralCardMoveHeight / 2.0f);
				cardMoveTweenWaiter.AddFinishable(dealToCommand.OutTween);
			}
			cardMoveTweenWaiter.Ready = true;
		}
	}

	private void PlayerWin()
	{
		_gameEndSymbolText.text = _players[_currentPlayer].PlayerSymbolText.text;
		Color playerSymbolColor = _players[_currentPlayer].PlayerSymbolText.color;
		playerSymbolColor.a = 0.0f;
		_gameEndSymbolText.color = playerSymbolColor;
		_gameEndSymbol.AddIncrementalAlphaTween(1.0f).TweenHolder
					  .AddIncrementalScaleTween(Vector3.one)
					  .AddLocalRotationTween(Vector3.up * 360.0f)
					  .SetDuration(3.0f)
					  .AddToOnFinishedOnce(() => 
						{
							_gameEndText.AddIncrementalAlphaTween(1.0f).TweenHolder
										.SetDuration(1.0f);

							_deck.ShuffleAnimationCamera.AddPositionTween(_deck.ShuffleAnimationCamera.transform.position + Vector3.up * 5.0f)
														.SetDuration(3.0f)
														.SetIgnoreTimeScale(true).Play();

							_players.ForEach(o => o.IfIsNotNullThen(p => p.Hand.ReadOnlyCards.ForEach(c =>
							{
								float animationDuration = UnityEngine.Random.Range(2.0f, 6.0f);
								Vector3 rotationVector = (Mathf.Round(UnityEngine.Random.Range(1.0f, 3.0f)) * 2.0f) * 360.0f * Vector3.one;
								c.AddPositionTween(c.gameObject.transform.position + Vector3.up * 13.0f)
								 .AddLocalRotationTween(rotationVector, true)
								 .SetDuration(animationDuration)
								 .SetIgnoreTimeScale(true).Play();
								c.ViewFSM.SetTextVisibility(true);
							})));

							Action<CardModViewtroller> animatePileCardFunction = c =>
							{
								float animationDuration = UnityEngine.Random.Range(2.0f, 6.0f);
								Vector3 rotationVector = (Mathf.Round(UnityEngine.Random.Range(1.0f, 3.0f)) * 2.0f) * 360.0f * Vector3.one;
								Vector3 destinationField = UnityEngine.Random.insideUnitCircle * 12.0f;
								destinationField.z = destinationField.y;
								destinationField.y = c.gameObject.transform.position.y + 13.0f;
								c.AddPositionTween(destinationField)
								 .AddLocalRotationTween(rotationVector, true)
								 .SetDuration(animationDuration)
								 .SetIgnoreTimeScale(true).Play();
								c.ViewFSM.SetTextVisibility(true);
							};

							_deck.ReadOnlyCards.ForEach(animatePileCardFunction);
							_wildcardPile.ReadOnlyCards.ForEach(animatePileCardFunction);
							_discardPile.ReadOnlyCards.ForEach(animatePileCardFunction);
						});
	}


	// Player/Camera perspective shift handling
	private void CycleCurrentPlayer(Action onFinished)
	{
		int prevPlayerIndex = _currentPlayer;
		_commander.ExecuteAndAddToCurrentTurnBundle(new SetCurrentPlayerCommand(this, _nextPlayerIndex));

		if (_players[_currentPlayer].IsHuman && _numHumanPlayers > 1 && _firstHumanHasPlayed)
		{
			UpdateCardVisibilityForPlayer(prevPlayerIndex, onFinished: () =>
			{
				_playerReadyButton.OnClicked.RemoveAllListeners();
				_playerReadyButton.gameObject.SetActive(true);
				_playerReadyButton.OnClicked.AddListener(() =>
				{
					_playerReadyButton.gameObject.SetActive(false);
					UpdateCamera(onFinished);
				});
			});
		}
		else
		{
			UpdateCardVisibilityForPlayer(prevPlayerIndex, onFinished: () => UpdateCamera(onFinished));
		}
	}

	private void UpdateCamera(Action onFinished)
	{
		Action onFinishedWithAnimCheck = () =>
		{
			if (_players[_currentPlayer].IsHuman)
			{
				_players[_currentPlayer].Hand.SetIntendedIncomingCardAnimState(CardModViewtroller.CardViewFSM.AnimState.VISIBLE);
			}
			onFinished();
		};

		if (_demoMode || !_players[_currentPlayer].IsHuman)
		{
			onFinished();
		}
		else if (_numHumanPlayers <= 1 || !_firstHumanHasPlayed)
		{
			onFinishedWithAnimCheck();
		}
		else
		{
			_mainCameraAnchor.AddLocalQuaternionRotationTween(Quaternion.Euler(Vector3.up * (float) _currentPlayer / _players.Length * 360.0f))
							 .AddToOnFinishedOnce(onFinishedWithAnimCheck);
		}
	}

	private void UpdateCardVisibilityForPlayer(int playerIndex, Action onFinished)
	{
		if (_players[playerIndex] != null && _players[playerIndex].IsHuman)
		{
			_players[playerIndex].Hand.SetCardsAnimStates((playerIndex == _currentPlayer)
				? CardModViewtroller.CardViewFSM.AnimState.VISIBLE
				: CardModViewtroller.CardViewFSM.AnimState.OBSCURED, onFinished: () =>
				{
					_players[playerIndex].Hand.CardsTextVisibility = playerIndex == _currentPlayer;
					onFinished();
				});
		}
		else
		{
			onFinished();
		}
	}


	// Direction change handling
	private void UpdateDirection()
	{
		bool unusedBool;
		UpdateDirection(out unusedBool);
	}

	private void UpdateDirection(out bool directionDidHardChange)
	{
		directionDidHardChange = false;
		
		CardModViewtroller lastCard = _discardPile.ReadOnlyCards.Last();
		
		if (lastCard == null)
		{
			_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.UNDECIDED));
			return;
		}

		if (_cardWhenDirectionWasLastUpdated == null)
		{
			_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.UNDECIDED));
			goto end;
		}

		if (_gameSettings.WildCardRule && lastCard.CardValue == WildCardValue)
		{
			if (Direction == PlayDirection.UP)
			{
				_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.DOWN));
			}
			else if (Direction == PlayDirection.DOWN)
			{
				_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.UP));
			}
			goto end;
		}

		if (lastCard.CardValue > _cardWhenDirectionWasLastUpdated.CardValue)
		{
			_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.UP));
		}
		else if (lastCard.CardValue < _cardWhenDirectionWasLastUpdated.CardValue)
		{
			_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.DOWN));
		}
		else
		{
			if (Direction == PlayDirection.UP)
			{
				_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.DOWN));
				directionDidHardChange = true;
			}
			else if (Direction == PlayDirection.DOWN)
			{
				_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.UP));
				directionDidHardChange = true;
			}
			else
			{
				_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.UNDECIDED));
			}
		}
	end:
		_commander.ExecuteAndAddToCurrentTurnBundle(new SetCardWhenDirectionWasLastUpdatedCommand(this, lastCard));
	}

	private void ResetDirection()
	{
		_commander.ExecuteAndAddToCurrentTurnBundle(new SetDirectionCommand(this, PlayDirection.UNDECIDED));
		_commander.ExecuteAndAddToCurrentTurnBundle(new SetCardWhenDirectionWasLastUpdatedCommand(this, null));
	}


	// Misc. helper funcs.
	private void FillDeck(Action onFinished)
	{
		CardHolder cardHolderToRefillDeckWith = _gameSettings.WildCardRule && _wildcardPile.CardCount > _discardPile.CardCount ? _wildcardPile : _discardPile;
		cardHolderToRefillDeckWith.CardsTextVisibility = true;
		_commander.ExecuteAndAddToCurrentTurnBundle(
				new RefillDeckCommand(_deck, cardHolderToRefillDeckWith, NUMBER_OF_CARDS_TO_LEAVE_IN_DECK_REFILL_PILE, onFinished: () =>
						_commander.ExecuteAndAddToCurrentTurnBundle(new ShuffleCommand(_deck, onFinished))));
	}

	private void UpdateUndoAndRedoButtons()
	{
		_undoButton.SetActive(_commander.UndoIsPossible);
		_redoButton.SetActive(_commander.RedoIsPossible);
		_miniViewCamera.Render();
		_miniViewUIGraphics.AddIncrementalAlphaTween(1.0f).TweenHolder
						   .SetDuration(1.0f);
	}

	private void HideUndoAndRedoButtons()
	{
		if (!_demoMode)
		{
			_tutorialSystem.HideTutorial();
			_undoButton.SetActive(false);
			_redoButton.SetActive(false);
			_miniViewUIGraphics.AddIncrementalAlphaTween(0.0f).TweenHolder
							   .SetDuration(1.0f);
		}
	}

	public void RemoveCardShadows()
	{
		_qualityLoweringSwapInfos.ForEach(q => q.MeshRenderer.material = q.SwapMaterial);
		Destroy(_shadowCamera);

		Action<CardModViewtroller> destroyCardShadowFunction = c => c.DestroyShadowObject();
		_deck.ReadOnlyCards.ForEach(destroyCardShadowFunction);
		_wildcardPile.IfIsNotNullThen(w => w.ReadOnlyCards.ForEach(destroyCardShadowFunction));
		_discardPile.ReadOnlyCards.ForEach(destroyCardShadowFunction);
		_players.ForEach(o => o.IfIsNotNullThen(p => p.Hand.ReadOnlyCards.ForEach(destroyCardShadowFunction)));
		_deck.ShouldCreateShadowlessNewCards = true;
	}

	public void ReduceCardQuality()
	{
		Action<CardModViewtroller> reduceQualityFunction = c => c.ReduceQuality();
		_deck.ReadOnlyCards.ForEach(reduceQualityFunction);
		_wildcardPile.IfIsNotNullThen(w => w.ReadOnlyCards.ForEach(reduceQualityFunction));
		_discardPile.ReadOnlyCards.ForEach(reduceQualityFunction);
		_players.ForEach(o => o.IfIsNotNullThen(p => p.Hand.ReadOnlyCards.ForEach(reduceQualityFunction)));
		_deck.ShouldReduceQualityOfNewCards = true;
	}


	// Button-invoked functions
	public void SetTimeScalePercentage(float percent)
	{
		Time.timeScale = Mathf.Lerp(MIN_TIMESCALE, MAX_TIMESCALE, _gameSettings.TimeScalePercentage = percent);
		_timeScaleText.IfIsNotNullThen(t => t.text = (Mathf.Round(Time.timeScale * 10.0f) / 10.0f).ToString() + "x");
		_timeScaleKnobSlider.IfIsNotNullThen(t => t.SetRadialFillPercentage(percent));
	}

	public void WriteGameSettingsToDisk()
	{
		_gameSettings.WriteToDisk();
	}

	public void OnUndoButtonClicked()
	{
		HideUndoAndRedoButtons();
		((HumanPlayerModtroller) _players[_currentPlayer]).CancelCardSelection(
			onFinished: () => StartCoroutine(_commander.UndoPlayerTurn(
			onFinished: () => UpdateCamera(
			onFinished: () => StartCoroutine(BeginPlayerTurn())))));
	}

	public void OnRedoButtonClicked()
	{
		HideUndoAndRedoButtons();
		((HumanPlayerModtroller) _players[_currentPlayer]).CancelCardSelection(
			onFinished: () => StartCoroutine(_commander.RedoPlayerTurn(
			onFinished: () => UpdateCamera(
			onFinished: () => StartCoroutine(BeginPlayerTurn())))));
	}

	public void OnSubmitCardsButtonClicked()
	{
		((HumanPlayerModtroller)_players[_currentPlayer]).SubmitCards();
	}

	public void EndGame()
	{
		SceneManager.LoadScene("MainMenu");
	}
}