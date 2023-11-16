using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using TMPro;
using TootTally.Replays;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Audio.Handle;

namespace TootTally.GameplayModifier
{
    public static class GameModifiers
    {

        public class Hidden : GameModifierBase
        {
            public override string Name => "HD";

            public override ModifierType ModifierType => ModifierType.Hidden;

            public Hidden() : base() { }

            public static List<GameObject> _allnoteList;
            public static List<FullNoteComponents> _activeNotesComponents;
            public static List<FullNoteComponents> _notesToRemove;
            public static Color _headOutColor, _headInColor, _headOutColorLerpEnd, _headInColorLerpEnd;
            public static Color _tailOutColor, _tailInColor, _tailOutColorLerpEnd, _tailInColorLerpEnd;
            public static Color _bodyOutStartColor, _bodyOutEndColor, _bodyOutStartColorLerpEnd, _bodyOutEndColorLerpEnd;
            public static Color _bodyInStartColor, _bodyInEndColor, _bodyInStartColorLerpEnd, _bodyInEndColorLerpEnd;
            public const float START_FADEOUT_POSX = 3.7f;
            public const float END_FADEOUT_POSX = -0.7f;

            public override void Initialize(GameController __instance)
            {
                _allnoteList = __instance.allnotes;
                _activeNotesComponents = new List<FullNoteComponents>();
                _notesToRemove = new List<FullNoteComponents>();

                //Doing all this to make sure its future proof in case cosmetic plugin changes the outline colors or some shit
                var note = _allnoteList.First();
                _headOutColor = note.transform.Find("StartPoint").GetComponent<Image>().color;
                _headOutColorLerpEnd = GetColorZeroAlpha(_headOutColor);

                _headInColor = note.transform.Find("StartPoint/StartPointColor").GetComponent<Image>().color;
                _headInColorLerpEnd = GetColorZeroAlpha(_headInColor);

                _tailOutColor = note.transform.Find("EndPoint").GetComponent<Image>().color;
                _tailOutColorLerpEnd = GetColorZeroAlpha(_tailOutColor);

                _tailInColor = note.transform.Find("EndPoint/EndPointColor").GetComponent<Image>().color;
                _tailInColorLerpEnd = GetColorZeroAlpha(_tailInColor);

                _bodyOutStartColor = note.transform.Find("OutlineLine").GetComponent<LineRenderer>().startColor;
                _bodyOutStartColorLerpEnd = GetColorZeroAlpha(_bodyOutStartColor);

                _bodyOutEndColor = note.transform.Find("OutlineLine").GetComponent<LineRenderer>().endColor;
                _bodyOutEndColorLerpEnd = GetColorZeroAlpha(_bodyOutEndColor);

                _bodyInStartColor = note.transform.Find("Line").GetComponent<LineRenderer>().startColor;
                _bodyInStartColorLerpEnd = GetColorZeroAlpha(_bodyInStartColor);

                _bodyInEndColor = note.transform.Find("Line").GetComponent<LineRenderer>().endColor;
                _bodyInEndColorLerpEnd = GetColorZeroAlpha(_bodyInEndColor);
            }

            public override void Update(GameController __instance)
            {
                //Start pos * 2.7f is roughly the complete right of the screen, which means the notes are added as they enter the screen
                foreach (GameObject currentNote in _allnoteList.Where(n => n.transform.position.x <= START_FADEOUT_POSX * 2.7f && !_activeNotesComponents.Any(c => c.note == n)))
                {
                    if (currentNote.transform.position.x <= START_FADEOUT_POSX * 2.7f && currentNote.transform.Find("EndPoint").position.x > END_FADEOUT_POSX + 1)
                    {
                        var noteComp = new FullNoteComponents()
                        {
                            startPoint = currentNote.transform.Find("StartPoint").GetComponent<Image>(),
                            startPointColor = currentNote.transform.Find("StartPoint/StartPointColor").GetComponent<Image>(),
                            endPoint = currentNote.transform.Find("EndPoint").GetComponent<Image>(),
                            endPointColor = currentNote.transform.Find("EndPoint/EndPointColor").GetComponent<Image>(),
                            outlineLine = currentNote.transform.Find("OutlineLine").GetComponent<LineRenderer>(),
                            line = currentNote.transform.Find("Line").GetComponent<LineRenderer>(),
                            note = currentNote,
                        };

                        noteComp.outlineLine.startColor = _bodyOutStartColor;
                        noteComp.outlineLine.endColor = _bodyOutEndColor;
                        noteComp.line.startColor = _bodyInStartColor;
                        noteComp.line.endColor = _bodyInEndColor;
                        _activeNotesComponents.Add(noteComp);
                    }
                }

                foreach (FullNoteComponents note in _activeNotesComponents)
                {
                    //Fuck 4am coding im tired I wanna sleep
                    var perc = (note.note.transform.position.x - END_FADEOUT_POSX) / (START_FADEOUT_POSX - END_FADEOUT_POSX);
                    var percEnd = (note.note.transform.Find("EndPoint").position.x - END_FADEOUT_POSX) / (START_FADEOUT_POSX - END_FADEOUT_POSX);
                    var lerpStartBy = 1 - Mathf.Clamp(perc, 0, 1);
                    var lerpEndBy = 1 - Mathf.Clamp(percEnd, 0, 1);

                    note.outlineLine.startColor = Color.Lerp(_bodyOutStartColor, _bodyOutStartColorLerpEnd, lerpStartBy);
                    note.outlineLine.endColor = Color.Lerp(_bodyOutEndColor, _bodyOutEndColorLerpEnd, lerpEndBy);

                    note.line.startColor = Color.Lerp(_bodyInStartColor, _bodyInStartColorLerpEnd, lerpStartBy);
                    note.line.endColor = Color.Lerp(_bodyInEndColor, _bodyInEndColorLerpEnd, lerpEndBy);

                    note.startPoint.color = Color.Lerp(_headOutColor, _headOutColorLerpEnd, lerpStartBy);
                    note.startPointColor.color = Color.Lerp(_headInColor, _headInColorLerpEnd, lerpStartBy);

                    note.endPoint.color = Color.Lerp(_tailOutColor, _tailOutColorLerpEnd, lerpEndBy);
                    note.endPointColor.color = Color.Lerp(_tailInColor, _tailInColorLerpEnd, lerpEndBy);

                    if (note.note.transform.Find("EndPoint").position.x <= END_FADEOUT_POSX)
                        _notesToRemove.Add(note);

                }

                // :SkullEmoji:
                if (_notesToRemove.Count > 0)
                {
                    _activeNotesComponents.RemoveAll(_notesToRemove.Contains);
                    _notesToRemove.Clear();
                }
            }
            public class FullNoteComponents
            {
                public Image startPoint, endPoint;
                public Image startPointColor, endPointColor;
                public LineRenderer outlineLine, line;
                public GameObject note;

            }

            public static Color GetColorZeroAlpha(Color color) => new(color.r, color.g, color.b, 0);
        }

        public class Flashlight : GameModifierBase
        {
            public override string Name => "FL";
            public override ModifierType ModifierType => ModifierType.Flashlight;

            public Flashlight() : base() { }

            public override void Initialize(GameController __instance)
            {
                var rightSquare = new GameObject("FLImage", typeof(Image), typeof(CanvasScaler));
                var scaler = rightSquare.GetComponent<CanvasScaler>();
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                rightSquare.transform.SetParent(__instance.pointer.transform);
                rightSquare.transform.position = new Vector3(5, 0, 1);
                rightSquare.transform.localScale = new Vector2(IsAspect16_10() ? 8.75f : 8.3f, 20);
                var image = rightSquare.GetComponent<Image>();
                image.maskable = true;
                image.color = new Color(0, 0, 0, 1f);
                var topSquare = GameObject.Instantiate(rightSquare, __instance.pointer.transform);
                var bottomSquare = GameObject.Instantiate(topSquare, __instance.pointer.transform);
                var cursorMask = GameObject.Instantiate(topSquare, __instance.pointer.transform);

                topSquare.transform.localScale = new Vector2(8.35f, 3f);
                topSquare.GetComponent<RectTransform>().pivot = new Vector2(0.5f, -0.45f);

                bottomSquare.transform.localScale = new Vector2(8.35f, 3f);
                bottomSquare.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1.49f);

                cursorMask.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                cursorMask.transform.localScale = Vector2.one * 3f;
                cursorMask.GetComponent<Image>().sprite = AssetManager.GetSprite("FLMask.png");
                cursorMask.AddComponent<Mask>();

            }

            public override void Update(GameController __instance)
            {
            }

            private static bool IsAspect16_10() => Camera.main.aspect < 1.7f && Camera.main.aspect >= 1.6f;
            private static bool IsAspect16_9() => Camera.main.aspect >= 1.7f;
            private static bool IsAspect3_2() => Camera.main.aspect < 1.6f && Camera.main.aspect >= 1.5f;
            private static bool IsAspect4_3() => Camera.main.aspect < 1.5f;

        }

        public class Brutal : GameModifierBase
        {
            public override string Name => "BT";

            public override ModifierType ModifierType => ModifierType.Brutal;

            private float _defaultSpeed;
            private float _speed;
            private int _lastCombo;

            public override void Initialize(GameController __instance)
            {
                _defaultSpeed = ReplaySystemManager.gameSpeedMultiplier;
                _speed = _defaultSpeed;
                _lastCombo = 0;
            }

            public override void Update(GameController __instance)
            {
                if (__instance.paused || __instance.quitting || __instance.retrying || __instance.level_finished)
                {
                    _speed = _defaultSpeed;
                    Time.timeScale = 1f;
                }
                else if (__instance.musictrack.outputAudioMixerGroup == __instance.audmix_bgmus)
                    __instance.musictrack.outputAudioMixerGroup = __instance.audmix_bgmus_pitchshifted;
            }

            public override void SpecialUpdate(GameController __instance)
            {
                if (_lastCombo != __instance.highestcombocounter || __instance.highestcombocounter == 0)
                {
                    var shouldIncreaseSpeed = _lastCombo < __instance.highestcombocounter && __instance.highestcombocounter != 0;
                    _speed = Mathf.Clamp(_speed + (shouldIncreaseSpeed ? .015f : -.07f), _defaultSpeed, 2f);
                    Time.timeScale = _speed / _defaultSpeed;
                    __instance.musictrack.pitch = _speed;
                    __instance.audmix.SetFloat("pitchShifterMult", 1f / _speed);
                }
                _lastCombo = __instance.highestcombocounter;
            }
        }

        public class InstaFails : GameModifierBase
        {
            public override string Name => "IF";

            public override ModifierType ModifierType => ModifierType.InstaFail;

            public override void Initialize(GameController __instance) { }

            public override void Update(GameController __instance) { }

            public override void SpecialUpdate(GameController __instance)
            {
                if (!__instance.paused && !__instance.quitting && !__instance.retrying && !__instance.level_finished)
                {
                    __instance.notebuttonpressed = false;
                    __instance.musictrack.Pause();
                    __instance.sfxrefs.backfromfreeplay.Play();
                    __instance.curtainc.closeCurtain(true);
                    __instance.paused = true;
                    __instance.retrying = true;
                    __instance.quitting = true;
                }
            }

        }

        public enum ModifierType
        {
            Hidden,
            Flashlight,
            Brutal,
            InstaFail
        }
    }
}
