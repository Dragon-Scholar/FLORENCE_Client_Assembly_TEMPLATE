﻿using OpenTK;

namespace FLORENCE_Client.FrameworkSpace.ClientSpace.DataSpace.OutputSpace.GraphicsSpace
{
    class Light
    {
        public Light(Vector3 position, Vector3 color, float diffuseintensity = 1.0f, float ambientintensity = 1.0f)
        {
            //System.Console.WriteLine("FLORENCE: Graphics: Light");

            Position = position;
            Color = color;

            DiffuseIntensity = diffuseintensity;
            AmbientIntensity = ambientintensity;
        }

        public Vector3 Position;
        public Vector3 Color = new Vector3();
        public float DiffuseIntensity = 1.0f;
        public float AmbientIntensity = 0.1f;
    }
}
