using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Platform;

namespace VoidTraveler.Game.Core
{
    public class TransformedInputTracker : IInputTracker
    {
        public Vector2 PointerPosition => Vector2.Transform(_inputTracker.PointerPosition, _transform);

        public Vector2 PointerDelta => Vector2.Transform(_inputTracker.PointerDelta, _transform);

        public bool PointerDown => _inputTracker.PointerDown;

        public bool PointerPressed => _inputTracker.PointerPressed;

        public Vector2 MousePosition => Vector2.Transform(_inputTracker.MousePosition, _transform);

        public Vector2 MouseDelta => Vector2.Transform(_inputTracker.MouseDelta, _transform);

        private Matrix3x2 _transform;

        private readonly IInputTracker _inputTracker;

        public TransformedInputTracker(IInputTracker inputTracker)
        {
            _inputTracker = inputTracker;
        }

        public void SetTransform(Matrix3x2 transform)
        {
            _transform = transform;
        }

        public bool GetKey(TKey key) => _inputTracker.GetKey(key);

        public bool GetKeyDown(TKey key) => _inputTracker.GetKeyDown(key);

        public bool GetMouseButtonDown(TMouseButton button) => _inputTracker.GetMouseButtonDown(button);

        public bool GetMouseButtonPressed(TMouseButton button) => _inputTracker.GetMouseButtonPressed(button);
    }
}
