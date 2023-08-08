using System.Collections.Generic;
using System.Linq;
using TootTally.Utils;
using UnityEngine;
using UnityEngine.UI;

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
            public static List<GameObject> _processedNotes;
            public static List<FullNoteComponents> _activeNotesComponents;
            public static List<FullNoteComponents> _notesToRemove;
            public static Color _headOutColor, _headInColor, _headOutColorLerpEnd, _headInColorLerpEnd;
            public static Color _tailOutColor, _tailInColor, _tailOutColorLerpEnd, _tailInColorLerpEnd;
            public static Color _bodyOutStartColor, _bodyOutEndColor, _bodyOutStartColorLerpEnd, _bodyOutEndColorLerpEnd;
            public static Color _bodyInStartColor, _bodyInEndColor, _bodyInStartColorLerpEnd, _bodyInEndColorLerpEnd;
            public const float START_FADEOUT_POSX = 3f;
            public const float END_FADEOUT_POSX = -5f;

            public override void Initialize(GameController __instance)
            {
                _allnoteList = __instance.allnotes;
                _processedNotes = new List<GameObject>();
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

                TootTallyLogger.LogInfo("CS: " + note.transform.position.x);
                TootTallyLogger.LogInfo("CM: " + note.transform.Find("EndPoint").transform.position.x);
            }

            public override void Update(GameController __instance)
            {
                foreach (GameObject currentNote in _allnoteList.Where(n => !_processedNotes.Contains(n)))
                {
                    if (currentNote.transform.position.x <= START_FADEOUT_POSX && currentNote.transform.Find("EndPoint").position.x > END_FADEOUT_POSX + 1)
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
                        _processedNotes.Add(currentNote);
                        _activeNotesComponents.Add(noteComp);
                        TootTallyLogger.LogInfo(currentNote.name + " Added to comp.");
                    }
                }

                foreach (FullNoteComponents note in _activeNotesComponents)
                {
                    //Fuck 4am coding im tired I wanna sleep
                    var perc = (note.note.transform.position.x - END_FADEOUT_POSX) / (START_FADEOUT_POSX - END_FADEOUT_POSX);
                    var percEnd = (note.note.transform.Find("EndPoint").position.x - END_FADEOUT_POSX) / (START_FADEOUT_POSX - END_FADEOUT_POSX);
                    var lerpStartBy = 1-Mathf.Clamp(perc, 0, 1);
                    var lerpEndBy = 1-Mathf.Clamp(percEnd, 0, 1);

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
            }

            public override void Update(GameController __instance)
            {
            }
        }

        public enum ModifierType
        {
            Hidden,
            Flashlight
        }
    }
}
