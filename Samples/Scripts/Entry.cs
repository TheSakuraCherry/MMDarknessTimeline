using System;
using MMDarkness;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

public class Entry : MonoBehaviour
{
    [ObjectPathSelector(typeof(TextAsset))]
    public string textPath;
    public TimelineGraphProcessor Processor;
    public GameObject Owner;

    public float time = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(textPath);
        // 获取字节数据
        byte[] data = textAsset.bytes;

        // 反序列化数据到对象
        TimelineGraph graph = SerializationUtility.DeserializeValue<TimelineGraph>(data, DataFormat.Binary);

        Processor = new TimelineGraphProcessor(graph);

        Processor.Owner = Owner;

    }

    public void Update()
    {
        if (Processor.Active)
        {
            time += Time.deltaTime;
            Processor.Sample(time);
        }
    }

    [Button]
    public void Play()
    {
        time = 0;
        Processor.Play();
    }
}