using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace VoidTraveler.Game.Core
{
    public class Transform
    {
        public Entity? Parent { get; set; }
        public Transform ParentTransform => Parent.Value.Get<Transform>();
        public bool IsInheiritingParentTransform => Parent.HasValue;
        public Vector2 Position { get; set; }
        public Vector2 WorldPosition
        {
            get
            {
                return IsInheiritingParentTransform ? ParentTransform.GetWorld(Position) : Position;
            }
            set
            {
                if (IsInheiritingParentTransform)
                {
                    Position = ParentTransform.GetLocal(value);
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
                return IsInheiritingParentTransform ? Rotation + ParentTransform.WorldRotation : Rotation;
            }
            set
            {
                if (IsInheiritingParentTransform)
                {
                    Rotation = value - ParentTransform.WorldRotation;
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
                return IsInheiritingParentTransform ? Vector2.Multiply(Scale, ParentTransform.WorldScale) : Scale;
            }
            set
            {
                if (IsInheiritingParentTransform)
                {
                    Scale = Vector2.Divide(value, ParentTransform.WorldScale);
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

        public Matrix3x2 WorldMatrix => IsInheiritingParentTransform ? ParentTransform.GetWorldMatrix(Matrix) : Matrix;

        public Matrix3x2 InverseMatrix => Matrix3x2.CreateTranslation(-Position) *
            Matrix3x2.CreateRotation(-Rotation) *
            Matrix3x2.CreateScale(1f / Scale.X, 1f / Scale.Y);

        public Matrix3x2 WorldInverseMatrix => IsInheiritingParentTransform ? ParentTransform.WorldInverseMatrix * InverseMatrix : InverseMatrix;

        private Matrix3x2 GetWorldMatrix(Matrix3x2 localMatrix)
        {
            var transformed = localMatrix * Matrix;
            return IsInheiritingParentTransform ? ParentTransform.GetWorldMatrix(transformed) : transformed;
        }

        public Transform()
        {
            Scale = Vector2.One;
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
