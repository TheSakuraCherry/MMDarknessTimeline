using System;
using MMDarkness;
using Sirenix.Serialization;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public TimelineGraphProcessor Processor;

    public TimelineGraphAsset TimelineGraphAsset;
    
    public float time = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 反序列化数据到对象
        TimelineGraph graph = TimelineGraphAsset.GetGraphModel();

        Processor = new TimelineGraphProcessor(graph);
        Processor.Play();
    }

    public void Update()
    {
        time += Time.deltaTime;
        Processor.Sample(time);
    }
}