using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using TootTally.Graphics.Animation;
using TootTally.Utils;
using TootTally.Utils.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public class TootTallyLoginPanel
    {
        private Canvas _canvas;
        private GameObject _gameObject;
        private GameObject _mainPanelBG, _mainPanelFG;
        private GameObject _menuMain, _loginMain, _signUpMain, _helpMain, _loadingMain;
        private GameObject _topPanel, _bottomPanel;
        private GameObject _topLeftContainer, _topRightContainer;
        private GameObject _bottomLeftContainer, _bottomRightContainer;
        private GameObject _previousPage, _activePage;
        private TMP_Text _titleText;
        private LoadingIcon _loadingIcon;

        private TMP_InputField _loginUsername, _loginPassword;
        private TMP_InputField _signUpUsername, _signUpPassword, _signUpConfirm;

        public TootTallyLoginPanel()
        {
            _gameObject = GameObject.Instantiate(AssetBundleManager.GetPrefab("toottallylogincanvas"));
            GameObject.DontDestroyOnLoad(_gameObject);
            _canvas = _gameObject.GetComponent<Canvas>();

            _mainPanelBG = _gameObject.transform.GetChild(0).gameObject;
            _mainPanelFG = _mainPanelBG.transform.GetChild(0).gameObject;

            _topPanel = _mainPanelFG.transform.GetChild(0).gameObject;
            _topLeftContainer = _topPanel.gameObject.transform.GetChild(0).gameObject;
            _topRightContainer = _topPanel.gameObject.transform.GetChild(1).gameObject;

            _menuMain = _mainPanelFG.transform.GetChild(1).gameObject;
            _loginMain = _mainPanelFG.transform.GetChild(2).gameObject;
            _signUpMain = _mainPanelFG.transform.GetChild(3).gameObject;
            _helpMain = _mainPanelFG.transform.GetChild(4).gameObject;
            _loadingMain = _mainPanelFG.transform.GetChild(5).gameObject;

            _bottomPanel = _mainPanelFG.transform.GetChild(6).gameObject;
            _bottomLeftContainer = _bottomPanel.gameObject.transform.GetChild(0).gameObject;
            _bottomRightContainer = _bottomPanel.gameObject.transform.GetChild(1).gameObject;

            _activePage = _menuMain;

            #region MainMenu
            _titleText = GameObjectFactory.CreateSingleText(_topLeftContainer.transform, "WelcomeText", "Welcome to toottally", GameTheme.themeColors.leaderboard.text);
            _titleText.fontSize = 66;
            _titleText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;

            GameObjectFactory.CreateDefaultButton(_menuMain.transform, Vector2.zero, new Vector2(325, 85), "LOGIN", 48, "LoginButton", OnLoginButtonClick);
            GameObjectFactory.CreateDefaultButton(_menuMain.transform, Vector2.zero, new Vector2(325, 85), "SIGN-UP", 48, "SignUpButton", OnSignUpButtonClick);

            GameObjectFactory.CreateCustomButton(_topRightContainer.transform, Vector2.zero, Vector2.one * 72, AssetManager.GetSprite("Close64.png"), false, "ReturnButton", ReturnToHomePage);

            GameObjectFactory.CreateClickableImageHolder(_bottomLeftContainer.transform, Vector2.zero, Vector2.one * 72, AssetManager.GetSprite("question128.png"), "QuestionButton", () => { OnLogoButtonClick(LogoNames.Question); });

            GameObjectFactory.CreateClickableImageHolder(_bottomRightContainer.transform, Vector2.zero, Vector2.one * 92, AssetManager.GetSprite("toottally128.png"), "TootTallyIcon", () => { OnLogoButtonClick(LogoNames.TootTally); });
            GameObjectFactory.CreateClickableImageHolder(_bottomRightContainer.transform, Vector2.zero, Vector2.one * 72, AssetManager.GetSprite("patreon128.png"), "PatreonButton", () => { OnLogoButtonClick(LogoNames.Patreon); });
            GameObjectFactory.CreateClickableImageHolder(_bottomRightContainer.transform, Vector2.zero, Vector2.one * 72, AssetManager.GetSprite("twitter128.png"), "TwitterButton", () => { OnLogoButtonClick(LogoNames.Twitter); });
            GameObjectFactory.CreateClickableImageHolder(_bottomRightContainer.transform, Vector2.zero, Vector2.one * 96, AssetManager.GetSprite("discord128.png"), "DiscordButton", () => { OnLogoButtonClick(LogoNames.Discord); });
            #endregion


            #region LoginMenu
            var textOffset = new Vector2(.7f, 0);
            _loginUsername = GameObjectFactory.CreateInputField(_loginMain.transform, Vector2.zero, new Vector2(350, 25), "LoginNameInput", false);
            GameObjectFactory.CreateSingleText(_loginUsername.transform, "UsernameText", "Username:", textOffset, new Vector2(350, 25), Color.white);

            _loginPassword = GameObjectFactory.CreateInputField(_loginMain.transform, Vector2.zero, new Vector2(350, 25), "PasswordNameInput", true);
            GameObjectFactory.CreateSingleText(_loginPassword.transform, "PasswordText", "Password:", textOffset, new Vector2(350, 25), Color.white);

            GameObjectFactory.CreateCustomButton(_loginMain.transform, Vector2.zero, new Vector2(225, 45), "Login", "SubmitLoginButton", OnSubmitLogin);
            #endregion


            #region SignUpMenu
            _signUpUsername = GameObjectFactory.CreateInputField(_signUpMain.transform, Vector2.zero, new Vector2(350, 25), "LoginNameInput", false);
            GameObjectFactory.CreateSingleText(_signUpUsername.transform, "UsernameText", "Username:", textOffset, new Vector2(350, 25), Color.white);

            _signUpPassword = GameObjectFactory.CreateInputField(_signUpMain.transform, Vector2.zero, new Vector2(350, 25), "PasswordNameInput", true);
            GameObjectFactory.CreateSingleText(_signUpPassword.transform, "PasswordText", "Password:", textOffset, new Vector2(350, 25), Color.white);

            _signUpConfirm = GameObjectFactory.CreateInputField(_signUpMain.transform, Vector2.zero, new Vector2(350, 25), "ConfirmNameInput", true);
            GameObjectFactory.CreateSingleText(_signUpConfirm.transform, "ConfirmText", "Confirm:", new Vector2(.65f, 0), new Vector2(350, 25), Color.white);

            GameObjectFactory.CreateCustomButton(_signUpMain.transform, Vector2.zero, new Vector2(225, 45), "Sign Up", "SubmitSignUpButton", OnSubmitSignUp);
            #endregion


            #region HelpMenu

            #endregion

            #region Loading
            _loadingIcon = GameObjectFactory.CreateLoadingIcon(_loadingMain.transform, Vector2.zero, Vector2.one * 64, AssetManager.GetSprite("toottally128.png"), false, "LoadingIcon");
            #endregion
        }

        private void OnSubmitLogin()
        {
            ShowLoading();
            PopUpNotifManager.DisplayNotif("Sending login info... Please wait.", GameTheme.themeColors.notification.defaultText);
            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetLoginToken(_loginUsername.text, _loginPassword.text, (token) =>
            {
                if (token.token == "")
                {
                    PopUpNotifManager.DisplayNotif("Username or password wrong... Try logging in again.", GameTheme.themeColors.notification.errorText);
                    ReturnPage("TootTally Login");
                    return;
                }

                Plugin.Instance.StartCoroutine(TootTallyAPIService.GetUserFromToken(token.token, (user) =>
                {
                    if (user == null)
                    {
                        PopUpNotifManager.DisplayNotif("Couldn't get user info... Please contact TootTally's moderator on discord.", GameTheme.themeColors.notification.errorText);
                        ReturnPage("TootTally Login");
                        return;
                    }
                    PopUpNotifManager.DisplayNotif($"Login with {user.username} successful!", GameTheme.themeColors.notification.defaultText);
                    Plugin.OnUserLogin(user);
                    Hide();
                }));
            }));
        }

        private void OnLoginButtonClick()
        {
            _titleText.text = "TootTally Login";
            ChangePage(_loginMain);
        }

        private void OnSubmitSignUp()
        {
            if (!IsValidUsername(_signUpUsername.text))
            {
                PopUpNotifManager.DisplayNotif("Please enter a valid Username.", GameTheme.themeColors.notification.defaultText);
                return;
            }
            if (!IsValidPassword(_signUpPassword.text))
            {
                if (_signUpPassword.text.Length <= 5)
                    PopUpNotifManager.DisplayNotif("Password has to be at least 5 characters long.", GameTheme.themeColors.notification.defaultText);
                else
                    PopUpNotifManager.DisplayNotif("Please enter a valid Password.", GameTheme.themeColors.notification.defaultText);
                return;
            }

            if (_signUpPassword.text != _signUpConfirm.text)
            {
                _signUpPassword.text = "";
                _signUpConfirm.text = "";
                PopUpNotifManager.DisplayNotif($"Passwords did not match! Type your password again.", GameTheme.themeColors.notification.errorText);
                return; //skip requests
            }
            PopUpNotifManager.DisplayNotif($"Sending sign up request... Please wait.", GameTheme.themeColors.notification.defaultText);
            ShowLoading();
            Plugin.Instance.StartCoroutine(TootTallyAPIService.SignUpRequest(_signUpUsername.text, _signUpPassword.text, _signUpConfirm.text, isValid =>
            {
                if (isValid)
                {
                    PopUpNotifManager.DisplayNotif($"Getting new user info...", GameTheme.themeColors.notification.defaultText);
                    Plugin.Instance.StartCoroutine(TootTallyAPIService.GetLoginToken(_signUpUsername.text, _signUpPassword.text, token =>
                    {
                        if (token.token != "")
                        {
                            Plugin.Instance.StartCoroutine(TootTallyAPIService.GetUserFromToken(token.token, user =>
                            {
                                if (user != null)
                                {
                                    PopUpNotifManager.DisplayNotif($"Login with {user.username} successful!", GameTheme.themeColors.notification.defaultText);
                                    Plugin.OnUserLogin(user);
                                    Plugin.Instance.APIKey.Value = user.api_key;
                                    Hide();
                                }
                                else
                                {
                                    PopUpNotifManager.DisplayNotif($"Unexpected Error Occured...", GameTheme.themeColors.notification.warningText);
                                    ReturnPage("TootTally Sign-up");
                                }
                            }));
                        }
                        else
                        {
                            PopUpNotifManager.DisplayNotif($"Unexpected Error Occured...", GameTheme.themeColors.notification.warningText);
                            ReturnPage("TootTally Sign-up");
                        }
                    }));
                }
                else
                {
                    PopUpNotifManager.DisplayNotif($"Username or password denied by server.", GameTheme.themeColors.notification.warningText);
                    ReturnPage("TootTally Sign-up");
                }
            }));
        }

        private bool IsValidUsername(string username) => username != "" && !username.ToLower().Contains("username");
        private bool IsValidPassword(string password) => !password.ToLower().Contains("password") && password.Length > 5 && !password.ToLower().Contains(_signUpUsername.text);

        private void ShowLoading()
        {
            _loadingIcon.Show();
            _loadingIcon.StartRecursiveAnimation();
            ChangePage(_loadingMain);
        }

        private void HideLoading()
        {
            _loadingIcon.StopRecursiveAnimation(true);
            _loadingIcon.Hide();
        }

        private void OnSignUpButtonClick()
        {
            _titleText.text = "TootTally Sign-up";
            ChangePage(_signUpMain);
        }


        private void OnLogoButtonClick(LogoNames logoName)
        {
            switch (logoName)
            {
                case LogoNames.Question:
                    ChangePage(_helpMain);
                    break;
                case LogoNames.TootTally:
                    Application.OpenURL("https://toottally.com/");
                    break;

                case LogoNames.Patreon:
                    Application.OpenURL("https://patreon.com/TootTally");
                    break;

                case LogoNames.Twitter:
                    Application.OpenURL("https://twitter.com/TootTally");
                    break;

                case LogoNames.Discord:
                    Application.OpenURL("https://discord.gg/9jQmVEDVTp");
                    break;
            }
        }

        private void ChangePage(GameObject targetPage)
        {
            _previousPage = _activePage;
            _activePage = targetPage;
            _previousPage.SetActive(false);
            _activePage.SetActive(true);
        }

        private void ReturnPage(string titleText = "")
        {
            if (_previousPage != null)
            {
                if (titleText == "")
                    _titleText.text = "Welcome To Toottally";
                else
                    _titleText.text = titleText;
                ChangePage(_previousPage);
                _previousPage = null;
            }
        }

        private void ReturnToHomePage()
        {
            if (_activePage == _menuMain)
                Hide();
            else
            {
                _titleText.text = "Welcome To Toottally";
                ChangePage(_menuMain);
                _previousPage = null;
            }



        }

        public void ApplyTheme()
        {

        }

        public void Show()
        {
            _gameObject.SetActive(true);
            _mainPanelBG.transform.localScale = Vector2.zero;
            AnimationManager.AddNewScaleAnimation(_mainPanelBG, Vector3.one, 1f, GetSecondDegreeAnimation(1.5f));
        }

        public void Hide()
        {
            AnimationManager.AddNewScaleAnimation(_mainPanelBG, Vector2.zero, .7f, new EasingHelper.SecondOrderDynamics(1.75f, 0.75f, 1f), sender => _gameObject.SetActive(false));
        }

        public static EasingHelper.SecondOrderDynamics GetSecondDegreeAnimation(float speedMult = 1f) => new EasingHelper.SecondOrderDynamics(speedMult, 0.75f, 1.15f);

        private enum LogoNames
        {
            Question,
            TootTally,
            Patreon,
            Twitter,
            Discord
        }

    }
}
