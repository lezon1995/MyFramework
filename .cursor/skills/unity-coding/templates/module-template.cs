using System;
using UnityEngine;

namespace MyGame.ModuleName
{
    /// <summary>
    /// [一句话描述本模块的职责与生命周期归属]
    /// </summary>
    public interface IXxxService
    {
        void Initialize();
        void Tick(float deltaTime);
        void Shutdown();
    }

    /// <summary>
    /// [模块名] 管理器。纯 C# 类，通过构造函数注入依赖。
    /// </summary>
    public class XxxManager : IXxxService, IDisposable
    {
        private readonly IXxxData _data;
        private readonly XxxConfig _config;
        private bool _initialized;

        public XxxManager(IXxxData data, XxxConfig config)
        {
            _data = data;
            _config = config;
        }

        public bool IsInitialized => _initialized;

        public void Initialize()
        {
            if (_initialized)
            {
                Debug.LogWarning("[XxxManager] 重复初始化，已忽略。");
                return;
            }

            _initialized = true;
        }

        public void Tick(float deltaTime)
        {
            if (!_initialized)
            {
                return;
            }
        }

        public void Shutdown()
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;
        }

        public void Dispose()
        {
            Shutdown();
        }
    }
}
