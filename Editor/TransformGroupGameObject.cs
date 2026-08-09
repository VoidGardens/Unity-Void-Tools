using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Name
{    
    public class TransformGroupManipulator : EditorWindow
    {
        private enum TransformField { Position, Rotation, Scale }
        private enum OperationType { Add, Subtract, Multiply, Divide, Set }

        private TransformField _selectedField = TransformField.Position;
        private OperationType _selectedOperation = OperationType.Add;

        private Vector3 _positionValue = Vector3.zero;
        private Vector3 _rotationValue = Vector3.zero;
        private Vector3 _scaleValue = Vector3.one;

        private bool _useLocalPosition = false;
        private bool _useLocalRotation = false;

        private bool _applyToX = true;
        private bool _applyToY = true;
        private bool _applyToZ = true;

        [MenuItem("VoidGardens/Transform Group Manipulator")]
        public static void ShowWindow()
        {
            GetWindow<TransformGroupManipulator>("Transform Manipulator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Transform Group Manipulator", EditorStyles.boldLabel);

            _selectedField = (TransformField)EditorGUILayout.EnumPopup("Transform Field", _selectedField);
            _selectedOperation = (OperationType)EditorGUILayout.EnumPopup("Operation", _selectedOperation);

            EditorGUILayout.Space();

            switch (_selectedField)
            {
                case TransformField.Position:
                    _positionValue = EditorGUILayout.Vector3Field("Position Value", _positionValue);
                    _useLocalPosition = EditorGUILayout.Toggle("Use Local Position", _useLocalPosition);
                    DrawAxisToggles();
                    break;

                case TransformField.Rotation:
                    _rotationValue = EditorGUILayout.Vector3Field("Rotation Value", _rotationValue);
                    _useLocalRotation = EditorGUILayout.Toggle("Use Local Rotation", _useLocalRotation);
                    DrawAxisToggles();
                    break;

                case TransformField.Scale:
                    _scaleValue = EditorGUILayout.Vector3Field("Scale Value", _scaleValue);
                    DrawAxisToggles();
                    break;
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply to Selection"))
            {
                ApplyToSelection();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            if (GUILayout.Button("Center Parent to Children Bounds"))
            {
                CenterParentToChildrenBounds();
            }
        }

        private void DrawAxisToggles()
        {
            EditorGUILayout.BeginHorizontal();
            _applyToX = EditorGUILayout.ToggleLeft("X", _applyToX, GUILayout.Width(30));
            _applyToY = EditorGUILayout.ToggleLeft("Y", _applyToY, GUILayout.Width(30));
            _applyToZ = EditorGUILayout.ToggleLeft("Z", _applyToZ, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
        }

        private void ApplyToSelection()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        // Правильный Undo для Transform
        Undo.RecordObjects(selectedObjects.Select(go => go.transform).ToArray(), "Transform Group Manipulation");

        foreach (GameObject go in selectedObjects)
        {
            Transform t = go.transform;

            switch (_selectedField)
            {
                case TransformField.Position:
                    if (_useLocalPosition)
                    {
                        Vector3 localPos = t.localPosition;
                        if (_applyToX) localPos.x = ApplyOperation(localPos.x, _positionValue.x);
                        if (_applyToY) localPos.y = ApplyOperation(localPos.y, _positionValue.y);
                        if (_applyToZ) localPos.z = ApplyOperation(localPos.z, _positionValue.z);
                        t.localPosition = localPos;
                    }
                    else
                    {
                        Vector3 worldPos = t.position;
                        if (_applyToX) worldPos.x = ApplyOperation(worldPos.x, _positionValue.x);
                        if (_applyToY) worldPos.y = ApplyOperation(worldPos.y, _positionValue.y);
                        if (_applyToZ) worldPos.z = ApplyOperation(worldPos.z, _positionValue.z);
                        t.position = worldPos;
                    }
                    break;

                case TransformField.Rotation:
                    if (_useLocalRotation)
                    {
                        Quaternion rotationDelta = Quaternion.identity;
                        if (_applyToX) rotationDelta *= Quaternion.AngleAxis(_rotationValue.x, t.right);
                        if (_applyToY) rotationDelta *= Quaternion.AngleAxis(_rotationValue.y, t.up);
                        if (_applyToZ) rotationDelta *= Quaternion.AngleAxis(_rotationValue.z, t.forward);
                        t.rotation = rotationDelta * t.rotation;
                    }
                    else
                    {
                        Quaternion delta = Quaternion.identity;
                        if (_applyToX) delta *= Quaternion.AngleAxis(_rotationValue.x, Vector3.right);
                        if (_applyToY) delta *= Quaternion.AngleAxis(_rotationValue.y, Vector3.up);
                        if (_applyToZ) delta *= Quaternion.AngleAxis(_rotationValue.z, Vector3.forward);
                        t.rotation = delta * t.rotation;
                    }
                    break;

                case TransformField.Scale:
                    Vector3 scale = t.localScale;
                    if (_applyToX) scale.x = ApplyOperation(scale.x, _scaleValue.x);
                    if (_applyToY) scale.y = ApplyOperation(scale.y, _scaleValue.y);
                    if (_applyToZ) scale.z = ApplyOperation(scale.z, _scaleValue.z);
                    t.localScale = scale;
                    break;
            }
        }
    }


        private float ApplyOperation(float original, float value)
        {
            switch (_selectedOperation)
            {
                case OperationType.Add: return original + value;
                case OperationType.Subtract: return original - value;
                case OperationType.Multiply: return original * value;
                case OperationType.Divide: return value == 0f ? original : original / value;
                case OperationType.Set: return value;
                default: return original;
            }
        }

        private void CenterParentToChildrenBounds()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected!");
                return;
            }

            foreach (GameObject parentObj in selectedObjects)
            {
                Transform parentTransform = parentObj.transform;

                if (parentTransform.childCount == 0)
                {
                    Debug.LogWarning($"{parentObj.name} has no children!");
                    continue;
                }

                // Получаем все Renderer'ы у детей
                Renderer[] childRenderers = parentObj.GetComponentsInChildren<Renderer>();

                if (childRenderers.Length == 0)
                {
                    Debug.LogWarning($"{parentObj.name} has no renderers in children!");
                    continue;
                }

                // Вычисляем общий bounding box
                Bounds bounds = childRenderers[0].bounds;
                for (int i = 1; i < childRenderers.Length; i++)
                {
                    bounds.Encapsulate(childRenderers[i].bounds);
                }

                Vector3 boundsCenter = bounds.center;

                // Записываем мировые позиции всех детей
                Vector3[] childWorldPositions = new Vector3[parentTransform.childCount];
                for (int i = 0; i < parentTransform.childCount; i++)
                {
                    childWorldPositions[i] = parentTransform.GetChild(i).position;
                }

                // Регистрируем Undo для родителя и всех детей
                Undo.RecordObject(parentTransform, "Center Parent to Children Bounds");
                foreach (Transform child in parentTransform)
                {
                    Undo.RecordObject(child, "Center Parent to Children Bounds");
                }

                // Перемещаем родителя в центр bounds
                parentTransform.position = boundsCenter;

                // Восстанавливаем мировые позиции детей (корректируем их локальные позиции)
                for (int i = 0; i < parentTransform.childCount; i++)
                {
                    parentTransform.GetChild(i).position = childWorldPositions[i];
                }

                Debug.Log($"Centered {parentObj.name} to children bounds at {boundsCenter}");
            }
        }
    }
}
