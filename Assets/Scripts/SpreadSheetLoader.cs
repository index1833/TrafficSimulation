using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Reflection;

public class SpreadSheetLoader : MonoBehaviour
{
    //���� �������� ��Ʈ�� TSV�������� �о�� �� �ֵ��� �ּҸ� ������ݴϴ�.
    public static string GetSheetDataAddress(string address, string range, long sheetID)
    {
        return $"{address}/export?format=tsv&range={range}&gid={sheetID}";
    }

    public readonly string ADDRESS = "https://docs.google.com/spreadsheets/d/1l19VB-2DnqvXAVI5WChFedMhhE71DFgEK0UBMPxnw44";

    public readonly string RANGE = "A2:D7"; // A2:B11
    public readonly long SHEET_ID = 0;
    //�о� �� ��Ʈ�� �����͸� �ӽ������س���
    private string loadString = string.Empty;

    //���� �������� ��Ʈ�� TSV��� �ּҸ� �̿��� �����͸� �о�ɴϴ�.
    private IEnumerator LoadData(Action<string> onMessageRecevied)
    {
        //���� ������ �ε� ����
        UnityWebRequest www = UnityWebRequest.Get(GetSheetDataAddress(ADDRESS, RANGE, SHEET_ID));
        yield return www.SendWebRequest();
        //������ �ε� �Ϸ�.
        Debug.Log(www.downloadHandler.text);
        if (onMessageRecevied != null)
        {
            onMessageRecevied(www.downloadHandler.text);
        }


        yield return null;
    }
    //
    public string StartLoader()
    {
        StartCoroutine(LoadData(output => loadString = output));

        return loadString;
    }

    
}
