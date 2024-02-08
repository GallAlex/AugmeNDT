// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2023 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using UnityEngine;

namespace MagicLeap.MRTK.Utilities
{
    /// <summary>
    /// A MonoBehaviour object with bindable actions associated with Unity methods
    /// </summary>
    public class MonoBehaviourProxy : MonoBehaviour
    {
        /// <summary>
        /// Invoked when Start is called
        /// </summary>
        public Action OnStart;
        /// <summary>
        /// Invoked when Update is called
        /// </summary>
        public Action OnUpdate;
        /// <summary>
        /// Invoked when LateUpdate is called
        /// </summary>
        public Action OnLateUpdate;
        /// <summary>
        /// Invoked when OnApplicationPause is called
        /// </summary>
        public Action<bool> OnPause;

        private void Start()
        {
            OnStart?.Invoke();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
            OnPause?.Invoke(pause);
        }
    }
}