﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;

using Sanguosha.Core.Players;
using Sanguosha.Core.Heroes;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Games;
using Sanguosha.Core.Cards;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading;
using Sanguosha.Core.UI;
using System.Diagnostics;

namespace Sanguosha.UI.Controls
{
    public class NumRolesToComboBoxEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Role> roles = value as List<Role>;
            return (roles != null) && (roles.Count > 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PlayerInfoViewModel : SelectableItem, IAsyncUiProxy
    {
        #region Constructors
        public PlayerInfoViewModel()
        {
            IsSelectionMode = false;
            SkillCommands = new ObservableCollection<SkillCommand>();
            HeroSkillNames = new ObservableCollection<string>();
            heroNameChars = new ObservableCollection<string>();
            MultiChoiceCommands = new ObservableCollection<ICommand>();
            
            SubmitAnswerCommand = new SimpleRelayCommand(ExecuteSubmitAnswerCommand);
            CancelAnswerCommand = new SimpleRelayCommand(ExecuteCancelCommand);
            AbortAnswerCommand = new SimpleRelayCommand(ExecuteAbortCommand);
            
            _UpdateEnabledStatusHandler = (o, e) => { _UpdateCommandStatus(); };
            _timer = new System.Timers.Timer();
        }

        public PlayerInfoViewModel(Player p) : this()
        {
            Player = p;
        }
        #endregion

        #region Fields
        Player _player;

        // @todo: to be deprecated. use HostPlayer instead.
        public Player Player
        {
            get { return _player; }
            set 
            {
                if (_player == value) return;
                if (_player != null)
                {
                    PropertyChangedEventHandler handler = _PropertyChanged;
                    _player.PropertyChanged -= handler;
                }
                _player = value;
                _PropertyChanged = _OnPlayerPropertyChanged;                
                _player.PropertyChanged += _PropertyChanged;
                var properties = typeof(Player).GetProperties();
                foreach (var property in properties)
                {
                    _OnPlayerPropertyChanged(_player, new PropertyChangedEventArgs(property.Name));
                }
            }
        }

        private GameViewModel _game;

        public GameViewModel GameModel
        {
            get { return _game; }
            set
            {
                if (_game == value) return;
                bool changed = (_game == null || _game.GetType() != value.GetType());                
                _game = value;
                _game.PropertyChanged += new PropertyChangedEventHandler(_game_PropertyChanged);
                if (changed)
                {
                    OnPropertyChanged("PossibleRoles");
                }
            }
        }

        void _game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string name = e.PropertyName;
            if (name == "CurrentPlayer")
            {   
                OnPropertyChanged("IsCurrentPlayer");
            }
            else if (name == "CurrentPhase")
            {                
                OnPropertyChanged("CurrentPhase");
            }
        }

        private PropertyChangedEventHandler _PropertyChanged;

        private void _UpdateSkills()
        {
            SkillCommands.Clear();
            foreach (ISkill skill in _player.Skills)
            {
                SkillCommands.Add(new SkillCommand() { Skill = skill, IsEnabled = false });
            }            
        }

        private void _UpdateHeroInfo()
        {
            HeroSkillNames.Clear();
            heroNameChars.Clear();

            if (Hero != null)
            {
                string name = Application.Current.TryFindResource(string.Format("Hero.{0}.Name", Hero.Name)) as string;
                if (name != null)
                {
                    foreach (var heroChar in name)
                    {
                        heroNameChars.Add(heroChar.ToString());
                    }
                }
                foreach (var skill in Hero.Skills)
                {
                    HeroSkillNames.Add(skill.GetType().Name);
                }
            } 
        }

        private void _OnPlayerPropertyChanged(object o, PropertyChangedEventArgs e)
        {
            string name = e.PropertyName;
            if (name == "Role")
            {
                OnPropertyChanged("PossibleRoles");
            }
            else if (name == "Hero")
            {       
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    _UpdateHeroInfo();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((ThreadStart)_UpdateHeroInfo);                    
                    OnPropertyChanged("HeroName");                    
                }
            }
            else if (name == "Skills")
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    _UpdateSkills();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke((ThreadStart)delegate(){ _UpdateSkills(); });
                }                
            }
            else
            {
                var propNames = from prop in this.GetType().GetProperties() select prop.Name;
                if (propNames.Contains(name))
                {
                    OnPropertyChanged(name);
                }
            }    
        }

        #endregion

        #region Commands

        public SimpleRelayCommand SubmitAnswerCommand
        {
            get;
            internal set;
        }

        public SimpleRelayCommand CancelAnswerCommand
        {
            get;
            internal set;
        }

        public SimpleRelayCommand AbortAnswerCommand
        {
            get;
            internal set;
        }

        #endregion

        #region Interactions

        private double _timeOutSeconds;
        public double TimeOutSeconds
        {
            get
            {
                return _timeOutSeconds;
            }
            set
            {
                if (_timeOutSeconds == value) return;
                _timeOutSeconds = value;
                OnPropertyChanged("TimeOutSeconds");                
            }
        }

        #endregion

        #region Player Properties
        public Allegiance Allegiance
        {
            get
            {
                if (_player == null)
                {
                    return Allegiance.Unknown;
                }
                else
                {
                    return _player.Allegiance;
                }
            }
        }

        public int Health
        {
            get
            {
                if (_player == null)
                {
                    return 0;
                }
                else
                {
                    return _player.Health;
                }
            }
        }

        public int MaxHealth
        {
            get
            {
                if (_player == null)
                {
                    return 0;
                }
                else
                {
                    return _player.MaxHealth;
                }
            }
        }

        public Hero Hero
        {
            get
            {
                return _player.Hero;
            }
        }

        public Hero Hero2
        {
            get
            {
                return _player.Hero2;
            }
        }

        public int Id
        {
            get
            {
                return _player.Id;
            }
        }

        public bool IsFemale
        {
            get
            {
                return _player.IsFemale;
            }
        }

        public bool IsMale
        {
            get
            {
                return _player.IsMale;
            }
        }

        public bool IsTargeted { get { return _player.IsTargeted; } }

        public bool IsCurrentPlayer 
        {
            get 
            {
                if (_game == null) return false;
                else
                {
                    return _player == _game.Game.CurrentPlayer;
                }
            } 
        }

        public TurnPhase CurrentPhase
        {
            get
            {
                if (!IsCurrentPlayer)
                {
                    return TurnPhase.Inactive;
                }
                return _game.Game.CurrentPhase;
            }
        }

        #endregion
        
        #region Derived Player Properties

        public ObservableCollection<CardViewModel> HandCards
        {
            get;
            set;
        }


        public ObservableCollection<SkillCommand> SkillCommands
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the skill names of the primary hero.
        /// </summary>
        /// <remarks>For displaying tooltip purposes.</remarks>
        public ObservableCollection<string> HeroSkillNames
        {
            get;
            private set;
        }

        public string HeroName
        {
            get
            {
                if (_player == null || _player.Hero == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (_player.Hero.Name);
                }
            }
        }

        ObservableCollection<string> heroNameChars;

        public ObservableCollection<string> HeroNameChars
        {
            get
            {
                return heroNameChars;
            }
        }

        private static List<Role> roleGameRoles = new List<Role>() { Role.Loyalist, Role.Defector, Role.Rebel };

        public List<Role> PossibleRoles
        {
            get
            {
                List<Role> roles = new List<Role>();
                roles.Add(Role.Unknown);
                if (GameModel != null)
                {
                    if (GameModel.Game is RoleGame)
                    {
                        if (_player.Role == Role.Unknown)
                        {
                            roles.AddRange(roleGameRoles);
                        }
                        else if (_player.Role == Role.Ruler)
                        {
                            roles.Clear();
                            roles.Add(_player.Role);
                        }
                        else
                        {
                            roles.Add(_player.Role);
                        }
                    }
                    else
                    {
                        // @todo: add other possibilities here.
                    }
                }
                return roles;
            }
        }

        #endregion

        #region Equipments and DelayedTools

        public EquipCommand WeaponCommand { get; set; }

        public EquipCommand ArmorCommand { get; set; }

        public EquipCommand DefensiveHorseCommand { get; set; }

        public EquipCommand OffensiveHorseCommand { get; set; }

        public IList<EquipCommand> EquipCommands
        {
            get
            {
                IList<EquipCommand> result = new List<EquipCommand>();
                if (WeaponCommand != null) result.Add(WeaponCommand);
                if (ArmorCommand != null) result.Add(ArmorCommand);
                if (DefensiveHorseCommand != null) result.Add(DefensiveHorseCommand);
                if (OffensiveHorseCommand != null) result.Add(OffensiveHorseCommand);
                return result;
            }
        }

        #endregion

        #region Commands

        #region SubmitAnswerCommand

        public void ExecuteSubmitAnswerCommand(object parameter)
        {
            List<Card> cards = _GetSelectedHandCards();
            List<Player> players = _GetSelectedPlayers();
            ISkill skill = null;
            bool isEquipSkill;
            SkillCommand skillCommand = _GetSelectedSkillCommand(out isEquipSkill);

            foreach (var equipCommand in EquipCommands)
            {
                if (!isEquipSkill && equipCommand.IsSelected)
                {
                    cards.Add(equipCommand.Card);
                }
            }

            if (skillCommand != null)
            {
                skill = skillCommand.Skill;
            }

            _ResetAll();

            // Card usage question
            if (currentUsageVerifier != null)
            {
                currentUsageVerifier = null;
                CardUsageAnsweredEvent(skill, cards, players);
            }
        }

        #endregion

        #region CancelAnswerCommand

        public void ExecuteCancelCommand(object parameter)
        {
            if (currentUsageVerifier != null)
            {
                // @todo
                //if (currentUsageVerifier.GetType().Name!="PlayerActionStageVerifier")
                {
                    CardUsageAnsweredEvent(null, null, null);
                    currentUsageVerifier = null;
                    _ResetAll();
                }
                /*else
                {
                    _ResetAll();
                }*/
            }
        }
        #endregion

        #region AbortAnswerCommand

        public void ExecuteAbortCommand(object parameter)
        {
            _timer.Stop();
            if (currentUsageVerifier != null)
            {
                CardUsageAnsweredEvent(null, null, null);
                currentUsageVerifier = null;
                _ResetAll();
            }
        }
        #endregion

        #region MultiChoiceCommand

        public ObservableCollection<ICommand> MultiChoiceCommands
        {
            get;
            private set;
        }

        public void ExecuteMultiChoiceCommand(object parameter)
        {
            _ResetAll();
            MultipleChoiceAnsweredEvent((int)parameter);
        }
        #endregion

        #endregion

        #region View Related Fields

        private int _handCardCount;
        
        public int HandCardCount
        {
            get { return _handCardCount; }
            set
            {
                if (_handCardCount == value) return;
                _handCardCount = value;
                OnPropertyChanged("HandCardCount");
            }
        }

        private string _prompt;
        public string CurrentPrompt
        {
            get
            {
                return _prompt;
            }
            set
            {
                if (_prompt == value) return;
                _prompt = value;
                OnPropertyChanged("CurrentPrompt");
            }
        }

        #endregion

        #region IASyncUiProxy Helpers
        ICardUsageVerifier currentUsageVerifier;
        bool isMultiChoiceQuestion;

        SkillCommand _GetSelectedSkillCommand(out bool isEquipSkill)
        {
            foreach (var skillCommand in SkillCommands)
            {
                if (skillCommand.IsSelected)
                {
                    isEquipSkill = false;
                    return skillCommand;
                }
            }
            foreach (EquipCommand equipCmd in EquipCommands)
            {
                if (equipCmd.IsSelected)
                {
                    isEquipSkill = true;
                    return equipCmd.SkillCommand;
                }
            }
            isEquipSkill = false;
            return null;
        }

        private List<Card> _GetSelectedHandCards()
        {
            List<Card> cards = new List<Card>();
            foreach (var card in HandCards)
            {
                if (card.IsSelected)
                {
                    Trace.Assert(card.Card != null);
                    cards.Add(card.Card);
                }
            }
            return cards;
        }

        private List<Player> _GetSelectedPlayers()
        {
            List<Player> players = new List<Player>();
            foreach (var playerModel in _game.PlayerModels)
            {
                if (playerModel.IsSelected)
                {
                    players.Add(playerModel.Player);
                }
            }
            return players;
        }

        private void _ResetCommands()
        {
            foreach (var equipCommand in EquipCommands)
            {
                equipCommand.OnSelectedChanged -= _UpdateEnabledStatusHandler;
                equipCommand.IsSelectionMode = false;
            }

            foreach (var skillCommand in SkillCommands)
            {
                skillCommand.IsSelected = false;
                skillCommand.IsEnabled = false;
            }

            foreach (CardViewModel card in HandCards)
            {
                card.OnSelectedChanged -= _UpdateEnabledStatusHandler;
                card.IsSelectionMode = false;
            }

            foreach (var playerModel in _game.PlayerModels)
            {
                playerModel.OnSelectedChanged -= _UpdateEnabledStatusHandler;
                playerModel.IsSelectionMode = false;
            }

            CurrentPrompt = string.Empty;

            SubmitAnswerCommand.CanExecuteStatus = false;
            CancelAnswerCommand.CanExecuteStatus = false;
            AbortAnswerCommand.CanExecuteStatus = false;
            TimeOutSeconds = 0;
        }

        private void _ResetAll()
        {
            MultiChoiceCommands.Clear();
            _ResetCommands();
        }

        private void _UpdateCardUsageStatus()
        {
            List<Card> cards = _GetSelectedHandCards();
            List<Player> players = _GetSelectedPlayers();
            ISkill skill = null;
            bool isEquipCommand;
            SkillCommand command = _GetSelectedSkillCommand(out isEquipCommand);

            if (command != null)
            {
                skill = command.Skill;
            }

            // Handle skill down            
            foreach (var skillCommand in SkillCommands)
            {
                skillCommand.IsEnabled = (currentUsageVerifier.Verify(skillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);
            }

            if (skill == null)
            {
                foreach (var equipCommand in EquipCommands)
                {
                    if (equipCommand.SkillCommand.Skill == null)
                    {
                        equipCommand.IsEnabled = false;
                    }
                    equipCommand.IsEnabled = (currentUsageVerifier.Verify(equipCommand.SkillCommand.Skill, new List<Card>(), new List<Player>()) != VerifierResult.Fail);
                }
            }
            if (!isEquipCommand)
            {
                foreach (var equipCommand in EquipCommands)
                {
                    if (equipCommand.IsSelected)
                        cards.Add(equipCommand.Card);
                }
            }

            var status = currentUsageVerifier.Verify(skill, cards, players);

            if (status == VerifierResult.Fail)
            {
                SubmitAnswerCommand.CanExecuteStatus = false;
                foreach (var playerModel in _game.PlayerModels)
                {
                    playerModel.IsSelected = false;
                }
            }
            else if (status == VerifierResult.Partial)
            {
                SubmitAnswerCommand.CanExecuteStatus = false;
            }
            else if (status == VerifierResult.Success)
            {
                SubmitAnswerCommand.CanExecuteStatus = true;
            }

            List<Card> attempt = new List<Card>(cards);

            foreach (var card in HandCards)
            {
                if (card.IsSelected)
                {
                    continue;
                }
                attempt.Add(card.Card);
                bool disabled = (currentUsageVerifier.Verify(skill, attempt, players) == VerifierResult.Fail);
                card.IsEnabled = !disabled;
                attempt.Remove(card.Card);
            }

            if (skill != null)
            {
                foreach (var equipCommand in EquipCommands)
                {
                    if (equipCommand.IsSelected) continue;

                    attempt.Add(equipCommand.Card);
                    bool disabled = (currentUsageVerifier.Verify(skill, attempt, players) == VerifierResult.Fail);
                    equipCommand.IsEnabled = !disabled;
                    attempt.Remove(equipCommand.Card);
                }
            }

            // Invalidate target selection
            List<Player> attempt2 = new List<Player>(players);
            int validCount = 0;
            bool[] enabledMap = new bool[_game.PlayerModels.Count];
            int i = 0;
            foreach (var playerModel in _game.PlayerModels)
            {
                enabledMap[i] = false;
                if (playerModel.IsSelected)
                {
                    i++;
                    continue;
                }
                attempt2.Add(playerModel.Player);
                bool disabled = (currentUsageVerifier.Verify(skill, cards, attempt2) == VerifierResult.Fail);
                if (!disabled)
                {
                    validCount++;
                    enabledMap[i] = true;
                }
                attempt2.Remove(playerModel.Player);
                i++;

            }
            i = 0;

            bool allowSelection = (cards.Count != 0 || validCount != 0 || skill != null);
            foreach (var playerModel in _game.PlayerModels)
            {
                if (playerModel.IsSelected)
                {
                    i++;
                    continue;
                }
                playerModel.IsSelectionMode = allowSelection;
                if (allowSelection)
                {
                    playerModel.IsEnabled = enabledMap[i];
                }
                i++;
            }
        }

        private void _UpdateCommandStatus()
        {
            if (currentUsageVerifier != null)
            {
                _UpdateCardUsageStatus();
            }
            else if (isMultiChoiceQuestion)
            {
                _ResetCommands();
            }
        }

        private void _StartTimer(int timeOutSeconds)
        {
            if (timeOutSeconds > 0)
            {
                TimeOutSeconds = timeOutSeconds;

                _timer = new System.Timers.Timer(timeOutSeconds * 1000);
                _timer.AutoReset = false;
                _timer.Elapsed +=
                    (o, e) =>
                    {
                        Application.Current.Dispatcher.Invoke(
                            (ThreadStart)delegate() { ExecuteAbortCommand(null); });
                    };
                _timer.Start();
            }
        }

        private System.Timers.Timer _timer;

        private EventHandler _UpdateEnabledStatusHandler;
        #endregion

        #region IAsyncUiProxy
        public Player HostPlayer
        {
            get
            {
                return Player;
            }
            set
            {
                Player = value;
            }
        }

        public void AskForCardUsage(Prompt prompt, ICardUsageVerifier verifier, int timeOutSeconds)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                currentUsageVerifier = verifier;
                Game.CurrentGame.CurrentActingPlayer = HostPlayer;
                CurrentPrompt = PromptFormatter.Format(prompt);

                foreach (var equipCommand in EquipCommands)
                {
                    equipCommand.OnSelectedChanged += _UpdateEnabledStatusHandler;
                    equipCommand.IsSelectionMode = true;
                }

                foreach (var card in HandCards)
                {
                    card.IsSelectionMode = true;
                    card.OnSelectedChanged += _UpdateEnabledStatusHandler;
                }

                foreach (var playerModel in _game.PlayerModels)
                {
                    playerModel.IsSelectionMode = true;
                    playerModel.OnSelectedChanged += _UpdateEnabledStatusHandler;
                }

                foreach (var skillCommand in SkillCommands)
                {
                    skillCommand.OnSelectedChanged += _UpdateEnabledStatusHandler;
                }

                // @todo: update this.
                CancelAnswerCommand.CanExecuteStatus = true;
                AbortAnswerCommand.CanExecuteStatus = true;

                _StartTimer(timeOutSeconds);
                
                _UpdateCommandStatus();
            });
        }

        public void AskForCardChoice(Prompt prompt, List<DeckPlace> sourceDecks, List<string> resultDeckNames, List<int> resultDeckMaximums, ICardChoiceVerifier verifier, int timeOutSeconds)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                CurrentPrompt = PromptFormatter.Format(prompt);
                CardChoiceAnsweredEvent(null);
            });
        }

        public void AskForMultipleChoice(Prompt prompt, List<string> choices, int timeOutSeconds)
        {
            Application.Current.Dispatcher.Invoke((ThreadStart)delegate()
            {
                CurrentPrompt = PromptFormatter.Format(prompt);
                for (int i = 0; i < choices.Count; i++)
                {
                    if (choices[i] == Prompt.YesChoice)
                    {

                    }
                    else if (choices[i] == Prompt.NoChoice)
                    {
                    }
                    else
                    {
                        MultiChoiceCommands.Add(
                            new MultiChoiceCommand(ExecuteMultiChoiceCommand)
                            {
                                CanExecuteStatus = true,
                                ChoiceKey = choices[i],
                                ChoiceIndex = i
                            });
                    }
                }
                isMultiChoiceQuestion = true;
                _UpdateCommandStatus();
            });
        }

        public event CardUsageAnsweredEventHandler CardUsageAnsweredEvent;

        public event CardChoiceAnsweredEventHandler CardChoiceAnsweredEvent;

        public event MultipleChoiceAnsweredEventHandler MultipleChoiceAnsweredEvent;

        public void Freeze()
        {
            _ResetAll();
        }        
        #endregion

    }
}
