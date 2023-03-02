using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Graphics;
using TootTally.Graphics.Animation;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TootTally.Multiplayer
{
    public class MultiplayerController
    {
        private static PlaytestAnims _currentInstance;
        private static GameObject _mainPanel, _mainPanelFg, _mainPanelBorder, _acceptButton, _declineButton, _topBar;
        private static GameObject _activeLobbyPanel, _titlePanel, _lobbyInfoPanel, _buttonsPanel, _createLobbyPanel;
        private static CanvasGroup _acceptButtonCanvasGroup, _topBarCanvasGroup, _mainTextCanvasGroup, _declineButtonCanvasGroup;
        private static List<SerializableClass.MultiplayerLobbyInfo> _lobbyInfoList;

        #region LocalTesting
        private static readonly SerializableClass.MultiplayerUserInfo _gristUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 0,
            country = "USA",
            rank = -1,
            username = "gristCollector",
            state = "null"
        };

        private static readonly SerializableClass.MultiplayerUserInfo _electrUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 1,
            country = "CAD",
            rank = 2,
            username = "Electrostats",
            state = "null"
        };

        private static readonly SerializableClass.MultiplayerUserInfo _gloomhonkUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 2,
            country = "AUS",
            rank = 20,
            username = "GloomHonk",
            state = "null"
        };
        private static readonly SerializableClass.MultiplayerUserInfo _lumpytfUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 3,
            country = "MOM",
            rank = 250000,
            username = "Lumpytf",
            state = "null"
        };
        private static readonly SerializableClass.MultiplayerUserInfo _jampotUser = new SerializableClass.MultiplayerUserInfo()
        {
            id = 4,
            country = "DAD",
            rank = 1,
            username = "Jampot",
            state = "null"
        };
        #endregion

        public MultiplayerController(PlaytestAnims __instance)
        {
            _currentInstance = __instance;
            _currentInstance.factpanel.gameObject.SetActive(false);

            GameObject canvasWindow = GameObject.Find("Canvas-Window").gameObject;
            Transform panelTransform = canvasWindow.transform.Find("Panel");

            _mainPanel = GameObjectFactory.CreateMultiplayerMainPanel(panelTransform, "MultiPanel");

            _mainPanelFg = _mainPanel.transform.Find("panelfg").gameObject;
            _mainPanelFg.AddComponent<Mask>();

            _mainPanelBorder = _mainPanel.transform.Find("Panelbg1").gameObject;

            _topBar = _mainPanel.transform.Find("top").gameObject;
            _topBarCanvasGroup = _topBar.GetComponent<CanvasGroup>();
            _mainTextCanvasGroup = _mainPanelFg.transform.Find("FactText").GetComponent<CanvasGroup>();

            _lobbyInfoList = new List<SerializableClass.MultiplayerLobbyInfo>();
        }

        public void GetLobbyInfo()
        {
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 1,
                name = "TestMulti1",
                title = "gristCollector's Lobby",
                maxPlayerCount = 16,
                currentSong = "Never gonna give you up",
                ping = 69f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _gristUser }
            });
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 2,
                name = "TestMulti2",
                title = "Electrostats's Lobby",
                maxPlayerCount = 32,
                currentSong = "Taps",
                ping = 1f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _electrUser, _jampotUser }
            });
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 3,
                name = "TestMulti3",
                title = "Lumpytf's private room",
                maxPlayerCount = 1,
                currentSong = "Forever Alone",
                ping = 12f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _lumpytfUser }
            });
            _lobbyInfoList.Add(new SerializableClass.MultiplayerLobbyInfo()
            {
                id = 4,
                name = "TestMulti4",
                title = "GloomHonk's Meme songs",
                maxPlayerCount = 99,
                currentSong = "tt is love tt is life",
                ping = 224f,
                users = new List<SerializableClass.MultiplayerUserInfo> { _gloomhonkUser }
            });

            GameObject leftPanelFG = _activeLobbyPanel.transform.Find("panelfg").gameObject;

            foreach (SerializableClass.MultiplayerLobbyInfo multiLobbyInfo in _lobbyInfoList)
                GameObjectFactory.CreateLobbyInfoRow(leftPanelFG.transform, $"{multiLobbyInfo.name}Lobby", multiLobbyInfo);


        }

        public void AddAcceptDeclineButtonsToPanelFG()
        {
            _acceptButton = GameObjectFactory.CreateCustomButton(_mainPanelFg.transform, new Vector2(-80, -340), new Vector2(200, 50), "Accept", "AcceptButton", OnAcceptButtonClick).gameObject;
            _acceptButtonCanvasGroup = _acceptButton.AddComponent<CanvasGroup>();
            _declineButton = GameObjectFactory.CreateCustomButton(_mainPanelFg.transform, new Vector2(-320, -340), new Vector2(200, 50), "Decline", "DeclineButton", OnDeclineButtonClick).gameObject;
            _declineButtonCanvasGroup = _declineButton.AddComponent<CanvasGroup>();
        }

        public void OnMultiplayerHomeScreenEnter()
        {
            DestroyFactTextTopBarAndAcceptDeclineButtons();
            AddHomeScreenPanelsToMainPanel();
            AnimateHomeScreenPanels();
        }

        public void AddHomeScreenPanelsToMainPanel()
        {
            #region TitlePanel
            _titlePanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "TitlePanel", new Vector2(1230, 50), new Vector2(0, 284));
            _titlePanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            HorizontalLayoutGroup topPanelLayoutGroup = _titlePanel.transform.Find("panelfg").gameObject.AddComponent<HorizontalLayoutGroup>();
            topPanelLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            Text lobbyText = GameObjectFactory.CreateSingleText(_titlePanel.transform.Find("panelfg"), "TitleText", "TootTally Multiplayer Lobbies", Color.white);
            lobbyText.alignment = TextAnchor.MiddleLeft;
            Text serverText = GameObjectFactory.CreateSingleText(_titlePanel.transform.Find("panelfg"), "ServerText", "Current Server: localHost", Color.white);
            serverText.alignment = TextAnchor.MiddleRight;
            #endregion

            #region ActiveLobbyPanel
            _activeLobbyPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "ActiveLobbyPanel", new Vector2(750, 564), new Vector2(-240, -28));
            _activeLobbyPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            VerticalLayoutGroup leftPanelLayoutGroup = _activeLobbyPanel.transform.Find("panelfg").gameObject.AddComponent<VerticalLayoutGroup>();
            leftPanelLayoutGroup.childForceExpandHeight = leftPanelLayoutGroup.childScaleHeight = leftPanelLayoutGroup.childControlHeight = false;
            leftPanelLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            GetLobbyInfo();
            #endregion

            #region LobbyInfoPanel
            _lobbyInfoPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "LobbyInfoPanel", new Vector2(426, 280), new Vector2(402, -170));
            _lobbyInfoPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            #endregion

            #region ButtonsPanel
            _buttonsPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "ButtonsPanel", new Vector2(426, 280), new Vector2(402, 114));
            _buttonsPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObjectFactory.CreateCustomButton(_buttonsPanel.transform, Vector2.one, new Vector2(190, 60), "Create Lobby", "CreateLobbyButton", OnCreateLobbyButtonClick);
            #endregion

            #region CreateLobbyPanel
            _createLobbyPanel = GameObjectFactory.CreateEmptyMultiplayerPanel(_mainPanelFg.transform, "CreateLobbyPanel", new Vector2(750, 564), new Vector2(1041, -28));


            #endregion
        }

        public void OnCreateLobbyButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.CreatingLobby);
            GameObject leftPanelFG = _activeLobbyPanel.transform.Find("panelfg").gameObject;
            Vector2 animationPositionOffset = Vector2.zero;
            animationPositionOffset.x = leftPanelFG.GetComponent<RectTransform>().sizeDelta.x + 57;
            AnimationManager.AddNewPositionAnimation(_activeLobbyPanel, _activeLobbyPanel.GetComponent<RectTransform>().anchoredPosition - animationPositionOffset, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewPositionAnimation(_buttonsPanel, _buttonsPanel.GetComponent<RectTransform>().anchoredPosition - animationPositionOffset, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewPositionAnimation(_lobbyInfoPanel, _lobbyInfoPanel.GetComponent<RectTransform>().anchoredPosition - animationPositionOffset, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewPositionAnimation(_createLobbyPanel, _createLobbyPanel.GetComponent<RectTransform>().anchoredPosition - animationPositionOffset, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
        }

        public void AnimateHomeScreenPanels()
        {
            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelFg, new Vector2(1240, 630), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelBorder, new Vector2(1250, 640), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) =>
            {
                AnimationManager.AddNewScaleAnimation(_titlePanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(_activeLobbyPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));

                //testing button
                /*CustomButton selectSongButton = GameObjectFactory.CreateCustomButton(leftPanel.transform, Vector2.zero, new Vector2(200, 200), "SelectSong", "SelectSongButton", delegate
                {
                    MultiplayerManager.UpdateMultiplayerState(MultiplayerState.SelectSong);
                });
                selectSongButton.GetComponent<RectTransform>().localScale = Vector2.zero;
                AnimationManager.AddNewScaleAnimation(selectSongButton.gameObject, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));*/

                AnimationManager.AddNewScaleAnimation(_buttonsPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(_lobbyInfoPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));

                MultiplayerManager.UpdateMultiplayerState(MultiplayerState.Home);
            });
        }

        public void EnterMainPanelAnimation()
        {
            AnimationManager.AddNewPositionAnimation(_mainPanel, new Vector2(0, -20), 2f, new EasingHelper.SecondOrderDynamics(1.25f, 1f, 0f));
        }

        public void OnAcceptButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.LoadHome);
            AnimationManager.AddNewSizeDeltaAnimation(_acceptButton, Vector2.zero, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewSizeDeltaAnimation(_declineButton, Vector2.zero, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) => DestroyFactTextTopBarAndAcceptDeclineButtons());
            _currentInstance.sfx_ok.Play();
        }

        public void OnDeclineButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.ExitScene);
            GameObject.Destroy(_mainPanel);
        }

        public void DestroyFactTextTopBarAndAcceptDeclineButtons()
        {
            GameObject.DestroyImmediate(_mainPanelFg.transform.Find("FactText").gameObject);
            GameObject.DestroyImmediate(_topBar);
            if (_acceptButton != null)
                GameObject.DestroyImmediate(_acceptButton);
            if (_declineButton != null)
                GameObject.DestroyImmediate(_declineButton);
        }

        public void OnExitAnimation()
        {
            AnimationManager.AddNewScaleAnimation(_mainPanel, Vector2.zero, 2f, new EasingHelper.SecondOrderDynamics(.75f, 1f, 0f));
        }



        public enum MultiplayerState
        {
            None,
            FirstTimePopUp,
            LoadHome,
            Home,
            CreatingLobby,
            Lobby,
            Hosting,
            SelectSong,
            ExitScene,
        }
    }
}
