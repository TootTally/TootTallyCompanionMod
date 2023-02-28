﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Utils.Helpers;
using UnityEngine;

namespace TootTally.Graphics.Animation
{
    public static class AnimationManager
    {
        private static List<CustomAnimation> _animationList;
        private static List<CustomAnimation> _animationToAdd;
        private static List<CustomAnimation> _animationToRemove;
        private static bool _isInitialized;

        public static CustomAnimation AddNewPositionAnimation(GameObject gameObject, Vector2 targetVector,
            float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.GetComponent<RectTransform>().anchoredPosition, targetVector, 1f, timeSpan, CustomAnimation.VectorType.Position, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewSizeDeltaAnimation(GameObject gameObject, Vector2 targetVector,
            float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.GetComponent<RectTransform>().sizeDelta, targetVector, 1f, timeSpan, CustomAnimation.VectorType.SizeDelta, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewScaleAnimation(GameObject gameObject, Vector2 targetVector,
           float timeSpan, EasingHelper.SecondOrderDynamics secondDegreeAnimation, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, gameObject.GetComponent<RectTransform>().localScale, targetVector, 1f, timeSpan, CustomAnimation.VectorType.Scale, secondDegreeAnimation, true, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        public static CustomAnimation AddNewAnimation(GameObject gameObject, Vector2 startingVector, Vector2 targetVector, float speedMultiplier,
            float timeSpan, CustomAnimation.VectorType vectorType, EasingHelper.SecondOrderDynamics secondDegreeAnimation, bool disposeOnFinish, Action<GameObject> onFinishCallback = null)
        {
            CustomAnimation anim = new CustomAnimation(gameObject, startingVector, targetVector, speedMultiplier, timeSpan, vectorType, secondDegreeAnimation, disposeOnFinish, onFinishCallback);
            AddToList(anim);
            return anim;
        }

        [HarmonyPatch(typeof(HomeController), nameof(HomeController.Start))]
        [HarmonyPostfix]
        public static void Initialize()
        {
            if (_isInitialized) return;

            _animationList = new List<CustomAnimation>();
            _animationToAdd = new List<CustomAnimation>();
            _animationToRemove = new List<CustomAnimation>();
            _isInitialized = true;
        }

        public static void AddToList(CustomAnimation anim) => _animationToAdd.Add(anim);
        public static void RemoveFromList(CustomAnimation anim) => _animationToRemove.Add(anim);

        [HarmonyPatch(typeof(Plugin), nameof(Plugin.Update))]
        [HarmonyPostfix]
        public static void Update()
        {
            if (!_isInitialized) return;

            //add animation the needs to be added
            if (_animationToAdd.Count > 0)
            {
                _animationToAdd.ForEach(anim => _animationList.Add(anim));
                _animationToAdd.Clear();
            }

            //update all animations
            _animationList.ForEach(anim => anim.UpdateVector());

            //remove animations that are done
            if (_animationToRemove.Count > 0)
            {
                _animationToRemove.ForEach(anim => _animationList.Remove(anim));
                _animationToRemove.Clear();
            }

        }

    }
}