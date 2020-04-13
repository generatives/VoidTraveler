using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Platform;

namespace VoidTraveler.Game.Core
{
    public class ActiveInputTracker : IInputTracker
    {
        public Vector2 PointerPosition => _inputTracker.PointerPosition;

        public Vector2 PointerDelta => _inputTracker.PointerDelta;

        public bool PointerDown => _inputTracker.PointerDown;

        public bool PointerPressed => _inputTracker.PointerPressed;

        public Vector2 MousePosition => _inputTracker.MousePosition;

        public Vector2 MouseDelta => _inputTracker.MouseDelta;

        private bool _active;

        private readonly IInputTracker _inputTracker;

        public ActiveInputTracker(IInputTracker inputTracker)
        {
            _inputTracker = inputTracker;
        }

        public void SetActive(bool active)
        {
            _active = active;
        }

        public bool GetKey(TKey key) => _active ? _inputTracker.GetKey(key) : false;

        public bool GetKeyDown(TKey key) => _active ? _inputTracker.GetKeyDown(key) : false;

        public bool GetMouseButtonDown(TMouseButton button) => _active ? _inputTracker.GetMouseButtonDown(button) : false;

        public bool GetMouseButtonPressed(TMouseButton button) => _active ? _inputTracker.GetMouseButtonPressed(button) : false;
    }
}
