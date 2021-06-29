using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Linq;

namespace Tymski.Editor.TransformRandomizer
{
    public class TransformRandomizer : OdinEditorWindow
    {
        const string key = "Tools/Tymski/TransformRandomizer";
        public List<RandomTransform> transformations;
        [Min(1), MaxValue(100)] public int iterations = 1;



        [MenuItem(key)]
        static void OpenWindow()
        {
            GetWindow<TransformRandomizer>().Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            var data = EditorPrefs.GetString(key, JsonUtility.ToJson(this, false));
            JsonUtility.FromJsonOverwrite(data, this);
        }

        void OnDisable()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(key, data);
        }

        [Button("@\"Randomize (\" + Selection.gameObjects.Length + \")\"", ButtonSizes.Large)]

        public void Randomize()
        {
            Undo.RecordObjects(Selection.gameObjects.Select(go => go.transform).ToArray(), "Randomize Transforms");
            UnityEngine.Object[] ary = new UnityEngine.Object[Selection.gameObjects.Length];

            foreach (GameObject go in Selection.gameObjects)
            {
                Randomize(go.transform);
            }
        }

        public void Randomize(Transform t)
        {
            for (int i = 0; i < iterations; i++)
            {
                foreach (var transformation in transformations)
                {
                    transformation.ApplyTransform(t);
                }
            }

        }

    }

    [Serializable]
    public class RandomTransform
    {
        public AnimationCurve randomFunction = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0, 0, 2.5f), new Keyframe(1, 1, 2.5f, 0) });
        public Space space;
        public VectorType vectorType;
        public Vector3 vector;

        public void ApplyTransform(Transform transform)
        {
            if (vectorType == VectorType.Position)
            {
                if (space == Space.Local) transform.localPosition += RandomVector(vector);
                if (space == Space.Global) transform.position += RandomVector(vector);
            }
            if (vectorType == VectorType.Rotation)
            {
                if (space == Space.Local) transform.localEulerAngles += RandomVector(vector);
                if (space == Space.Global) transform.eulerAngles += RandomVector(vector);
            }
            if (vectorType == VectorType.Scale)
            {
                if (space == Space.Local) transform.localScale += RandomVector(vector);
                if (space == Space.Global)
                {
                    Transform parent = transform.parent;
                    transform.parent = null;
                    transform.localScale += RandomVector(vector);
                    transform.parent = parent;
                }
            }
        }

        public Vector3 RandomVector(Vector3 vector)
        {
            return new Vector3(
                vector.x * RandomSample(),
                vector.y * RandomSample(),
                vector.z * RandomSample()
            );
        }

        public float RandomSample()
        {
            return randomFunction.Evaluate(UnityEngine.Random.Range(0f, 1f)) * 2 - 1f;
        }
    }

    public enum Space
    {
        Local,
        Global,
    }

    public enum VectorType
    {
        Position,
        Rotation,
        Scale,
    }
}