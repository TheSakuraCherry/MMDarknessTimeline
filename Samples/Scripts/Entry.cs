using MMDarkness;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public TimelineGraphAsset TimelineGraphAsset;

    public float time;

    public TimelineGraphProcessor Processor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // 反序列化数据到对象
        var graph = TimelineGraphAsset.GetGraphModel();

        Processor = new TimelineGraphProcessor(graph);
        Processor.Play();
    }

    public void Update()
    {
        time += Time.deltaTime;
        Processor.Sample(time);
    }
}