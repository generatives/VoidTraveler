using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoidTraveler.Game.Core
{
    public class Transform
    {
        public Transform Parent { get; private set; }

        private List<Transform> _children;
        public IEnumerable<Transform> Children { get => _children; }
        public bool InheiritParentTransform { get; set; } = true;
        public bool IsInheiritingParentTransform => InheiritParentTransform && Parent != null;
        public Vector2 Position { get; set; }
        public Vector2 WorldPosition
        {
            get
            {
                return IsInheiritingParentTransform ? Parent.GetWorld(Position) : Position;
            }
            set
            {
                if (IsInheiritingParentTransform)
                {
                    Position = Parent.GetLocal(value);
                }
                else
                {
                    Position = value;
                }
            }
        }
        public float Rotation { get; set; }
        public float WorldRotation
        {
            get
            {
                return IsInheiritingParentTransform ? Rotation + Parent.WorldRotation : Rotation;
            }
            set
            {
                if (IsInheiritingParentTransform)
                {
                    Rotation = value - Parent.WorldRotation;
                }
                else
                {
                    Rotation = value;
                }
            }
        }
        public Vector2 Scale { get; set; }
        public Vector2 WorldScale
        {
            get
            {
                return IsInheiritingParentTransform ? Vector2.Multiply(Scale, Parent.WorldScale) : Scale;
            }
            set
            {
                if (IsInheiritingParentTransform)
                {
                    Scale = Vector2.Divide(value, Parent.WorldScale);
                }
                else
                {
                    Scale = value;
                }
            }
        }
        public Matrix3x2 Matrix => Matrix3x2.CreateScale(Scale) *
            Matrix3x2.CreateRotation(Rotation) *
            Matrix3x2.CreateTranslation(Position);

        public Matrix3x2 WorldMatrix => IsInheiritingParentTransform ? Parent.GetWorldMatrix(Matrix) : Matrix;

        public Matrix3x2 InverseMatrix => Matrix3x2.CreateTranslation(-Position) *
            Matrix3x2.CreateRotation(-Rotation) *
            Matrix3x2.CreateScale(1f / Scale.X, 1f / Scale.Y);

        public Matrix3x2 WorldInverseMatrix => IsInheiritingParentTransform ? Parent.WorldInverseMatrix * InverseMatrix : InverseMatrix;

        private Matrix3x2 GetWorldMatrix(Matrix3x2 localMatrix)
        {
            var transformed = localMatrix * Matrix;
            return IsInheiritingParentTransform ? Parent.GetWorldMatrix(transformed) : transformed;
        }

        public Transform()
        {
            _children = new List<Transform>(0);
            Scale = Vector2.One;
        }

        public void AddChild(Transform child)
        {
            if (child.Parent != null)
            {
                child.Parent.RemoveChild(child);
            }
            _children.Add(child);
            child.Parent = this;
        }

        public void RemoveChild(Transform child)
        {
            if (child.Parent == this)
            {
                child.Parent = null;
                _children.Remove(child);
            }
        }

        public void SetParent(Transform parent)
        {
            Parent?.RemoveChild(this);

            if (parent != null)
            {
                parent.AddChild(this);
            }
            else
            {
                Parent = null;
            }
        }

        public Vector2 GetLocal(Vector2 world)
        {
            return Vector2.Transform(world, WorldInverseMatrix);
        }

        public Vector2 GetWorld(Vector2 local)
        {
            return Vector2.Transform(local, WorldMatrix);
        }

        public Matrix4x4 GetCameraMatrix()
        {
            return Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0f) * Matrix4x4.CreateRotationZ(-Rotation);
        }
    }
}
