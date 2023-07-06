using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextParser : MonoBehaviour
{
    //Find a keyWord in your text , and replace with a value
    //KeyWord is need to be defined in text as '<'your text word'/>'
    public static string ParseText(string keyWord, string text, int param)
    {
        return Parser(keyWord, text, param.ToString());
    }

    public static string ParseText(string keyWord, string text, string param)
    {
        return Parser(keyWord, text, param);
    }

    public static string ParseText(string keyWord, string text, float param)
    {
        return Parser(keyWord, text, param.ToString());
    }

    private static string Parser(string _key, string _text, string _param)
    {
        string keyWord = "<" + _key + "/>";
        _text = _text.Replace(keyWord, _param);
        return _text;
    }
}
