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

						private	readonly	float						TIME_TO_WAIT_BEFORE_POPULATING_DECK			= 1.5f;
						private	readonly	int							NUMBER_OF_CARDS_TO_LEAVE_IN_DISCARD_PILE	= 1;

	[SerializeField]	private				bool						_demoMode;

	[SerializeField]	private				HumanPlayerModtroller		_humanPlayerPrefab;
	[SerializeField]	private				EasyAIPlayerModtroller		_easyAIPlayerPrefab;
	[SerializeField]	private				HardAIPlayerModtroller		_hardAIPlayerPrefab;

	[SerializeField]	private				CardAnimationData			_cardAnimationData;

	[SerializeField]	private				Deck						_deck;
	[SerializeField]	private				CardPile					_discardPile;
						public				int							DiscardPileLastValue	{ get { return _discardPile.ReadOnlyCards.Last().CardValue; } }
	[SerializeField]	private				CardPile					_wildcardPile;
						public				int							WildCardValue			{ get { return _wildcardPile.ReadOnlyCards.Last().CardValue; } }

	[SerializeField]	private				TweenHolder					_mainCameraAnchor;
	[SerializeField]	private				Camera						_mainCamera;
						public				Camera						MainCamera				{ get { return _mainCamera; } }
	[SerializeField]	private				NxDynamicButton				_playerReadyButton;
	[SerializeField]	private				GameObject					_submitCardsButton;
						public				GameObject					SubmitCardsButton		{ get { return _submitCardsButton; } }
	[SerializeField]	private				GameObject					_undoButton;
	[SerializeField]	private				GameObject					_redoButton;
	[SerializeField]	private				GameObject					_gameEndObject;

	[SerializeField]	private				Text						_directionText;

	[SerializeField]	private				Transform[]					_playerRoots;
						private				AbstractPlayerModtroller[]	_players				= new AbstractPlayerModtroller[4];
						private				int							_currentPlayer;
						private				int							_indexOfLastPlayerToPlayACard;

						private				GameSettings				_gameSettings;
						public				bool						SeeAICards				{ get { return _gameSettings.SeeAICards; } }
						public				bool						WildcardRule			{ get { return _gameSettings.WildCardRule; } }
						public				bool						OptionalPlayRule		{ get { return _gameSettings.OptionalPlayRule; } }
						public				bool						MaxDeviationRule		{ get { return _gameSettings.MaxDeviationRule; } }
						public				int							MaxDeviationThreshold	{ get { return _gameSettings.MaxDeviationThreshold; } }

						private				Commander					_commander;

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
			string directionTextColor = "black";
			if (_direction == PlayDirection.UP)
			{
				directionTextColor = "#00ff00ff";
			}
			else if (_direction == PlayDirection.DOWN)
			{
				directionTextColor = "red";
			}
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
			_directionText.text = directionString.ToRichText(size: 600, color: directionTextColor);
		}
	}


	// Startup functions
	private void Start()
	{
		_commander = new Commander(_cardAnimationData);
		Direction = PlayDirection.UNDECIDED;
		_cardWhenDirectionWasLastUpdated = null;

		if (_demoMode)
		{
			var demoData = new GameSettings();
			demoData.WildCardRule = false;
			demoData.EliminationRule = true;
			demoData.OptionalPlayRule = false;
			demoData.RefillHandRule = true;
			demoData.AllOrNothingRule = true;
			demoData.MaxDeviationRule = false;
			demoData.DSwitchLBCRule = false;
			demoData.SeeAICards = true;
			demoData.Players = new CustomPlayer.PlayerType[4]
			{
				CustomPlayer.PlayerType.AI_HARD,
				CustomPlayer.PlayerType.AI_EASY,
				CustomPlayer.PlayerType.AI_HARD,
				CustomPlayer.PlayerType.AI_EASY,
			};
			demoData.NumberOfDecks = 2;
			demoData.NumberOfCardsPerPlayer = 5;
			demoData.NumberOfPointsToWin = 1;
			demoData.MaxDeviationThreshold = 3;

			SetupAndStartGame(demoData);
		}
		else
		{
			var mainMenuData = GameObject.FindObjectOfType<MainMenuModtroller>();
			SetupAndStartGame(mainMenuData.GameSettings);
			Destroy(mainMenuData.gameObject);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0, iMax = _players.Length; i < iMax; ++i)
		{
			if (_players[i] != null && _players[i].IsHuman)
			{
				((HumanPlayerModtroller) _players[i]).OnHumanTurnBegan -= ProcessCommandSystemOnHumanPlayerTurn;
			}
		}
	}

	private void SetupAndStartGame(GameSettings gameSettings)
	{
		_gameSettings = gameSettings;
		if (_numHumanPlayers <= 0)
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}
		for (int i = 0, iMax = _gameSettings.Players.Length; i < iMax; ++i)
		{
			switch (_gameSettings.Players[i])
			{
			case CustomPlayer.PlayerType.HUMAN:
				_players[i] = ((HumanPlayerModtroller)Instantiate(_humanPlayerPrefab)).Init(this);
				((HumanPlayerModtroller) _players[i]).OnHumanTurnBegan += ProcessCommandSystemOnHumanPlayerTurn;
				Screen.sleepTimeout = SleepTimeout.SystemSetting;
				break;
			case CustomPlayer.PlayerType.AI_EASY:
				_players[i] = ((EasyAIPlayerModtroller)Instantiate(_easyAIPlayerPrefab)).Init(this);
				break;
			case CustomPlayer.PlayerType.AI_HARD:
				_players[i] = ((HardAIPlayerModtroller)Instantiate(_hardAIPlayerPrefab)).Init(this);
				break;
			case CustomPlayer.PlayerType.NONE:
			default:
				_players[i] = null;
				break;
			}
			if (_players[i] != null)
			{
				_players[i].transform.parent = _playerRoots[i];
				_players[i].transform.ResetLocal();
				_players[i].PlayerName += i + 1;
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
			_mainCameraAnchor.AddLocalQuaternionRotationTween(Quaternion.Euler(Vector3.up * (float) _currentPlayer / _players.Length * 360.0f));
		}

		StartCoroutine(PopulateDeck());
	}

	private IEnumerator PopulateDeck()
	{
		yield return new WaitForSeconds(TIME_TO_WAIT_BEFORE_POPULATING_DECK);
		int[] unShuffleData;
		var cardCreationTweenWaiter = new FinishableGroupWaiter(() => _deck.Shuffle(out unShuffleData, onFinished: () => StartCoroutine(DealCardsToPlayers())));
		var suits = (Card.CardSuit[])Enum.GetValues(typeof(Card.CardSuit));
		var values = (Card.CardValue[])Enum.GetValues(typeof(Card.CardValue));

		for (int i = 0, iMax = _gameSettings.NumberOfDecks; i < iMax; ++i)
		{
			for (int j = 0, jMax = suits.Length; j < jMax; ++j)
			{
				for (int k = 0, kMax = values.Length; k < kMax; ++k)
				{
					TweenHolder createCardTweenHolder;
					_deck.CreateCard(values[k], suits[j], out createCardTweenHolder, fancyEntrance: true, angleOffsetForFancyEntrance:
						(float)(k + j * kMax + i * jMax * kMax) / (iMax * jMax * kMax) * Mathf.PI * 2.0f);
					cardCreationTweenWaiter.AddFinishable(createCardTweenHolder);
					yield return new WaitForSeconds(_cardAnimationData.CardCreationTotalDuration / (iMax * jMax * kMax));
				}
			}
		}
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
				_gameEndObject.SetActive(true);
				return false;
			}

			if (_gameSettings.EliminationRule)
			{
				_players.ForEach(p => p.IfIsNotNullThen(() => p.Eliminated = false));
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

	public void EndPlayerTurn(int[] cardIndexes)
	{
		StartCoroutine(EndPlayerTurnInternal(cardIndexes));
	}

	private IEnumerator EndPlayerTurnInternal(int[] cardIndexes)
	{
		HideUndoAndRedoButtons();
		if (NxUtils.IsNullOrEmpty(cardIndexes))
		{
			if (_gameSettings.EliminationRule)
			{
				_players[_currentPlayer].Eliminated = true;
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
			if (_gameSettings.DSwitchLBCRule && directionDidHardChange)
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
			}
			cardMoveTweenWaiter.Ready = true;
		}
	}


	// Player/Camera perspective shift handling
	private void CycleCurrentPlayer(Action onFinished)
	{
		int prevPlayerIndex = _currentPlayer;
		_commander.ExecuteAndAddToCurrentTurnBundle(new SetCurrentPlayerCommand(this, _nextPlayerIndex));

		if (_players[_currentPlayer].IsHuman && _numHumanPlayers > 1)
		{
			UpdateCardVisibilityForPlayer(prevPlayerIndex, onFinished: () =>
			{
				_playerReadyButton.ClearOnClickedDelegates();
				_playerReadyButton.gameObject.SetActive(true);
				_playerReadyButton.AddToOnClicked(() =>
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
		if (_demoMode)
		{
			onFinished();
		}
		else
		{
			_mainCameraAnchor.AddLocalQuaternionRotationTween(Quaternion.Euler(Vector3.up * (float) _currentPlayer / _players.Length * 360.0f))
				.AddToOnFinishedOnce(() =>
				{
					if (_players[_currentPlayer].IsHuman)
					{
						_players[_currentPlayer].Hand.SetIntendedIncomingCardAnimState(CardModViewtroller.CardViewFSM.AnimState.VISIBLE);
					}
					onFinished();
				});
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


	// Misc. Helper Funcs.
	private void FillDeck(Action onFinished)
	{
		_discardPile.CardsTextVisibility = true;
		_commander.ExecuteAndAddToCurrentTurnBundle(
				new RefillDeckCommand(_deck, _discardPile, NUMBER_OF_CARDS_TO_LEAVE_IN_DISCARD_PILE, onFinished));
	}

	private void UpdateUndoAndRedoButtons()
	{
		_undoButton.SetActive(_commander.UndoIsPossible);
		_redoButton.SetActive(_commander.RedoIsPossible);
	}

	private void HideUndoAndRedoButtons()
	{
		if (!_demoMode)
		{
			_undoButton.SetActive(false);
			_redoButton.SetActive(false);
		}
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
		((HumanPlayerModtroller) _players[_currentPlayer]).SubmitCards();
	}

	public void EndGame()
	{
		SceneManager.LoadScene("MainMenu");
	}
}