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
        private static bool _isPointerOver;
        private static RectTransform _mainPanelRectTransform;
        private static GameObject _mainPanel, _mainPanelFg, _mainPanelBorder, _acceptButton, _declineButton, _topBar;
        private static CanvasGroup _acceptButtonCanvasGroup, _topBarCanvasGroup, _mainTextCanvasGroup, _declineButtonCanvasGroup;

        public MultiplayerController(PlaytestAnims __instance)
        {
            _currentInstance = __instance;
            _currentInstance.factpanel.gameObject.SetActive(false);

            GameObject canvasWindow = GameObject.Find("Canvas-Window").gameObject;
            Transform panelTransform = canvasWindow.transform.Find("Panel");

            _mainPanel = GameObjectFactory.CreateMultiplayerMainPanel(panelTransform, "MultiPanel");
            _mainPanelRectTransform = _mainPanel.GetComponent<RectTransform>();


            _mainPanelFg = _mainPanel.transform.Find("panelfg").gameObject;

            _mainPanelBorder = _mainPanel.transform.Find("Panelbg1").gameObject;

            _topBar = _mainPanel.transform.Find("top").gameObject;
            _topBarCanvasGroup = _topBar.GetComponent<CanvasGroup>();
            _mainTextCanvasGroup = _mainPanelFg.transform.Find("FactText").GetComponent<CanvasGroup>();

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
            AddHomeScreenPanelsToMainPanel();
            AnimateHomeScreenPanels();
        }

        public void AddHomeScreenPanelsToMainPanel()
        {
            GameObject topPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "TopPanel", new Vector2(1230, 50), new Vector2(0, 284));
            topPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObject leftPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "LeftPanel", new Vector2(750, 564), new Vector2(-240, -28));
            leftPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObject topRightPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "TopRightPanel", new Vector2(426, 280), new Vector2(402, 114));
            topRightPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
            GameObject bottomRightPanel = GameObjectFactory.CreateMultiplayerPanel(_mainPanel.transform, "BottomRightPanel", new Vector2(426, 280), new Vector2(402, -170));
            bottomRightPanel.GetComponent<RectTransform>().localScale = Vector2.zero;
        }

        public void AnimateHomeScreenPanels()
        {
            GameObject topPanel = _mainPanel.transform.Find("TopPanel").gameObject;
            GameObject leftPanel = _mainPanel.transform.Find("LeftPanel").gameObject;
            GameObject topRightPanel = _mainPanel.transform.Find("TopRightPanel").gameObject;
            GameObject bottomRightPanel = _mainPanel.transform.Find("BottomRightPanel").gameObject;

            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelFg, new Vector2(1240, 630), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            AnimationManager.AddNewSizeDeltaAnimation(_mainPanelBorder, new Vector2(1250, 640), 0.8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f), (sender) =>
            {
                AnimationManager.AddNewScaleAnimation(topPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(leftPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));

                //testing button
                CustomButton selectSongButton = GameObjectFactory.CreateCustomButton(leftPanel.transform, Vector2.zero, new Vector2(200, 200), "SelectSong", "SelectSongButton", delegate
                {
                    MultiplayerManager.UpdateMultiplayerState(MultiplayerState.SelectSong);
                });
                selectSongButton.GetComponent<RectTransform>().localScale = Vector2.zero;
                AnimationManager.AddNewScaleAnimation(selectSongButton.gameObject, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));

                AnimationManager.AddNewScaleAnimation(topRightPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
                AnimationManager.AddNewScaleAnimation(bottomRightPanel, Vector2.one, .8f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));

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
            AnimationManager.AddNewSizeDeltaAnimation(_declineButton, Vector2.zero, 1f, new EasingHelper.SecondOrderDynamics(1.75f, 1f, 0f));
            _currentInstance.sfx_ok.Play();
        }

        public void OnDeclineButtonClick()
        {
            MultiplayerManager.UpdateMultiplayerState(MultiplayerState.ExitScene);
            _currentInstance.clickedOK();
            GameObject.Destroy(_mainPanel);
            GameObject.Destroy(_acceptButton);
            GameObject.Destroy(_declineButton);
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
            Lobby,
            Hosting,
            SelectSong,
            ExitScene,
        }
    }
}
