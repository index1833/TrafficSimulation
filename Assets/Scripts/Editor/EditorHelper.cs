using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Unity.VisualScripting;

public static class EditorHelper
{
    public static void SetUpdoGroup(string label)
    {
        //�� �ڷ� ������ ��� ��ȭ�� �ϳ��� �׷����� ���´ٴ� ��
        //Ctrl+z�� �ϸ� �׷������ ��҉�
        Undo.SetCurrentGroupName(label); 

    }
    public static void BeginUndoGruop(string undoName, TrafficHeadquarter trafficHeadquarter)
    {
        //undo �׷� ����
        Undo.SetCurrentGroupName(undoName);
        //headquarter���� �߻��ϴ� ��� ��ȭ�� ����ϰ� �˴ϴ�.
        Undo.RegisterFullObjectHierarchyUndo(trafficHeadquarter.gameObject, undoName);

    }
    //���� ������Ʈ �����ؼ� Ʈ������ �����ؼ� �����ݴϴ�
    public static GameObject CreateGameObject(string name, Transform parent = null)
    {
        GameObject newGameObeject = new GameObject(name);
        newGameObeject.transform.position = Vector3.zero;
        newGameObeject.transform.localScale = Vector3.one;
        newGameObeject.transform.localRotation = Quaternion.identity;

        Undo.RegisterFullObjectHierarchyUndo(newGameObeject, "Spawn Create GameObject");
        Undo.SetTransformParent(newGameObeject.transform, parent, "Set Parent");
        return newGameObeject;
    }
    //������Ʈ ���̴� �۾��� undo�� �����ϵ��� ����
    public static T AddComponent<T>(GameObject target) where T : Component
    {
        return Undo.AddComponent<T>(target);
    }
    //���̿� ���� �浹 �Ǻ�����, ���� True��� �� �ݰ濡 ���̰� hit �Ǿ��ٴ� ��
    public static bool SphereHit(Vector3 center, float radius, Ray ray)
    {
        Vector3 originToCenter = ray.origin = center;
        float a = Vector3.Dot(ray.direction, ray.direction);
        float b = 2f * Vector3.Dot(originToCenter, ray.direction);
        float c = Vector3.Dot(originToCenter, originToCenter) - (radius * radius);
        float discriminant = b * b - 4f * a * c;
        // ���� �浹���� �ʾ���
        if(discriminant < 0f)
        {
            return false;
        }
        //���� �̻� �浹�Ǿ����
        float sqrt = Mathf.Sqrt(discriminant);
        return -b - sqrt > 0f || -b + sqrt > 0f;
    }
    //���� ���̾� �����ϴ� �Լ�
    public static void CraeteLayer(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("name", "���ο�̾ �߰��ҷ��� �̸��� �� �Է����ּ���.");
        }

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath
            ("ProjectSettings/TagManager.asset")[0]);
        var layerProps = tagManager.FindProperty("layers");
        var propCount = layerProps.arraySize;

        SerializedProperty firstEmptyProp = null;
        for(var i = 0; i < propCount; i++)
        {
            var layerProp = layerProps.GetArrayElementAtIndex(i);
            var stringValue = layerProp.stringValue;
            if(stringValue == name)
            {
                return;
            }
            //buuiltin, �̹� �ٸ� ���̾ �ڸ��� �����ϰ� �ִٸ�
            if( i < 8 || stringValue != string.Empty)
            {
                continue;
            }
            if (firstEmptyProp == null)
            {
                firstEmptyProp = layerProp;
                break;
            }

        }
        if(firstEmptyProp == null)
        {
            Debug.LogError($"���̾ �ִ� ������ �����Ͽ����ϴ�. �׷��� {name}�� �������� ���߽��ϴ�.");
            return;
        }

        firstEmptyProp.stringValue = name;
        tagManager.ApplyModifiedProperties();
    }
    /// <summary>
    /// GameObject�� ���̾ �����մϴ�. ���ϸ� �ڽĵ鵵 ���δ� �����մϴ�.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="layer"></param>
    /// <param name="�ڽĵ� ��ü�մϱ�?"></param>
    public static void SetLayer(this GameObject gameObject, int layer, bool includeChildren =false)
    {
        if(!includeChildren)
        {
            gameObject.layer = layer;
            return;
        }
        foreach(var child in gameObject.GetComponentsInChildren<Transform>(true))
        {
           child.gameObject.layer = layer;
        }
    }
   

}
