using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrainingSample
{
    // Sensor data (5 raycast distances)
    public float[] inputs;

    // Player actions (steering, throttle)
    public float[] outputs;

    // Creates one training sample from inputs and outputs
    public TrainingSample(float[] inputs, float[] outputs)
    {
        this.inputs = inputs;
        this.outputs = outputs;
    }
}