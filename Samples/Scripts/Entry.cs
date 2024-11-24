using System;
using MMDarkness;
using Sirenix.Serialization;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public TextAsset textAsset;
    public TimelineGraphProcessor Processor;

    public float time = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        // 获取字节数据
        byte[] data = textAsset.bytes;

        // 反序列化数据到对象
        TimelineGraph graph = SerializationUtility.DeserializeValue<TimelineGraph>(data, DataFormat.Binary);

        Processor = new TimelineGraphProcessor(graph);
        //Processor.Play();
    }

    // public void Update()
    // {
    //     time += Time.deltaTime;
    //     Processor.Sample(time);
    // }
}